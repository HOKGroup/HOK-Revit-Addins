using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;

namespace HOK.CeilingHeight
{
    public class CeilingHeightUtil
    {
        private UIApplication m_app;
        private Document m_doc;
        private List<Element> selectedRooms = new List<Element>();
        private Dictionary<Level, double/*elevation*/> levels = new Dictionary<Level, double>();
        private Dictionary<int/*roomId*/, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();
        private List<ViewPlan> viewPlans = new List<ViewPlan>();

        public Dictionary<int, RoomProperties> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }

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
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<Element> levelList = collector.OfClass(typeof(Level)).WhereElementIsNotElementType().ToElements().ToList();
                Dictionary<Level, double> levelDictionary = new Dictionary<Level, double>();
                foreach (Element element in levelList)
                {
                    Level level = element as Level;
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
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewPlan> elementList = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
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
            StringBuilder strBuilder = new StringBuilder();
            foreach (Element element in selectedRooms)
            {
                try
                {
                    Room room = element as Room;
                    Ceiling mainCeiling = null;
                    List<Element> ceilings = FindIntersectsCeiling(room, out mainCeiling);
                    if (ceilings.Count > 0 && null != mainCeiling)
                    {
                        RoomProperties rp = new RoomProperties();
                        rp.RoomId = room.Id.IntegerValue;
                        rp.RoomName = room.Name;
                        rp.RoomObj = room;
                        rp.MainCeiling = mainCeiling;
                        rp.CeilingHeight = GetCeilingHeight(rp.MainCeiling, room);
                        rp.MainCeilingPlan = FindCeilingPlan(rp.MainCeiling);

                        Dictionary<int, CeilingProperties> ceilingProperties = new Dictionary<int, CeilingProperties>();
                        foreach (Element elem in ceilings)
                        {
                            CeilingProperties cp = new CeilingProperties();
                            cp.CeilingId = elem.Id.IntegerValue;
                            cp.CeilingObj = elem;
                            cp.HeightOffsetFromLevel = elem.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
                            cp.CeilingHeight = GetCeilingHeight(elem, room);
                            cp.CeilingType = GetCeilingType(elem);
                            cp.RoomId = room.Id.IntegerValue;

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

                        roomDictionary.Add(rp.RoomId, rp);
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
            List<Element> intersectsCeilings = new List<Element>();
            mainCeiling = null;
            Level originUpperLimit = room.UpperLimit;

            try
            {
                int roomLevelIndex = FindRoomLevelIndex(room);
                int index = roomLevelIndex + 1;
                double maxVol = 0;
                int indexUpperLimit = (index + 3) > levels.Count - 1 ? levels.Count - 1 : index + 3;

                Level level = levels.ElementAt(indexUpperLimit).Key;
                using (Transaction trans = new Transaction(m_doc))
                {
                    trans.Start("Set Upper Limit");
                    room.UpperLimit = level;
                    trans.Commit();
                }
                
                Solid roomSolid = FindRoomSolid(room);
                FilteredElementCollector elementCollector = new FilteredElementCollector(m_doc);
                elementCollector.OfClass(typeof(Ceiling)).WherePasses(new ElementIntersectsSolidFilter(roomSolid)).WhereElementIsNotElementType();
                IList<Element> elementList = elementCollector.ToElements().ToList();

                if (elementList.Count > 0)
                {
#if RELEASE2013
                    var ceilings = from validCeiling in elementList where validCeiling.Level.Id.IntegerValue == room.Level.Id.IntegerValue select validCeiling;
#else
                    var ceilings = from validCeiling in elementList where validCeiling.LevelId.IntegerValue == room.LevelId.IntegerValue select validCeiling;
#endif
                    intersectsCeilings = ceilings.ToList();

                    if (intersectsCeilings.Count > 0)
                    {

                        foreach (Element element in intersectsCeilings)
                        {
                            Solid ceilingSolid = FindCeilingSolid(element);
                            if (null != ceilingSolid)
                            {
                                Solid intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomSolid, ceilingSolid, BooleanOperationsType.Intersect);
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
                using (Transaction transaction = new Transaction(m_doc))
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
                GeometryElement geomElem = room.ClosedShell;
                foreach (GeometryObject geoObj in geomElem)
                {
                    Solid solid = geoObj as Solid;
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
                Options opt = m_app.Application.Create.NewGeometryOptions();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;

                GeometryElement geomElem = ceiling.get_Geometry(opt);
                foreach (GeometryObject obj in geomElem)
                {
                    Solid solid = obj as Solid;
                    if (null != solid)
                    {
                        if (solid.Volume > 0)
                        {
                            ceilingSolid = solid;
                            break;
                        }
                        else
                        {
                            List<CurveLoop> curveLoopList = new List<CurveLoop>();
                            XYZ normal = null;
                            foreach (Face face in solid.Faces)
                            {
                                if (face.EdgeLoops.Size > 0)
                                {
                                    normal = face.ComputeNormal(new UV(0, 0));
                                    foreach (EdgeArray edgeArray in face.EdgeLoops)
                                    {
                                        CurveLoop curveLoop = new CurveLoop();
                                        foreach (Edge edge in edgeArray)
                                        {
                                            curveLoop.Append(edge.AsCurve());
                                        }
                                        curveLoopList.Add(curveLoop);
                                    }
                                }
                            }
                            Solid extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoopList, normal, 1);
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
            int index = 0;
            try
            {
#if RELEASE2013
                Level roomLevel = room.Level;
#else
                Level roomLevel = m_doc.GetElement(room.LevelId) as Level;
#endif

                foreach (Level level in levels.Keys)
                {
                    if (level.Id.IntegerValue == roomLevel.Id.IntegerValue)
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
            string ceilingPlan = "";
            try
            {
                foreach (ViewPlan view in viewPlans)
                {
                    if (!view.IsTemplate)
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(m_doc, view.Id);
                        List<ElementId> elementIds = collector.OfClass(typeof(Ceiling)).ToElementIds().ToList();
                        var ceilingIds = from ceilingId in elementIds where ceilingId.IntegerValue == ceiling.Id.IntegerValue select ceilingId;
                        List<ElementId> results = ceilingIds.ToList();
                        if (results.Count > 0)
                        {
                            ceilingPlan = view.ViewName;
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
#if RELEASE2013
                int ceilingLevelId = ceiling.Level.Id.IntegerValue;
                int roomLevelId = room.Level.Id.IntegerValue;
#else
                int ceilingLevelId = ceiling.LevelId.IntegerValue;
                int roomLevelId = room.LevelId.IntegerValue;
#endif
                
                double roomBaseOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
                double ceilingHeightOffset = ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).AsDouble();
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
            string typeMark = "";
            try
            {
                ElementId typeId = ceiling.GetTypeId();
                ElementType ceilingType = m_doc.GetElement(typeId) as ElementType;

                Parameter param = ceilingType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
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
            using (Transaction trans = new Transaction(m_doc))
            {
                try
                {
                    string paramName = "Ceiling Height";
#if RELEASE2015||RELEASE2016 || RELEASE2017
                    Parameter parameter = room.LookupParameter(paramName);
#else
                    Parameter parameter = room.get_Parameter(paramName);
#endif

                    if (null != parameter)
                    {
                        trans.Start("Set Ceiling Height");
                        parameter.Set(rp.CeilingHeight);
                        trans.Commit();
                    }
                    else if (CreateSharedParameter(paramName, ParameterType.Length, BuiltInParameterGroup.PG_GEOMETRY))
                    {
#if RELEASE2015||RELEASE2016 || RELEASE2017
                        parameter = room.LookupParameter(paramName);
#else
                        parameter = room.get_Parameter(paramName);
#endif

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
            using (Transaction trans = new Transaction(m_doc))
            {
                try
                {
                    string paramName = "Secondary Ceiling Heights";
                    List<double> ceilingHeightList = new List<double>();

#if RELEASE2015||RELEASE2016 || RELEASE2017
                    Parameter tempParam = room.LookupParameter("Ceiling Height");
#else
                    Parameter tempParam = room.get_Parameter("Ceiling Height");
#endif

                    string ceilingHeights = "";
                    foreach (int ceilingId in rp.IntersectCeilings.Keys)
                    {
                        CeilingProperties cp = rp.IntersectCeilings[ceilingId];

                        if (null != tempParam && !ceilingHeightList.Contains(cp.CeilingHeight) && cp.CeilingHeight != rp.CeilingHeight)
                        {
                            ceilingHeightList.Add(cp.CeilingHeight);

                            trans.Start("Collect Ceiling Heights");
                            tempParam.Set(cp.CeilingHeight);
                            ceilingHeights += tempParam.AsValueString() + "; ";
                            trans.RollBack();
                        }
                    }

#if RELEASE2015||RELEASE2016 || RELEASE2017
                    Parameter parameter = room.LookupParameter(paramName);
#else
                    Parameter parameter = room.get_Parameter(paramName);
#endif

                    if (null != parameter)
                    {
                        trans.Start("Set Secondary Ceiling Height");
                        parameter.Set(ceilingHeights);
                        trans.Commit();
                    }
                    else if (CreateSharedParameter(paramName, ParameterType.Text, BuiltInParameterGroup.PG_GEOMETRY))
                    {
#if RELEASE2015||RELEASE2016 || RELEASE2017
                        parameter = room.LookupParameter(paramName);
#else
                        parameter = room.get_Parameter(paramName);
#endif

                        if (null != parameter)
                        {
                            trans.Start("Set Ceiling Height");
                            parameter.Set(ceilingHeights);
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

        private void SetCeilingPlan(Element room, RoomProperties rp)
        {
            using (Transaction trans = new Transaction(m_doc))
            {
                try
                {
                    string paramName = "Ceiling Plan";

#if RELEASE2015||RELEASE2016 || RELEASE2017
                    Parameter parameter = room.LookupParameter(paramName);
#else
                    Parameter parameter = room.get_Parameter(paramName);
#endif
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
                    else if (CreateSharedParameter(paramName, ParameterType.Text, BuiltInParameterGroup.INVALID))
                    {
#if RELEASE2015||RELEASE2016 || RELEASE2017
                        parameter = room.LookupParameter(paramName);
#else
                        parameter = room.get_Parameter(paramName);
#endif
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
            using (Transaction trans = new Transaction(m_doc))
            {
                try
                {
                    string paramName = "Ceiling Type";
                    List<string> ceilingTypeList = new List<string>();

                    string ceilingTypes = "";
                    foreach (int ceilingId in rp.IntersectCeilings.Keys)
                    {
                        CeilingProperties cp = rp.IntersectCeilings[ceilingId];

                        if (!ceilingTypeList.Contains(cp.CeilingType))
                        {
                            ceilingTypeList.Add(cp.CeilingType);
                            ceilingTypes += cp.CeilingType + ", ";
                        }
                    }

#if RELEASE2015||RELEASE2016 || RELEASE2017
                    Parameter parameter = room.LookupParameter(paramName);
#else
                    Parameter parameter = room.get_Parameter(paramName);
#endif
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
                    else if (CreateSharedParameter(paramName, ParameterType.Text, BuiltInParameterGroup.INVALID))
                    {
#if RELEASE2015||RELEASE2016 || RELEASE2017
                        parameter = room.LookupParameter(paramName);
#else
                        parameter = room.get_Parameter(paramName);
#endif
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

        private bool CreateSharedParameter(string paramName, ParameterType paramType, BuiltInParameterGroup pramGroup)
        {
            bool created = false;
            using (Transaction trans = new Transaction(m_doc))
            {
                try
                {
                    DefinitionFile definitionFile = m_app.Application.OpenSharedParameterFile();
                    if (null != definitionFile)
                    {
                        trans.Start("Create a shared parameter");
                        DefinitionGroups groups = definitionFile.Groups;
                        DefinitionGroup group = groups.get_Item("HOK Tools");
                        if (null == group)
                        {
                            group = groups.Create("HOK Tools");
                        }
                        Definition definition = group.Definitions.get_Item(paramName);
                        if (definition == null)
                        {
#if RELEASE2015
                            definition = group.Definitions.Create(paramName, paramType);
#else
                            var options = new ExternalDefinitionCreationOptions(paramName, paramType);
                            definition = group.Definitions.Create(options);
#endif
                        }

                        CategorySet categorySet = m_app.Application.Create.NewCategorySet();
                        Category roomCategory = m_doc.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms);
                        categorySet.Insert(roomCategory);

                        InstanceBinding instanceBinding = m_app.Application.Create.NewInstanceBinding(categorySet);
                        BindingMap bindingMap = m_doc.ParameterBindings;
                        bool instanceBindOK = bindingMap.Insert(definition, instanceBinding, pramGroup);
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
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public Element RoomObj { get; set; }
        public Element MainCeiling { get; set; }
        public string MainCeilingPlan { get; set; }
        public Dictionary<int/*ceilingId*/, CeilingProperties> IntersectCeilings { get; set; }
        public double CeilingHeight { get; set; }
    }

    public class CeilingProperties
    {
        public int CeilingId { get; set; }
        public Element CeilingObj { get; set; }
        public double HeightOffsetFromLevel { get; set; }
        public double CeilingHeight { get; set; }
        public string CeilingPlan { get; set; }
        public string CeilingType { get; set; }
        public int RoomId { get; set; }
    }
}
