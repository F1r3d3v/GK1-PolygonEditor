using System;
using System.Windows.Forms;

namespace GK1_PolygonEditor
{
    internal class BezierCurve : Segment
    {
        public Vertex ControlPoint1 { get; set; }
        public Vertex ControlPoint2 { get; set; }

        private List<Vertex> _vertices = [];
        private double _step;

        public BezierCurve(Vertex start, Vertex end) : base(start, end)
        {
            Edge temp = new Edge(start, end);
            float length = temp.Length() / 4;
            ControlPoint1 = temp.OverEdgePoint(length);
            ControlPoint2 = temp.OverEdgePoint(-length);
            ComputePoints();
        }

        public BezierCurve(Vertex start, Vertex controlPoint1, Vertex controlPoint2, Vertex end) : base(start, end)
        {
            ControlPoint1 = controlPoint1;
            ControlPoint2 = controlPoint2;
            ComputePoints();
        }

        public List<Vertex> ComputePoints()
        {
            _vertices = new List<Vertex>();
            int points = Math.Max((int)Start.DistanceTo(End) / 8, 25);
            _step = 1.0 / (points - 1);
            Vertex A0 = Start;
            Vertex A1 = 3 * (ControlPoint1 - Start);
            Vertex A2 = 3 * (ControlPoint2 - 2 * ControlPoint1 + Start);
            Vertex A3 = End - 3 * ControlPoint2 + 3 * ControlPoint1 - Start;

            Vertex P = A0;
            Vertex d1P = Math.Pow(_step, 3) * A3 + Math.Pow(_step, 2) * A2 + _step * A1;
            Vertex d2P = 6 * Math.Pow(_step, 3) * A3 + 2 * Math.Pow(_step, 2) * A2;
            Vertex d3P = 6 * Math.Pow(_step, 3) * A3;

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
