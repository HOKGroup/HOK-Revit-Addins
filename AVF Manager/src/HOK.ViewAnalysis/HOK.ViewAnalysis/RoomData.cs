using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;

namespace HOK.ViewAnalysis
{
    //regulary occupied rooms
    public class RoomData
    {
        private Document m_doc=null;
        private Room m_room = null;

        private int roomId = -1;
        private Solid roomSolid = null;
        private Face roomFace = null; //bottom face
        private List<SegmentData> boundarySegments = new List<SegmentData>();
        private List<PointData> pointDataList = new List<PointData>();
        private double visibilityRatio = 0; //percentage of area with views
        private double areaWithViews = 0; //in area unit
        private double roomArea = 0;

        private double tolerance = 0; //short curve tolerance
        private int minTransparency = 20;

        public Document RoomDocument { get { return m_doc; } set { m_doc = value; } }
        public Room RoomObj { get { return m_room; } set { m_room = value; } }
        public int RoomId { get { return roomId; } set { roomId = value; } }
        public Face RoomFace { get { return roomFace; } set { roomFace = value; } }
        public List<SegmentData> BoundarySegments { get { return boundarySegments; } set { boundarySegments = value; } }
        public List<PointData> PointDataList { get { return pointDataList; } set { pointDataList = value; } }
        public double VisiblityRatio { get { return visibilityRatio; } set { visibilityRatio = value; } }
        public double AreaWithViews { get { return areaWithViews; } set { areaWithViews = value; } }
        public double RoomArea { get { return roomArea; } set { roomArea = value; } }

        public RoomData(Room room, Document doc)
        {
            m_doc = doc;
            m_room = room;

            roomId = m_room.Id.IntegerValue;
            roomArea = m_room.Area;

            tolerance = Double.Epsilon;
            
            roomFace = GetRoomFace();
            
        }

        public RoomData(RoomData rd)
        {
            this.RoomDocument = rd.RoomDocument;
            this.RoomObj = rd.RoomObj;
            this.RoomId = rd.RoomId;
            this.RoomFace = rd.RoomFace;
            this.boundarySegments = rd.boundarySegments;
            this.PointDataList = rd.PointDataList;
            this.VisiblityRatio = rd.VisiblityRatio;
            this.AreaWithViews = rd.AreaWithViews;
            this.RoomArea = rd.RoomArea;
        }

