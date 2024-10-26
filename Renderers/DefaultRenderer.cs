using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace GK1_PolygonEditor
{
    internal class DefaultRenderer : IRenderer, IDisposable
    {
        public Graphics Graphics { get; set; }
        public Camera Camera { get; set; }

        public DefaultRenderer(Bitmap bitmap, Camera camera)
        {
            Graphics = Graphics.FromImage(bitmap);
            Camera = camera;
        }

        public void Visit(Vertex vertex)
        {
            Point vert = Camera.WorldToScreen(vertex);
            Graphics.FillEllipse(Brushes.Black, vert.X - vertex.Radius, vert.Y - vertex.Radius, 2 * vertex.Radius, 2 * vertex.Radius);
        }

        public void Visit(Edge edge)
        {
            Point start = Camera.WorldToScreen(edge.Start);
            Point end = Camera.WorldToScreen(edge.End);
            Graphics.DrawLine(Pens.Black, start.X, start.Y, end.X, end.Y);
            PointF p = Camera.WorldToScreen(edge.OverEdgePoint(24));
            if (edge.Constraint != null)
            {
                if (edge.Constraint.Icon != null)
                    Graphics.DrawImage(edge.Constraint.Icon, p.X - 16, p.Y - 16, 32, 32);

                if (edge.Constraint.Text != null)
                {
                    using (var brush = new SolidBrush(Color.Black))
                    {
                        using (var font = new Font("Calibri", 10))
                        {
                            if (edge.Constraint.Text != null && edge.Constraint.Text != "")
                            {
                                var textSize = TextRenderer.MeasureText(edge.Constraint.Text, font);
                                Graphics.FillRectangle(brush, p.X - textSize.Width / 2, p.Y - textSize.Height / 2 - 32, textSize.Width, textSize.Height);
                                Graphics.DrawString(edge.Constraint.Text, new Font("Calibri", 10), Brushes.White, new Rectangle((int)(p.X - textSize.Width/2), (int)(p.Y - textSize.Height / 2 - 32), textSize.Width, textSize.Height));
                            }
                        }
                    }
                }
            }
        }

        public void Visit(BezierCurve bezierCurve)
        {
            List<Vertex> vertices = bezierCurve.GetImplicitVertices();
            for (int i = 0; i < vertices.Count - 1; ++i)
            {
                Point a = Camera.WorldToScreen(vertices[i]);
                Point b = Camera.WorldToScreen(vertices[i + 1]);
                Graphics.DrawLine(Pens.Black, a.X, a.Y, b.X, b.Y);
            }
            Pen pen = new Pen(Brushes.Black, 2);
            pen.DashPattern = [5.0f, 3.0f];
            Point start = Camera.WorldToScreen(bezierCurve.Start);
            Point end = Camera.WorldToScreen(bezierCurve.End);
            Point cp1 = Camera.WorldToScreen(bezierCurve.ControlPoint1);
            Point cp2 = Camera.WorldToScreen(bezierCurve.ControlPoint2);
            Graphics.DrawLine(pen, start.X, start.Y, cp1.X, cp1.Y);
            //Graphics.DrawLine(pen, cp1.X, cp1.Y, cp2.X, cp2.Y);
            Graphics.DrawLine(pen, cp2.X, cp2.Y, end.X, end.Y);
            //Graphics.DrawLine(pen, end.X, end.Y, start.X, start.Y);
            Visit(bezierCurve.ControlPoint1);
            Visit(bezierCurve.ControlPoint2);
            pen.Dispose();
        }

        public void Visit(Shape shape)
        {
            shape.Vertices.ForEach(Visit);
            shape.Segments.ForEach((x) => x.Accept(this));
        }

        public void Clear(Color color)
        {
            Graphics.Clear(color);
        }

        public void Fill(Shape shape, Color color)
        {
            List<(int ymin, int ymax, float xmin, float slope)> ET = [];
            List<(int ymax, float x, float slopeInverse)> AET = [];
            Pen pen = new Pen(color, 1);

            foreach (var seg in shape.Segments)
            {
                seg.Accept(new EdgeTableSegmentVisitor(ET));
            }

            ET.Sort((x, y) =>
            {
                int res = x.ymin.CompareTo(y.ymin);
                if (res == 0) res = x.xmin.CompareTo(y.xmin);
                return res;
            });

            if (ET.Count == 0) return;
            int scanline = ET.First().ymin;

            while (ET.Count > 0 || AET.Count > 0)
            {
                while (ET.Count > 0 && ET[0].ymin == scanline)
                {
                    var edge = ET[0];
                    AET.Add((edge.ymax, edge.xmin, (edge.slope != 0) ? 1 / edge.slope : 0));
                    ET.RemoveAt(0);
                }

                AET.RemoveAll(edge => edge.ymax == scanline);

                AET.Sort((x, y) => x.x.CompareTo(y.x));

                for (int i = 0; i < AET.Count - 1; i += 2)
                {
                    Point start = Camera.WorldToScreen(new PointF(AET[i].x, scanline));
                    Point end = Camera.WorldToScreen(new PointF(AET[i + 1].x, scanline));
                    Graphics.DrawLine(pen, start, end);
                }

                scanline++;

                for (int i = 0; i < AET.Count; i++)
                {
                    var edge = AET[i];
                    edge.x += edge.slopeInverse;
                    AET[i] = edge;
                }
            }

            pen.Dispose();
        }

        public void Dispose()
        {
            Graphics?.Dispose();
        }
    }
}
