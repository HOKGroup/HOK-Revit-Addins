using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB.IFC;

namespace HOK.RoomMeasure
{
    public static class MeasureUtil
    {
        public const string roomWidthParamName = "Room Width";
        public const string roomLengthParamName = "Room Length";

        public static bool ExistRoomParameter(Room room)
        {
            bool exist = false;
            try
            {
                
#if RELEASE2013||RELEASE2014
                Parameter parameter = room.get_Parameter(roomWidthParamName);
                if (null != parameter)
                {
                    parameter = room.get_Parameter(roomLengthParamName);
                    if (null != parameter)
                    {
                        exist = true;
                    }
                    else
                    {
                        MessageBox.Show("Room Parameter [" + roomLengthParamName + "] doesn't exist.\n Please add room parameters before running this tool.", "Room Parameter Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Room Parameter [" + roomWidthParamName + "] doesn't exist.\n Please add room parameters before running this tool.", "Room Parameter Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                }
#elif RELEASE2015||RELEASE2016
                Parameter parameter = room.LookupParameter(roomWidthParamName);
                if (null != parameter)
                {
                    parameter = room.LookupParameter(roomLengthParamName);
                    if (null != parameter)
                    {
                        exist = true;
                    }
                    else
                    {
                        MessageBox.Show("Room Parameter [" + roomLengthParamName + "] doesn't exist.\n Please add room parameters before running this tool.", "Room Parameter Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Room Parameter [" + roomWidthParamName + "] doesn't exist.\n Please add room parameters before running this tool.", "Room Parameter Missing", MessageBoxButton.OK, MessageBoxImage.Information);
                }
#endif
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return exist;
        }

        public static DirectShape CreateRoomDirectShape(Room room)
        {
            DirectShape roomShape = null;
            try
            {
                List<GeometryObject> geoList = new List<GeometryObject>();
                var solidGeometries = from geoObj in room.ClosedShell
                                      where geoObj.GetType() == typeof(Solid) && (geoObj as Solid).Volume != 0
                                      select geoObj;
                if (solidGeometries.Count() > 0)
                {
                    geoList = solidGeometries.ToList();
                }

                roomShape = DirectShape.CreateElement(room.Document, room.Category.Id, "Measure", room.UniqueId);
                roomShape.SetShape(geoList);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return roomShape;
        }

        public static void RotateDirectShape(DirectShape shape, Room room, out bool perpendicularSwitch)
        {
            perpendicularSwitch = false;
            try
            {
                Line axis = null;
                double angle = FindRotationAngle(room, out axis);
                if (angle == 0 || angle == Math.PI) { return; }

                //longest curve is more close to Y axis
                if (angle > 0.25 * Math.PI && angle < 0.75 * Math.PI) { perpendicularSwitch = true; }
                if (angle > 1.25 * Math.PI && angle < 1.75 * Math.PI) { perpendicularSwitch = true; }
                ElementTransformUtils.RotateElement(shape.Document, shape.Id, axis, angle);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static double FindRotationAngle(Room room, out Line axis)
        {
            double angle = 0;
            axis = null;
            try
            {
                Face bottomFace = GetBottomFace(room);
                if (null == bottomFace) { return angle; }
                Curve longestCurve = FindLongestCurve(bottomFace);
                if (null == longestCurve) { return angle; }

                XYZ firstPt = longestCurve.GetEndPoint(0);
                XYZ secondPt = longestCurve.GetEndPoint(1);

                if (firstPt.X > secondPt.X)
                {
                    //switch
                    XYZ tempPt = firstPt;
                    firstPt = secondPt;
                    secondPt = tempPt;
                }
              
                XYZ vector = secondPt.Subtract(firstPt);
                angle = vector.AngleOnPlaneTo(XYZ.BasisX, XYZ.BasisZ);
                axis = Line.CreateBound(firstPt, new XYZ(firstPt.X, firstPt.Y, firstPt.Z + 10));
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return angle;
        }


        public static void CalculateWidthAndLength(DirectShape shape, out double width, out double length)
        {
            width = 0;
            length = 0;
            try
            {
                BoundingBoxXYZ bb = shape.get_BoundingBox(null);
                XYZ min = bb.Min;
                XYZ max = bb.Max;

                width = Math.Abs(max.X - min.X);
                length = Math.Abs(max.Y - min.Y);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }


        private static Face GetBottomFace(Room room)
        {
            Face bottomFace = null;
            try
            {
                GeometryElement geoElem = room.ClosedShell;
                if (null != geoElem)
                {
                    foreach (GeometryObject geoObj in geoElem)
                    {
                        if (geoObj is Solid)
                        {
                            Solid roomSolid = geoObj as Solid;
                            
                            foreach (Face roomFace in roomSolid.Faces)
                            {
                                XYZ normal = roomFace.ComputeNormal(new UV(0, 0));
                                if (normal.Z < 0)
                                {
                                    bottomFace = roomFace;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return bottomFace;
        }

        private static Curve FindLongestCurve(Face roomFace)
        {
            Curve longestCurve = null;
            try
            {
                
                IList<CurveLoop> curveLoopList = roomFace.GetEdgesAsCurveLoops();

                List<Curve> outerCurves = new List<Curve>();
                IList<IList<CurveLoop>> curveLoopLoop = ExporterIFCUtils.SortCurveLoops(curveLoopList);
                foreach (IList<CurveLoop> curveLoops in curveLoopLoop)
                {
                    foreach (CurveLoop curveLoop in curveLoops)
                    {
                        if (curveLoop.IsCounterclockwise(roomFace.ComputeNormal(new UV(0, 0))))
                        {
                            foreach (Curve curve in curveLoop)
                            {
                                outerCurves.Add(curve);
                            }
                            break;
                        }
                    }
                }

                outerCurves = outerCurves.OrderByDescending(o => o.ApproximateLength).ToList();
                longestCurve = outerCurves.First();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return longestCurve;
        }

    }
}
