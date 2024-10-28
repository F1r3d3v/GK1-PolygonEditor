using System.Drawing;
using System.Numerics;

namespace GK1_PolygonEditor
{
    internal class Edge(Vertex start, Vertex end, Shape? parent = null) : Segment(start, end, parent)
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

        public void SplitEdge()
        {
            Vertex mp = MidPoint();
            mp.Parent = Parent;
            int vert_idx = Parent!.Vertices.IndexOf(Start);
            Parent.Vertices.Insert(vert_idx, mp);
            int edge_idx = Parent.Segments.IndexOf(this);
            Edge e1 = new Edge(Start, mp, Parent);
            Edge e2 = new Edge(mp, End, Parent);

            if (e1.Start.FirstSegment is BezierCurve b1)
                b1.ControlPoint2.SecondSegment = e1;

            if (e2.End.SecondSegment is BezierCurve b2)
                b2.ControlPoint1.FirstSegment = e2;

            Parent.Segments.Remove(this);
            Parent.Segments.Insert(edge_idx, e1);
            Parent.Segments.Insert(edge_idx + 1, e2);

            Parent?.PreserveContinuity();
        }

        public void ChangeToBezier()
        {
            BezierCurve bc = new BezierCurve(Start, End, Parent);
            int edge_idx = Parent!.Segments.IndexOf(this);
            Parent.Segments[edge_idx] = bc;

            if (bc.Start.FirstSegment is BezierCurve b1)
                b1.ControlPoint2.SecondSegment = bc;

            if (bc.End.SecondSegment is BezierCurve b2)
                b2.ControlPoint1.FirstSegment = bc;

            Parent?.PreserveContinuity();
        }

        public void SetHorizontal()
        {
            if (Start.FirstSegment!.Constraint?.ConstraintType == ConstraintType.Horizontal ||
                End.SecondSegment!.Constraint?.ConstraintType == ConstraintType.Horizontal)
            {
                MessageBox.Show("Two adjecent horizontal edges are forbidden!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Vector2 moveDelta = new Vector2(0, End.Y - Start.Y);
            if (CouldSetConstraint(moveDelta))
            {
                Constraint = new HorizontalContraint(this);
                Start.Move(moveDelta);
            }
            else
                MessageBox.Show("Cannot set constraint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void SetVertical()
        {
            if (Start.FirstSegment!.Constraint?.ConstraintType == ConstraintType.Vertical ||
                End.SecondSegment!.Constraint?.ConstraintType == ConstraintType.Vertical)
            {
                MessageBox.Show("Two adjecent vertical edges are forbidden!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Vector2 moveDelta = new Vector2(End.X - Start.X, 0);
            if (CouldSetConstraint(moveDelta))
            {
                Constraint = new VerticalConstraint(this);
                Start.Move(moveDelta);
            }
            else
                MessageBox.Show("Cannot set constraint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void SetFixedLengthDialog()
        {
            SetLengthForm dialog = new SetLengthForm();
            dialog.Length = (decimal)Length();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetFixedLength((float)dialog.Length);
            }
        }

        public void SetFixedLength(float length)
        {
            Constraint = new FixedLengthConstraint(this, length);
            if (Math.Abs(Length() - length) > 1e-3)
            {
                var edgeLength = Length();
                var p = length / edgeLength;
                float x = ((p * (End.X - Start.X) + Start.X));
                float y = ((p * (End.Y - Start.Y) + Start.Y));
                Vector2 moveDelta = new Vector2(x - End.X, y - End.Y);
                if (CouldSetConstraint(moveDelta))
                    End.Move(moveDelta);
                else
                {
                    MessageBox.Show("Cannot set constraint!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Constraint = null;
                }
            }
        }

        public void RemoveConstraint()
        {
            Constraint = null;
        }

        private bool CouldSetConstraint(Vector2 delta)
        {
            return CouldSet(true) && CouldSet(false);

            bool CouldSet(bool orientX)
            {
                float move = orientX ? delta.X : delta.Y;
                ConstraintType constraint = orientX ? ConstraintType.Horizontal : ConstraintType.Vertical;
                if (move != 0)
                {
                    Segment? e = End.SecondSegment;

                    for (; e != this; e = e.End.SecondSegment)
                        if (e!.Constraint == null || e.Constraint.ConstraintType == constraint)
                            break;

                    if (e == this)
                        return false;
                }
                return true;
            }
        }
    }
}
