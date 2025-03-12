using System.Collections.Generic;
using Autodesk.Revit.DB;
using ColorBasedIssueFinder.IssueFinderLib;

namespace ColorBasedIssueFinder
{
    class ErrorArea
    {
        public string AreaName;
        public System.Windows.Rect Rectangle;
        public System.Drawing.Color Color;
        public WorldFileRevit WorldFile;

        public ErrorArea(string areaName, System.Windows.Rect rectangle, System.Drawing.Color color, WorldFileRevit worldFile)
        {
            AreaName = areaName;
            Rectangle = rectangle;
            Color = color;
            WorldFile = worldFile;
        }

        public ErrorArea(ErrorRect errorRect, WorldFileRevit worldFile)
        {
            AreaName = errorRect.AreaName;
            Rectangle = new System.Windows.Rect(errorRect.Rectangle.X, errorRect.Rectangle.Y, errorRect.Rectangle.Width, errorRect.Rectangle.Height);
            Color = errorRect.Color;
            WorldFile = worldFile;
        }

        // Converting the rectangle to a list of curve loops in Revit
        public CurveLoop ConvertToCurveLoops(System.Windows.Rect rect)
        {
            // Ensure that both width and height values are not 0 for rect
            if(rect.Width < 1)
            {
                rect.Width = 1;
            }
            if(rect.Height < 1)
            {
                rect.Height = 1;
            }
            IEnumerable<Curve> lines = new List<Curve>
            {
                // Using the worldfile convert the rectangle's pixel point to Revit coordinates
                Line.CreateBound(WorldFile.PixelToWorld(new System.Windows.Point(rect.X,rect.Y)), WorldFile.PixelToWorld(new System.Windows.Point(rect.Right,rect.Y))),
                Line.CreateBound(WorldFile.PixelToWorld(new System.Windows.Point(rect.Right,rect.Y)), WorldFile.PixelToWorld(new System.Windows.Point(rect.Right,rect.Bottom))),
                Line.CreateBound(WorldFile.PixelToWorld(new System.Windows.Point(rect.Right,rect.Bottom)), WorldFile.PixelToWorld(new System.Windows.Point(rect.Left,rect.Bottom))),
                Line.CreateBound(WorldFile.PixelToWorld(new System.Windows.Point(rect.Left,rect.Bottom)), WorldFile.PixelToWorld(new System.Windows.Point(rect.X,rect.Y)))
            };
            CurveLoop curveLoop = new CurveLoop();

            foreach (Line line in lines)
            {
                curveLoop.Append(line);
            }

            return curveLoop;
        }
        public override string ToString()
        {
            return AreaName;
        }
    }
}
