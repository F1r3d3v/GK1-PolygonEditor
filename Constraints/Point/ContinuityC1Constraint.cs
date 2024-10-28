using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal class ContinuityC1Constraint : IContinuityConstraint
    {
        public VertexContinuity ContinuityType => VertexContinuity.C1Continuity;

        public string? Text => "C¹";
    }
}
