using Autodesk.Revit.DB;
using Rhino.Geometry.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.RhinoImporter.RhinoUtils
{
    public static class RhinoConverter
    {
        public static List<double> ConvertRhinoKnotList(NurbsSurfaceKnotList knotList)
        {
            List<double> knots = new List<double>();
            try
            {
                foreach(double knot in knotList)
                {
                    knots.Add(knot);
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
            return knots;
        }

        public static List<double> ConvertRhinoKnotList(NurbsCurveKnotList knotList)
        {
            List<double> knots = new List<double>();
            try
            {
                foreach (double knot in knotList)
                {
                    knots.Add(knot);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return knots;
        }

        public static List<XYZ> ConvertRhinoControlPoints(NurbsSurfacePointList ctrlPoints, out List<double> weights)
        {
            List<XYZ> points = new List<XYZ>();
            weights = new List<double>();
            try
            {
                foreach(Rhino.Geometry.ControlPoint ctrlPoint in ctrlPoints)
                {
                    points.Add(new XYZ(ctrlPoint.Location.X, ctrlPoint.Location.Y, ctrlPoint.Location.Z));
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
            return points;
        }

        public static List<XYZ> ConvertRhinoControlPoints(NurbsCurvePointList ctrlPoints, out List<double> weights)
        {
            List<XYZ> points = new List<XYZ>();
            weights = new List<double>();
            try
            {
                foreach (Rhino.Geometry.ControlPoint ctrlPoint in ctrlPoints)
                {
                    points.Add(new XYZ(ctrlPoint.Location.X, ctrlPoint.Location.Y, ctrlPoint.Location.Z));
                    weights.Add(ctrlPoint.Weight);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return points;
        }
    }
}
