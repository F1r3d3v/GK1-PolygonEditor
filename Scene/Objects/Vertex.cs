using System.Numerics;

namespace GK1_PolygonEditor
{
    internal class Vertex : IComponent
    {
        public float X { get; set; }
        public float Y { get; set; }

        public float Radius { get; set; }

        public Segment? FirstSegment { get; set; }
        public Segment? SecondSegment { get; set; }

        public IContinuityConstraint? ContinuityConstraint { get; set; }

        public Vertex(float x, float y, float radius = 5)
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public static implicit operator Vertex(Point p) => new Vertex(p.X, p.Y);
        public static implicit operator Vertex(PointF p) => new Vertex(p.X, p.Y);
        public static implicit operator Point(Vertex v) => new Point((int)v.X, (int)v.Y);
        public static implicit operator PointF(Vertex v) => new PointF(v.X, v.Y);

        public static Vertex operator +(Vertex a, Vertex b)
            => new Vertex(a.X + b.X, a.Y + b.Y);
        public static Vertex operator -(Vertex a, Vertex b)
            => new Vertex(a.X - b.X, a.Y - b.Y);
        public static Vertex operator *(int a, Vertex b)
             => new Vertex(a * b.X, a * b.Y);
        public static Vertex operator *(double a, Vertex b)
             => new Vertex((float)a * b.X, (float)a * b.Y);

        public void Move(PointF p, bool checkConstraints = true)
        {
            Vector2 delta = new Vector2(p.X - X, p.Y - Y);
            Move(delta, checkConstraints);
        }

        public void Move(Vector2 delta, bool checkConstraints = true)
        {
            X += delta.X;
            Y += delta.Y;

            if (checkConstraints)
                PreserveConstraints(this, delta);
        }

        public float DistanceToSquared(Vertex other)
        {
            return (float)(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public float DistanceTo(Vertex other)
        {
            return (float)Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        private static void PreserveConstraints(Vertex v, Vector2 delta)
        {
            Vector2 lastDelta = delta;
            Segment e = v.FirstSegment!;
            bool allVerticesMoevd = false;

            while (true)
            {
                if (e.Start == v)
                {
                    allVerticesMoevd = true;
                    break;
                }

                if (e.Constraint == null || e.Constraint.IsPreserved())
                    break;

                Vector2 temp = new Vector2();
                if (e.Constraint.ConstraintType == ConstraintType.Horizontal)
                {
                    temp = new Vector2(0, e.End.Y - e.Start.Y);
                    lastDelta = temp;
                }
                else if (e.Constraint.ConstraintType == ConstraintType.Vertical)
                {
                    temp = new Vector2(e.End.X - e.Start.X, 0);
                    lastDelta = temp;
                }
                else if (e.Constraint.ConstraintType == ConstraintType.FixedLength)
                    temp = new Vector2(lastDelta.X, lastDelta.Y);

                e.Start.Move(temp, false);
                e = e.Start.FirstSegment!;
            }

            if (!allVerticesMoevd)
                lastDelta = delta;
            else if (lastDelta.X == 0)
                lastDelta = new Vector2(delta.X, 0);
            else if (lastDelta.Y == 0)
                lastDelta = new Vector2(0, delta.Y);

            e = v.SecondSegment!;
            while (true)
            {
                if (e.End == v)
                    break;

                if (e.Constraint == null || e.Constraint.IsPreserved())
                    break;

                Vector2 temp = new Vector2();
                if (e.Constraint.ConstraintType == ConstraintType.Horizontal)
                {
                    temp = new Vector2(0, e.Start.Y - e.End.Y);
                    lastDelta = temp;
                }
                else if (e.Constraint.ConstraintType == ConstraintType.Vertical)
                {
                    temp = new Vector2(e.Start.X - e.End.X, 0);
                    lastDelta = temp;
                }
                else if (e.Constraint.ConstraintType == ConstraintType.FixedLength)
                    temp = new Vector2(lastDelta.X, lastDelta.Y);

                e.End.Move(temp, false);
                e = e.End.SecondSegment!;
            };
        }

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
