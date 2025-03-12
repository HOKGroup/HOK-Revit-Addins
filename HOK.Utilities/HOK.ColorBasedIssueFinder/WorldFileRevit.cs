using Autodesk.Revit.DB;
using System.IO;

namespace ColorBasedIssueFinder
{
    public class WorldFileRevit
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public double E { get; set; }
        public double F { get; set; }

        // Constructor from a world file filepath
        public WorldFileRevit(string worldFilePath)
        {
            string[] lines = File.ReadAllLines(worldFilePath);
            if (lines.Length != 6)
            {
                throw new InvalidDataException("Invalid world file format.");
            }

            A = double.Parse(lines[0]);
            D = double.Parse(lines[1]);
            B = double.Parse(lines[2]);
            E = double.Parse(lines[3]);
            C = double.Parse(lines[4]);
            F = double.Parse(lines[5]);
        }
        public XYZ PixelToWorld(System.Windows.Point point)
        {
            double wx = A * point.X + B * point.Y + C;
            double wy = D * point.X + E * point.Y + F;
            XYZ revitCoordinate = new XYZ(wx, wy, 0);
            return revitCoordinate;
        }
    }
}
