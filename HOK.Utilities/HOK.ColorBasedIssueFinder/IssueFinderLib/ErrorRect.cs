using System.Drawing;

namespace HOK.ColorBasedIssueFinder.IssueFinderLib
{
    public class ErrorRect
    {
        public string AreaName;
        public Rectangle Rectangle;
        public System.Drawing.Color Color;

        public ErrorRect(string areaName, Rectangle rectangle, System.Drawing.Color color)
        {
            AreaName = areaName;
            Rectangle = rectangle;
            Color = color;
        }
        public override string ToString()
        {
            return AreaName;
        }
    }
}
