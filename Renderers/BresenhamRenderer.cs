using System.Drawing.Drawing2D;
using System.Drawing;

namespace GK1_PolygonEditor
{
    internal class BresenhamRenderer : IRenderer
    {
        private UnsafeBitmap _bitmap;

        public Camera Camera { get; set; }

        public BresenhamRenderer(UnsafeBitmap unsafeBitmap, Camera camera)
        {
            _bitmap = unsafeBitmap;
            Camera = camera;
        }

        public void Visit(Vertex vertex)
        {
            Point vert = Camera.WorldToScreen(vertex);
            using (Graphics graphics = Graphics.FromImage(_bitmap.Bitmap))
            {
                graphics.FillEllipse(Brushes.Black, vert.X - vertex.Radius, vert.Y - vertex.Radius, 2 * vertex.Radius, 2 * vertex.Radius);
                if (vertex.ContinuityConstraint != null)
                {
                    if (vertex.ContinuityConstraint.Text != null && vertex.ContinuityConstraint.Text != "")
                    {
                        using (var brush = new SolidBrush(Color.Black))
                        {
                            using (var font = new Font("Calibri", 10))
                            {
                                var textSize = TextRenderer.MeasureText(vertex.ContinuityConstraint.Text, font);
                                PointF p = new PointF(vert.X + vertex.Radius + 5, vert.Y - vertex.Radius - 5);
                                graphics.FillRectangle(brush, p.X, p.Y - textSize.Height / 2, textSize.Width, textSize.Height);
                                graphics.DrawString(vertex.ContinuityConstraint.Text, new Font("Calibri", 10), Brushes.White, new Rectangle((int)(p.X), (int)(p.Y - textSize.Height / 2), textSize.Width, textSize.Height));
                            }
                        }
                    }
                }
            }
        }

