using System.Drawing;
using System.Drawing.Imaging;

namespace GK1_PolygonEditor
{
    internal unsafe class UnsafeBitmap(int width, int heigth) : IDisposable
    {
        private Bitmap _bitmap = new Bitmap(width, heigth, PixelFormat.Format32bppArgb);
        private BitmapData? _bmpData;
        private UInt32* _data;

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;
        public Bitmap Bitmap => _bitmap;

        public UInt32* Data => _data;

        public void Begin()
        {
            Rectangle rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
            _bmpData = _bitmap.LockBits(rect, ImageLockMode.ReadWrite, _bitmap.PixelFormat);
            _data = (UInt32*)_bmpData.Scan0;
        }

        public void End()
        {
            _bitmap.UnlockBits(_bmpData!);
        }

        public void Resize(int width, int height)
        {
            _bitmap.Dispose();
            _bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        }

        public void Clear(Color color)
        {
            UInt32 value = (UInt32)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
            UInt32 size = (UInt32)(_bitmap.Width * _bitmap.Height);
            for (int i = 0; i < size; i++)
                *(_data + i) = value;
        }

        public unsafe void SetHorizontalLine(int x1, int x2, int y, Color color)
        {
            UInt32 value = (UInt32)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
            int length = x2 - x1;
            for (int i = 0; i <= length; i++)
                *(_data + y * _bitmap.Width + x1 + i) = value;
        }

        public unsafe void SetPixel(int x, int y, Color color)
        {
            UInt32 value = (UInt32)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
            *(_data + y * _bitmap.Width + x) = value;
        }

        public unsafe Color GetPixel(int x, int y)
        {
            UInt32 pixel = *(_data + y * _bitmap.Width + x);
            byte alpha = (byte)((pixel >> 24) & 0xFF);
            byte red = (byte)((pixel >> 16) & 0xFF);
            byte green = (byte)((pixel >> 8) & 0xFF);
            byte blue = (byte)((pixel >> 0) & 0xFF);
            return Color.FromArgb(red, green, blue);
        }

        public void Dispose() => _bitmap.Dispose();
    }
}
