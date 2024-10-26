using System.Drawing;

namespace GK1_PolygonEditor
{
    internal class Edge(Vertex start, Vertex end) : Segment(start, end)
    {
        public override void Accept(IVisitor visitor) => visitor.Visit(this);

        public override bool IsPointOnSegment(PointF p, float eps = 5f)
        {
            float x1 = Start.X;
            float y1 = Start.Y;
            float x2 = End.X;
            float y2 = End.Y;

            float px = p.X;
            float py = p.Y;

            float dx = x2 - x1;
            float dy = y2 - y1;

            if (dx == 0 && dy == 0)
            {
                return Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1)) < eps;
            }

            float t = ((px - x1) * dx + (py - y1) * dy) / (dx * dx + dy * dy);

            t = Math.Max(0, Math.Min(1, t));

            float projX = x1 + t * dx;
            float projY = y1 + t * dy;

            float distance = (float)Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));

            return distance < eps;
        }

        public float Length() => Start.DistanceTo(End);

        public float Slope() => (Start.X != End.X) ? (Start.Y - End.Y) / (Start.X - End.X) : 0;

        public Vertex MidPoint() => new Vertex((Start.X + End.X) / 2, (Start.Y + End.Y) / 2);

        public PointF OverEdgePoint(float d)
        {
            Vertex mp = MidPoint();

            float dx = End.X - Start.X;
            float dy = End.Y - Start.Y;

            float length = Length();
            if (length == 0) return new PointF(mp.X, mp.Y);

            float perpX = -(dy / length);
            float perpY = dx / length;

            float xHigh = mp.X + d * perpX;
            float yHigh = mp.Y + d * perpY;

            return new PointF(xHigh, yHigh);
        }
    }
}
