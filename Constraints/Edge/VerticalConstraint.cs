using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal class VerticalConstraint : IConstraint
    {
        public Edge Edge { get; init; }
        public ConstraintType ConstraintType { get; } = ConstraintType.Vertical;
        public string? Text { get; } = null;

        public Image Icon
        {
            get => Resources.vertical_icon;
        }

        public VerticalConstraint(Edge edge)
        {
            Edge = edge;
        }

        public bool IsPreserved()
        {
            return Math.Abs(Edge.Start.X - Edge.End.X) < 1e-3;
        }
    }
}
