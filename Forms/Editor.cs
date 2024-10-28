using System.Numerics;

namespace GK1_PolygonEditor
{
    public partial class PolygonEditor : Form
    {
        private Scene _scene;
        private Renderer _renderer;
        private UnsafeBitmap _unsafeBitmap;
        private Camera _camera;

        private ContextMenuVisitor _contextMenuVisitor;

        private Vertex? _selectedVertex = null;
        private Shape? _selectedShape = null;
        private Shape? _newShape = null;

        private bool _isDraggingVertex = false;
        private bool _isDraggingShape = false;
        private bool _isDraggingViewport = false;
        private bool _isCreatingShape = false;

        private Point _previousMousePosition;

        public PolygonEditor()
        {
            InitializeComponent();

            _scene = new Scene();
            _scene.ClearColor = Color.White;
            _unsafeBitmap = new UnsafeBitmap(c_Canvas.Width, c_Canvas.Height);
            _camera = new Camera(_unsafeBitmap.Width, _unsafeBitmap.Height);
            _renderer = new Renderer(_scene, c_Canvas, _unsafeBitmap, _camera);
            _contextMenuVisitor = new ContextMenuVisitor(_renderer);

            Utils.AddRadioCheckedBinding(rb_Library, _renderer, "RendererEnum", RendererEnum.Library);
            Utils.AddRadioCheckedBinding(rb_Bresenham, _renderer, "RendererEnum", RendererEnum.Bresenham);


            // Hardcoding shape for presentation
            var verts = new Vertex[]
            {
                new Vertex(160, -115),
                new Vertex(160, 50),
                new Vertex(35, 130),
                new Vertex(-105, 85),
                new Vertex(-85, -115),
            };

            var segments = new Segment[]
            {
                new Edge(verts[0], verts[1]),
                new Edge(verts[1], verts[2]),
                new Edge(verts[2], verts[3]),
                new BezierCurve(verts[3], verts[4]),
                new Edge(verts[4], verts[0]),
            };

            (segments[3] as BezierCurve)!.ControlPoint1.FirstSegment = segments[2];
            (segments[3] as BezierCurve)!.ControlPoint2.SecondSegment = segments[4];

            (segments[0] as Edge)!.SetVertical();
            (segments[1] as Edge)!.SetFixedLength(150);
            (segments[4] as Edge)!.SetHorizontal();
            verts[3].SetG1Continuity();

            _scene.AddShape(new Shape(verts, segments));
            _scene.Shapes.ForEach((x) => x.IsClosed = true);

            _renderer.RenderScene();
        }

