using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.LPDCalculator.Schedule;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using HOK.Core.Utilities;


namespace HOK.LPDCalculator
{
    public class CalculatorMethods
    {
        private readonly UIApplication m_app;
        private readonly Document m_doc;
        private readonly List<Element> selectedAreas;
        private readonly ModelSelection lightingSelection;
        private readonly FamilySymbol annotationType;
        private readonly Dictionary<int/*areaId*/, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();
        private readonly Dictionary<int/*lighting typeId*/, double/*Apparent Load*/> lightingTypeDictionary = new Dictionary<int, double>();
        private readonly List<string> linkedDocuments = new List<string>();

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

        /// <summary>
        /// Collects all Linked Revit Documents names.
        /// </summary>
        private void CollectRvtLink()
        {
            try
            {
                var revitLinkInstances = new FilteredElementCollector(m_doc)
                    .OfCategory(BuiltInCategory.OST_RvtLinks)
                    .WhereElementIsNotElementType();

                foreach (var instance in revitLinkInstances)
                {
                    var typeId = instance.GetTypeId();
                    var rvtLinkType = (RevitLinkType)m_doc.GetElement(typeId);
                    var docName = rvtLinkType.Name;
                    if (!linkedDocuments.Contains(docName))
                    {
                        linkedDocuments.Add(docName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Collects all lighting types and their load.
        /// </summary>
        private void CollectLightingTypes()
        {
            try
            {
                foreach (Document doc in m_app.Application.Documents)
                {
                    var lightingTypes = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_LightingFixtures)
                        .WhereElementIsElementType();

                    foreach (var etype in lightingTypes)
                    {
                        var param = etype.get_Parameter(BuiltInParameter.RBS_ELEC_APPARENT_LOAD);
                        if (null == param) continue;

                        var apparentLoad = param.AsDouble();
                        if (!lightingTypeDictionary.ContainsKey(etype.Id.IntegerValue))
                        {
                            lightingTypeDictionary.Add(etype.Id.IntegerValue, apparentLoad);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public bool CalculateLPD(ToolStripProgressBar bar)
        {
            bool result;
            try
            {
                if (null != bar)
                {
                    bar.Maximum = selectedAreas.Count;
                    bar.Value = 0;
                }

                using (var group = new TransactionGroup(m_doc, "Update Areas"))
                {
                    group.Start();

                    foreach (var e in selectedAreas)
                    {
                        bar?.PerformStep();

                        var area = e as Area;
                        if (area.Area <= 0) { continue; } //skip empty area elements.

                        Room correlatedRoom;
                        var elementList = GetLightingFixtures(area, out correlatedRoom);
                        if (elementList.Count > 0)
                        {
                            using (var trans = new Transaction(m_doc, "Set LPD"))
                            {
                                trans.Start();
                                try
                                {
                                    var ap = new AreaProperties(area);
                                    if (null != correlatedRoom)
                                    {
                                        var rp = new RoomProperties(correlatedRoom);
                                        rp.UpdateAreaParameter(area);
                                    }

                                    var lightingDictionary = new Dictionary<int, LightingProperties>();
                                    double totalAL = 0;
                                    foreach (var element in elementList)
                                    {
                                        var lp = new LightingProperties(element);
                                        var calculatedParam = element.LookupParameter("Apparent VA Calculated Load");

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

                                    var param = area.LookupParameter("ActualLightingLoad");

                                    if (null != param)
                                    {
                                        param.Set(ap.ActualLightingLoad);
                                    }
                                    param = area.LookupParameter("ActualLPD");

                                    if (null != param)
                                    {
                                        param.Set(ap.LPD);
                                    }

                                    ap.LightingFixtures = lightingDictionary;
                                    areaDictionary.Add(ap.AreaId, ap);
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                    trans.RollBack();
                                }
                            }
                        }
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
            var lightingFixtures = new List<Element>();
            try
            {
                var fromHost = new List<Element>();
                var fromLink = new List<Element>();

                var profiles = new CurveArrArray();
                var outline = GetBoundingBox(area, out profiles);
                var topFace = GetAreaFace(profiles);

                var intersectFilter = new BoundingBoxIntersectsFilter(outline);
                var insideFilter = new BoundingBoxIsInsideFilter(outline);
                var orFilter = new LogicalOrFilter(intersectFilter, insideFilter);

                var collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_LightingFixtures);
                var elementIds = collector.WherePasses(orFilter).ToElementIds().ToList();

                var roomCollector = new FilteredElementCollector(m_doc);
                roomCollector.OfCategory(BuiltInCategory.OST_Rooms);
                var roomsFound = roomCollector.WherePasses(orFilter).ToElements().Cast<Room>().ToList();
                foreach (var room in roomsFound)
                {
                    if (area.LevelId.IntegerValue == room.LevelId.IntegerValue)
                    {
                        correlatedRoom = room; break;
                    }
                }

                var bltParam1 = BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM;
                var pvp1 = new ParameterValueProvider(new ElementId((int)bltParam1));
                var bltParam2 = BuiltInParameter.FAMILY_LEVEL_PARAM;
                var pvp2 = new ParameterValueProvider(new ElementId((int)bltParam2));

                FilterNumericRuleEvaluator fnrv = new FilterNumericEquals();
                var ruleValueId = new ElementId(area.LevelId.IntegerValue);

                FilterRule filterRule1 = new FilterElementIdRule(pvp1, fnrv, ruleValueId);
                FilterRule filterRule2 = new FilterElementIdRule(pvp2, fnrv, ruleValueId);

                var paramFilter1 = new ElementParameterFilter(filterRule1);
                var paramFilter2 = new ElementParameterFilter(filterRule2);
                var paramFilter = new LogicalOrFilter(paramFilter1, paramFilter2);

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
                                    foreach (var room in roomsFound)
                                    {
                                        if (area.LevelId.IntegerValue == room.LevelId.IntegerValue)
                                        {
                                            correlatedRoom = room; break;
                                        }
                                    }
                                }

                                collector = new FilteredElementCollector(doc);
                                collector.OfCategory(BuiltInCategory.OST_LightingFixtures);
                                elementIds = collector.WherePasses(orFilter).ToElementIds().ToList();
                                if (elementIds.Count > 0)
                                {
                                    collector = new FilteredElementCollector(doc, elementIds);
                                    var elements = collector.WherePasses(paramFilter).ToElements().ToList();
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
            var minXYZ = new XYZ(0, 0, 0);
            var maxXYZ = new XYZ(0, 0, 0);
            var outline = new Outline(minXYZ, maxXYZ);
            try
            {
                if (area.GetBoundarySegments(new SpatialElementBoundaryOptions()) != null)
                {
                    double maxLength = 0;
                    foreach (var boundarySegments in area.GetBoundarySegments(new SpatialElementBoundaryOptions()))
                    {
                        if (boundarySegments.Count > 0)
                        {
                            var profile = new CurveArray();
                            double length = 0;
                            foreach (var boundarySegment in boundarySegments)
                            {
                                var curve = boundarySegment.GetCurve();
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
                double maxZ = 0;
                var first = true;

                if (profiles.Size > 0)
                {
                    foreach (CurveArray curveArray in profiles)
                    {
                        foreach (Curve curve in curveArray)
                        {
                            var point = curve.GetEndPoint(0);

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
                var areaSolid = FindAreaSolid(profiles);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return topFace;
        }

        private List<Element> GetPolygonIntersect(Face topFace, List<Element> elements)
        {
            var fixtures = new List<Element>();
            try
            {
                foreach (var element in elements)
                {
                    var locationPt = element.Location as LocationPoint;
                    if (null != locationPt)
                    {
                        var pt = locationPt.Point;
                        var result = topFace.Project(pt);
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
                var curveLoops = new List<CurveLoop>();
                foreach (CurveArray curveArray in profiles)
                {
                    var pointList = new List<XYZ>();
                    var curveLoop = new CurveLoop();

                    foreach (Curve curve in curveArray)
                    {
                        var pointCount = curve.Tessellate().Count;
                        if (pointCount > 2)
                        {
                            var tPoints = curve.Tessellate();
                            tPoints.RemoveAt(tPoints.Count - 1);
                            pointList.AddRange(tPoints);
                        }
                        else if (pointCount == 2)
                        {
                            var pt = curve.GetEndPoint(0);

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
                        for (var i = 0; i < pointList.Count; i++)
                        {
                            if (i == pointList.Count - 1)
                            {
                                newCurve = Line.CreateBound(pointList[i], pointList[0]);
                            }
                            else
                            {
                                newCurve = Line.CreateBound(pointList[i], pointList[i + 1]);
                            }

                            curveLoop.Append(newCurve);
                        }
                        curveLoops.Add(curveLoop);
                    }

                    if (curveLoops.Count > 0)
                    {
                        var direction = new XYZ(0, 0, 1);
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
            var result = false;
            try
            {
                var scheduleManager = new ScheduleDataManager(m_app, CalculationTypes.SpaceBySpace);
                var allowableLightingLoad = scheduleManager.GetAllowableLightingLoad();
                var actualLightingLoad = scheduleManager.GetActualLightingLoad();
                var savings = scheduleManager.GetSavings();
                var area = scheduleManager.GetArea();
                var actualLPD = actualLightingLoad / area;
                var reduction = savings / allowableLightingLoad;

                using (var trans = new Transaction(m_doc))
                {
                    try
                    {
                        trans.Start("Update Annotation");

                        var annotation = new AnnotationProperties(annotationType, CalculationTypes.SpaceBySpace)
                        {
                            TotalAllowableLightingLoad = allowableLightingLoad,
                            TotalActualLightingLoad = actualLightingLoad,
                            TotalSavingsOverage = savings,
                            Area = area,
                            ActualLPD = actualLPD,
                            Reduction = reduction,
                            LPDCalculatedBy = Environment.UserName
                        };

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
            var result = false;
            try
            {
                var scheduleManager = new ScheduleDataManager(m_app, CalculationTypes.BuildingArea);
                var category = scheduleManager.GetLPDCategoryBAM();
                var allowableLPD = scheduleManager.GetAllowableLPDBAM();
                var targetLPD = scheduleManager.GetTargetLPDBAM();
                var actualLightingLoad = scheduleManager.GetActualLightingLoad();
                var area = scheduleManager.GetArea();
                var actualLPD = actualLightingLoad / area;
                var reduction = 1 - (actualLPD / allowableLPD);

                using (var trans = new Transaction(m_doc))
                {
                    try
                    {
                        trans.Start("Update Annotation");

                        var annotation = new AnnotationProperties(annotationType, CalculationTypes.BuildingArea)
                        {
                            ASHRAELPDCategory = category,
                            ASHRAEAllowableLPD = allowableLPD,
                            TargetLPD = targetLPD,
                            ActualLightingLoad = actualLightingLoad,
                            Area = area,
                            ActualLPD = actualLPD,
                            Reduction = reduction,
                            LPDCalculatedBy = Environment.UserName
                        };

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
        public Area AreaElement { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public double Area { get; set; }
        public double ActualLightingLoad { get; set; }
        public double LPD { get; set; }
        public Dictionary<int, LightingProperties> LightingFixtures { get; set; } = new Dictionary<int, LightingProperties>();
        public RoomProperties CorrelatedRoom { get; set; } = null;

        public AreaProperties(Area areaObj)
        {
            AreaElement = areaObj;
            AreaId = AreaElement.Id.IntegerValue;
            AreaName = AreaElement.Name;
            Area = AreaElement.Area;
        }
    }

    public class LightingProperties
    {
        public Element LightingElement { get; set; }
        public int LightingId { get; set; }
        public int LightingTypeId { get; set; }
        public double ApparentLoad { get; set; }

        public LightingProperties(Element element)
        {
            LightingElement = element;
            LightingId = LightingElement.Id.IntegerValue;
            LightingTypeId = LightingElement.GetTypeId().IntegerValue;
        }
    }

    public class RoomProperties
    {
        public Room RoomObj { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string RoomNumber { get; set; }

        public RoomProperties(Room roomElement)
        {
            RoomObj = roomElement;
            RoomId = RoomObj.Id.IntegerValue;
            GetRoomInfo();
        }

        private void GetRoomInfo()
        {
            if (null == RoomObj) return;

            var param = RoomObj.get_Parameter(BuiltInParameter.ROOM_NAME);
            if (param != null)
            {
                RoomName = param.AsString();
            }

            param = RoomObj.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            if (param != null)
            {
                RoomNumber = param.AsString();
            }
        }

        // TODO: Update this to BuiltInParameters?
        public void UpdateAreaParameter(Area area)
        {
            try
            {
                var param = area.LookupParameter("RoomName");

                if (param?.StorageType == StorageType.String)
                {
                    param.Set(RoomName);
                }
                param = area.LookupParameter("RoomNumber");
                if (param?.StorageType == StorageType.String)
                {
                    param.Set(RoomNumber);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
