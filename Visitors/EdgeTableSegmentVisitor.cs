namespace GK1_PolygonEditor
{
    internal class EdgeTableSegmentVisitor : IVisitor
    {
        List<(int ymin, int ymax, float xmin, float slope)> ET = [];

        public EdgeTableSegmentVisitor(List<(int ymin, int ymax, float xmin, float slope)> et)
        {
            ET = et;
        }

        public void Visit(BezierCurve bezierCurve)
        {
            List<Vertex> verts = bezierCurve.GetImplicitVertices();
            for (int i = 0; i < verts.Count - 1; ++i)
            {
                Vertex v1 = verts[i];
                Vertex v2 = verts[i + 1];
                if (v1.Y == v2.Y) continue;
                int ymin = (int)Math.Round(Math.Min(v1.Y, v2.Y));
                int ymax = (int)Math.Round(Math.Max(v1.Y, v2.Y));
                float xmin = (v1.Y <= v2.Y) ? v1.X : v2.X;
                float slope = (v1.X != v2.X) ? (v1.Y - v2.Y) / (v1.X - v2.X) : 0;
                ET.Add((ymin, ymax, xmin, slope));
            }
        }

        public void Visit(Edge edge)
        {
            Vertex v1 = edge.Start;
            Vertex v2 = edge.End;
            if (v1.Y == v2.Y) return;
            int ymin = (int)Math.Min(v1.Y, v2.Y);
            int ymax = (int)Math.Max(v1.Y, v2.Y);
            float xmin = (v1.Y <= v2.Y) ? v1.X : v2.X;
            float slope = edge.Slope();
            ET.Add((ymin, ymax, xmin, slope));
        }
    }
}