        public void Visit(Edge edge)
        {
            Point start = Camera.WorldToScreen(edge.Start);
            Point end = Camera.WorldToScreen(edge.End);
            _bitmap.Begin();
            if (edge.IsAntialiased)
                WuLine(start.X, start.Y, end.X, end.Y, Color.Black);
            else
                BresenhamLine(start.X, start.Y, end.X, end.Y, Color.Black);
            _bitmap.End();
            PointF p = Camera.WorldToScreen(edge.OverEdgePoint(24));
            if (edge.Constraint != null)
            {
                using (Graphics graphics = Graphics.FromImage(_bitmap.Bitmap))
                {
                    if (edge.Constraint.Icon != null)
                        graphics.DrawImage(edge.Constraint.Icon, p.X - 16, p.Y - 16, 32, 32);

                    if (edge.Constraint.Text != null)
                    {
                        using (var brush = new SolidBrush(Color.Black))
                        {
                            using (var font = new Font("Calibri", 10))
                            {
                                if (edge.Constraint.Text != null && edge.Constraint.Text != "")
                                {
                                    var textSize = TextRenderer.MeasureText(edge.Constraint.Text, font);
                                    graphics.FillRectangle(brush, p.X - textSize.Width / 2, p.Y - textSize.Height / 2 - 32, textSize.Width, textSize.Height);
                                    graphics.DrawString(edge.Constraint.Text, new Font("Calibri", 10), Brushes.White, new Rectangle((int)(p.X - textSize.Width / 2), (int)(p.Y - textSize.Height / 2 - 32), textSize.Width, textSize.Height));
                                }
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
                _bitmap.Begin();
                if (bezierCurve.IsAntialiased)
                    WuLine(a.X, a.Y, b.X, b.Y, Color.Black);
                else
                    BresenhamLine(a.X, a.Y, b.X, b.Y, Color.Black);
                _bitmap.End();
            }
            Pen pen = new Pen(Brushes.Black, 2);
            pen.DashPattern = [5.0f, 5.0f];
            Point start = Camera.WorldToScreen(bezierCurve.Start);
            Point end = Camera.WorldToScreen(bezierCurve.End);
            Point cp1 = Camera.WorldToScreen(bezierCurve.ControlPoint1);
            Point cp2 = Camera.WorldToScreen(bezierCurve.ControlPoint2);
            using (Graphics graphics = Graphics.FromImage(_bitmap.Bitmap))
            {
                graphics.DrawLine(pen, start.X, start.Y, cp1.X, cp1.Y);
                graphics.DrawLine(pen, cp1.X, cp1.Y, cp2.X, cp2.Y);
                graphics.DrawLine(pen, cp2.X, cp2.Y, end.X, end.Y);
                graphics.DrawLine(pen, end.X, end.Y, start.X, start.Y);
            }
            Visit(bezierCurve.ControlPoint1);
            Visit(bezierCurve.ControlPoint2);
            pen.Dispose();
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

            int scanline = ET.First().ymin;

            _bitmap.Begin();
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
                    DrawScanline(start.X, end.X, start.Y, color);
                }

                scanline++;

                for (int i = 0; i < AET.Count; i++)
                {
                    var edge = AET[i];
                    edge.x += edge.slopeInverse;
                    AET[i] = edge;
                }
            }
            _bitmap.End();

            pen.Dispose();
        }

        public void Clear(Color color)
        {
            _bitmap.Begin();
            _bitmap.Clear(color);
            _bitmap.End();
        }

        private void DrawScanline(int x1, int x2, int y, Color color)
        {
            if (y < 0 || y >= _bitmap.Height) return;
            int _x1 = Math.Max(x1, 0);
            int _x2 = Math.Min(x2, _bitmap.Width);

            _bitmap.SetHorizontalLine(_x1, _x2, y, color);
        }

        private void BresenhamLine(int x1, int y1, int x2, int y2, Color color)
        {
            int dx = Math.Abs(x2 - x1);
            int sx = x1 < x2 ? 1 : -1;
            int dy = -Math.Abs(y2 - y1);
            int sy = y1 < y2 ? 1 : -1;
            int err = dx + dy, e2;

            while (true)
            {
                DrawPixel(x1, y1, color);
                if (x1 == x2 && y1 == y2) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x1 += sx; }
                if (e2 <= dx) { err += dx; y1 += sy; }
            }
        }

        private void WuLine(int x1, int y1, int x2, int y2, Color color)
        {
            float frac(float x) => (float)(x - Math.Floor(x));

            // Change x,y based on quarter
            bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if (steep)
            {
                (x1, y1) = (y1, x1);
                (x2, y2) = (y2, x2);
            }
            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }

            float dx = x2 - x1;
            float dy = y2 - y1;
            float slope = (dx == 0) ? 1 : dy / dx;

            float y = y1;
            if (steep)
            {
                for (int i = x1; i < x2; i++, y += slope)
                {
                    DrawPixelWithIntensity((int)y, i, color, (1 - frac(y)));
                    DrawPixelWithIntensity((int)(y + 1), i, color, frac(y));
                }
            }
            else
            {
                for (int i = x1; i < x2; i++, y += slope)
                {
                    DrawPixelWithIntensity(i, (int)y, color, (1 - frac(y)));
                    DrawPixelWithIntensity(i, (int)(y + 1), color, frac(y));
                }
            }
        }

        private void DrawPixelWithIntensity(int x, int y, Color color, float intensity)
        {
            if (x >= 0 && y >= 0 && x < _bitmap.Width && y < _bitmap.Height)
            {
                Color existingColor = _bitmap.GetPixel(x, y);
                Color blendedColor = Color.FromArgb(
                (int)(intensity * color.A + (1 - intensity) * existingColor.A),
                (int)(intensity * color.R + (1 - intensity) * existingColor.R),
                (int)(intensity * color.G + (1 - intensity) * existingColor.G),
                (int)(intensity * color.B + (1 - intensity) * existingColor.B));

                _bitmap.SetPixel(x, y, blendedColor);
            }
        }

        private void DrawPixel(int x, int y, Color color)
        {
            if (x >= 0 && y >= 0 && x < _bitmap.Width && y < _bitmap.Height)
            {
                _bitmap.SetPixel(x, y, color);
            }
        }
    }
}
