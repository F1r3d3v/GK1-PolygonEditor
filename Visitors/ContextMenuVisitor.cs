using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace GK1_PolygonEditor
{
    internal class ContextMenuVisitor : IVisitor
    {
        private ContextMenuStrip? _contextMenu;
        private Renderer _renderer;
        private Scene _scene;
        private Shape? _shape;

        public void SetCurrentShape(Shape shape) => _shape = shape;

        public ContextMenuVisitor(Scene scene, Renderer renderer)
        {
            _scene = scene;
            _renderer = renderer;
        }

        public void Visit(Vertex vertex)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Delete vertex", null, (s, e) => DeleteVertex(vertex)));
            if (vertex.FirstSegment is BezierCurve || vertex.SecondSegment is BezierCurve)
            {
                ToolStripMenuItem contraints = new ToolStripMenuItem("Set continuity", null);
                contraints.DropDownItems.Add(new ToolStripMenuItem("G0", null));
                contraints.DropDownItems.Add(new ToolStripMenuItem("G1", null));
                contraints.DropDownItems.Add(new ToolStripMenuItem("C1", null));
                _contextMenu.Items.Add(contraints);
            }
            _contextMenu.Items.Add(new ToolStripMenuItem("Remove continuity", null));
            _contextMenu.Show(Cursor.Position);
        }

        public void Visit(Edge edge)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Split edge", null, (s, e) => SplitEdge(edge)));
            _contextMenu.Items.Add(new ToolStripMenuItem("Change to bezier", null, (s, e) => ChangeToBezier(edge)));
            if (edge.Constraint == null)
            {
                ToolStripMenuItem contraints = new ToolStripMenuItem("Set constraint", null);
                contraints.DropDownItems.Add(new ToolStripMenuItem("Horizontal", Resources.horizontal_icon, (s, e) => SetHorizontal(edge)));
                contraints.DropDownItems.Add(new ToolStripMenuItem("Vertical", Resources.vertical_icon, (s, e) => SetVertical(edge)));
                contraints.DropDownItems.Add(new ToolStripMenuItem("Fixed length", Resources.fixed_length_icon, (s, e) => SetFixedLength(edge)));
                _contextMenu.Items.Add(contraints);
            }
            else
            {
                _contextMenu.Items.Add(new ToolStripMenuItem("Remove constraint", null, (s, e) => RemoveConstraint(edge)));
            }
            _contextMenu.Show(Cursor.Position);
        }

        public void Visit(BezierCurve bezierCurve)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Change to edge", null, (s, e) => ChangeToEdge(bezierCurve)));
            _contextMenu.Show(Cursor.Position);
        }

        public void Visit(Shape shape)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Delete shape", null, (s, e) => DeleteShape(shape)));
            _contextMenu.Show(Cursor.Position);
        }

        private void DeleteVertex(Vertex vertex)
        {
            if (_shape.Vertices.Count <= 3)
            {
                DeleteShape(_shape);
                _shape = null;
                return;
            }
            Segment s1 = _shape.Segments.Where((x) => x.End == vertex).First();
            Segment s2 = _shape.Segments.Where((x) => x.Start == vertex).First();
            Edge e = new Edge(s1.Start, s2.End);
            int edge_idx = _shape.Segments.IndexOf(s1);
            _shape.Segments.Insert(edge_idx, e);
            _shape.Segments.Remove(s1);
            _shape.Segments.Remove(s2);
            _shape.Vertices.Remove(vertex);
            _renderer.RenderScene();
        }

        private void SplitEdge(Edge edge)
        {
            Vertex mp = edge.MidPoint();
            int vert_idx = _shape.Vertices.IndexOf(edge.Start);
            _shape.Vertices.Insert(vert_idx, mp);
            int edge_idx = _shape.Segments.IndexOf(edge);
            Edge e1 = new Edge(edge.Start, mp);
            Edge e2 = new Edge(mp, edge.End);
            _shape.Segments.Remove(edge);
            _shape.Segments.Insert(edge_idx, e1);
            _shape.Segments.Insert(edge_idx + 1, e2);
            _renderer.RenderScene();

        }

        private void DeleteShape(Shape shape)
        {
            _scene.Shapes.Remove(shape);
            _renderer.RenderScene();
        }

        private void ChangeToEdge(BezierCurve bezierCurve)
        {
            Edge e = new Edge(bezierCurve.Start, bezierCurve.End);
            int bc_idx = _shape.Segments.IndexOf(bezierCurve);
            _shape.Segments[bc_idx] = e;
            _renderer.RenderScene();
        }
        private void ChangeToBezier(Edge edge)
        {
            BezierCurve bc = new BezierCurve(edge.Start, edge.End);
            int edge_idx = _shape.Segments.IndexOf(edge);
            _shape.Segments[edge_idx] = bc;
            _renderer.RenderScene();
        }

        private void SetHorizontal(Edge edge)
        {
            if (edge.Start.FirstSegment!.Constraint?.ConstraintType == ConstraintType.Horizontal ||
                edge.End.SecondSegment!.Constraint?.ConstraintType == ConstraintType.Horizontal)
            {
                MessageBox.Show("Two adjecent horizontal edges are forbidden!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Vector2 moveDelta = new Vector2(0, edge.End.Y - edge.Start.Y);
            if (CouldSetConstraint(edge, moveDelta))
            {
                edge.Constraint = new HorizontalContraint(edge);
                edge.Start.Move(moveDelta);
            }
            else
                MessageBox.Show("Cannot set constraint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _renderer.RenderScene();
        }

        private void SetVertical(Edge edge)
        {
            if (edge.Start.FirstSegment!.Constraint?.ConstraintType == ConstraintType.Vertical ||
                edge.End.SecondSegment!.Constraint?.ConstraintType == ConstraintType.Vertical)
            {
                MessageBox.Show("Two adjecent vertical edges are forbidden!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Vector2 moveDelta = new Vector2(edge.End.X - edge.Start.X, 0);
            if (CouldSetConstraint(edge, moveDelta))
            {
            edge.Constraint = new VerticalConstraint(edge);
                edge.Start.Move(moveDelta);
            }
            else
                MessageBox.Show("Cannot set constraint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _renderer.RenderScene();
        }

        private void SetFixedLength(Edge edge)
        {
            SetLengthForm dialog = new SetLengthForm();
            dialog.Length = (decimal)edge.Length();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                float length = (float)dialog.Length;
                edge.Constraint = new FixedLengthConstraint(edge, length);
                if (Math.Abs(edge.Length() - length) > 1e-3)
                {
                    var edgeLength = edge.Length();
                    var p = length / edgeLength;
                    float x = ((p * (edge.End.X - edge.Start.X) + edge.Start.X));
                    float y = ((p * (edge.End.Y - edge.Start.Y) + edge.Start.Y));
                    Vector2 moveDelta = new Vector2(x - edge.End.X, y - edge.End.Y);
                    if (CouldSetConstraint(edge, moveDelta))
                        edge.End.Move(moveDelta);
                    else
                    {
                        MessageBox.Show("Cannot set constraint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        edge.Constraint = null;
                    }
                }

                _renderer.RenderScene();
            }
        }
        private void RemoveConstraint(Edge edge)
        {
            edge.Constraint = null;
            _renderer.RenderScene();
        }

        private bool CouldSetConstraint(Edge edge, Vector2 delta)
        {
            return CoudlSet(true) && CoudlSet(false);

            bool CoudlSet(bool orientX)
            {
                float move = orientX ? delta.X : delta.Y;
                ConstraintType constraint = orientX ? ConstraintType.Horizontal : ConstraintType.Vertical;
                if (move != 0)
                {
                    Segment? e = edge.End.SecondSegment;

                    for (; e != edge; e = e.End.SecondSegment)
                        if (e!.Constraint == null || e.Constraint.ConstraintType == constraint)
                            break;

                    if (e == edge)
                        return false;
                }
                return true;
            }
        }
    }
}
