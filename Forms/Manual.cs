using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GK1_PolygonEditor
{
    partial class ManualForm : Form
    {
        public ManualForm()
        {
            InitializeComponent();
            textBoxDescription.Text = """
Overview:
This Polygon Editor is a graphical application for creating, modifying, and managing polygon shapes, along with various geometric constraints. Users can define shapes on a canvas, apply vertex continuity and edge constraints, and choose different rendering methods.

1. Creating and Modifying Shapes:
- Draw Polygon:
Double-click anywhere on the canvas to start creating a new shape.
Click to add vertices sequentially. A shape closes automatically once the final vertex connects to the first.

- Add/Move Vertices:
Click and drag existing vertices to reposition them.
For an existing edge, right-click to access the context menu and choose "Split edge" to add a new vertex in between.

- Adjust Shape Position:
Drag an entire shape to move it across the canvas. This is enabled by selecting the shape's interior.

2. Applying Constraints:
- Vertex Continuity:
Right-click a vertex and choose "Set continuity" to apply G0, G1, or C1 continuity constraints.

- Edge Constraints:
Right-click an edge to access options like Horizontal, Vertical, and Fixed-Length constraints.
Removing constraints is done by selecting the edge, right-clicking, and choosing "Remove constraint."

- Fixed Length:
Choose the Fixed Length constraint to lock an edge's length. A dialog box will prompt you to enter the length.

3. Bezier Curves:
Convert edges into Bezier curves by selecting "Change to bezier" in the edge’s context menu.
Modify control points directly to adjust the curve shape.

4. Renderer Selection:
Use the radio buttons to choose between Library and Bresenham rendering methods.

5. Additional Options:
- Deleting Shapes/Vertices:
Right-click a shape or vertex and choose "Delete" from the context menu.

- Camera Controls:
Hold and drag the right mouse button to adjust the camera’s position, viewing different parts of the canvas.
""";

        }
    }
}
