namespace GK1_PolygonEditor
{
    internal class Shape : IComponent
    {
        public List<Vertex> Vertices { get; private set; } = [];
        public List<Segment> Segments { get; private set; } = [];
        public bool IsClosed { get; set; }

        private List<Vertex> _implicitVertices = [];

        public Shape() { }

        public Shape(Vertex[] vertices, Segment[] segments)
        {
            Vertices.AddRange(vertices);
            Segments.AddRange(segments);
        }

        public bool IsInShape(PointF p)
        {
            if (!IsClosed) return false;

            _implicitVertices = [];
            foreach (var s in Segments)
            {
                if (s is Edge e) _implicitVertices.Add(e.Start);
                else if (s is BezierCurve bc) _implicitVertices.AddRange(bc.GetImplicitVertices().SkipLast(1));
            }

            bool result = false;
            var previous = _implicitVertices.Last();

            foreach (var next in _implicitVertices)
            {
                if ((next.X == p.X) && (next.Y == p.Y))
                    return true;

                if ((next.Y == previous.Y) && (p.Y == previous.Y))
                {
                    if ((previous.X <= p.X) && (p.X <= next.X) || (next.X <= p.X) && (p.X <= previous.X))
                        return true;
                }

                if ((next.Y < p.Y) && (previous.Y >= p.Y) || (previous.Y < p.Y) && (next.Y >= p.Y))
                {
                    if (p.X <= next.X + (p.Y - next.Y) / (previous.Y - next.Y) * (previous.X - next.X))
                        result = !result;
                }

                previous = next;
            }

            return result;
        }

        public void Draw(IRenderer renderer)
        {
            Segments.ForEach((x) => { if (x is BezierCurve bc) bc.ComputePoints(); });
            if (IsClosed)
                renderer.Fill(this, Color.Gray);
            Segments.ForEach((x) => x.Accept(renderer));
            Vertices.ForEach(renderer.Visit);
        }

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
