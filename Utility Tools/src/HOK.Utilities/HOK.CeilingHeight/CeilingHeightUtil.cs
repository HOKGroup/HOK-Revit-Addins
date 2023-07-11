using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.CeilingHeight
{
    public class CeilingHeightUtil
    {
        private UIApplication m_app;
        private Document m_doc;
        private List<Element> selectedRooms = new List<Element>();
        private Dictionary<Level, double/*elevation*/> levels = new Dictionary<Level, double>();
        private List<ViewPlan> viewPlans = new List<ViewPlan>();
        public Dictionary<long, RoomProperties> RoomDictionary { get; set; } = new Dictionary<long, RoomProperties>();

        public CeilingHeightUtil(UIApplication uiapp, List<Element> rooms)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            selectedRooms = rooms;
            FindLevels();
            CollectCeilingPlans();
        }

        private void FindLevels()
        {
            try
            {
                var collector = new FilteredElementCollector(m_doc);
                IList<Element> levelList = collector.OfClass(typeof(Level)).WhereElementIsNotElementType().ToElements().ToList();
                var levelDictionary = new Dictionary<Level, double>();
                foreach (var element in levelList)
                {
                    var level = element as Level;
                    if (!levelDictionary.ContainsKey(level))
                    {
                        levelDictionary.Add(level, level.Elevation);
                    }
                }

                var sortedDict = (from entry in levelDictionary orderby entry.Value ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                levels = sortedDict;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find levels.\n"+ex.Message, "CeilingHeightUtil : FindLevels", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectCeilingPlans()
        {
            try
            {
                var collector = new FilteredElementCollector(m_doc);
                var elementList = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
                var planViews = from view in elementList where view.ViewType == ViewType.CeilingPlan select view;
                viewPlans = planViews.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the ceiling plans.\n" + ex.Message, "CeilingHeightUtil : FindCeilingPlans", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void MeasureHeight()
        {
            var strBuilder = new StringBuilder();
            foreach (var element in selectedRooms)
            {
                try
                {
                    var room = element as Room;
                    Ceiling mainCeiling;
                    var ceilings = FindIntersectsCeiling(room, out mainCeiling);
                    if (ceilings.Count > 0 && null != mainCeiling)
                    {
                        var rp = new RoomProperties
                        {
                            RoomId = GetElementIdValue(room.Id),
                            RoomName = room.Name,
                            RoomObj = room,
                            MainCeiling = mainCeiling
                        };
                        rp.CeilingHeight = GetCeilingHeight(rp.MainCeiling, room);
                        rp.MainCeilingPlan = FindCeilingPlan(rp.MainCeiling);

                        var ceilingProperties = new Dictionary<long, CeilingProperties>();
                        foreach (var elem in ceilings)
                        {
                            var cp = new CeilingProperties();
                            cp.CeilingId = GetElementIdValue(elem.Id);
                            cp.CeilingObj = elem;
                            cp.HeightOffsetFromLevel = elem.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
                            cp.CeilingHeight = GetCeilingHeight(elem, room);
                            cp.CeilingType = GetCeilingType(elem);
                            cp.RoomId = GetElementIdValue(room.Id);

                            if (!ceilingProperties.ContainsKey(cp.CeilingId))
                            {
                                ceilingProperties.Add(cp.CeilingId, cp);
                            }
                        }

                        var sortedDict = (from entry in ceilingProperties orderby entry.Value.CeilingHeight ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                        rp.IntersectCeilings = sortedDict;

                        SetCeilingHeight(room, rp);
                        SetSecondaryCeilingHeight(room, rp);
                        SetCeilingPlan(room, rp);
                        SetCeilingType(room, rp);

                        RoomDictionary.Add(rp.RoomId, rp);
                    }
                }
                catch (Exception ex)
                {
                    strBuilder.AppendLine(element.Name + " : " + ex.Message);
                    //MessageBox.Show("Could not measure the ceiling height.\n" + ex.Message, "CeilingHeightUtil : MeasureHeight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
            }
            if (strBuilder.Length > 0)
            {
                MessageBox.Show("Following rooms are failed to measure the height of celings.\n"+strBuilder.ToString(), "Measure Height Results", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        private List<Element> FindIntersectsCeiling(Room room, out Ceiling mainCeiling)
        {
            var intersectsCeilings = new List<Element>();
            mainCeiling = null;
            var originUpperLimit = room.UpperLimit;

            try
            {
                var roomLevelIndex = FindRoomLevelIndex(room);
                var index = roomLevelIndex + 1;
                double maxVol = 0;
                var indexUpperLimit = (index + 3) > levels.Count - 1 ? levels.Count - 1 : index + 3;

                var level = levels.ElementAt(indexUpperLimit).Key;
                using (var trans = new Transaction(m_doc))
                {
                    trans.Start("Set Upper Limit");
                    room.UpperLimit = level;
                    trans.Commit();
                }
                
                var roomSolid = FindRoomSolid(room);
                var elementCollector = new FilteredElementCollector(m_doc);
                elementCollector.OfClass(typeof(Ceiling)).WherePasses(new ElementIntersectsSolidFilter(roomSolid)).WhereElementIsNotElementType();
                IList<Element> elementList = elementCollector.ToElements().ToList();

                if (elementList.Count > 0)
                {
                    var ceilings = from validCeiling in elementList where GetElementIdValue(validCeiling.LevelId) == GetElementIdValue(room.LevelId) select validCeiling;
                    intersectsCeilings = ceilings.ToList();

                    if (intersectsCeilings.Count > 0)
                    {

                        foreach (var element in intersectsCeilings)
                        {
                            var ceilingSolid = FindCeilingSolid(element);
                            if (null != ceilingSolid)
                            {
                                var intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomSolid, ceilingSolid, BooleanOperationsType.Intersect);
                                if (null != intersectSolid)
                                {
                                    if (maxVol < intersectSolid.Volume)
                                    {
                                        maxVol = intersectSolid.Volume;
                                        mainCeiling = element as Ceiling;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the intersecting ceiling to the room space.\n" + ex.Message, "CeilingHeightUtil : FindIntersectsCeiling", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                using (var transaction = new Transaction(m_doc))
                {
                    transaction.Start("Set Original Upper Limit");
                    room.UpperLimit = originUpperLimit;
                    transaction.Commit();
                }
            }
            return intersectsCeilings;
        }

        private Solid FindRoomSolid(Room room)
        {
            Solid roomSolid = null;
            try
            {
                var geomElem = room.ClosedShell;
                foreach (var geoObj in geomElem)
                {
                    var solid = geoObj as Solid;
                    if (null != solid)
                    {
                        if (solid.Volume > 0)
                        {
                            roomSolid = solid;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the room solid.\n" + ex.Message, "FindRoomSolid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return roomSolid;
        }

        private Solid FindCeilingSolid(Element ceiling)
        {
            Solid ceilingSolid = null;
            try
            {
                var opt = m_app.Application.Create.NewGeometryOptions();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;

                var geomElem = ceiling.get_Geometry(opt);
                foreach (var obj in geomElem)
                {
                    var solid = obj as Solid;
                    if (null != solid)
                    {
                        if (solid.Volume > 0)
                        {
                            ceilingSolid = solid;
                            break;
                        }
                        else
                        {
                            var curveLoopList = new List<CurveLoop>();
                            XYZ normal = null;
                            foreach (Face face in solid.Faces)
                            {
                                if (face.EdgeLoops.Size > 0)
                                {
                                    normal = face.ComputeNormal(new UV(0, 0));
                                    foreach (EdgeArray edgeArray in face.EdgeLoops)
                                    {
                                        var curveLoop = new CurveLoop();
                                        foreach (Edge edge in edgeArray)
                                        {
                                            curveLoop.Append(edge.AsCurve());
                                        }
                                        curveLoopList.Add(curveLoop);
                                    }
                                }
                            }
                            var extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoopList, normal, 1);
                            if (extrusion.Volume > 0)
                            {
                                ceilingSolid = extrusion;
                                break;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the solid of the ceiling.\n" + ex.Message, "FindCeilingSolid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return ceilingSolid;
        }

        private int FindRoomLevelIndex(Room room)
        {
            var index = 0;
            try
            {
                var roomLevel = m_doc.GetElement(room.LevelId) as Level;
                foreach (var level in levels.Keys)
                {
                    if (GetElementIdValue(level.Id) == GetElementIdValue(roomLevel.Id))
                    {
                        break;
                    }
                    index++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the index of the room level.\n" + ex.Message, "FindRoomLevelIndex", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return index;
        }

        private string FindCeilingPlan(Element ceiling)
        {
            var ceilingPlan = "";
            try
            {
                foreach (var view in viewPlans)
                {
                    if (!view.IsTemplate)
                    {
                        var collector = new FilteredElementCollector(m_doc, view.Id);
                        var elementIds = collector.OfClass(typeof(Ceiling)).ToElementIds().ToList();
                        var ceilingIds = from ceilingId in elementIds where GetElementIdValue(ceilingId) == GetElementIdValue(ceiling.Id) select ceilingId;
                        var results = ceilingIds.ToList();
                        if (results.Count > 0)
                        {
                            ceilingPlan = view.Name;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not find the ceiling plan.\n" + ex.Message, "FindCeilingPlan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return ceilingPlan;   
        }

        private double GetCeilingHeight(Element ceiling, Room room)
        {
            double height = 0;
            try
            {
                var ceilingLevelId = GetElementIdValue(ceiling.LevelId);
                var roomLevelId = GetElementIdValue(room.LevelId);
                var roomBaseOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
                var ceilingHeightOffset = ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
                height = ceilingHeightOffset - roomBaseOffset;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not calculate the ceiling height.\n" + ex.Message, "CeilingHeightUtil : GetCeilingHeight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return height;
        }

        private string GetCeilingType(Element ceiling)
        {
            var typeMark = "";
            try
            {
                var typeId = ceiling.GetTypeId();
                var ceilingType = m_doc.GetElement(typeId) as ElementType;

                var param = ceilingType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
                if (null != param)
                {
                    typeMark = param.AsString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not calculate the ceiling height.\n" + ex.Message, "CeilingHeightUtil : GetCeilingHeight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return typeMark;
        }

        private void SetCeilingHeight(Element room, RoomProperties rp)
        {
            using (var trans = new Transaction(m_doc))
            {
                try
                {
                    var paramName = "Ceiling Height";
                    var parameter = room.LookupParameter(paramName);
                    if (null != parameter)
                    {
                        trans.Start("Set Ceiling Height");
                        parameter.Set(rp.CeilingHeight);
                        trans.Commit();
                    }
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                    else if (CreateSharedParameter(paramName, SpecTypeId.Length, BuiltInParameterGroup.PG_GEOMETRY))
                    {
#else
                    else if (CreateSharedParameter(paramName, ParameterType.Length, BuiltInParameterGroup.PG_GEOMETRY))
                    {
#endif
                        parameter = room.LookupParameter(paramName);
                        if (null != parameter)
                        {
                            trans.Start("Set Ceiling Height2");
                            parameter.Set(rp.CeilingHeight);
                            trans.Commit();
                        }
                    }
                    else
                    {
                        trans.RollBack();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not set a value of the Ceiling Height parametert.\n" + ex.Message, "CeilingHeightUtil : SetCeilingHeight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void SetSecondaryCeilingHeight(Element room, RoomProperties rp)
        {
            using (var trans = new Transaction(m_doc))
            {
                try
                {
                    var paramName = "Secondary Ceiling Heights";
                    var ceilingHeightList = new List<double>();
                    var tempParam = room.LookupParameter("Ceiling Height");
                    var ceilingHeights = "";
                    foreach (var ceilingId in rp.IntersectCeilings.Keys)
                    {
                        var cp = rp.IntersectCeilings[ceilingId];

                        if (null != tempParam && !ceilingHeightList.Contains(cp.CeilingHeight) && cp.CeilingHeight != rp.CeilingHeight)
                        {
                            ceilingHeightList.Add(cp.CeilingHeight);

                            trans.Start("Collect Ceiling Heights");
                            tempParam.Set(cp.CeilingHeight);
                            ceilingHeights += tempParam.AsValueString() + "; ";
                            trans.RollBack();
                        }
                    }

                    var parameter = room.LookupParameter(paramName);
                    if (null != parameter)
                    {
                        trans.Start("Set Secondary Ceiling Height");
                        parameter.Set(ceilingHeights);
                        trans.Commit();
                    }
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                    else if (CreateSharedParameter(paramName, SpecTypeId.String.Text, BuiltInParameterGroup.PG_GEOMETRY))
                    {
#else
                    else if (CreateSharedParameter(paramName, ParameterType.Text, BuiltInParameterGroup.PG_GEOMETRY))
                    {
#endif
                        parameter = room.LookupParameter(paramName);
                        if (null != parameter)
                        {
                            trans.Start("Set Ceiling Height");
                            parameter.Set(ceilingHeights);
                            trans.Commit();
                        }
                    }
                    else
                    {
                        if (trans.HasStarted())
                        {
                            trans.RollBack();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not set a value of the Ceiling Height parametert.\n" + ex.Message, "CeilingHeightUtil : SetCeilingHeight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void SetCeilingPlan(Element room, RoomProperties rp)
        {
            using (var trans = new Transaction(m_doc))
            {
                try
                {
                    var paramName = "Ceiling Plan";
                    var parameter = room.LookupParameter(paramName);
                    if (null != parameter)
                    {
                        try
                        {
                            trans.Start("Set Secondary Ceiling Height");
                            parameter.Set(rp.MainCeilingPlan);
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Commit();
                        }
                    }
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                    else if (CreateSharedParameter(paramName, SpecTypeId.String.Text, BuiltInParameterGroup.INVALID))
                    {
#else
                    else if (CreateSharedParameter(paramName, ParameterType.Text, BuiltInParameterGroup.INVALID))
                    {
#endif
                        parameter = room.LookupParameter(paramName);
                        if (null != parameter)
                        {
                            try
                            {
                                trans.Start("Set Ceiling Plan");
                                parameter.Set(rp.MainCeilingPlan);
                                trans.Commit();
                            }
                            catch
                            {
                                trans.RollBack();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not set a value of the Ceiling Height parametert.\n" + ex.Message, "CeilingHeightUtil : SetCeilingHeight", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void SetCeilingType(Element room, RoomProperties rp)
        {
            using (var trans = new Transaction(m_doc))
            {
                try
                {
                    var paramName = "Ceiling Type";
                    var ceilingTypeList = new List<string>();

                    var ceilingTypes = "";
                    foreach (var ceilingId in rp.IntersectCeilings.Keys)
                    {
                        var cp = rp.IntersectCeilings[ceilingId];

                        if (!ceilingTypeList.Contains(cp.CeilingType))
                        {
                            ceilingTypeList.Add(cp.CeilingType);
                            ceilingTypes += cp.CeilingType + ", ";
                        }
                    }
                    var parameter = room.LookupParameter(paramName);
                    if (null != parameter)
                    {
                        try
                        {
                            trans.Start("Set Ceiling Types");
                            parameter.Set(ceilingTypes);
                            trans.Commit();
                        }
                        catch
                        {
                            trans.RollBack();
                        }
                    }
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                    else if (CreateSharedParameter(paramName, SpecTypeId.String.Text, BuiltInParameterGroup.INVALID))
                    {
#else
                    else if (CreateSharedParameter(paramName, ParameterType.Text, BuiltInParameterGroup.INVALID))
                    {
#endif
                        parameter = room.LookupParameter(paramName);
                        if (null != parameter)
                        {
                            try
                            {
                                trans.Start("Set Ceiling Types");
                                parameter.Set(ceilingTypes);
                                trans.Commit();
                            }
                            catch
                            {
                                trans.RollBack();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not set a value of the Ceiling Materials.\n" + ex.Message, "CeilingHeightUtil : Set Ceiling Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

#if RELEASE2022 || RELEASE2023 || RELEASE2024
        private bool CreateSharedParameter(string paramName, ForgeTypeId paramType, BuiltInParameterGroup pramGroup)
        {
#else
        private bool CreateSharedParameter(string paramName, ParameterType paramType, BuiltInParameterGroup pramGroup)
        {
#endif
            var created = false;
            using (var trans = new Transaction(m_doc))
            {
                try
                {
                    var definitionFile = m_app.Application.OpenSharedParameterFile();
                    if (null != definitionFile)
                    {
                        trans.Start("Create a shared parameter");
                        var groups = definitionFile.Groups;
                        var group = groups.get_Item("HOK Tools");
                        if (null == group)
                        {
                            group = groups.Create("HOK Tools");
                        }
                        var definition = group.Definitions.get_Item(paramName);
                        if (definition == null)
                        {
                            var options = new ExternalDefinitionCreationOptions(paramName, paramType);
                            definition = group.Definitions.Create(options);
                        }

                        var categorySet = m_app.Application.Create.NewCategorySet();
                        var roomCategory = m_doc.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms);
                        categorySet.Insert(roomCategory);

                        var instanceBinding = m_app.Application.Create.NewInstanceBinding(categorySet);
                        var bindingMap = m_doc.ParameterBindings;
                        var instanceBindOK = bindingMap.Insert(definition, instanceBinding, pramGroup);
                        trans.Commit();
                        created = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not create the Ceiling Height parametert." + paramName + "\n" + ex.Message, "CeilingHeightUtil : CreateSharedParameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                    created = false;
                }
            }
            return created;
        }

    }

    public class RoomProperties
    {
        public long RoomId { get; set; }
        public string RoomName { get; set; }
        public Element RoomObj { get; set; }
        public Element MainCeiling { get; set; }
        public string MainCeilingPlan { get; set; }
        public Dictionary<long/*ceilingId*/, CeilingProperties> IntersectCeilings { get; set; }
        public double CeilingHeight { get; set; }
    }

    public class CeilingProperties
    {
        public long CeilingId { get; set; }
        public Element CeilingObj { get; set; }
        public double HeightOffsetFromLevel { get; set; }
        public double CeilingHeight { get; set; }
        public string CeilingPlan { get; set; }
        public string CeilingType { get; set; }
        public long RoomId { get; set; }
    }
}
