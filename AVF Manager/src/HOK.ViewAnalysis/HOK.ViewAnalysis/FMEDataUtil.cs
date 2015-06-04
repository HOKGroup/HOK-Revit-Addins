using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace HOK.ViewAnalysis
{
    public static class FMEDataUtil
    {
        public static Face CreateFacebyFMEData(List<FMEArea> fmeAreaList)
        {
            Face faceCreated = null;
            try
            {
                List<CurveLoop> curveLoopList = new List<CurveLoop>();
                foreach (FMEArea fmeArea in fmeAreaList)
                {
                    List<XYZ> pointList = new List<XYZ>();
                    string[] points = fmeArea.Coordinates.Split(' ');
                    foreach (string point in points)
                    {
                        string[] coordinates = point.Split(',');
                        if (coordinates.Length == 3)
                        {
                            XYZ xyz = new XYZ(double.Parse(coordinates[0]), double.Parse(coordinates[1]), double.Parse(coordinates[2]));
                            pointList.Add(xyz);
                        }
                    }

                    if (pointList.Count > 0)
                    {
                        List<Curve> curveList = new List<Curve>();
                        for (int i = 0; i < pointList.Count -1; i++)
                        {
                            Line line = Line.CreateBound(pointList[i], pointList[i + 1]);
                            curveList.Add(line);
                        }

                        curveList.Add(Line.CreateBound(pointList[pointList.Count - 1], pointList[0]));
                        CurveLoop curveLoop = CurveLoop.Create(curveList);
                        curveLoopList.Add(curveLoop);
                    }
                }

                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoopList, new XYZ(0, 0, 1), 1);
                if (null != solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        XYZ normal = face.ComputeNormal(new UV(0, 0));
                        if (normal.Z < 0)
                        {
                            faceCreated = face; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return faceCreated;
        }

        public static List<FMEArea> ConvertRevitFaceToFMEArea(Face revitFace)
        {
            List<FMEArea> fmeAreaList = new List<FMEArea>();
            try
            {
                foreach (EdgeArray edgeArray in revitFace.EdgeLoops)
                {
                    FMEArea fmeArea = new FMEArea();
                    List<XYZ> tessellatedPoints = new List<XYZ>();
                    foreach (Edge edge in edgeArray)
                    {
                        if (null!=edge.Tessellate())
                        {
                            tessellatedPoints.AddRange(edge.Tessellate());
                            tessellatedPoints.RemoveAt(tessellatedPoints.Count - 1); //to prevent the duplicated point
                        }
                    }
                    if (tessellatedPoints.Count > 0)
                    {
                        StringBuilder strPointBuilder = new StringBuilder();
                        foreach (XYZ xyz in tessellatedPoints)
                        {
                            string strPoint = Math.Round(xyz.X, 3).ToString() + "," + Math.Round(xyz.Y, 3).ToString() + "," + Math.Round(xyz.Z, 3)+" ";
                            strPointBuilder.Append(strPoint);
                        }
                        fmeArea.Coordinates = strPointBuilder.ToString().Remove(strPointBuilder.Length - 1);
                        fmeAreaList.Add(fmeArea);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return fmeAreaList;
        }

        private static PointData ConvertToPointData(Face revitFace, FMEPoint fmePoint)
        {
            PointData pointData = null;
            try
            {
                string[] coordinates = fmePoint.Coordinate.Split(',');
                XYZ xyz = null;
                if (coordinates.Length == 3)
                {
                    xyz = new XYZ(double.Parse(coordinates[0]), double.Parse(coordinates[1]), double.Parse(coordinates[2]));
                }

                UV uv = null;
                if (null != revitFace && null != xyz)
                {
                    IntersectionResult intersectionResult = revitFace.Project(xyz);
                    if (null != intersectionResult)
                    {
                        uv = intersectionResult.UVPoint;
                    }
                }

                pointData = new PointData(uv, xyz, fmePoint.PointValue);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return pointData;
        }

        private static FMEPoint ConvertToFMEPoint(PointData pointData)
        {
            FMEPoint fmePoint = new FMEPoint();
            try
            {
                XYZ xyz = pointData.XYZPoint;
                fmePoint.Coordinate = Math.Round(xyz.X, 3).ToString() + "," + Math.Round(xyz.Y, 3).ToString() + "," + Math.Round(xyz.Z, 3).ToString();
                fmePoint.PointValue = pointData.PointValue;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return fmePoint;
        }

        public static List<FMEPoint> ConvertToFMEPointList(List<PointData> pointDataList)
        {
            List<FMEPoint> fmePointList = new List<FMEPoint>();
            try
            {
                foreach (PointData ptData in pointDataList)
                {
                    FMEPoint fmePoint = FMEDataUtil.ConvertToFMEPoint(ptData);
                    fmePointList.Add(fmePoint);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return fmePointList;
        }

        public static List<PointData> ConvertToPointDataList(Face revitFace, List<FMEPoint> fmePointList)
        {
            List<PointData> pointDataList = new List<PointData>();
            try
            {
                foreach (FMEPoint fmePoint in fmePointList)
                {
                    PointData ptData = FMEDataUtil.ConvertToPointData(revitFace, fmePoint);
                    pointDataList.Add(ptData);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return pointDataList;
        }


    }

    public class AnalysisDataCollection
    {
        private List<AnalysisData> analysisDataList = new List<AnalysisData>();

        public List<AnalysisData> AnalysisDataList { get { return analysisDataList; } set { analysisDataList = value; } }

        public AnalysisDataCollection()
        {
        }
    }

    public class AnalysisData
    {
        private int roomId = -1;
        private double roomArea = 0;
        private double visibleArea = 0;
        private List<FMEArea> roomFace = new List<FMEArea>();
        private List<FMEPoint> pointValues = new List<FMEPoint>();

        public int RoomId { get { return roomId; } set { roomId = value; } }
        public double RoomArea { get { return roomArea; } set { roomArea = value; } }
        public double VisibleArea { get { return visibleArea; } set { visibleArea = value; } }
        public List<FMEArea> RoomFace { get { return roomFace; } set { roomFace = value; } }
        public List<FMEPoint> PointValues { get { return pointValues; } set { pointValues = value; } }

        public AnalysisData()
        {
        }
    }

    public class FMEArea
    {
        private string coordinates = "";

        public string Coordinates { get { return coordinates; } set { coordinates = value; } }

        public FMEArea()
        {
        }

        public FMEArea(string coord)
        {
            coordinates = coord;
        }
    }

    public class FMEPoint
    {
        private string coordinate = "";
        private double pointValue = 0;

        public string Coordinate { get { return coordinate; } set { coordinate = value; } }

         [System.Xml.Serialization.XmlAttributeAttribute()]
        public double PointValue { get { return pointValue; } set { pointValue = value; } }

        public FMEPoint()
        {
        }

        public FMEPoint(string coord, double value)
        {
            coordinate = coord;
            pointValue = value;
        }
    }

}
