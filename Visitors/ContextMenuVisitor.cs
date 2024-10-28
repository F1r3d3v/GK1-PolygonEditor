using System.Windows.Forms.VisualStyles;

namespace GK1_PolygonEditor
{
    internal class ContextMenuVisitor(Renderer _renderer) : IVisitor
    {
        private ContextMenuStrip? _contextMenu;

        public void Visit(Vertex vertex)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Delete vertex", null, (s, e) => { vertex.Delete(); _renderer.RenderScene(); }));
            if (vertex.CouldAddContinuityConstrain())
            {
                ToolStripMenuItem contraints = new ToolStripMenuItem("Set continuity", null);
                contraints.DropDownItems.Add(new ToolStripMenuItem("G0", null, (s, e) => { vertex.SetG0Continuity(); _renderer.RenderScene(); }));
                contraints.DropDownItems.Add(new ToolStripMenuItem("G1", null, (s, e) => { vertex.SetG1Continuity(); _renderer.RenderScene(); }));
                contraints.DropDownItems.Add(new ToolStripMenuItem("C1", null, (s, e) => { vertex.SetC1Continuity(); _renderer.RenderScene(); }));
                _contextMenu.Items.Add(contraints);
            }
            _contextMenu.Show(Cursor.Position);
        }

        public void Visit(Edge edge)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Split edge", null, (s, e) => { edge.SplitEdge(); _renderer.RenderScene(); }));
            _contextMenu.Items.Add(new ToolStripMenuItem("Change to bezier", null, (s, e) => { edge.ChangeToBezier(); _renderer.RenderScene(); }));
            if (edge.Constraint == null)
            {
                ToolStripMenuItem contraints = new ToolStripMenuItem("Set constraint", null);
                contraints.DropDownItems.Add(new ToolStripMenuItem("Horizontal", Resources.horizontal_icon, (s, e) => { edge.SetHorizontal(); _renderer.RenderScene(); }));
                contraints.DropDownItems.Add(new ToolStripMenuItem("Vertical", Resources.vertical_icon, (s, e) => { edge.SetVertical(); _renderer.RenderScene(); }));
                contraints.DropDownItems.Add(new ToolStripMenuItem("Fixed length", Resources.fixed_length_icon, (s, e) => { edge.SetFixedLengthDialog(); _renderer.RenderScene(); }));
                _contextMenu.Items.Add(contraints);
            }
            else
            {
                _contextMenu.Items.Add(new ToolStripMenuItem("Remove constraint", null, (s, e) => { edge.RemoveConstraint(); _renderer.RenderScene(); }));
            }
            _contextMenu.Items.Add(new ToolStripMenuItem("Toggle antialiasing", null, (s, e) => { edge.ToggleAntialiasing(); _renderer.RenderScene(); }));
            _contextMenu.Show(Cursor.Position);
        }

        public void Visit(BezierCurve bezierCurve)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Change to edge", null, (s, e) => { bezierCurve.ChangeToEdge(); _renderer.RenderScene(); }));
            _contextMenu.Items.Add(new ToolStripMenuItem("Toggle antialiasing", null, (s, e) => { bezierCurve.ToggleAntialiasing(); _renderer.RenderScene(); }));
            _contextMenu.Show(Cursor.Position);
        }

        public void Visit(Shape shape)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("Delete shape", null, (s, e) => { shape.Scene!.DeleteShape(shape); _renderer.RenderScene(); }));
            _contextMenu.Show(Cursor.Position);
        }
    }
}