        private Face GetRoomFace()
        {
            Face bottomFace = null;
            try
            {
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;

                GeometryElement geomElem = m_room.get_Geometry(options);
                if (null != geomElem)
                {
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        Solid solid = geomObj as Solid;
                        if (null != solid)
                        {
                            roomSolid = solid;
                            foreach (Face face in roomSolid.Faces)
                            {
                                XYZ normal = face.ComputeNormal(new UV(0, 0));
                                if (normal.Z < 0)
                                {
                                    if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                                    {
                                        bottomFace = face; break;
                                    }
                                }
                            }
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
            List<SegmentData> segmentDataList = new List<SegmentData>();
            try
            {
                SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                IList<IList<Autodesk.Revit.DB.BoundarySegment>> segments = m_room.GetBoundarySegments(options);
                foreach (IList<Autodesk.Revit.DB.BoundarySegment> segmentList in segments)
                {
                    foreach (Autodesk.Revit.DB.BoundarySegment segment in segmentList)
                    {
                        SegmentData sData = new SegmentData(segment);
                        if (null != segment.Element)
                        {
                            if (null != segment.Element.Category)
                            {
                                Wall wall = null;
                                Transform transform = Transform.Identity;
                                if (segment.Element.Category.Id.IntegerValue == (int)(BuiltInCategory.OST_Walls))
                                {
                                    wall = segment.Element as Wall;
                                    if (null != wall)
                                    {
                                        var linkIds = from linkId in exteriorElementIds where linkId.HostElementId == wall.Id select linkId;
                                        if (linkIds.Count() > 0)
                                        {
                                            sData.IsExteriror = true;
                                        }
                                    }
                                }
                                else if (includeLinkedModel &&  segment.Element.Category.Id.IntegerValue == (int)(BuiltInCategory.OST_RvtLinks))
                                {
                                    RevitLinkInstance instance = segment.Element as RevitLinkInstance;
                                    transform = instance.GetTotalTransform();
                                    Document linkDoc = instance.GetLinkDocument();

                                    XYZ centerPoint = sData.BoundaryCurve.Evaluate(0.5, true);
                                    centerPoint = transform.Inverse.OfPoint(centerPoint);

                                    BoundingBoxContainsPointFilter bbFilter = new BoundingBoxContainsPointFilter(centerPoint);

                                    FilteredElementCollector collector = new FilteredElementCollector(linkDoc);
                                    List<Wall> walls = collector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Wall)).WherePasses(bbFilter).WhereElementIsNotElementType().ToElements().Cast<Wall>().ToList();
                                    if (walls.Count > 0)
                                    {
                                        wall = walls.First();
                                        var linkIds = from linkId in exteriorElementIds where linkId.LinkedElementId == wall.Id && linkId.LinkInstanceId == instance.Id select linkId;
                                        if (linkIds.Count() > 0)
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
                                        List<Curve> windowCurves = GetWindowsDoorCurves(wall, sData.BoundaryCurve, transform);
                                        if (windowCurves.Count > 0)
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
                string message = ex.Message;
            }
            return segmentDataList;
        }

        private List<Curve> GetWindowsDoorCurves(Wall wall, Curve segmentCurve/*alwaysInHost*/, Transform transform)
        {
            List<Curve> windowCurves = new List<Curve>();
            try
            {
                LocationCurve curve = wall.Location as LocationCurve;
                Curve wallCurve = curve.Curve;

                XYZ startSegment = segmentCurve.GetEndPoint(0);
                XYZ endSegment = segmentCurve.GetEndPoint(1);

                startSegment = transform.Inverse.OfPoint(startSegment);
                endSegment = transform.Inverse.OfPoint(endSegment);

                IntersectionResult result = wallCurve.Project(startSegment);
                double param1 = result.Parameter;

                result = wallCurve.Project(endSegment);
                double param2 = result.Parameter;

                double segStartParam = param1 < param2 ? param1 : param2;
                double segEndParam = param1 > param2 ? param1 : param2;


                IList<ElementId> insertedElementIds = (wall as HostObject).FindInserts(true, true, true, true);
                List<FamilyInstance> instances = new List<FamilyInstance>();
                foreach (ElementId elementId in insertedElementIds)
                {
                    FamilyInstance instance = wall.Document.GetElement(elementId)  as FamilyInstance;
                    if(null!=instance)
                    {
                        if (instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || instance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                        {
                            bool isInsideSegment = false;

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
                                    ICollection<ElementId> materialIds = instance.GetMaterialIds(false);
                                    foreach (ElementId matId in materialIds)
                                    {
                                        Material mat = instance.Document.GetElement(matId) as Material;
                                        if (null != mat)
                                        {
                                            if (mat.Transparency > minTransparency)
                                            {
                                                instances.Add(instance);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (FamilyInstance instace in instances)
                {
                    BoundingBoxXYZ bb = instace.get_BoundingBox(null);

                    XYZ minPoint = transform.OfPoint(bb.Min);
                    double startParam =0;
                    XYZ startPoint = null;
                    result = segmentCurve.Project(minPoint);
                    if (null != result)
                    {
                        startParam = result.Parameter;
                        startPoint = result.XYZPoint;
                    }

                    XYZ maxPoint = transform.OfPoint(bb.Max);
                    double endParam = 0;
                    XYZ endPoint = null;
                    result = segmentCurve.Project(maxPoint);
                    if (null != result)
                    {
                        endParam = result.Parameter;
                        endPoint = result.XYZPoint;
                    }
                   
                    if (startParam != 0 && endParam != 0)
                    {
                        if (startPoint.DistanceTo(endPoint) > tolerance)
                        {
                            Curve windowCurve = Line.CreateBound(startPoint, endPoint);
                            windowCurves.Add(windowCurve);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return windowCurves;
        }
        public bool SetResultParameterValue(string paramName, double value)
        {
            bool result = false;
            try
            {
                Parameter param = m_room.LookupParameter(paramName);
                if (null != param)
                {
                    result = param.Set(value);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }
    }

    public class SegmentData
    {
        private Autodesk.Revit.DB.BoundarySegment m_segment = null;
        private Curve boundaryCurve = null;
        private ElementId wallId = ElementId.InvalidElementId;
        private bool isExterior = false;
        private List<Curve> visibleCurves = new List<Curve>();
        private List<XYZ> viewPoints = new List<XYZ>();
        
        public Autodesk.Revit.DB.BoundarySegment SegmentObj { get { return m_segment; } set { m_segment = value; } }
        public Curve BoundaryCurve { get { return boundaryCurve; } set { boundaryCurve = value; } }
        public ElementId WallId { get { return wallId; } set { wallId = value; } }
        public bool IsExteriror { get { return isExterior; } set { isExterior = value; } }
        public List<Curve> VisibleCurves { get { return visibleCurves; } set { visibleCurves = value; } }
        public List<XYZ> ViewPoints { get { return viewPoints; } set { viewPoints = value; } }

        public SegmentData(Autodesk.Revit.DB.BoundarySegment segment)
        {
            m_segment = segment;
            boundaryCurve = segment.Curve;
            
            
            if (null == segment.Element)
            {
                visibleCurves.Add(boundaryCurve);
            }
        }

        public void GetViewPoints(bool isCurtainWall)
        {
            try
            {
                foreach (Curve curve in visibleCurves)
                {
                    if (isCurtainWall)
                    {
                        double interval = isExterior ? 2 : 1;
                        
                        double startParam = curve.GetEndParameter(0);
                        double endParam = curve.GetEndParameter(1);

                        for (double param = startParam + 0.2; param < endParam - 0.2; param += interval)
                        {
                            XYZ point = curve.Evaluate(param, false);
                            point = new XYZ(point.X, point.Y, point.Z + 3.5); //offset:3.5
                            viewPoints.Add(point);
                        }
                    }
                    else
                    {
                        //by division number e.g. windows
                        //normalized parameters
                        double[] parameters = new double[] { };
                        if (isExterior)
                        {
                            parameters = new double[] { 0.1, 0.3, 0.5, 0.7, 0.9 };
                        }
                        else
                        {
                            parameters = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
                        }

                        foreach (double param in parameters)
                        {
                            XYZ point = curve.Evaluate(param, true);
                            point = new XYZ(point.X, point.Y, point.Z + 3.5); //offset:3.5
                            viewPoints.Add(point);
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

    public class PointData
    {
        private UV uvPoint = null;
        private XYZ xyzPoint = null;
        private double pointValue = 0; //0:non-visible, 1:visible
        private List<double> pointValues = new List<double>();
        private ValueAtPoint valueAtPoint = null;

        public UV UVPoint { get { return uvPoint; } set { uvPoint = value; } }
        public XYZ XYZPoint { get { return xyzPoint; } set { xyzPoint = value; } }
        public double PointValue { get { return pointValue; } set { pointValue = value; } }
        public List<double> PointValues { get { return pointValues; } set { pointValues = value; } }
        public ValueAtPoint ValueAtPoint { get { return valueAtPoint; } set { valueAtPoint = value; } }

        public PointData()
        {
        }

        public PointData(UV uv, XYZ xyz, double value)
        {
            uvPoint = uv;
            xyzPoint = xyz;
            pointValue = value;
            pointValues.Add(pointValue);
            valueAtPoint = new ValueAtPoint(pointValues);
        }

        public PointData(UV uv, XYZ xyz, List<double> values)
        {
            uvPoint = uv;
            xyzPoint = xyz;
            pointValues = values;
            valueAtPoint = new ValueAtPoint(pointValues);
        }
    }

}
