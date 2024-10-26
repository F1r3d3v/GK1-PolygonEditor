namespace GK1_PolygonEditor
{
    internal interface IRenderer : IVisitor
    {
        public Camera Camera { get; set; }
        public void Clear(Color color);
        public void Fill(Shape shape, Color color) { }
    }
}
