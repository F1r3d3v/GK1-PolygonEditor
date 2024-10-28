using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal class HorizontalContraint : IConstraint
    {
        public Edge Edge { get; init; }
        public ConstraintType ConstraintType { get; } = ConstraintType.Horizontal;
        public string? Text { get; } = null;

        public Image Icon
        {
            get => Resources.horizontal_icon;
        }

        public HorizontalContraint(Edge edge)
        {
            Edge = edge;
        }

        public bool IsPreserved()
        {
            return Math.Abs(Edge.Start.Y - Edge.End.Y) < 1e-3;
        }
    }
}
