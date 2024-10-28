using System.Numerics;

namespace GK1_PolygonEditor
{
    internal class Vertex : IComponent
    {
        public float X { get; set; }
        public float Y { get; set; }

        public bool IsControlPoint { get; set; }

        public Shape? Parent { get; set; }

        public float Radius { get; set; }

        public Segment? FirstSegment { get; set; }
        public Segment? SecondSegment { get; set; }

        public IContinuityConstraint? ContinuityConstraint { get; set; }

        public Vertex(float x, float y, float radius = 5, bool isControlPoint = false)
        {
            X = x;
            Y = y;
            Radius = radius;
            IsControlPoint = isControlPoint;
        }

        public static implicit operator Vertex(Point p) => new Vertex(p.X, p.Y);
        public static implicit operator Point(Vertex v) => new Point((int)v.X, (int)v.Y);

        public static implicit operator Vertex(PointF p) => new Vertex(p.X, p.Y);
        public static implicit operator PointF(Vertex v) => new PointF(v.X, v.Y);

        public static implicit operator Vertex(Vector2 p) => new Vertex(p.X, p.Y);
        public static implicit operator Vector2(Vertex v) => new Vector2(v.X, v.Y);

        public static Vertex operator +(Vertex a, Vertex b)
            => new Vertex(a.X + b.X, a.Y + b.Y);
        public static Vertex operator -(Vertex a, Vertex b)
            => new Vertex(a.X - b.X, a.Y - b.Y);
        public static Vertex operator *(int a, Vertex b)
             => new Vertex(a * b.X, a * b.Y);
        public static Vertex operator *(double a, Vertex b)
             => new Vertex((float)a * b.X, (float)a * b.Y);
        public static Vertex operator /(Vertex a, double b)
             => new Vertex(a.X / (float)b, a.Y / (float)b);

        public void MoveAbs(PointF p, bool checkConstraints = true, bool checkContinuity = true)
        {
            Vector2 delta = new Vector2(p.X - X, p.Y - Y);
            Move(delta, checkConstraints, checkContinuity);
        }

        public void Move(Vector2 delta, bool checkConstraints = true, bool checkContinuity = true)
        {
            X += delta.X;
            Y += delta.Y;

            if (IsControlPoint && checkContinuity)
            {
                Parent?.MoveControlPoint(this, delta);
            }
            else if(checkConstraints)
            {
                Shape.PreserveConstraints(this, delta);
                Parent?.PreserveContinuity(this, delta);
            }
        }

        public float DistanceToSquared(Vertex other)
        {
            return (float)(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public float DistanceTo(Vertex other)
        {
            return (float)Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public bool CouldAddContinuityConstrain() => FirstSegment is BezierCurve || SecondSegment is BezierCurve;

        public void Delete()
        {
            if (Parent!.Vertices.Count <= 3)
            {
                Parent.Scene!.DeleteShape(Parent);
                Parent = null;
                return;
            }
            Segment s1 = FirstSegment!;
            Segment s2 = SecondSegment!;
            Edge e = new Edge(s1.Start, s2.End, Parent);
            int edge_idx = Parent.Segments.IndexOf(s1);
            Parent.Segments.Insert(edge_idx, e);

            if (!s1.Start.CouldAddContinuityConstrain())
                s1.Start.ContinuityConstraint = null;

            if (!s2.End.CouldAddContinuityConstrain())
                s2.End.ContinuityConstraint = null;

            Parent.Segments.Remove(s1);
            Parent.Segments.Remove(s2);
            Parent.Vertices.Remove(this);
            Parent.PreserveContinuity();
        }

        public void SetG0Continuity()
        {
            ContinuityConstraint = new ContinuityG0Constraint();
            Parent?.PreserveContinuity();
        }

        public void SetG1Continuity()
        {
            ContinuityConstraint = new ContinuityG1Constraint();
            Parent?.PreserveContinuity();
        }
        public void SetC1Continuity()
        {
            ContinuityConstraint = new ContinuityC1Constraint();
            Parent?.PreserveContinuity();
        }

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
