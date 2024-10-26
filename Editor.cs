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
        private Vertex? _selectedControlPoint = null;
        private Shape? _selectedShape = null;
        private Shape? _newShape = null;

        private bool _isDraggingVertex = false;
        private bool _isDraggingShape = false;
        private bool _isDraggingViewport = false;
        private bool _isCreatingShape = false;

        private Shape _shapeToDelete;

        private Point _previousMousePosition;

        public PolygonEditor()
        {
            InitializeComponent();

            _scene = new Scene();
            _unsafeBitmap = new UnsafeBitmap(c_Canvas.Width, c_Canvas.Height);
            _camera = new Camera(_unsafeBitmap.Width, _unsafeBitmap.Height);
            _renderer = new Renderer(_scene, c_Canvas, _unsafeBitmap, _camera);
            _contextMenuVisitor = new ContextMenuVisitor(_scene, _renderer);

            Utils.AddRadioCheckedBinding(rb_Library, _renderer, "RendererEnum", RendererEnum.Library);
            Utils.AddRadioCheckedBinding(rb_Bresenham, _renderer, "RendererEnum", RendererEnum.Bresenham);

            var verts1 = new Vertex[]
            {
                new Vertex(0, -50),
                new Vertex(100, 75),
                new Vertex(25, 125),
                new Vertex(50, 50),
            };

            var segments1 = new Segment[]
            {
                new Edge(verts1[0], verts1[1]),
                new Edge(verts1[1], verts1[2]),
                new Edge(verts1[2], verts1[3]),
                new BezierCurve(verts1[3], verts1[0])
            };

            var verts2 = new Vertex[]
            {
                new Vertex(0, 50),
                new Vertex(-100, -75),
                new Vertex(-25, -125),
                new Vertex(-50, -50),
            };

            var segments2 = new Segment[]
            {
                new Edge(verts2[0], verts2[1]),
                new Edge(verts2[1], verts2[2]),
                new Edge(verts2[2], verts2[3]),
                new Edge(verts2[3], verts2[0])
            };


            _scene.AddShape(new Shape(verts1, segments1));
            _scene.AddShape(new Shape(verts2, segments2));
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
                    Vertex first = _newShape!.Vertices.First();
                    if (_newShape!.Vertices.Count > 2 &&
                        first.DistanceToSquared(mouseWorldPosition) < first.Radius * first.Radius)
                    {
                        _newShape.Segments.Last().End = first;

                        _isCreatingShape = false;
                        _newShape.IsClosed = true;
                        _renderer.RenderScene();
                    }
                    else
                    {
                        var newVertex = new Vertex(mouseWorldPosition.X, mouseWorldPosition.Y);
                        _newShape.Vertices.Add(newVertex);
                        _newShape.Segments.Last().End = newVertex;
                        _newShape.Segments.Add(new Edge(newVertex, newVertex));
                        _renderer.RenderScene();
                    }
                }
                else if (e.Clicks == 2)
                {
                    _isCreatingShape = true;
                    _newShape = new Shape();
                    var startVertex = new Vertex(mouseWorldPosition.X, mouseWorldPosition.Y);
                    _newShape.Vertices.Add(startVertex);
                    _newShape.Segments.Add(new Edge(startVertex, startVertex));
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
                                        _selectedControlPoint = v;
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
                                _contextMenuVisitor.SetCurrentShape(shape);
                                v.Accept(_contextMenuVisitor);
                                return;
                            }
                        }

                        foreach (Segment s in shape.Segments)
                        {
                            if (s.IsPointOnSegment(mouseWorldPosition))
                            {
                                _contextMenuVisitor.SetCurrentShape(shape);
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
            else if (_isDraggingVertex)
            {
                if (_selectedControlPoint != null)
                {
                    _selectedControlPoint.X += deltaX;
                    _selectedControlPoint.Y -= deltaY;
                }
                else if (_selectedVertex != null)
                {
                    Vector2 delta = new Vector2(deltaX, -deltaY);
                    _selectedVertex.Move(delta);
                }

                _renderer.RenderScene();
            }
            else if (_isCreatingShape)
            {
                var currentMousePos = _camera.ScreenToWorld(e.Location);

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
                _selectedControlPoint = null;

                _isDraggingShape = false;
                _selectedShape = null;
            }

            if (e.Button == MouseButtons.Right)
            {
                _isDraggingViewport = false;
            }
        }
    }
}
