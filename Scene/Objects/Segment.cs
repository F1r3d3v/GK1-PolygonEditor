namespace GK1_PolygonEditor
{
    internal abstract class Segment : IComponent
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }

        public IConstraint? Constraint { get; set; }

        public Segment(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
            Start.SecondSegment = this;
            End.FirstSegment = this;
        }

        public abstract bool IsPointOnSegment(PointF p, float eps = 5f);
        public abstract void Accept(IVisitor visitor);
    }
}
