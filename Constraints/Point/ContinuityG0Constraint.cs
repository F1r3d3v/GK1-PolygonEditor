using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal class ContinuityG0Constraint : IContinuityConstraint
    {
        public VertexContinuity ContinuityType => VertexContinuity.G0Continuity;

        public string? Text => "G⁰";
    }
}
