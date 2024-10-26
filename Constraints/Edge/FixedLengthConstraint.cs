using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal class FixedLengthConstraint : IConstraint
    {
        public Edge Edge { get; init; }
        public ConstraintType ConstraintType { get; } = ConstraintType.FixedLength;
        public double Length { get; private set; } = 0;
        public string? Text
        {
            get => Math.Round(Length, 2).ToString();
        }
        public Image Icon
        {
            get => Resources.fixed_length_icon;
        }

        public FixedLengthConstraint(Edge edge, float length)
        {
            Length = length;
            Edge = edge;
        }

        public bool IsPreserved()
        {
            return Math.Abs(Edge.Length() - Length) < 1e-3;
        }
    }
}
