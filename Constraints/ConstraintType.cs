using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal enum ConstraintType
    {
        Horizontal,
        Vertical,
        FixedLength,
        BezierContinuity
    }

    internal enum VertexContinuity
    {
        G0Continuity,
        G1Continuity,
        C1Continuity
    }
}
