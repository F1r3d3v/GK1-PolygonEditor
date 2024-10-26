using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GK1_PolygonEditor
{
    public enum RendererEnum
    {
        Library,
        Bresenham
    }
    internal class Renderer : INotifyPropertyChanged
    {
        // boiler-plate
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private IRenderer _currentStrategy;
        private RendererEnum _rendererEnum;
        private Scene _scene;
        private Control _canvas;
        private UnsafeBitmap _bitmap;
        private Camera _camera;

        public RendererEnum RendererEnum
        {
            get { return _rendererEnum; }
            set
            {
                if (!SetField(ref _rendererEnum, value)) return;
                switch (value)
                {
                    case RendererEnum.Library:
                        _currentStrategy = new DefaultRenderer(_bitmap.Bitmap, _camera);
                        break;
                    case RendererEnum.Bresenham:
                        _currentStrategy = new BresenhamRenderer(_bitmap, _camera);
                        break;
                }
                RenderScene();
            }
        }

        public Renderer(Scene scene, Control canvas, UnsafeBitmap bitmap, Camera camera)
        {
            _scene = scene;
            _camera = camera;
            _canvas = canvas;
            _bitmap = bitmap;
            _canvas.Paint += OnPaint;
            _canvas.Resize += OnResize;
            _currentStrategy = new DefaultRenderer(_bitmap.Bitmap, _camera);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (_canvas.Width == 0 && _canvas.Height == 0) return;
            _bitmap.Resize(_canvas.Width, _canvas.Height);
            _camera.UpdateViewportSize(_canvas.Width, _canvas.Height);
            if (_currentStrategy is DefaultRenderer renderer)
            {
                renderer.Graphics.Dispose();
                renderer.Graphics = Graphics.FromImage(_bitmap.Bitmap);
            }
            RenderScene();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (_currentStrategy != null)
                _scene.Render(_currentStrategy);

            e.Graphics.DrawImage(_bitmap.Bitmap, 0, 0);
        }

        public void RenderScene()
        {
            _canvas.Invalidate();
        }
    }
}
