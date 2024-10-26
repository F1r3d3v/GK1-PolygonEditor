namespace GK1_PolygonEditor
{
    internal class Scene
    {
        public List<Shape> Shapes { get; private set; } = [];
        public Color ClearColor { get; set; } = Color.Transparent;

        public void AddShape(Shape shape)
        {
            Shapes.Add(shape);
        }

        public void Render(IRenderer renderer)
        {
            renderer.Clear(ClearColor);
            foreach (Shape shape in Shapes)
            {
                shape.Draw(renderer);
            }
        }
    }
}
