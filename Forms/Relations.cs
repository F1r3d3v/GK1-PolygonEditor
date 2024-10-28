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
    partial class RelationsForm : Form
    {
        public RelationsForm()
        {
            InitializeComponent();
            textBoxDescription.Text = """
Preserves geometric constraints (horizontal, vertical, fixed length) on a vertex and its connected segments by adjusting neighboring vertices accordingly.

Algorithm Steps:

1. Initialize Adjustment Vector:
Set the initial movement of vertex `v`.

2. Clockwise Adjustment:
- Start with the segment attached to 'v'.
- For each segment:
    - If the constraint is already met, stop adjusting.
    - Otherwise, check the constraint type:
        - Horizontal Constraint: Adjust movement along the Y-axis.
        - Vertical Constraint: Adjust movement along the X-axis.
        - Fixed Length Constraint: Maintain the previous adjustment vector.
    - Apply the computed adjustment to the segment’s vertex and proceed to the next connected segment.
     
3. Update Adjustment Vector for Polygon Movement:
- If only a subset of vertices was moved, reverse direction with the initial adjustment vector.
- If the entire polygon moved, set a new vector to ensure cohesive polygon movement.

4. Counterclockwise Adjustment:
- Start with the segment attached to 'v' and repeat the clockwise adjustment steps in reverse:
- Traverse segments counterclockwise, checking and adjusting each one until constraints are preserved.
""";

        }
    }
}
