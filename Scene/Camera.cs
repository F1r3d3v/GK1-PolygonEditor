namespace GK1_PolygonEditor
{
    internal class Camera
    {
        private float _x, _y, _offset_x, _offset_y;

        public float ViewportWidth { get; set; }
        public float ViewportHeight { get; set; }
        public float X
        {
            get => _x;
            set
            {
                _offset_x = value + ViewportWidth / 2;
                _x = value;
            }
        }
        public float Y
        {
            get => _y;
            set
            {
                _offset_y = value + ViewportHeight / 2;
                _y = value;
            }
        }

        public Camera(float viewportWidth, float viewportHeight)
        {
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            X = 0;
            Y = 0;
        }

        public void LookAt(PointF point)
        {
            X = point.X;
            Y = point.Y;
        }

        public void UpdateViewportSize(int viewportWidth, int viewportHeight)
        {
            ViewportWidth = viewportWidth;
            ViewportHeight = viewportHeight;
            _offset_x = _x + viewportWidth / 2;
            _offset_y = _y + viewportHeight / 2;
        }

        public Point WorldToScreen(Vertex worldPoint)
        {
            return new Point((int)(worldPoint.X + _offset_x), (int)(ViewportHeight - worldPoint.Y - _offset_y));
        }

        public Vertex ScreenToWorld(Point screenPoint)
        {
            return new Vertex(screenPoint.X - _offset_x, ViewportHeight - screenPoint.Y - _offset_y);
        }
    }
}
