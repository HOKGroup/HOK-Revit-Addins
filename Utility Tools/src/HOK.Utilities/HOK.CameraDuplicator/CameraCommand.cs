#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using static HOK.Core.Utilities.ElementIdExtension;

#endregion

namespace HOK.CameraDuplicator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CameraCommand : IExternalCommand
    {
        private UIApplication m_app;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-CameraDuplicator", m_app.Application.VersionNumber));

            if (m_app.Application.Documents.Size > 1)
            {
                var vm = new CameraViewModel();
                var cameraWindow = new CameraWindow(m_app)
                {
                    DataContext = vm
                };
                if (true == cameraWindow.ShowDialog())
                {
                    cameraWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("Please open more than two Revit documents before running this tool.\n A source model and a recipient model are required.", 
                    "Opened Revit Documents Required!", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }
    }

    public class CameraViewInfo
    {
        public long ViewId { get; set; }
        public string ViewName { get; set; }

        public long LinkedViewId { get; set; } = -1;
        public bool Linked { get; set; }

        public ElementId ViewTemplateId { get; set; }
        public ElementId PhaseId { get; set; } = ElementId.InvalidElementId;
        public Dictionary<WorksetId, WorksetVisibility> WorksetVisibilities { get; set; } = new Dictionary<WorksetId, WorksetVisibility>();

        public ViewOrientation3D Orientation { get; set; }
        public bool IsCropBoxOn { get; set; }
        public BoundingBoxXYZ CropBox { get; set; }
        public bool IsSectionBoxOn { get; set; }
        public BoundingBoxXYZ SectionBox { get; set; }
        public DisplayStyle Display { get; set; }
        public Dictionary<string, Parameter> ViewParameters { get; set; } = new Dictionary<string, Parameter>();
        public bool IsSelected { get; set; }

        public CameraViewInfo(View3D view3d)
        {
            ViewId = GetElementIdValue(view3d.Id);
            ViewName = view3d.Name;

            ViewTemplateId = view3d.ViewTemplateId;
            
            Orientation = view3d.GetOrientation();
            IsCropBoxOn = view3d.CropBoxActive;
            CropBox = view3d.CropBox;
            IsSectionBoxOn = view3d.IsSectionBoxActive;
            SectionBox = view3d.GetSectionBox();

            Display = view3d.DisplayStyle;
            GetParameters(view3d.Parameters);
        }

        public CameraViewInfo(CameraViewInfo vInfo)
        {
            ViewId = vInfo.ViewId;
            ViewName = vInfo.ViewName;
            LinkedViewId = vInfo.LinkedViewId;
            Linked = vInfo.Linked;
            ViewTemplateId = vInfo.ViewTemplateId;
            PhaseId = vInfo.PhaseId;
            WorksetVisibilities = vInfo.WorksetVisibilities;
            Orientation = vInfo.Orientation;
            CropBox = vInfo.CropBox;
            Display = vInfo.Display;
            ViewParameters = vInfo.ViewParameters;
            IsSelected = vInfo.IsSelected;
        }

        private void GetParameters(ParameterSet parameters)
        {
            try
            {
                foreach (Parameter param in parameters)
                {
                    var paramName = param.Definition.Name;
                    if (paramName.Contains(".Extensions")) { continue; }
                    if (GetElementIdValue(param.Id) == (int)BuiltInParameter.VIEW_PHASE) { PhaseId = param.AsElementId(); }
                    if (param.StorageType == StorageType.None || param.StorageType  == StorageType.ElementId) { continue; }

                    if (!ViewParameters.ContainsKey(paramName) && param.HasValue)
                    {
                        ViewParameters.Add(paramName, param);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void GetWorksetVisibilities(View3D view3d, List<WorksetId> worksetIds)
        {
            try
            {
                foreach (var wsId in worksetIds)
                {
                    if (!WorksetVisibilities.ContainsKey(wsId))
                    {
                        WorksetVisibilities.Add(wsId, view3d.GetWorksetVisibility(wsId));
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public class PlanViewInfo
    {
        public long ViewId { get; set; }
        public string ViewName { get; set; }
        public ViewType PlanViewType { get; set; }

        public long LinkedViewId { get; set; } = -1;
        public bool Linked { get; set; }

        public ElementId LevelId { get; set; } = ElementId.InvalidElementId;
        public string LevelName { get; set; } = "";

        public ElementId ViewTemplateId { get; set; }
        public ElementId ScopeBoxId { get; set; } = ElementId.InvalidElementId;
        public ElementId PhaseId { get; set; } = ElementId.InvalidElementId;
        public ElementId AreaSchemeId { get; set; } = ElementId.InvalidElementId;
        public Dictionary<WorksetId, WorksetVisibility> WorksetVisibilities { get; set; } = new Dictionary<WorksetId, WorksetVisibility>();

        public bool IsCropBoxOn { get; set; }
        public BoundingBoxXYZ CropBox { get; set; }
        public DisplayStyle Display { get; set; }

        public Dictionary<string, Parameter> ViewParameters { get; set; } = new Dictionary<string, Parameter>();
        public bool IsSelected { get; set; }

        public PlanViewInfo(ViewPlan planView)
        {
            ViewId = GetElementIdValue(planView.Id);
            ViewName = planView.Name;
            PlanViewType = planView.ViewType;

            if (null != planView.GenLevel)
            {
                LevelId = planView.GenLevel.Id;
                LevelName = planView.GenLevel.Name;
            }
            
            ViewTemplateId = planView.ViewTemplateId;
            if (null != planView.AreaScheme)
            {
                AreaSchemeId = planView.AreaScheme.Id;
            }

            IsCropBoxOn = planView.CropBoxActive;
            CropBox = planView.CropBox;

            Display = planView.DisplayStyle;
            GetParameters(planView.Parameters);
        }

        public PlanViewInfo(PlanViewInfo info)
        {
            ViewId = info.ViewId;
            ViewName = info.ViewName;
            PlanViewType = info.PlanViewType;
            LinkedViewId = info.LinkedViewId;
            Linked = info.Linked;
            LevelId = info.LevelId;
            LevelName = info.LevelName;
            ViewTemplateId = info.ViewTemplateId;
            ScopeBoxId = info.ScopeBoxId;
            PhaseId = info.PhaseId;
            AreaSchemeId = info.AreaSchemeId; 
            IsCropBoxOn = info.IsCropBoxOn;
            CropBox = info.CropBox;
            Display = info.Display;
            ViewParameters = info.ViewParameters;
            IsSelected = info.IsSelected;
        }

        private void GetParameters(ParameterSet parameters)
        {
            try
            {
                foreach (Parameter param in parameters)
                {
                    var paramName = param.Definition.Name;
                    if (paramName.Contains(".Extensions")) { continue; }
                    if (GetElementIdValue(param.Id) == (int)BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) { ScopeBoxId = param.AsElementId(); continue; }
                    if (GetElementIdValue(param.Id) == (int)BuiltInParameter.VIEW_PHASE) { PhaseId = param.AsElementId(); continue; }
                    if (param.StorageType == StorageType.None || param.StorageType == StorageType.ElementId) { continue; }
                    if (!ViewParameters.ContainsKey(paramName) && param.HasValue)
                    {
                        ViewParameters.Add(paramName, param);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void GetWorksetVisibilities(ViewPlan viewPlan, List<WorksetId> worksetIds)
        {
            try
            {
                foreach (var wsId in worksetIds)
                {
                    if (!WorksetVisibilities.ContainsKey(wsId))
                    {
                        WorksetVisibilities.Add(wsId, viewPlan.GetWorksetVisibility(wsId));
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    public class ModelInfo
    {
        private List<WorksetId> worksetIds = new List<WorksetId>();

        public string ModelId { get; set; }
        public string ModelName { get; set; }
        public Document ModelDoc { get; set; }
        public Dictionary<long, CameraViewInfo> CameraViews { get; set; } = new Dictionary<long, CameraViewInfo>();
        public Dictionary<long, PlanViewInfo> PlanViews { get; set; } = new Dictionary<long, PlanViewInfo>();

        public ModelInfo(Document doc, string id)
        {
            ModelDoc = doc;
            ModelName = doc.Title;
            ModelId = id;

            if (ModelDoc.IsWorkshared)
            {
                GetWorksets();
            }
            GetCameraViews();
            GetPlanViews();
        }

        private void GetWorksets()
        {
            try
            {
                var wsCollector = new FilteredWorksetCollector(ModelDoc);
                var wsFilter = new WorksetKindFilter(WorksetKind.UserWorkset);
                worksetIds = wsCollector.WherePasses(wsFilter).ToWorksetIds().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get worksets.\n"+ex.Message, "Get Worksets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetCameraViews()
        {
            try
            {
                var collector = new FilteredElementCollector(ModelDoc);
                var threeDViews = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                foreach (var view in threeDViews)
                {
                    if (view.ViewType != ViewType.ThreeD) { continue; }
                    if (view.IsTemplate) { continue; }
                    if (view.IsPerspective)
                    {
                        var viewInfo = new CameraViewInfo(view);
                        if (worksetIds.Count > 0)
                        {
                            viewInfo.GetWorksetVisibilities(view, worksetIds);
                        }
                        if (!CameraViews.ContainsKey(viewInfo.ViewId))
                        {
                            CameraViews.Add(viewInfo.ViewId, viewInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get camera views.\n" + ex.Message, "Get Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetPlanViews()
        {
            try
            {
                var collector = new FilteredElementCollector(ModelDoc);
                var viewPlans = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
                foreach (var pView in viewPlans)
                {
                    if (pView.IsTemplate) { continue; }
                    var parentParam = pView.get_Parameter(BuiltInParameter.SECTION_PARENT_VIEW_NAME);
                    if (null != parentParam)
                    {
                        if (parentParam.HasValue) { continue; }
                    }

                    if (!PlanViews.ContainsKey(GetElementIdValue(pView.Id)))
                    {
                        var pvi = new PlanViewInfo(pView);
                        if (worksetIds.Count > 0)
                        {
                            pvi.GetWorksetVisibilities(pView, worksetIds);
                        }
                        PlanViews.Add(pvi.ViewId, pvi);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get plan views.\n"+ex.Message, "Get Plan Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class ViewConfiguration
    {
        public bool ApplyWorksetVisibility { get; set; }
        public List<MapItemInfo> MapItems { get; set; } = new List<MapItemInfo>();

        public ViewConfiguration()
        {
        }

        public ViewConfiguration(bool apply, List<MapItemInfo> maps)
        {
            ApplyWorksetVisibility = apply;
            MapItems = maps;
        }
    }

    public class MapItemInfo
    {
        public string SourceModelId { get; set; } = "";
        public string RecipientModelId { get; set; } = "";
        public MapType MapItemType { get; set; } = MapType.None;
        public long SourceItemId { get; set; } = -1;
        public long RecipientItemId { get; set; } = -1;
        public string SourceItemName { get; set; } = "";
        public string RecipientItemName { get; set; } = "";
    }

    public enum MapType
    {
        Level, ViewTemplate, ScopeBox, Workset, Phase, AreaScheme, None
    }

    public class ViewTypeInfo
    {
        public string ViewTypeName { get; set; } = "";
        public ViewType ViewTypeEnum { get; set; } = ViewType.Undefined;
        public ViewFamily ViewFamilyEnum { get; set; } = ViewFamily.ThreeDimensional;

        public ViewTypeInfo()
        {
        }

        public ViewTypeInfo(string name, ViewType vType, ViewFamily vFamily)
        {
            ViewTypeName = name;
            ViewTypeEnum = vType;
            ViewFamilyEnum = vFamily;
        }
    }
}
