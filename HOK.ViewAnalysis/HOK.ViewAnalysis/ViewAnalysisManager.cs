﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Windows.Threading;
using static HOK.Core.Utilities.ElementIdExtension;


namespace HOK.ViewAnalysis
{
    public class ViewAnalysisManager
    {
        private UIApplication m_app;
        private Document m_doc;
        private View3D m_view;
        private List<Room> selectedRooms = new List<Room>();
        private AnalysisSettings analysisSettings = new AnalysisSettings();
        private AnalysisDataCollection dataCollection = new AnalysisDataCollection();
        private List<LinkElementId> exteriorElementIds = new List<LinkElementId>();
        private SpatialFieldManager m_sfm = null;
        private int resultIndex = -1;
        private bool includeLinkedModel = false;
        private bool overwriteData = false;

        private ElementId occupiedParamId = ElementId.InvalidElementId;
        private Dictionary<long/*roomId*/, RoomData> roomDictionary = new Dictionary<long, RoomData>();
        private List<ElementFilter> categoryFilters = new List<ElementFilter>();
        private Dictionary<long, LinkedInstanceData> linkedInstances = new Dictionary<long, LinkedInstanceData>();
        private double offsetHeight = 3.5; //42 inches above from floor
        private double epsilon = 0;
        private int minTransparency = 20;

        public Dictionary<long, RoomData> RoomDictionary { get { return roomDictionary; } set { roomDictionary = value; } }

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

