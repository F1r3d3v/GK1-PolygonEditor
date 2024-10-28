using GK1_PolygonEditor.Visitors;
using System.Numerics;

namespace GK1_PolygonEditor
{
    internal class Shape : IComponent
    {
        public List<Vertex> Vertices { get; private set; } = [];
        public List<Segment> Segments { get; private set; } = [];

        public Scene? Scene { get; set; }
        public bool IsClosed { get; set; }

        private List<Vertex> _implicitVertices = [];

        public Shape() { }

        public Shape(Vertex[] vertices, Segment[] segments)
        {
            Vertices.AddRange(vertices);
            Vertices.ForEach(vert => vert.Parent = this);
            Segments.AddRange(segments);
            Segments.ForEach(seg => seg.SetParent(this));
            PreserveContinuity();
        }

        public void AddVertex(Vertex v)
        {
            v.Parent = this;
            Vertices.Add(v);
        }

        public void AddSegment(Segment s)
        {
            s.SetParent(this);
            Segments.Add(s);
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

        public void MoveControlPoint(Vertex v, Vector2 delta)
        {
            Segment s = (v.FirstSegment != null) ? v.FirstSegment! : v.SecondSegment!;
            var visitor = new ForceContinuityVisitor(v, delta, v.FirstSegment != null);
            s.Accept(visitor);
        }

        public void PreserveContinuity(Vertex v, Vector2 delta)
        {
            if (v.FirstSegment is BezierCurve bc1 && v.SecondSegment is BezierCurve bc2)
            {
                bc1.ControlPoint2.Move(delta, false, false);
                bc2.ControlPoint1.Move(delta, false, false);
                return;
            }

            PreserveContinuity();
        }

        public void PreserveContinuity()
        {
            PreserveContinuity(true);
            PreserveContinuity(false);
        }

        public void PreserveContinuity(bool left)
        {
            foreach (Segment s in Segments)
            {
                if (s is BezierCurve bc)
                {
                    PreserveContinuity(bc, left);
                }
            }
        }

        public static void PreserveContinuity(BezierCurve bc, bool side)
        {
            Vertex v = (side) ? bc.Start : bc.End;
            Segment? s = (side) ? v.FirstSegment : v.SecondSegment;
            Vertex cp = (side) ? bc.ControlPoint1 : bc.ControlPoint2;
            Vertex vert = (s is BezierCurve b)
                ? ((side) ? b.ControlPoint2 : b.ControlPoint1)
                : ((side) ? v.FirstSegment!.Start : v.SecondSegment!.End);

            if (v.ContinuityConstraint!.ContinuityType == VertexContinuity.G1Continuity)
            {
                float len1 = v.DistanceTo(vert);
                if (len1 < 1e-3) return;
                Vertex normal = (vert - v) / len1;
                float len = v.DistanceTo(cp);
                cp.MoveAbs(v - len * normal, false, false);
            }
            else if (v.ContinuityConstraint!.ContinuityType == VertexContinuity.C1Continuity)
            {
                double coef = (s is BezierCurve) ? 1.0 : (1.0 / 3.0);
                Vertex d = vert - v;
                cp.MoveAbs(v - coef * d, false, false);
            }
        }

        public static void PreserveConstraints(Vertex v, Vector2 delta)
        {
            Vector2 lastDelta = delta;
            Segment e = v.FirstSegment!;
            bool isMovingPolygon = false;

            // Clockwise
            while (true)
            {
                if (e.Start == v)
                {
                    isMovingPolygon = true;
                    break;
                }

                if (e.Constraint == null || e.Constraint.IsPreserved())
                    break;

                Vector2 temp = new Vector2(0, 0);
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
                {
                    temp = new Vector2(lastDelta.X, lastDelta.Y);
                }

                e.Start.Move(temp, false);
                e = e.Start.FirstSegment!;
            }

            if (!isMovingPolygon)
                lastDelta = delta;
            else if (lastDelta.X == 0)
                lastDelta = new Vector2(delta.X, 0);
            else if (lastDelta.Y == 0)
                lastDelta = new Vector2(0, delta.Y);

            // Counter Clockwise
            e = v.SecondSegment!;
            while (true)
            {
                if (e.End == v)
                    break;

                if (e.Constraint == null || e.Constraint.IsPreserved())
                    break;

                Vector2 temp = new Vector2(0, 0);
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
                {
                    temp = new Vector2(lastDelta.X, lastDelta.Y);
                }

                e.End.Move(temp, false);
                e = e.End.SecondSegment!;
            };
        }

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
