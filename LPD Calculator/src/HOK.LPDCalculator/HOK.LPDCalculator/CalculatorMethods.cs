using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.LPDCalculator.Schedule;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;


namespace HOK.LPDCalculator
{
    public class CalculatorMethods
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        //private string originalDefFile = "";
        private List<Element> selectedAreas = new List<Element>();
        private ModelSelection lightingSelection;
        private FamilySymbol annotationType = null;
        private Dictionary<int/*areaId*/, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();
        private Dictionary<int/*lighting typeId*/, double/*Apparent Load*/> lightingTypeDictionary = new Dictionary<int, double>();
        private List<string> linkedDocuments = new List<string>();

        public CalculatorMethods(UIApplication uiApp, List<Element> areas, FamilySymbol symbolType, ModelSelection lightingFrom)
        {
            m_app = uiApp;
            m_doc = uiApp.ActiveUIDocument.Document;
            selectedAreas = areas;
            annotationType = symbolType;
            lightingSelection = lightingFrom;

            CollectLightingTypes();
            CollectRvtLink();
        }

        private void CollectRvtLink()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                List<RevitLinkInstance> revitLinkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();

                foreach (RevitLinkInstance instance in revitLinkInstances)
                {
                    ElementId typeId = instance.GetTypeId();
                    RevitLinkType rvtLinkType = m_doc.GetElement(typeId) as RevitLinkType;
                    string docName = rvtLinkType.Name;
                    if (!linkedDocuments.Contains(docName))
                    {
                        linkedDocuments.Add(docName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Revit Links.\n" + ex.Message, "Collect Revit Link", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectLightingTypes()
        {
            try
            {
                foreach (Document doc in m_app.Application.Documents)
                {

                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    IList<ElementType> lightingTypes = collector.OfCategory(BuiltInCategory.OST_LightingFixtures).WhereElementIsElementType().ToElements().Cast<ElementType>().ToList();

                    foreach (ElementType etype in lightingTypes)
                    {
                        Parameter param = etype.get_Parameter(BuiltInParameter.RBS_ELEC_APPARENT_LOAD);
                        if (null != param)
                        {
                            double apparentLoad = param.AsDouble();
                            if (!lightingTypeDictionary.ContainsKey(etype.Id.IntegerValue))
                            {
                                lightingTypeDictionary.Add(etype.Id.IntegerValue, apparentLoad);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not collect lighting types.\n" + ex.Message, "CollectLightingTypes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public bool CalculateLPD(ToolStripProgressBar bar)
        {
            bool result = false;
            try
            {
                if (null != bar)
                {
                    bar.Maximum = selectedAreas.Count;
                    bar.Value = 0;
                }

                using (TransactionGroup group = new TransactionGroup(m_doc, "Update Areas"))
                {
                    group.Start();

                    foreach (Element e in selectedAreas)
                    {
                        if (null != bar)
                        {
                            bar.PerformStep();
                        }

                        Area area = e as Area;
                        if (area.Area == 0) { continue; } //skip empty area elements.

                        Room correlatedRoom = null;
                        List<Element> elementList = GetLightingFixtures(area, out correlatedRoom);
                        if (elementList.Count > 0)
                        {
                            using (Transaction trans = new Transaction(m_doc, "Set LPD"))
                            {
                                trans.Start();
                                try
                                {
                                    AreaProperties ap = new AreaProperties(area);
                                    if (null != correlatedRoom)
                                    {
                                        RoomProperties rp = new RoomProperties(correlatedRoom);
                                        rp.UpdateAreaParameter(area);
                                    }

                                    Dictionary<int, LightingProperties> lightingDictionary = new Dictionary<int, LightingProperties>();
                                    double totalAL = 0;
                                    foreach (Element element in elementList)
                                    {
                                        LightingProperties lp = new LightingProperties(element);
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                            Parameter calculatedParam = element.LookupParameter("Apparent VA Calculated Load");
#else
                                        Parameter calculatedParam = element.get_Parameter("Apparent VA Calculated Load");
#endif

                                        if (null != calculatedParam)
                                        {
                                            lp.ApparentLoad = calculatedParam.AsDouble();
                                            totalAL += lp.ApparentLoad;
                                        }
                                        else if (lightingTypeDictionary.ContainsKey(lp.LightingTypeId))
                                        {
                                            lp.ApparentLoad = lightingTypeDictionary[lp.LightingTypeId];
                                            totalAL += lp.ApparentLoad;
                                        }

                                        if (!lightingDictionary.ContainsKey(lp.LightingId))
                                        {
                                            lightingDictionary.Add(lp.LightingId, lp);
                                        }
                                    }

                                    ap.ActualLightingLoad = totalAL;
                                    ap.LPD = totalAL / ap.Area;

#if RELEASE2015 || RELEASE2016 || RELEASE2017
                                    Parameter param = area.LookupParameter("ActualLightingLoad");
#else
                                    Parameter param = area.get_Parameter("ActualLightingLoad");
#endif

                                    if (null != param)
                                    {
                                        bool setLoad = param.Set(ap.ActualLightingLoad);
                                    }
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                                    param = area.LookupParameter("ActualLPD");
#else
                                    param = area.get_Parameter("ActualLPD");
#endif

                                    if (null != param)
                                    {
                                        bool setLPD = param.Set(ap.LPD);
                                    }

                                    ap.LightingFixtures = lightingDictionary;
                                    areaDictionary.Add(ap.AreaId, ap);
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Failed to set LPD.\n" + ex.Message, "Set LPD Values", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    trans.RollBack();
                                }
                            }
                        }
                        else { continue; }
                    }

                    group.Assimilate();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not calculate LPD.\n" + ex.Message, "Calculate LPD", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return result;
        }

        private List<Element> GetLightingFixtures(Area area, out Room correlatedRoom)
        {
            correlatedRoom = null;
            List<Element> lightingFixtures = new List<Element>();
            try
            {
                List<Element> fromHost = new List<Element>();
                List<Element> fromLink = new List<Element>();

                CurveArrArray profiles = new CurveArrArray();
                Outline outline = GetBoundingBox(area, out profiles);
                Face topFace = GetAreaFace(profiles);

                BoundingBoxIntersectsFilter intersectFilter = new BoundingBoxIntersectsFilter(outline);
                BoundingBoxIsInsideFilter insideFilter = new BoundingBoxIsInsideFilter(outline);
                LogicalOrFilter orFilter = new LogicalOrFilter(intersectFilter, insideFilter);

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_LightingFixtures);
                List<ElementId> elementIds = collector.WherePasses(orFilter).ToElementIds().ToList();

                FilteredElementCollector roomCollector = new FilteredElementCollector(m_doc);
                roomCollector.OfCategory(BuiltInCategory.OST_Rooms);
                List<Room> roomsFound = roomCollector.WherePasses(orFilter).ToElements().Cast<Room>().ToList();
                foreach (Room room in roomsFound)
                {
#if RELEASE2013
                    if (null != area.Level && null!=room.Level)
                    {
                        if (area.Level.Id.IntegerValue == room.Level.Id.IntegerValue)
                        {
                            correlatedRoom = room; break;
                        }
                    }
#else
                    if (area.LevelId.IntegerValue == room.LevelId.IntegerValue)
                    {
                        correlatedRoom = room; break;
                    }
#endif
                }

                BuiltInParameter bltParam1 = BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM;
                ParameterValueProvider pvp1 = new ParameterValueProvider(new ElementId((int)bltParam1));
                BuiltInParameter bltParam2 = BuiltInParameter.FAMILY_LEVEL_PARAM;
                ParameterValueProvider pvp2 = new ParameterValueProvider(new ElementId((int)bltParam2));

                FilterNumericRuleEvaluator fnrv = new FilterNumericEquals();
#if RELEASE2013
                ElementId ruleValueId = new ElementId(area.Level.Id.IntegerValue);
#else
                ElementId ruleValueId = new ElementId(area.LevelId.IntegerValue);
#endif

                FilterRule filterRule1 = new FilterElementIdRule(pvp1, fnrv, ruleValueId);
                FilterRule filterRule2 = new FilterElementIdRule(pvp2, fnrv, ruleValueId);

                ElementParameterFilter paramFilter1 = new ElementParameterFilter(filterRule1);
                ElementParameterFilter paramFilter2 = new ElementParameterFilter(filterRule2);
                LogicalOrFilter paramFilter = new LogicalOrFilter(paramFilter1, paramFilter2);

                if (elementIds.Count > 0)
                {
                    collector = new FilteredElementCollector(m_doc, elementIds);
                    fromHost = collector.WherePasses(paramFilter).ToElements().ToList();
                    fromHost = GetPolygonIntersect(topFace, fromHost);
                }
                
                if (lightingSelection != ModelSelection.Host)
                {
                    if (linkedDocuments.Count > 0)
                    {
                        foreach (Document doc in m_app.Application.Documents)
                        {
                            if (linkedDocuments.Contains(doc.Title))
                            {
                                if (null == correlatedRoom)
                                {
                                    roomCollector = new FilteredElementCollector(doc);
                                    roomCollector.OfCategory(BuiltInCategory.OST_Rooms);
                                    roomsFound = roomCollector.WherePasses(orFilter).ToElements().Cast<Room>().ToList();
                                    foreach (Room room in roomsFound)
                                    {
#if RELEASE2013
                                        if (null != area.Level && null != room.Level)
                                        {
                                            if (area.Level.Id.IntegerValue == room.Level.Id.IntegerValue)
                                            {
                                                correlatedRoom = room; break;
                                            }
                                        }
#else
                                        if (area.LevelId.IntegerValue == room.LevelId.IntegerValue)
                                        {
                                            correlatedRoom = room; break;
                                        }
#endif
                                    }
                                }

                                collector = new FilteredElementCollector(doc);
                                collector.OfCategory(BuiltInCategory.OST_LightingFixtures);
                                elementIds = collector.WherePasses(orFilter).ToElementIds().ToList();
                                if (elementIds.Count > 0)
                                {
                                    collector = new FilteredElementCollector(doc, elementIds);
                                    List<Element> elements = collector.WherePasses(paramFilter).ToElements().ToList();
                                    elements = GetPolygonIntersect(topFace, elements);
                                    fromLink.AddRange(elements);
                                }
                            }
                        }
                    }
                }

                switch (lightingSelection)
                {
                    case ModelSelection.Host:
                        lightingFixtures = fromHost;
                        break;
                    case ModelSelection.Link:
                        lightingFixtures = fromLink;
                        break;
                    case ModelSelection.Both:
                        lightingFixtures.AddRange(fromHost);
                        lightingFixtures.AddRange(fromLink);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get lighting fixtures.\n" + ex.Message, "Get Lighting Fixtures", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return lightingFixtures;
        }

        private Outline GetBoundingBox(Area area, out CurveArrArray profiles)
        {
            profiles = new CurveArrArray();
            XYZ minXYZ = new XYZ(0, 0, 0);
            XYZ maxXYZ = new XYZ(0, 0, 0);
            Outline outline = new Outline(minXYZ, maxXYZ);
            try
            {
                if (area.GetBoundarySegments(new SpatialElementBoundaryOptions()) != null)
                {
                    double maxLength = 0;
                    foreach (IList<Autodesk.Revit.DB.BoundarySegment> boundarySegments in area.GetBoundarySegments(new SpatialElementBoundaryOptions()))
                    {
                        if (boundarySegments.Count > 0)
                        {
                            CurveArray profile = new CurveArray();
                            double length = 0;
                            foreach (Autodesk.Revit.DB.BoundarySegment boundarySegment in boundarySegments)
                            {
#if RELEASE2016 || RELEASE2017
                                Curve curve = boundarySegment.GetCurve();
#else
                                Curve curve = boundarySegment.Curve;
#endif

                                profile.Append(curve);
                                length += curve.Length;
                            }
                            if (length > maxLength)
                            {
                                maxLength = length;
                                profiles.Insert(profile, 0); //first curve array will be the out most profile.
                            }
                            else
                            {
                                profiles.Append(profile);
                            }
                        }
                    }
                }

                double minX = 0;
                double minY = 0;
                double minZ = 0;
                double maxX = 0;
                double maxY = 0;
                double maxZ = 0; ;
                bool first = true;

                if (profiles.Size > 0)
                {
                    foreach (CurveArray curveArray in profiles)
                    {
                        foreach (Curve curve in curveArray)
                        {
#if RELEASE2013
                            XYZ point = curve.get_EndPoint(0);
#else
                            XYZ point = curve.GetEndPoint(0);
#endif
                            if (first)
                            {
                                minX = point.X;
                                minY = point.Y;
                                minZ = point.Z;
                                maxX = point.X;
                                maxY = point.Y;
                                maxZ = minZ + 50;

                                first = false;
                            }
                            else
                            {
                                if (point.X < minX) { minX = point.X; }
                                if (point.Y < minY) { minY = point.Y; }
                                if (point.X > maxX) { maxX = point.X; }
                                if (point.Y > maxY) { maxY = point.Y; }
                            }
                        }
                    }
                }

                minXYZ = new XYZ(minX, minY, minZ);
                maxXYZ = new XYZ(maxX, maxY, maxZ);
                outline = new Outline(minXYZ, maxXYZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get profiles.\n" + ex.Message, "Get Area Profiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return outline;
        }

        private Face GetAreaFace(CurveArrArray profiles)
        {
            Face topFace = null;
            try
            {
                Solid areaSolid = FindAreaSolid(profiles);
                Options geomOptions = new Options();
                foreach (Face face in areaSolid.Faces)
                {
                    if (face.ComputeNormal(new UV(0, 0)).Z > 0)
                    {
                        topFace = face; break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get area face.\n" + ex.Message, "Get Face from Area", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return topFace;
        }

        private List<Element> GetPolygonIntersect(Face topFace, List<Element> elements)
        {
            List<Element> fixtures = new List<Element>();
            try
            {
                foreach (Element element in elements)
                {
                    LocationPoint locationPt = element.Location as LocationPoint;
                    if (null != locationPt)
                    {
                        XYZ pt = locationPt.Point;
                        IntersectionResult result = topFace.Project(pt);
                        if (null != result)
                        {
                            fixtures.Add(element);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find lighting fixtures within the area boundaries.\n" + ex.Message, "Get Polygon Intersect", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return fixtures;
        }

        private Solid FindAreaSolid(CurveArrArray profiles)
        {
            Solid areaSolid = null;
            try
            {
                List<CurveLoop> curveLoops = new List<CurveLoop>();
                foreach (CurveArray curveArray in profiles)
                {
                    List<XYZ> pointList = new List<XYZ>();
                    CurveLoop curveLoop = new CurveLoop();

                    foreach (Curve curve in curveArray)
                    {
                        int pointCount = curve.Tessellate().Count;
                        if (pointCount > 2)
                        {
                            IList<XYZ> tPoints = curve.Tessellate();
                            tPoints.RemoveAt(tPoints.Count - 1);
                            pointList.AddRange(tPoints);
                        }
                        else if (pointCount == 2)
                        {
#if RELEASE2013
                            XYZ pt = curve.get_EndPoint(0);
#else
                            XYZ pt = curve.GetEndPoint(0);
#endif

                            if (pointList.Count == 0) { pointList.Add(pt); }
                            if (pointList.Count > 0 && pointList[pointList.Count - 1].DistanceTo(pt) > 0.0026)//revit tolerance will be 1/32"
                            {
                                pointList.Add(pt);
                            }
                        }
                    }

                    if (pointList.Count > 2)
                    {
                        Curve newCurve = null;
                        for (int i = 0; i < pointList.Count; i++)
                        {
#if RELEASE2013
                            if (i == pointList.Count - 1)
                            {
                                newCurve = m_app.Application.Create.NewLine(pointList[i], pointList[0], true);
                            }
                            else
                            {
                                newCurve = m_app.Application.Create.NewLine(pointList[i], pointList[i + 1], true);
                            }
#else
                            if (i == pointList.Count - 1)
                            {
                                newCurve = Line.CreateBound(pointList[i], pointList[0]);
                            }
                            else
                            {
                                newCurve = Line.CreateBound(pointList[i], pointList[i + 1]);
                            }
#endif

                            curveLoop.Append(newCurve);
                        }
                        curveLoops.Add(curveLoop);
                    }

                    if (curveLoops.Count > 0)
                    {
                        XYZ direction = new XYZ(0, 0, 1);
                        areaSolid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, direction, 5);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the area solid.\n" + ex.Message, "FindAreaSolid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return areaSolid;
        }

        public bool UpdateSpaceAnnotationFamily()
        {
            bool result = false;
            try
            {
                ScheduleDataManager scheduleManager = new ScheduleDataManager(m_app, CalculationTypes.SpaceBySpace);
                double allowableLightingLoad = scheduleManager.GetAllowableLightingLoad();
                double actualLightingLoad = scheduleManager.GetActualLightingLoad();
                double savings = scheduleManager.GetSavings();
                double area = scheduleManager.GetArea();
                double actualLPD = actualLightingLoad / area;
                //double allowableLPD = scheduleManager.GetAllowableLPD();
                double reduction = savings / allowableLightingLoad;

                using (Transaction trans = new Transaction(m_doc))
                {
                    try
                    {
                        trans.Start("Update Annotation");

                        AnnotationProperties annotation = new AnnotationProperties(annotationType, CalculationTypes.SpaceBySpace);
                        annotation.TotalAllowableLightingLoad = allowableLightingLoad;
                        annotation.TotalActualLightingLoad = actualLightingLoad;
                        annotation.TotalSavingsOverage = savings;
                        annotation.Area = area;
                        annotation.ActualLPD = actualLPD;
                        annotation.Reduction = reduction;
                        annotation.LPDCalculatedBy = Environment.UserName;

                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update annotation family.\n" + ex.Message, "Update Annotation Family", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public bool UpdateBuildingAnnotationFamily()
        {
            bool result = false;
            try
            {
                ScheduleDataManager scheduleManager = new ScheduleDataManager(m_app, CalculationTypes.BuildingArea);
                string category = scheduleManager.GetLPDCategoryBAM();
                double allowableLPD = scheduleManager.GetAllowableLPDBAM();
                double targetLPD = scheduleManager.GetTargetLPDBAM();
                double actualLightingLoad = scheduleManager.GetActualLightingLoad();
                double area = scheduleManager.GetArea();
                double actualLPD = actualLightingLoad / area;
                double reduction = 1 - (actualLPD / allowableLPD);

                using (Transaction trans = new Transaction(m_doc))
                {
                    try
                    {
                        trans.Start("Update Annotation");

                        AnnotationProperties annotation = new AnnotationProperties(annotationType, CalculationTypes.BuildingArea);
                        annotation.ASHRAELPDCategory = category;
                        annotation.ASHRAEAllowableLPD = allowableLPD;
                        annotation.TargetLPD = targetLPD;
                        annotation.ActualLightingLoad = actualLightingLoad;
                        annotation.Area = area;
                        annotation.ActualLPD = actualLPD;
                        annotation.Reduction = reduction;
                        annotation.LPDCalculatedBy = Environment.UserName;

                        trans.Commit();
                    }
                    catch
                    {
                        trans.RollBack();
                    }

                }

                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update annotation family.\n" + ex.Message, "Update Annotation Family", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }
    }

    public class AreaProperties
    {
        private Area areaElement = null;
        private int areaId = -1;
        private string areaName = "";
        private double area = 0;
        private double actualLightingLoad = 0;
        private double lpd = 0;
        private Dictionary<int, LightingProperties> lightingFixtures = new Dictionary<int, LightingProperties>();
        private RoomProperties correlatedRoom = null;

        public Area AreaElement { get { return areaElement; } set { areaElement = value; } }
        public int AreaId { get { return areaId; } set { areaId = value; } }
        public string AreaName { get { return areaName; } set { areaName = value; } }
        public double Area { get { return area; } set { area = value; } }
        public double ActualLightingLoad { get { return actualLightingLoad; } set { actualLightingLoad = value; } }
        public double LPD { get { return lpd; } set { lpd = value; } }
        public Dictionary<int, LightingProperties> LightingFixtures { get { return lightingFixtures; } set { lightingFixtures = value; } }
        public RoomProperties CorrelatedRoom { get { return correlatedRoom; } set { correlatedRoom = value; } }

        public AreaProperties(Area areaObj)
        {
            areaElement = areaObj;
            areaId = areaElement.Id.IntegerValue;
            areaName = areaElement.Name;
            area = areaElement.Area;
        }
    }

    public class LightingProperties
    {
        private Element lightingElement = null;
        private int lightingId = -1;
        private int lightingTypeId = -1;
        private double apparentLoad = 0;

        public Element LightingElement { get { return lightingElement; } set { lightingElement = value; } }
        public int LightingId { get { return lightingId; } set { lightingId = value; } }
        public int LightingTypeId { get { return lightingTypeId; } set { lightingTypeId = value; } }
        public double ApparentLoad { get { return apparentLoad; } set { apparentLoad = value; } }

        public LightingProperties(Element element)
        {
            lightingElement = element;
            lightingId = lightingElement.Id.IntegerValue;
            lightingTypeId = lightingElement.GetTypeId().IntegerValue;
        }

    }

    public class RoomProperties
    {
        private Room m_room = null;
        private int roomId = -1;
        private string roomName = "";
        private string roomNumber = "";

        public Room RoomObj { get { return m_room; } set { m_room = value; } }
        public int RoomId { get { return roomId; } set { roomId = value; } }
        public string RoomName { get { return roomName; } set { roomName = value; } }
        public string RoomNumber { get { return roomNumber; } set { roomNumber = value; } }

        public RoomProperties(Room roomElement)
        {
            m_room = roomElement;
            roomId = m_room.Id.IntegerValue;
            GetRoomInfo();
        }

        private void GetRoomInfo()
        {
            if (null != m_room)
            {
                Parameter param = m_room.get_Parameter(BuiltInParameter.ROOM_NAME);
                if (null != param)
                {
                    roomName = param.AsString();
                }

                param = m_room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
                if (null != param)
                {
                    roomNumber = param.AsString();
                }
            }
        }

        public bool UpdateAreaParameter(Area area)
        {
            bool result = false;
            try
            {
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = area.LookupParameter("RoomName");
#else
                Parameter param = area.get_Parameter("RoomName");
#endif
                
                if (null != param)
                {
                    if (param.StorageType == StorageType.String)
                    {
                        param.Set(roomName);
                    }
                }

#if RELEASE2015 || RELEASE2016 || RELEASE2017
                param = area.LookupParameter("RoomNumber");
#else
                param = area.get_Parameter("RoomNumber");
#endif
                if (null != param)
                {
                    if (param.StorageType == StorageType.String)
                    {
                        param.Set(roomNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }
    }
}