        public ViewAnalysisManager(UIApplication uiapp, List<Room> rooms, AnalysisSettings settings, AnalysisDataCollection analysisDataCollection)
        {
            try
            {
                m_app = uiapp;
                m_doc = m_app.ActiveUIDocument.Document;
                selectedRooms = rooms;
                analysisSettings = settings;
                overwriteData = analysisSettings.OverwriteData;
                dataCollection = analysisDataCollection;
                epsilon = m_app.Application.ShortCurveTolerance;

                categoryFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Walls)); //intercepting elements.
                categoryFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Windows)); //passing elements
                categoryFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_CurtainWallPanels)); //passing elements
                categoryFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Doors)); //passing elements
                categoryFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns)); //intercepting elements
                if (includeLinkedModel) { categoryFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks)); } //find elements in link

                m_view = FindDefault3DView();
                linkedInstances = GetLinkedInstancesInfo();

                if (analysisSettings.ExteriorWallByParameter)
                {
                    exteriorElementIds = BuildingEnvelopUtil.FindExteriorWallsByParameter(m_doc, linkedInstances, includeLinkedModel);
                }
                else
                {
                    exteriorElementIds = BuildingEnvelopUtil.FindExteriorWallsByAnalyzer(m_doc, m_view, selectedRooms.First(), includeLinkedModel);
                    bool paramResult = BuildingEnvelopUtil.SetExteriorWallParameter(m_doc, exteriorElementIds);
                }

                if (exteriorElementIds.Count > 0)
                {
                    roomDictionary = GetRoomData(selectedRooms);
                    if (roomDictionary.Count > 0)
                    {
                        m_sfm = SetSpatialFieldManager(m_doc.ActiveView, out resultIndex);
                    }
                }
                else
                {
                    MessageBox.Show("Please select exterior walls and set LEED_IsExteriorWall parameter true.\n", "Exterior Walls Not Defined", MessageBoxButton.OK, MessageBoxImage.Warning);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run View Analysis.\n" + ex.Message, "View Analysis Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private Dictionary<long, LinkedInstanceData> GetLinkedInstancesInfo()
        {
            Dictionary<long, LinkedInstanceData> linkedInstances = new Dictionary<long, LinkedInstanceData>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<RevitLinkInstance> revitLinkInstances = collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();

                foreach (RevitLinkInstance instance in revitLinkInstances)
                {
                    if (null != instance.Document)
                    {
                        LinkedInstanceData lid = new LinkedInstanceData(instance);
                        if (!linkedInstances.ContainsKey(lid.InstanceId))
                        {
                            linkedInstances.Add(lid.InstanceId, lid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information of linked instances.\n" + ex.Message, "Find Linked Instances", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return linkedInstances;
        }

        private View3D FindDefault3DView()
        {
            View3D view3d = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<View3D> views = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
#if RELEASE2018
                var linq = from view in views where view.IsTemplate == false && view.ViewName == "{3D}" select view;
#else
                var linq = from view in views where view.IsTemplate == false && view.Name == "{3D}" select view;
#endif
                if (linq.Count() > 0)
                {
                    view3d = linq.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find default 3d views.\n" + ex.Message, "Find Default 3D Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return view3d;
        }

        private SpatialFieldManager SetSpatialFieldManager(Autodesk.Revit.DB.View view, out int index)
        {
            SpatialFieldManager sfm = null;
            index = -1;
            try
            {
                sfm = SpatialFieldManager.GetSpatialFieldManager(view);

                using (Transaction trans = new Transaction(m_doc))
                {
                    trans.Start("Create Spatial Manager");
                    try
                    {
                        if (null == sfm)
                        {
                            sfm = SpatialFieldManager.CreateSpatialFieldManager(view, 1);
                            List<string> names = new List<string>();
                            List<string> descriptions = new List<string>();
                            names.Add("Visibility Index");
                            descriptions.Add("0: area with no views, 1: area with views");
                            sfm.SetMeasurementNames(names);
                            sfm.SetMeasurementDescriptions(descriptions);
                        }

                        IList<int> resultIndices = sfm.GetRegisteredResults();

                        foreach (int i in resultIndices)
                        {
                            AnalysisResultSchema resultSchema = sfm.GetResultSchema(i);
                            if (resultSchema.Name == "View Analysis")
                            {
                                index = i;
                            }
                        }
                        if (index == -1)
                        {
                            AnalysisResultSchema resultSchema = new AnalysisResultSchema("View Analysis", "Calculating area with views");
                            index = sfm.RegisterResult(resultSchema);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        trans.RollBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set Spatial Field Manager.\n" + ex.Message, "Set Spatial Field Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return sfm;
        }

        private Dictionary<long, RoomData> GetRoomData(List<Room> rooms)
        {
            Dictionary<long, RoomData> dictionary = new Dictionary<long, RoomData>();
            try
            {
                foreach (Room room in rooms)
                {
                    Parameter param = room.LookupParameter(LEEDParameters.LEED_NonRegularyOccupied.ToString());
                    if (null != param)
                    {
                        int paramValue = param.AsInteger();
                        if (paramValue == 1) { continue; } //non-regulary occupied
                    }

                    if (room.Area > 0)
                    {
                        RoomData roomData = new RoomData(room, m_doc);
                        int index = dataCollection.AnalysisDataList.FindIndex(o => o.RoomId == roomData.RoomId);
                        if (index == -1 || overwriteData)
                        {
                            roomData.RoomFace = roomData.GetRoomFace();
                            roomData.BoundarySegments = roomData.CollectSegmentData(exteriorElementIds, includeLinkedModel);
                        }

                        if (!dictionary.ContainsKey(roomData.RoomId))
                        {
                            dictionary.Add(roomData.RoomId, roomData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect room  data.\n" + ex.Message, "Get Room Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return dictionary;
        }

        public bool RunViewAnalysis(ProgressBar progressBar, TextBlock statusLable)
        {
            bool result = true;
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Run View Analysis");
                try
                {
                    UpdateLableDelegate updateLabelDelegate = new UpdateLableDelegate(statusLable.SetValue);

                    List<long> keys = roomDictionary.Keys.ToList();
                    int finishedRoom = 0;
                    foreach (int roomId in keys)
                    {
                        if (AbortFlag.GetAbortFlag()) { return false; }

                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Find Visibility");
                            try
                            {
                                RoomData rData = roomDictionary[roomId];
                                string roomInfo = rData.RoomObj.Name + " (" + finishedRoom + " of " + keys.Count + ")";
                                Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, roomInfo });
                                progressBar.Visibility = System.Windows.Visibility.Visible;

                                RoomData updatedData = null;
                                int index = dataCollection.AnalysisDataList.FindIndex(o => o.RoomId == rData.RoomId);
                                if (index == -1 || overwriteData)
                                {
                                    updatedData = FindVisibility(rData, progressBar);
                                }
                                else
                                {
                                    AnalysisData aData = dataCollection.AnalysisDataList[index];
                                    updatedData = FindVisibilityByPointData(rData, aData, progressBar);
                                }

                                if (null != updatedData)
                                {
                                    roomDictionary.Remove(roomId);
                                    roomDictionary.Add(roomId, updatedData);
                                }

                                finishedRoom++;
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                result = false;
                                string message = ex.Message;
                            }
                        }
                    }

                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    result = false;
                    MessageBox.Show("Failed to run view analysis.\n" + ex.Message, "Run View Analysis", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tg.RollBack();
                }
            }
            return result;
        }

        private RoomData FindVisibilityByPointData(RoomData rd, AnalysisData aData, ProgressBar progressBar)
        {
            RoomData updatedData = new RoomData(rd);
            try
            {
                updatedData.RoomFace = FMEDataUtil.CreateFacebyFMEData(aData.RoomFace);
                if (null != updatedData.RoomFace)
                {
                    updatedData.PointDataList = FMEDataUtil.ConvertToPointDataList(updatedData.RoomFace, aData.PointValues);

                    IList<UV> uvPoints = new List<UV>();
                    IList<ValueAtPoint> valList = new List<ValueAtPoint>();

                    progressBar.Value = 0;
                    progressBar.Minimum = 0;
                    progressBar.Maximum = updatedData.PointDataList.Count;
                    UpdateProgressDelegate updateProgressDelegate = new UpdateProgressDelegate(progressBar.SetValue);

                    int visibleCount = 0;
                    double progressValue = 0;
                    foreach (PointData ptData in updatedData.PointDataList)
                    {
                        if (AbortFlag.GetAbortFlag()) { return updatedData; }
                        Dispatcher.CurrentDispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                        if (null != ptData.UVPoint && null != ptData.ValueAtPoint)
                        {
                            uvPoints.Add(ptData.UVPoint);
                            valList.Add(ptData.ValueAtPoint);
                            if (ptData.PointValue > 0) { visibleCount++; }
                        }
                        progressValue++;
                    }

                    double ratio = (double)visibleCount / (double)uvPoints.Count;
                    updatedData.VisiblityRatio = ratio;
                    updatedData.AreaWithViews = rd.RoomArea * ratio;
                    updatedData.SetResultParameterValue(LEEDParameters.LEED_AreaWithViews.ToString(), rd.AreaWithViews);

                    //visualize
                    Transform transform = Transform.CreateTranslation(new XYZ(0, 0, offsetHeight));
                    int index = m_sfm.AddSpatialFieldPrimitive(updatedData.RoomFace, transform);

                    FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPoints);
                    FieldValues values = new FieldValues(valList);

                    m_sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find visibility.\n" + ex.Message, "Find Visibility", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedData;
        }

        private RoomData FindVisibility(RoomData rd, ProgressBar progressBar)
        {
            RoomData updatedData = new RoomData(rd);
            try
            {
                LogicalOrFilter orFilter = new LogicalOrFilter(categoryFilters);
                ReferenceIntersector intersector = null;
                intersector = new ReferenceIntersector(orFilter, FindReferenceTarget.Face, m_view);
                intersector.FindReferencesInRevitLinks = includeLinkedModel;

                Face face = rd.RoomFace;
                BoundingBoxUV bb = face.GetBoundingBox();

                IList<UV> uvPoints = new List<UV>();
                IList<ValueAtPoint> valList = new List<ValueAtPoint>();

                List<PointData> pointDataList = new List<PointData>();
                double interval = analysisSettings.Interval;
                List<double> uList = new List<double>();
                List<double> vList = new List<double>();
                GetUVArray(bb, interval, out uList, out vList);


                progressBar.Value = 0;
                progressBar.Minimum = 0;
                progressBar.Maximum = uList.Count * vList.Count;
                UpdateProgressDelegate updateProgressDelegate = new UpdateProgressDelegate(progressBar.SetValue);


                List<XYZ> exteriorPoints = new List<XYZ>();
                List<XYZ> interiorPoints = new List<XYZ>();
                bool sorted = SortViewPoints(rd.BoundarySegments, out exteriorPoints, out interiorPoints);

                int visibleCount = 0;
                double progressValue = 0;
                foreach (double u in uList) //start from in the middle of grid
                {
                    foreach (double v in vList)
                    {
                        if (AbortFlag.GetAbortFlag()) { return updatedData; }
                        Dispatcher.CurrentDispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });

                        UV uvPoint = new UV(u, v);
                        if (face.IsInside(uvPoint))
                        {
                            XYZ evalPoint = face.Evaluate(uvPoint);
                            XYZ xyzPoint = new XYZ(evalPoint.X, evalPoint.Y, evalPoint.Z + offsetHeight); //4.2 inches above from the floor
                            double pointValue = 0;

                            List<XYZ> viewPoints = new List<XYZ>();
                            if (exteriorPoints.Count > 0)
                            {
                                exteriorPoints = exteriorPoints.OrderBy(o => o.DistanceTo(xyzPoint)).ToList();
                                viewPoints.AddRange(exteriorPoints);
                            }
                            if (interiorPoints.Count > 0)
                            {
                                interiorPoints = interiorPoints.OrderBy(o => o.DistanceTo(xyzPoint)).ToList();
                                viewPoints.AddRange(interiorPoints);
                            }

                            if (viewPoints.Count > 0)
                            {
                                bool visible = CheckVisibilityByMaterial(intersector, xyzPoint, viewPoints);
                                if (visible) { pointValue = 1; visibleCount++; }
                                else { pointValue = 0; }

                            }

                            PointData pData = new PointData(uvPoint, xyzPoint, pointValue);
                            pointDataList.Add(pData);

                            uvPoints.Add(pData.UVPoint);
                            valList.Add(pData.ValueAtPoint);
                        }
                        progressValue++;
                    }
                }

                updatedData.PointDataList = pointDataList;
                double ratio = (double)visibleCount / (double)uvPoints.Count;
                updatedData.VisiblityRatio = ratio;
                updatedData.AreaWithViews = rd.RoomArea * ratio;
                updatedData.SetResultParameterValue(LEEDParameters.LEED_AreaWithViews.ToString(), rd.AreaWithViews);

                //visualize
                Transform transform = Transform.CreateTranslation(new XYZ(0, 0, offsetHeight));
                int index = m_sfm.AddSpatialFieldPrimitive(face, transform);
                FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPoints);
                FieldValues values = new FieldValues(valList);

                m_sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find visibility.\n" + ex.Message, "Find Visibility", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return updatedData;
        }

        private void GetUVArray(BoundingBoxUV bb, double interval, out List<double> uList, out List<double> vList)
        {
            uList = new List<double>();
            vList = new List<double>();
            double offset = 0.05;
            try
            {
                for (double u = bb.Min.U + offset; u < bb.Max.U; u += interval)
                {
                    uList.Add(u);
                }
                if (uList[uList.Count - 1] < bb.Max.U - offset)
                {
                    uList.Add(bb.Max.U - offset);
                }

                for (double v = bb.Min.V + offset; v < bb.Max.V; v += interval)
                {
                    vList.Add(v);
                }
                if (vList[vList.Count - 1] < bb.Max.V - offset)
                {
                    vList.Add(bb.Max.V - offset);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get UV array.\n" + ex.Message, "Get UV Array", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool SortViewPoints(List<SegmentData> boundarySegments, out List<XYZ> exteriorPoints, out List<XYZ> interiorPoints)
        {
            bool sorted = false;
            exteriorPoints = new List<XYZ>();
            interiorPoints = new List<XYZ>();
            try
            {
                var exteriorSegments = from segment in boundarySegments where segment.IsExteriror select segment;
                var interiorSegments = from segment in boundarySegments where segment.IsExteriror == false select segment;

                if (exteriorSegments.Count() > 0)
                {
                    foreach (SegmentData sData in exteriorSegments)
                    {
                        exteriorPoints.AddRange(sData.ViewPoints); //check exterior walls first
                    }
                }

                if (interiorSegments.Count() > 0)
                {
                    foreach (SegmentData sData in interiorSegments)
                    {
                        interiorPoints.AddRange(sData.ViewPoints);
                    }
                }

                sorted = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return sorted;
        }

        private bool CheckVisibilityByMaterial(ReferenceIntersector intersector, XYZ pointOrigin, List<XYZ> viewPoints)
        {
            bool visible = false;
            try
            {
                foreach (XYZ vPt in viewPoints)
                {
                    XYZ direction = vPt - pointOrigin;
                    IList<ReferenceWithContext> contexts = intersector.Find(pointOrigin, direction);

                    if (null != contexts)
                    {
                        var filteredContexts = from context in contexts where context.Proximity > epsilon select context;
                        filteredContexts = filteredContexts.OrderBy(x => x.Proximity).ToList();

                        if (filteredContexts.Count() > 0)
                        {
                            visible = true;
                            foreach (ReferenceWithContext context in filteredContexts)
                            {
                                Reference reference = context.GetReference();
                                Element element = null;

                                if (reference.LinkedElementId != ElementId.InvalidElementId)
                                {
                                    //element from linked models
                                    if (linkedInstances.ContainsKey(GetElementIdValue(reference.ElementId)))
                                    {
                                        LinkedInstanceData lid = linkedInstances[GetElementIdValue(reference.ElementId)];
                                        element = lid.LinkedDocument.GetElement(reference.LinkedElementId);
                                    }
                                }
                                else
                                {
                                    //element from host
                                    element = m_doc.GetElement(reference.ElementId);
                                }

                                if (null != element)
                                {
                                    long categoryId = GetElementIdValue(element.Category.Id);
                                    if (categoryId == (int)BuiltInCategory.OST_Walls)
                                    {
                                        Wall wall = element as Wall;
                                        if (null != wall)
                                        {
                                            if (wall.WallType.Kind != WallKind.Curtain)
                                            {
                                                visible = false; break;
                                            }
                                            else
                                            {
                                                var exteriorWalls = from exWall in exteriorElementIds
                                                                    where exWall.LinkInstanceId == reference.ElementId && (exWall.HostElementId == reference.LinkedElementId || exWall.LinkedElementId == reference.LinkedElementId)
                                                                    select exWall;
                                                if (exteriorWalls.Count() > 0)
                                                {
                                                    break; //exterior curtain walls
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //family instance of wall 
                                            visible = false; break;
                                        }

                                    }
                                    else if (categoryId == (int)BuiltInCategory.OST_StructuralColumns)
                                    {
                                        visible = false; break;
                                    }
                                    else if (categoryId == (int)BuiltInCategory.OST_Windows || categoryId == (int)BuiltInCategory.OST_Doors || categoryId == (int)BuiltInCategory.OST_CurtainWallPanels)
                                    {
                                        if (reference.LinkedElementId == ElementId.InvalidElementId)
                                        {
                                            GeometryObject geomObj = element.GetGeometryObjectFromReference(reference);
                                            if (null != geomObj)
                                            {
                                                Face face = geomObj as Face;
                                                if (null != face)
                                                {
                                                    if (face.MaterialElementId != ElementId.InvalidElementId)
                                                    {
                                                        Material material = element.Document.GetElement(face.MaterialElementId) as Material;
                                                        if (material.Transparency < minTransparency)
                                                        {
                                                            visible = false; break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (categoryId == (int)BuiltInCategory.OST_Doors)
                                        {
                                            visible = false; break;
                                        }


                                        FamilyInstance instance = element as FamilyInstance;
                                        if (null != instance)
                                        {
                                            if (null != instance.Host)
                                            {
                                                ElementId wallId = instance.Host.Id;
                                                var exteriorWalls = from exWall in exteriorElementIds
                                                                    where exWall.LinkInstanceId == reference.ElementId && (exWall.HostElementId == wallId || exWall.LinkedElementId == wallId)
                                                                    select exWall;
                                                if (exteriorWalls.Count() > 0)
                                                {
                                                    break; //exterior curtain walls
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (visible)
                            {
                                break; //at least one direction should be visible
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return visible;
        }

    }

}