        private void c_Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            Vertex mouseWorldPosition = _camera.ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Left && _isDraggingViewport == false)
            {
                if (_isCreatingShape)
                {
                    // Closing shape
                    Vertex first = _newShape!.Vertices.First();
                    if (_newShape!.Vertices.Count > 2 &&
                        first.DistanceToSquared(mouseWorldPosition) < first.Radius * first.Radius)
                    {
                        first.FirstSegment = _newShape.Segments.Last();
                        first.FirstSegment.End = first;
                        _newShape.IsClosed = true;

                        _isCreatingShape = false;
                        _renderer.RenderScene();
                    }
                    else // Add vertex to shape
                    {
                        var newVertex = new Vertex(mouseWorldPosition.X, mouseWorldPosition.Y);
                        Edge edge = new Edge(newVertex, newVertex);
                        newVertex.FirstSegment = _newShape.Segments.Last();
                        newVertex.FirstSegment.End = newVertex;

                        _newShape.AddVertex(newVertex);
                        _newShape.AddSegment(edge);

                        _renderer.RenderScene();
                    }
                }
                else if (e.Clicks == 2) // Start creating shape
                {
                    _isCreatingShape = true;
                    _newShape = new Shape();
                    var startVertex = new Vertex(mouseWorldPosition.X, mouseWorldPosition.Y);
                    _newShape.AddVertex(startVertex);
                    _newShape.AddSegment(new Edge(startVertex, startVertex));
                    _scene.AddShape(_newShape);
                    _renderer.RenderScene();
                }
                else
                {
                    for (int i = _scene.Shapes.Count - 1; i >= 0; --i)
                    {
                        Shape shape = _scene.Shapes[i];
                        foreach (Vertex v in shape.Vertices)
                        {
                            if (v.DistanceToSquared(mouseWorldPosition) < v.Radius * v.Radius)
                            {
                                _selectedVertex = v;
                                _isDraggingVertex = true;
                                return;
                            }
                        }

                        shape.Segments.ForEach((x) =>
                        {
                            if (x is BezierCurve bc)
                            {
                                foreach (Vertex v in new Vertex[] { bc.ControlPoint1, bc.ControlPoint2 })
                                {
                                    if (v.DistanceToSquared(mouseWorldPosition) < v.Radius * v.Radius)
                                    {
                                        _selectedVertex = v;
                                        _isDraggingVertex = true;
                                        return;
                                    }
                                }
                            }
                        });


                        if (shape.IsInShape(mouseWorldPosition) && !_isDraggingVertex)
                        {
                            _selectedShape = shape;
                            _scene.Shapes.RemoveAt(i);
                            _scene.Shapes.Add(shape);
                            _isDraggingShape = true;
                            _renderer.RenderScene();
                            return;
                        }
                    }
                }
            }
            else if (e.Button == MouseButtons.Right && _isDraggingVertex == false)
            {
                if (_isCreatingShape)
                {
                    _isCreatingShape = false;
                    _scene.Shapes.Remove(_newShape!);
                    _renderer.RenderScene();
                }
                else
                {
                    foreach (Shape shape in _scene.Shapes)
                    {
                        foreach (Vertex v in shape.Vertices)
                        {
                            if (v.DistanceToSquared(mouseWorldPosition) < v.Radius * v.Radius)
                            {
                                v.Accept(_contextMenuVisitor);
                                return;
                            }
                        }

                        foreach (Segment s in shape.Segments)
                        {
                            if (s.IsPointOnSegment(mouseWorldPosition))
                            {
                                s.Accept(_contextMenuVisitor);
                                return;
                            }
                        }

                        if (shape.IsInShape(mouseWorldPosition))
                        {
                            shape.Accept(_contextMenuVisitor);
                            return;
                        }
                    }
                }
                _isDraggingViewport = true;
            }
        }

        private void c_Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            int deltaX = (e.X - _previousMousePosition.X);
            int deltaY = (e.Y - _previousMousePosition.Y);
            var currentMousePos = _camera.ScreenToWorld(e.Location);
            tssl_Position.Text = $"X: {currentMousePos.X:0}, Y:{currentMousePos.Y:0}";

            if (_isDraggingViewport)
            {
                _camera.X += deltaX;
                _camera.Y -= deltaY;

                _renderer.RenderScene();
            }
            else if (_isDraggingShape && _selectedShape != null)
            {
                _selectedShape.Vertices.ForEach((x) => { x.X += deltaX; x.Y -= deltaY; });
                _selectedShape.Segments.ForEach((x) =>
                {
                    if (x is BezierCurve bc)
                    {
                        bc.ControlPoint1.X += deltaX; bc.ControlPoint1.Y -= deltaY;
                        bc.ControlPoint2.X += deltaX; bc.ControlPoint2.Y -= deltaY;
                    }
                });
                _renderer.RenderScene();
            }
            else if (_isDraggingVertex && _selectedVertex != null)
            {
                Vector2 delta = new Vector2(deltaX, -deltaY);
                _selectedVertex.Move(delta);

                _renderer.RenderScene();
            }
            else if (_isCreatingShape)
            {
                _newShape!.Segments.Last().End = currentMousePos;
                _renderer.RenderScene();
            }

            _previousMousePosition = e.Location;
        }

        private void c_Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDraggingVertex = false;
                _selectedVertex = null;

                _isDraggingShape = false;
                _selectedShape = null;
            }

            if (e.Button == MouseButtons.Right)
            {
                _isDraggingViewport = false;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void manualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new ManualForm();
            form.ShowDialog();
        }

        private void relationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new RelationsForm();
            form.ShowDialog();
        }
    }
}
