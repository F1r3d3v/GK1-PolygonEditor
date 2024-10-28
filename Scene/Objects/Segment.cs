namespace GK1_PolygonEditor
{
    internal abstract class Segment : IComponent
    {
        public Vertex Start { get; set; }
        public Vertex End { get; set; }
        public Shape? Parent { get; protected set; }

        public IConstraint? Constraint { get; set; }

        public Segment(Vertex start, Vertex end, Shape? parent = null)
        {
            Start = start;
            End = end;
            Parent = parent;
            Start.SecondSegment = this;
            End.FirstSegment = this;
        }

        public abstract bool IsPointOnSegment(PointF p, float eps = 5f);

        public virtual void SetParent(Shape? parent) => Parent = parent;
        public abstract void Accept(IVisitor visitor);
    }
}
