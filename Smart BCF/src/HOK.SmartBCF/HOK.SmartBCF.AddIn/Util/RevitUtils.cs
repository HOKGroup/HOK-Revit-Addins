using Autodesk.Revit.DB;
using HOK.SmartBCF.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.AddIn.Util
{
    //referenced by BCFier https://github.com/teocomi/BCFier

    public static class RevitUtils
    {
        public static ViewOrientation3D ConvertBasePoint(Document doc, XYZ c, XYZ view, XYZ up, bool negative)
        {
            double angle = 0;
            double x = 0;
            double y = 0;
            double z = 0;

            //VERY IMPORTANT
            //BuiltInParameter.BASEPOINT_EASTWEST_PARAM is the value of the BASE POINT LOCATION
            //position is the location of the BPL related to Revit's absolute origin
            //if BPL is set to 0,0,0 not always it corresponds to Revit's origin

            XYZ origin = new XYZ(0, 0, 0);
            ProjectPosition position = doc.ActiveProjectLocation.get_ProjectPosition(origin);

            int i = (negative) ? -1 : 1;

            x = i * position.EastWest;
            y = i * position.NorthSouth;
            z = i * position.Elevation;
            angle = i * position.Angle;

            if (negative) // I do the addition BEFORE
                c = new XYZ(c.X + x, c.Y + y, c.Z + z);

            //rotation
            double centX = (c.X * Math.Cos(angle)) - (c.Y * Math.Sin(angle));
            double centY = (c.X * Math.Sin(angle)) + (c.Y * Math.Cos(angle));

            XYZ newC = new XYZ();
            if (negative)
                newC = new XYZ(centX, centY, c.Z);
            else // I do the addition AFTERWARDS
                newC = new XYZ(centX + x, centY + y, c.Z + z);


            double viewX = (view.X * Math.Cos(angle)) - (view.Y * Math.Sin(angle));
            double viewY = (view.X * Math.Sin(angle)) + (view.Y * Math.Cos(angle));
            XYZ newView = new XYZ(viewX, viewY, view.Z);

            double upX = (up.X * Math.Cos(angle)) - (up.Y * Math.Sin(angle));
            double upY = (up.X * Math.Sin(angle)) + (up.Y * Math.Cos(angle));

            XYZ newUp = new XYZ(upX, upY, up.Z);
            return new ViewOrientation3D(newC, newUp, newView);
        }

        public static XYZ GetRevitXYZ(double X, double Y, double Z)
        {
            return new XYZ(X.ToFeet(), Y.ToFeet(), Z.ToFeet());
        }

        public static XYZ GetRevitXYZ(Direction d)
        {
            return new XYZ(d.X.ToFeet(), d.Y.ToFeet(), d.Z.ToFeet());
        }

        public static XYZ GetRevitXYZ(HOK.SmartBCF.Schemas.Point d)
        {
            return new XYZ(d.X.ToFeet(), d.Y.ToFeet(), d.Z.ToFeet());
        }

        public static double ToMeters(this double feet)
        {
            return UnitUtils.ConvertFromInternalUnits(feet, DisplayUnitType.DUT_METERS);
        }

        public static double ToFeet(this double meters)
        {
            return UnitUtils.ConvertToInternalUnits(meters, DisplayUnitType.DUT_METERS);
        }
    }
}
