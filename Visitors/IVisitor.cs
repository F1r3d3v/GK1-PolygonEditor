using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_PolygonEditor
{
    internal interface IVisitor
    {
        public void Visit(Vertex vertex) { }
        public void Visit(Edge edge) { }
        public void Visit(BezierCurve bezierCurve) { }
        public void Visit(Shape shape) { }
    }
}
