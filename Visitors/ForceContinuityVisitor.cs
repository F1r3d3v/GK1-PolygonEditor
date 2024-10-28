using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor.Visitors
{
    internal class ForceContinuityVisitor : IVisitor
    {
        public bool Side { get; set; }
        public Vertex ControlPoint { get; set; }
        public Vector2 Delta { get; set; }

        public ForceContinuityVisitor(Vertex cp, Vector2 delta, bool side)
        {
            ControlPoint = cp;
            Delta = delta;
            Side = side;
        }

        public void Visit(Edge edge)
        {
            Vertex v1 = (Side) ? edge.End : edge.Start;
            Vertex v2 = (Side) ? edge.Start : edge.End;
            if (v1.ContinuityConstraint!.ContinuityType == VertexContinuity.G1Continuity)
            {
                Vector2 d = new Vector2(0, 0);
                if (edge.Constraint?.ConstraintType == ConstraintType.Horizontal)
                    d = new Vector2(0, Delta.Y);
                else if (edge.Constraint?.ConstraintType == ConstraintType.Vertical)
                    d = new Vector2(Delta.X, 0);
                else if (edge.Constraint?.ConstraintType != ConstraintType.FixedLength)
                {
                    Vertex vertex = ControlPoint - v1;
                    vertex = vertex / (v1.DistanceTo(ControlPoint));
                    Vertex newPoint = v1 - v1.DistanceTo(v2) * vertex;
                    d = new Vector2(newPoint.X - v2.X, newPoint.Y - v2.Y);
                }

                v2.Move(d);
            }
            else if (v1.ContinuityConstraint.ContinuityType == VertexContinuity.C1Continuity)
            {
                Vector2 d = Delta;
                if (edge.Constraint?.ConstraintType == ConstraintType.Horizontal)
                    d.X *= -3;
                else if (edge.Constraint?.ConstraintType == ConstraintType.Vertical)
                    d.Y *= -3;
                else if (edge.Constraint?.ConstraintType != ConstraintType.FixedLength)
                    d *= -3;

                v2.Move(d);
            }
        }

        public void Visit(BezierCurve bezierCurve)
        {
            Vertex v = (Side) ? bezierCurve.End : bezierCurve.Start;
            Vertex cp = (Side) ? bezierCurve.ControlPoint2 : bezierCurve.ControlPoint1;
            if (v.ContinuityConstraint!.ContinuityType == VertexContinuity.G1Continuity)
            {
                Shape.PreserveContinuity(bezierCurve, !Side);
            }
            else if (v.ContinuityConstraint.ContinuityType == VertexContinuity.C1Continuity)
            {
                cp.Move(-Delta, false, false);
            }
        }
    }
}
