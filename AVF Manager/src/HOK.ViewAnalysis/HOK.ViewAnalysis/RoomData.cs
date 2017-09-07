using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;

namespace HOK.ViewAnalysis
{
    public class RoomData
    {
        private Solid roomSolid;

        private readonly double tolerance; //short curve tolerance
        private const int minTransparency = 20;

        public Document RoomDocument { get; set; }
        public Room RoomObj { get; set; }
        public int RoomId { get; set; }
        public Face RoomFace { get; set; }
        public List<SegmentData> BoundarySegments { get; set; } = new List<SegmentData>();
        public List<PointData> PointDataList { get; set; } = new List<PointData>();
        public double VisiblityRatio { get; set; }
        public double AreaWithViews { get; set; }
        public double RoomArea { get; set; }

        public RoomData(Room room, Document doc)
        {
            RoomDocument = doc;
            RoomObj = room;

            RoomId = RoomObj.Id.IntegerValue;
            RoomArea = RoomObj.Area;

            tolerance = double.Epsilon;
        }

        public RoomData(RoomData rd)
        {
            RoomDocument = rd.RoomDocument;
            RoomObj = rd.RoomObj;
            RoomId = rd.RoomId;
            RoomFace = rd.RoomFace;
            BoundarySegments = rd.BoundarySegments;
            PointDataList = rd.PointDataList;
            VisiblityRatio = rd.VisiblityRatio;
            AreaWithViews = rd.AreaWithViews;
            RoomArea = rd.RoomArea;
        }

