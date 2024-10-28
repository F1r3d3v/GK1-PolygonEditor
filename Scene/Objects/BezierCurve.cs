using System;
using System.Drawing;
using System.Windows.Forms;

namespace GK1_PolygonEditor
{
    internal class BezierCurve : Segment
    {
        public Vertex ControlPoint1 { get; set; }
        public Vertex ControlPoint2 { get; set; }

        private List<Vertex> _vertices = [];
        private double _step;

        public BezierCurve(Vertex start, Vertex end, Shape? parent = null) : base(start, end, parent)
        {
            Edge temp = new Edge(start, end);
            Start.SecondSegment = this;
            End.FirstSegment = this;

            float length = temp.Length() / 4;
            ControlPoint1 = temp.OverEdgePoint(length);
            ControlPoint1.IsControlPoint = true;
            ControlPoint1.FirstSegment = Start.FirstSegment;
            ControlPoint1.Parent = Parent;
            ControlPoint2 = temp.OverEdgePoint(-length);
            ControlPoint2.IsControlPoint = true;
            ControlPoint2.SecondSegment = End.SecondSegment;
            ControlPoint2.Parent = Parent;
            ComputePoints();

            if (start.ContinuityConstraint == null)
                start.ContinuityConstraint = new ContinuityC1Constraint();

            if (end.ContinuityConstraint == null)
                end.ContinuityConstraint = new ContinuityC1Constraint();
        }

        public override void SetParent(Shape? parent)
        {
            base.SetParent(parent);
            ControlPoint1.Parent = Parent;
            ControlPoint2.Parent = Parent;
        }

        public void ChangeToEdge()
        {
            Edge e = new Edge(Start, End, Parent);
            int bc_idx = Parent!.Segments.IndexOf(this);
            Parent.Segments[bc_idx] = e;
            if (e.Start.FirstSegment is Edge)
                e.Start.ContinuityConstraint = null;
            else if (e.Start.FirstSegment is BezierCurve b1)
                b1.ControlPoint2.SecondSegment = e;

            if (e.End.SecondSegment is Edge)
                e.End.ContinuityConstraint = null;
            else if (e.End.SecondSegment is BezierCurve b2)
                b2.ControlPoint1.FirstSegment = e;
        }

        public List<Vertex> ComputePoints()
        {
            _vertices = new List<Vertex>();
            int points = Math.Max((int)Start.DistanceTo(End) / 8, 30);
            _step = 1.0 / (points - 1);
            Vertex A0 = Start;
            Vertex A1 = 3 * (ControlPoint1 - Start);
            Vertex A2 = 3 * (ControlPoint2 - 2 * ControlPoint1 + Start);
            Vertex A3 = End - 3 * ControlPoint2 + 3 * ControlPoint1 - Start;

            double _step2 = _step * _step;
            double _step3 = _step2 * _step;
            Vertex P = A0;
            Vertex d1P = _step3 * A3 + _step2 * A2 + _step * A1;
            Vertex d2P = 6 * _step3 * A3 + 2 * _step2 * A2;
            Vertex d3P = 6 * _step3 * A3;

            _vertices.Add(Start);
            for (int i = 1; i < points - 1; i++)
            {
                P += d1P;
                d1P += d2P;
                d2P += d3P;
                _vertices.Add(P);
            }
            _vertices.Add(End);

            return _vertices;
        }

        public override bool IsPointOnSegment(PointF p, float eps = 5f)
        {
            List<(Vertex v, double t)> LUT = [];
            float closestDistance = float.MaxValue;
            int index = 0;
            double t = 0;
            for (int i = 0; i < _vertices.Count; i++, t += _step)
            {
                LUT.Add((_vertices[i], t));
                float dist = _vertices[i].DistanceToSquared(p);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    index = i;
                }
            }

            for (int i = 0; i < 20; i++)
            {
                int il = index == 0 ? 0 : index - 1;
                int ir = index == LUT.Count - 1 ? LUT.Count - 1 : index + 1;
                double tl = LUT[il].t;
                double tr = LUT[ir].t;
                double step = (tr - tl) / 4;

                List<(Vertex v, double t)> lut = [];

                if (step < 1e-3)
                    break;

                lut.Add(LUT[il]);
                if (index == 0 || index == LUT.Count - 1)
                {
                    double temp_t = tl + 2 * step;
                    Vertex n = GetPointAt(temp_t);

                    float distance = n.DistanceToSquared(p);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        index = 1;
                    }
                    else if (index == LUT.Count - 1)
                    {
                        index = 2;
                    }

                    lut.Add((n, temp_t));
                }
                else
                {
                    bool newClosest = false;
                    for (int j = 1; j <= 3; j++)
                    {
                        if (j == 2)
                        {
                            lut.Add(LUT[index]);
                            continue;
                        }

                        double temp_t = tl + j * step;
                        Vertex n = GetPointAt(temp_t);

                        float distance = n.DistanceToSquared(p);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            newClosest = true;
                            index = j;
                        }

                        lut.Add((n, temp_t));
                    }

                    if (!newClosest) index = 2;
                }

                lut.Add(LUT[ir]);

                LUT = lut;
            }

            return closestDistance < eps * eps;
        }

        public List<Vertex> GetImplicitVertices() => _vertices;

        public override void Accept(IVisitor visitor) => visitor.Visit(this);

        private Vertex GetPointAt(double t)
        {
            Vertex A0 = Start;
            Vertex A1 = 3 * (ControlPoint1 - Start);
            Vertex A2 = 3 * (ControlPoint2 - 2 * ControlPoint1 + Start);
            Vertex A3 = End - 3 * ControlPoint2 + 3 * ControlPoint1 - Start;

            return Horner([A3, A2, A1, A0], t);
        }

        private Vertex Horner(Vertex[] coef, double value)
        {
            Vertex result = coef[0];
            for (int i = 1; i < coef.Length; i++)
            {
                result = value * result + coef[i];
            }

            return result;
        }
    }
}
