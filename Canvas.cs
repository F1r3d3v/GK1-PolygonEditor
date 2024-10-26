namespace GK1_PolygonEditor
{
    public class Canvas : Control
    {
        public Canvas()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }
    }
}