        public Face GetRoomFace()
        {
            Face bottomFace = null;
            try
            {
                var options = new Options
                {
                    ComputeReferences = true,
                    IncludeNonVisibleObjects = true
                };

                var geomElem = RoomObj.get_Geometry(options);
                if (null != geomElem)
                {
                    foreach (var geomObj in geomElem)
                    {
                        var solid = geomObj as Solid;
                        if (null == solid) continue;

                        roomSolid = solid;
                        foreach (Face face in roomSolid.Faces)
                        {
                            var normal = face.ComputeNormal(new UV(0, 0));
                            if (!(normal.Z < 0)) continue;

                            if (!(Math.Abs(normal.Z) > Math.Abs(normal.X)) ||
                                !(Math.Abs(normal.Z) > Math.Abs(normal.Y))) continue;

                            bottomFace = face; break;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot find room face: "+ex.Message);
            }
            return bottomFace;
        }

        public List<SegmentData> CollectSegmentData(List<LinkElementId> exteriorElementIds, bool includeLinkedModel)
        {
            var segmentDataList = new List<SegmentData>();
            try
            {
                var options = new SpatialElementBoundaryOptions();
                options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                var segments = RoomObj.GetBoundarySegments(options);
                foreach (var segmentList in segments)
                {
                    foreach (var segment in segmentList)
                    {
                        var sData = new SegmentData(segment);
#if RELEASE2015
                        Element element = segment.Element;
#else
                        var element = RoomDocument.GetElement(segment.ElementId);
#endif

                        if (null != element)
                        {
                            if (null != element.Category)
                            {
                                Wall wall = null;
                                var transform = Transform.Identity;
                                if (element.Category.Id.IntegerValue == (int)(BuiltInCategory.OST_Walls))
                                {
                                    wall = element as Wall;
                                    if (null != wall)
                                    {
                                        var linkIds = from linkId in exteriorElementIds where linkId.HostElementId == wall.Id select linkId;
                                        if (linkIds.Any())
                                        {
                                            sData.IsExteriror = true;
                                        }
                                    }
                                }
                                else if (includeLinkedModel && element.Category.Id.IntegerValue == (int)(BuiltInCategory.OST_RvtLinks))
                                {
                                    var instance = element as RevitLinkInstance;
                                    transform = instance.GetTotalTransform();
                                    var linkDoc = instance.GetLinkDocument();

                                    var centerPoint = sData.BoundaryCurve.Evaluate(0.5, true);
                                    centerPoint = transform.Inverse.OfPoint(centerPoint);

                                    var bbFilter = new BoundingBoxContainsPointFilter(centerPoint);

                                    var collector = new FilteredElementCollector(linkDoc);
                                    var walls = collector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Wall)).WherePasses(bbFilter).WhereElementIsNotElementType().ToElements().Cast<Wall>().ToList();
                                    if (walls.Count > 0)
                                    {
                                        wall = walls.First();
                                        var linkIds = from linkId in exteriorElementIds where linkId.LinkedElementId == wall.Id && linkId.LinkInstanceId == instance.Id select linkId;
                                        if (linkIds.Any())
                                        {
                                            sData.IsExteriror = true;
                                        }
                                    }
                                }

                                if (null != wall)
                                {
                                    sData.WallId = wall.Id;
                                    if (wall.WallType.Kind == WallKind.Curtain)
                                    {
                                        sData.VisibleCurves.Add(sData.BoundaryCurve);
                                        sData.GetViewPoints(true);
                                        segmentDataList.Add(sData);
                                    }
                                    else
                                    {
                                        var windowCurves = GetWindowsDoorCurves(wall, sData.BoundaryCurve, transform);
                                        if (windowCurves.Any())
                                        {
                                            sData.VisibleCurves.AddRange(windowCurves);
                                            sData.GetViewPoints(false);
                                            segmentDataList.Add(sData);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            sData.GetViewPoints(true);
                            segmentDataList.Add(sData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return segmentDataList;
        }

        private List<Curve> GetWindowsDoorCurves(Wall wall, Curve segmentCurve/*alwaysInHost*/, Transform transform)
        {
            var windowCurves = new List<Curve>();
            try
            {
                var curve = wall.Location as LocationCurve;
                var wallCurve = curve.Curve;

                var startSegment = segmentCurve.GetEndPoint(0);
                var endSegment = segmentCurve.GetEndPoint(1);

                startSegment = transform.Inverse.OfPoint(startSegment);
                endSegment = transform.Inverse.OfPoint(endSegment);

                var result = wallCurve.Project(startSegment);
                var param1 = result.Parameter;

                result = wallCurve.Project(endSegment);
                var param2 = result.Parameter;

                var segStartParam = param1 < param2 ? param1 : param2;
                var segEndParam = param1 > param2 ? param1 : param2;


                var insertedElementIds = (wall as HostObject).FindInserts(true, true, true, true);
                var instances = new List<FamilyInstance>();
                foreach (var elementId in insertedElementIds)
                {
                    var instance = wall.Document.GetElement(elementId)  as FamilyInstance;
                    if(null!=instance)
                    {
                        if (instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                        {
                            var isInsideSegment = false;

                            if (instance.HostParameter > segStartParam && instance.HostParameter < segEndParam)
                            {
                                isInsideSegment = true;
                            }

                            if (isInsideSegment)
                            {
                                if (instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows)
                                {
                                    instances.Add(instance);
                                }
                                else
                                {
                                    var materialIds = instance.GetMaterialIds(false);
                                    foreach (var matId in materialIds)
                                    {
                                        var mat = instance.Document.GetElement(matId) as Material;
                                        if (!(mat?.Transparency > minTransparency)) continue;
                                        instances.Add(instance);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var instace in instances)
                {
                    var bb = instace.get_BoundingBox(null);

                    var minPoint = transform.OfPoint(bb.Min);
                    double startParam =0;
                    XYZ startPoint = null;
                    result = segmentCurve.Project(minPoint);
                    if (null != result)
                    {
                        startParam = result.Parameter;
                        startPoint = result.XYZPoint;
                    }

                    var maxPoint = transform.OfPoint(bb.Max);
                    double endParam = 0;
                    XYZ endPoint = null;
                    result = segmentCurve.Project(maxPoint);
                    if (null != result)
                    {
                        endParam = result.Parameter;
                        endPoint = result.XYZPoint;
                    }

                    if (startParam == 0 || endParam == 0) continue;
                    if (!(startPoint.DistanceTo(endPoint) > tolerance)) continue;

                    Curve windowCurve = Line.CreateBound(startPoint, endPoint);
                    windowCurves.Add(windowCurve);
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return windowCurves;
        }
        public bool SetResultParameterValue(string paramName, double value)
        {
            var result = false;
            try
            {
                var param = RoomObj.LookupParameter(paramName);
                if (null != param)
                {
                    result = param.Set(value);
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return result;
        }
    }

    public class SegmentData
    {
        public BoundarySegment SegmentObj { get; set; }
        public Curve BoundaryCurve { get; set; }
        public ElementId WallId { get; set; } = ElementId.InvalidElementId;
        public bool IsExteriror { get; set; }
        public List<Curve> VisibleCurves { get; set; } = new List<Curve>();
        public List<XYZ> ViewPoints { get; set; } = new List<XYZ>();

        public SegmentData(BoundarySegment segment)
        {
            SegmentObj = segment;
#if RELEASE2017
            BoundaryCurve = segment.GetCurve();
#else
            BoundaryCurve = segment.Curve;
#endif

#if RELEASE2015
            if (null == segment.Element)
            {
                visibleCurves.Add(boundaryCurve);
            }
#else
            if (ElementId.InvalidElementId!=segment.ElementId)
            {
                VisibleCurves.Add(BoundaryCurve);
            }
#endif

            
        }

        public void GetViewPoints(bool isCurtainWall)
        {
            try
            {
                foreach (var curve in VisibleCurves)
                {
                    if (isCurtainWall)
                    {
                        double interval = IsExteriror ? 2 : 1;
                        
                        var startParam = curve.GetEndParameter(0);
                        var endParam = curve.GetEndParameter(1);

                        for (var param = startParam + 0.2; param < endParam - 0.2; param += interval)
                        {
                            var point = curve.Evaluate(param, false);
                            point = new XYZ(point.X, point.Y, point.Z + 3.5); //offset:3.5
                            ViewPoints.Add(point);
                        }
                    }
                    else
                    {
                        //by division number e.g. windows
                        //normalized parameters
                        var parameters = IsExteriror ? new[] { 0.1, 0.3, 0.5, 0.7, 0.9 } : new[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };

                        foreach (var param in parameters)
                        {
                            var point = curve.Evaluate(param, true);
                            point = new XYZ(point.X, point.Y, point.Z + 3.5); //offset:3.5
                            ViewPoints.Add(point);
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
    }

    public class PointData
    {
        public UV UVPoint { get; set; }
        public XYZ XYZPoint { get; set; }
        public double PointValue { get; set; }
        public List<double> PointValues { get; set; } = new List<double>();
        public ValueAtPoint ValueAtPoint { get; set; }

        public PointData()
        {
        }

        public PointData(UV uv, XYZ xyz, double value)
        {
            UVPoint = uv;
            XYZPoint = xyz;
            PointValue = value;
            PointValues.Add(PointValue);
            ValueAtPoint = new ValueAtPoint(PointValues);
        }

        public PointData(UV uv, XYZ xyz, List<double> values)
        {
            UVPoint = uv;
            XYZPoint = xyz;
            PointValues = values;
            ValueAtPoint = new ValueAtPoint(PointValues);
        }
    }
}
