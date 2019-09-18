#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SmartBCF.AddIn.Util;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.Utils;

#endregion

namespace HOK.SmartBCF.AddIn
{
    public class BCFHandler : IExternalEventHandler
    {
        private UIApplication m_app;
        private readonly string addinDefinitionFile;
        public Document ActiveDoc { get; set; }
        public View3D BCFOrthoView { get; set; }
        public View3D BCFPersView { get; set; }
        public UIView BCFUIView { get; set; }
        public Request Request { get; } = new Request();
        public ComponentViewModel ViewModel { get; set; } = null;
        public AddViewModel ViewPointViewModel { get; set; } = null;
        public string DatabaseFile { get; set; } = "";

        public BCFHandler(UIApplication uiapp)
        {
            m_app = uiapp;
            ActiveDoc = uiapp.ActiveUIDocument.Document;
            addinDefinitionFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                                  "/Resources/Addins Shared Parameters.txt";
        }

        public void Execute(UIApplication app)
        {
            ActiveDoc = app.ActiveUIDocument.Document;
            try
            {
                switch (Request.Take())
                {
                    case RequestId.ApplyViews:
                        // (Jinsol) When component selection changed
                        if (null != BCFOrthoView)
                        {
                            CleanViewSettings(BCFOrthoView, true, true, true);
                            
                            SetDefaultView();
                            if (ViewModel.IsHighlightChecked) HighlightElements();
                            if (ViewModel.IsIsolateChecked) IsolateElement(BCFOrthoView);
                            else if (ViewModel.IsSectionBoxChecked) PlaceSectionBox(BCFOrthoView);
                        }
                        break;
                    case RequestId.HighlightElement:
                         if (null != BCFOrthoView)
                         {
                             CleanViewSettings(BCFOrthoView, true, false, false);
                             if (ViewModel.IsHighlightChecked) HighlightElements();
                         }
                        break;
                    case RequestId.IsolateElement:
                        if (null != BCFOrthoView)
                        {
                            CleanViewSettings(BCFOrthoView, false, true, true);
                            SetDefaultView();

                            if (ViewModel.IsIsolateChecked) IsolateElement(BCFOrthoView);
                            else SetViewPointView();
                        }
                        break;
                    case RequestId.PlaceSectionBox:
                         if (null != BCFOrthoView)
                         {
                             CleanViewSettings(BCFOrthoView, false, true, true);
                             SetDefaultView();

                             if (ViewModel.IsSectionBoxChecked) PlaceSectionBox(BCFOrthoView);
                             else SetViewPointView();
                         }
                        break;
                    case RequestId.WriteParameters:
                        WriteParameters();
                        break;
                    case RequestId.SetViewPointView:
                        // (Jinsol) When markup topic changed
                        SetViewPointView();
                        break;
                    case RequestId.StoreToolSettings:
                        StoreToolSettings();
                        break;
                    case RequestId.ExportImage:
                        ExportImage();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute the external event.\n"+ex.Message, "Execute Event", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HighlightElements()
        {
            if (null == ViewModel.SelectedComponent) return;

            try
            {
                var uidoc = new UIDocument(ActiveDoc);
                var selectedIds = new List<ElementId>();
                selectedIds.Add(ViewModel.SelectedComponent.ElementId);
                uidoc.Selection.SetElementIds(selectedIds);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcfView"></param>
        private void IsolateElement(View3D bcfView)
        {
            var selectedComponent = ViewModel.SelectedComponent;
            if (null == selectedComponent) return;

            using (var trans = new Transaction(ActiveDoc))
            {
                trans.Start("Isolate View");
                try
                {
                    bcfView.IsolateElementTemporary(ViewModel.SelectedComponent.ElementId);

                    var element = ViewModel.SelectedComponent.RvtElement;
                    if (element != null)
                    {
                        var boundingBox = element.get_BoundingBox(null);
                        if (boundingBox != null)
                        {
                            BCFUIView.ZoomAndCenterRectangle(boundingBox.Min, boundingBox.Max);
                            BCFUIView.Zoom(0.8);
                        }
                    }
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.RollBack();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcfView"></param>
        private void PlaceSectionBox(View3D bcfView)
        {
            var selectedComponent = ViewModel.SelectedComponent;
            if (null == selectedComponent) return;

            using (var trans = new Transaction(ActiveDoc))
            {
                trans.Start("Set Section Box");
                try
                {
                    var element = selectedComponent.RvtElement;
                    if (null != element)
                    {
                        var transform = selectedComponent.TransformValue;
                        var boundingBox = element.get_BoundingBox(null);
                        var minXYZ = transform.OfPoint(boundingBox.Min);
                        var maxXYZ = transform.OfPoint(boundingBox.Max);
                            

                        var offsetBox = new BoundingBoxXYZ();
                        offsetBox.Min = new XYZ(minXYZ.X - 3, minXYZ.Y - 3, minXYZ.Z - 3);
                        offsetBox.Max = new XYZ(maxXYZ.X + 3, maxXYZ.Y + 3, maxXYZ.Z + 3);

                        bcfView.SetSectionBox(offsetBox);
                        bcfView.GetSectionBox().Enabled = true;

                        BCFUIView.ZoomAndCenterRectangle(offsetBox.Min, offsetBox.Max);
                        BCFUIView.Zoom(0.8);
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to set sectionbox.\n" + ex.Message, "Set Section Box", MessageBoxButton.OK, MessageBoxImage.Warning);
                    trans.RollBack();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void WriteParameters()
        {
            try
            {
                var selectedComponent = ViewModel.SelectedComponent;
                if (null == selectedComponent) return;
                if (selectedComponent.IsLinked) return;

                var element = selectedComponent.RvtElement;
                if (null == element) return;

                using (var trans = new Transaction(ActiveDoc))
                {
                    trans.Start("Write Parameters");
                    try
                    {
                        WriteParameter(element, BCFParameters.BCF_Name, ViewModel.SelectedBCF.ZipFileName);
                        WriteParameter(element, BCFParameters.BCF_Topic, ViewModel.SelectedMarkup.Topic.Title);
                        WriteParameter(element, BCFParameters.BCF_TopicType, ViewModel.SelectedMarkup.Topic.TopicType.ToString());
                        WriteParameter(element, BCFParameters.BCF_TopicStatus, ViewModel.SelectedMarkup.Topic.TopicStatus.ToString());
                        WriteParameter(element, BCFParameters.BCF_AssignedTo, ViewModel.SelectedMarkup.Topic.AssignedTo);
                        WriteParameter(element, BCFParameters.BCF_Comment, ViewModel.SelectedComment.Comment1);
                        WriteParameter(element, BCFParameters.BCF_Author, ViewModel.SelectedComment.ModifiedAuthor);
                        WriteParameter(element, BCFParameters.BCF_Date, ViewModel.SelectedComment.ModifiedDate.ToString(CultureInfo.InvariantCulture));
                        WriteParameter(element, BCFParameters.BCF_Action, selectedComponent.Action.ParameterValue);
                        WriteParameter(element, BCFParameters.BCF_Responsibility, selectedComponent.Responsibility.ParameterValue);

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.RollBack();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool WriteParameter(Element element, BCFParameters bcfParam, string value)
        {
            var updated = false;
            try
            {
                var param = element.LookupParameter(bcfParam.ToString());
                if (null != param)
                {
                    if (!param.IsReadOnly)
                    {
                        updated = param.Set(value);
                    }
                }
                else
                {
                    var insertedParam = InsertBinding(bcfParam, element.Category);
                    if (insertedParam)
                    {
                        updated = WriteParameter(element, bcfParam, value);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return updated;
        }

        private bool InsertBinding(BCFParameters bcfParam, Category category)
        {
            var inserted = false;
            try
            {
                var iter = ActiveDoc.ParameterBindings.ForwardIterator();
                Definition definitionFound = null;
                while (iter.MoveNext())
                {
                    var definition = iter.Key;
                    var elemBinding = (ElementBinding)iter.Current;
                    if (definition.Name == bcfParam.ToString())
                    {
                        definitionFound = definition;
                    }
                }

                if (category.AllowsBoundParameters)
                {
                    if (null != definitionFound)
                    {
                        var elementBinding = (ElementBinding)ActiveDoc.ParameterBindings.get_Item(definitionFound);
                        if (null != elementBinding)
                        {
                            var catset = elementBinding.Categories;
                            catset.Insert(category);

                            var binding = m_app.Application.Create.NewInstanceBinding(catset);
                            inserted = ActiveDoc.ParameterBindings.ReInsert(definitionFound, binding);
                        }
                    }
                    else
                    {
                       
                        var originalDefinitionFile = m_app.Application.SharedParametersFilename;
                        m_app.Application.SharedParametersFilename = addinDefinitionFile;
                        var definitionFile = m_app.Application.OpenSharedParameterFile();
                        foreach (var group in definitionFile.Groups)
                        {
                            if (group.Name == "HOK BCF")
                            {
                                foreach (var definition in group.Definitions)
                                {
                                    if (definition.Name == bcfParam.ToString())
                                    {
                                        definitionFound = definition;
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                        if (null != definitionFound)
                        {
                            var catset = m_app.Application.Create.NewCategorySet();
                            catset.Insert(category);

                            var binding = m_app.Application.Create.NewInstanceBinding(catset);
                            inserted = ActiveDoc.ParameterBindings.Insert(definitionFound, binding);
                        }

                        m_app.Application.SharedParametersFilename = originalDefinitionFile;
                    }
                }
                
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return inserted;
        }

        private bool SetViewPointView()
        {
            var updated = false;
            try
            {
                //refresh background view
                if (null == BCFPersView)
                {
                    BCFPersView = CreateDefaultPersView();
                }
                if (null == BCFOrthoView)
                {
                    BCFOrthoView = CreateDefaultOrthoView();
                }

                SetDefaultView();
                var visInfo = ViewModel.SelectedMarkup.SelectedViewpoint.VisInfo;
                if (ViewModel.RvtComponents.Count > 0)
                {
                    //create orthoview to iterate elements
                    CleanViewSettings(BCFOrthoView, true, true, true);

                    BCFUIView = FindDefaultUIView(BCFOrthoView);

                    if (visInfo.IsPersepective)
                    {
                        updated = SetOrthogonalView(BCFOrthoView, visInfo.PerspectiveCamera);
                    }
                    else
                    {
                        updated = SetOrthogonalView(BCFOrthoView, visInfo.OrthogonalCamera);
                    }
                }
                else
                {
                    if (visInfo.IsPersepective && null != BCFPersView)
                    {
                        //PerspectiveCamera
                        CleanViewSettings(BCFPersView, true, true, true);

                        updated = SetPerspectiveView(BCFPersView, visInfo.PerspectiveCamera);
                        m_app.ActiveUIDocument.ActiveView = BCFPersView;
                        BCFUIView = FindDefaultUIView(BCFPersView);    
                    }
                    else if (!visInfo.IsPersepective && null != BCFOrthoView)
                    {
                        //OrthogonalCamera
                        CleanViewSettings(BCFOrthoView, true, true, true);

                        BCFUIView = FindDefaultUIView(BCFOrthoView);

                        updated = SetOrthogonalView(BCFOrthoView, visInfo.OrthogonalCamera);
                    }
                }

                m_app.ActiveUIDocument.RefreshActiveView();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return updated;
        }

        public bool SetDefaultView()
        {
            var result = false;
            try
            {
                if (null != BCFOrthoView)
                {
                    m_app.ActiveUIDocument.ActiveView = BCFOrthoView;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return result;
        }

        private bool SetOrthogonalView(View3D bcfView, OrthogonalCamera camera)
        {
            var result = false;
            try
            {
                var zoom = camera.ViewToWorldScale.ToFeet();
                var direction = RevitUtils.GetRevitXYZ(camera.CameraDirection);
                var upVector = RevitUtils.GetRevitXYZ(camera.CameraUpVector);
                var viewPoint = RevitUtils.GetRevitXYZ(camera.CameraViewPoint);
                var orientation = RevitUtils.ConvertBasePoint(ActiveDoc, viewPoint, direction, upVector, true);

                using (var trans = new Transaction(ActiveDoc))
                {
                    trans.Start("Set Orientation");
                    try
                    {
                        bcfView.SetOrientation(orientation);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        var message = ex.Message;
                    }
                }

                var m_xyzTl = bcfView.Origin.Add(bcfView.UpDirection.Multiply(zoom)).Subtract(bcfView.RightDirection.Multiply(zoom));
                var m_xyzBr = bcfView.Origin.Subtract(bcfView.UpDirection.Multiply(zoom)).Add(bcfView.RightDirection.Multiply(zoom));
                BCFUIView.ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);

                result = true;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Orthogonal View by Perspective Camera.
        /// </summary>
        /// <param name="bcfView"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        private bool SetOrthogonalView(View3D bcfView, PerspectiveCamera camera)
        {
            var result = false;
            try
            {
                var direction = RevitUtils.GetRevitXYZ(camera.CameraDirection);
                var upVector = RevitUtils.GetRevitXYZ(camera.CameraUpVector);
                var viewPoint = RevitUtils.GetRevitXYZ(camera.CameraViewPoint);
                var orientation = RevitUtils.ConvertBasePoint(ActiveDoc, viewPoint, direction, upVector, true);

                using (var trans = new Transaction(ActiveDoc))
                {
                    trans.Start("Set Orientation");
                    try
                    {
                        bcfView.SetOrientation(orientation);
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.RollBack();
                    }
                }

                SetViewPointBoundingBox(bcfView);
                result = true;
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        private bool SetPerspectiveView(View3D bcfView,  PerspectiveCamera camera)
        {
            var result = false;
            try
            {
                var zoom = camera.FieldOfView;
                var direction = RevitUtils.GetRevitXYZ(camera.CameraDirection);
                var upVector = RevitUtils.GetRevitXYZ(camera.CameraUpVector);
                var viewPoint = RevitUtils.GetRevitXYZ(camera.CameraViewPoint);
                var orientation = RevitUtils.ConvertBasePoint(ActiveDoc, viewPoint, direction, upVector, true);


                using (var trans = new Transaction(ActiveDoc))
                {
                    trans.Start("Set Orientation");
                    try
                    {
                        if (bcfView.CanResetCameraTarget()) bcfView.ResetCameraTarget();
                        bcfView.SetOrientation(orientation);
                        if (bcfView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).HasValue)
                        {
                            var m_farClip = bcfView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
                            m_farClip.Set(0); 
                        }

                        bcfView.CropBoxActive = true;
                        bcfView.CropBoxVisible = true;

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.RollBack();
                    }
                }
                result = true;
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcfView"></param>
        private void SetViewPointBoundingBox(View3D bcfView)
        {
            if (ViewModel.RvtComponents.Count == 0) return;

            var offsetBoundingBox = new BoundingBoxXYZ();
            using (var trans = new Transaction(ActiveDoc))
            {
                trans.Start("Set BoundingBox");
                try
                {
                    double minX = 0;
                    double minY = 0;
                    double minZ = 0;
                    double maxX = 0;
                    double maxY = 0;
                    double maxZ = 0;
                    var firstLoop = true;

                    foreach (var rvtComp in ViewModel.RvtComponents)
                    {
                        var element = rvtComp.RvtElement;
                        if (null != element)
                        {
                            var bb = element.get_BoundingBox(null);
                            if (null != bb)
                            {
                                var transform = rvtComp.TransformValue;

                                var minXYZ = transform.OfPoint(bb.Min);
                                var maxXYZ = transform.OfPoint(bb.Max);

                                if (firstLoop)
                                {
                                    minX = minXYZ.X;
                                    minY = minXYZ.Y;
                                    minZ = minXYZ.Z;
                                    maxX = maxXYZ.X;
                                    maxY = maxXYZ.Y;
                                    maxZ = maxXYZ.Z;
                                    firstLoop = false;
                                }
                                else
                                {
                                    if (minX > minXYZ.X) { minX = minXYZ.X; }
                                    if (minY > minXYZ.Y) { minY = minXYZ.Y; }
                                    if (minZ > minXYZ.Z) { minZ = minXYZ.Z; }
                                    if (maxX < maxXYZ.X) { maxX = maxXYZ.X; }
                                    if (maxY < maxXYZ.Y) { maxY = maxXYZ.Y; }
                                    if (maxZ < maxXYZ.Z) { maxZ = maxXYZ.Z; }
                                }
                            }
                        }
                    }
                    offsetBoundingBox.Min = new XYZ(minX - 3, minY - 3, minZ - 3);
                    offsetBoundingBox.Max = new XYZ(maxX + 3, maxY + 3, maxZ + 3);

                    bcfView.SetSectionBox(offsetBoundingBox);
                    bcfView.GetSectionBox().Enabled = true;

                    BCFUIView.ZoomAndCenterRectangle(offsetBoundingBox.Min, offsetBoundingBox.Max);
                    BCFUIView.Zoom(0.8);

                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.RollBack();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcfView"></param>
        /// <param name="removeHighlight"></param>
        /// <param name="removeIsolate"></param>
        /// <param name="removeSectionBox"></param>
        private void CleanViewSettings(View3D bcfView, bool removeHighlight, bool removeIsolate, bool removeSectionBox)
        {
            using (var trans = new Transaction(ActiveDoc))
            {
                trans.Start("Clean Views");
                try
                {
                    var uidoc = new UIDocument(ActiveDoc);
                    if (removeHighlight)
                    {
                        //remove selection
                        uidoc.Selection.SetElementIds(new List<ElementId>());
                    }

                    if (removeIsolate)
                    {
                        //remove isolation
                        if (bcfView.IsInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate))
                        {
                            bcfView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                        }
                    }

                    if (removeSectionBox)
                    {
                        //remove sectionbox
                        bcfView.GetSectionBox().Enabled = false;
                    }

                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.RollBack();
                }
            }
        }

        private View3D CreateDefaultOrthoView()
        {
            View3D view3D = null;
            try
            {
                var viewName = "SmartBCF - Orthogonal - " + Environment.UserName;
                var collector = new FilteredElementCollector(ActiveDoc);
                var view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
#if RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == false && view.ViewName == viewName select view;
#else
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == false && view.Name == viewName select view;
#endif
                if (viewfound.Any())
                {
                    view3D = viewfound.First();
                }
                else
                {
                    var viewFamilyTypeId = GetViewFamilyTypeId();
                    if (viewFamilyTypeId != ElementId.InvalidElementId)
                    {
                        using (var trans = new Transaction(ActiveDoc))
                        {
                            trans.Start("Create View");
                            try
                            {
                                view3D = View3D.CreateIsometric(ActiveDoc, viewFamilyTypeId);
                                view3D.Name = viewName;
                                trans.Commit();
                            }
                            catch (Exception)
                            {
                                trans.RollBack();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
               // ignored
            }

            return view3D;
        }

        private View3D CreateDefaultPersView()
        {
            View3D view3D = null;
            try
            {
                var viewName = "SmartBCF - Perspective - " + Environment.UserName;
                var collector = new FilteredElementCollector(ActiveDoc);
                var view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                //by the limitation of perspective view, create isometric instead
#if RELEASE2015 || RELEASE2016 || RELEASE2017 || RELEASE2018
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == false && view.ViewName == viewName select view;
#else
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == false && view.Name == viewName select view;
#endif
                if (viewfound.Any())
                {
                    view3D = viewfound.First();
                }
                else
                {
                    var viewFamilyTypeId = GetViewFamilyTypeId();
                    if (viewFamilyTypeId != ElementId.InvalidElementId)
                    {
                        using (var trans = new Transaction(ActiveDoc))
                        {
                            trans.Start("Create View");
                            try
                            {
                                view3D = View3D.CreatePerspective(ActiveDoc, viewFamilyTypeId);
                                view3D.Name = viewName;
                                trans.Commit();
                            }
                            catch (Exception)
                            {
                                trans.RollBack();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return view3D;
        }

        private UIView FindDefaultUIView(View3D bcfView)
        {
            UIView uiview = null;
            try
            {
                var uidoc = new UIDocument(ActiveDoc);
                var uiviews = uidoc.GetOpenUIViews();
                var viewFound = from view in uiviews where view.ViewId == bcfView.Id select view;
                if (viewFound.Any())
                {
                    uiview = viewFound.First();
                }   
            }
            catch (Exception)
            {
                // ignored
            }

            return uiview;
        }

        private ElementId GetViewFamilyTypeId()
        {
            var viewTypeId = ElementId.InvalidElementId;
            try
            {
                var collector = new FilteredElementCollector(ActiveDoc);
                var viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
                var vTypes = from vType in viewFamilyTypes where vType.ViewFamily == ViewFamily.ThreeDimensional select vType;
                if (vTypes.Any())
                {
                    var vfType = vTypes.First();
                    viewTypeId = vfType.Id;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return viewTypeId;
        }

        private void StoreToolSettings()
        {
            using (var trans = new Transaction(ActiveDoc))
            {
                trans.Start("Store Settings");
                try
                {
                    DataStorageUtil.UpdateLinkedDatabase(ActiveDoc, DatabaseFile);
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.RollBack();
                }
            }
        }

        private void ExportImage()
        {
            using (var trans = new Transaction(ActiveDoc))
            {
                trans.Start("Export Image");
                try
                {
                    var imgFileName = Path.Combine(Path.GetTempPath(), "SmartBCF", Path.GetTempFileName() + ".png");
                    if (File.Exists(imgFileName)) File.Delete(imgFileName);

                    var options = new ImageExportOptions
                    {
                        FilePath = imgFileName,
                        HLRandWFViewsFileType = ImageFileType.PNG,
                        ShadowViewsFileType = ImageFileType.PNG,
                        ExportRange = ExportRange.VisibleRegionOfCurrentView,
                        ZoomType = ZoomFitType.FitToPage,
                        ImageResolution = ImageResolution.DPI_72,
                        PixelSize = 1000
                    };

                    ActiveDoc.ExportImage(options);
                    trans.Commit();

                    SetViewPoint(imgFileName);
                }
                catch (Exception)
                {
                    trans.RollBack();
                }
            }
        }

        private void SetViewPoint(string imageFile)
        {
            try
            {
                var count = 5;
                while (!File.Exists(imageFile))
                {
                    count--;
                    if (count < 0) { break; }
                    Thread.Sleep(100);
                }

                if (!File.Exists(imageFile)) return;

                var vp = new ViewPoint();
                vp.Guid = Guid.NewGuid().ToString();
                vp.TopicGuid = ViewPointViewModel.SelectedMarkup.Topic.Guid;
                vp.Snapshot = Path.GetFileName(imageFile);
                vp.SnapshotImage = ImageUtil.GetImageArray(imageFile);

                var visInfo = new VisualizationInfo();
                visInfo.ViewPointGuid = vp.Guid;
                //update components
                visInfo.Components = GetElements(vp.Guid);

                //update camera
                var currentView = ActiveDoc.ActiveView as View3D;
                if (null != currentView)
                {
                    if (currentView.IsPerspective)
                    {
                        visInfo.PerspectiveCamera = GetPerspectiveCamera(currentView, vp.Guid);
                    }
                    else
                    {
                        visInfo.OrthogonalCamera = GetOrthogonalCamera(currentView, vp.Guid);
                    }
                }

                vp.VisInfo = visInfo;
                ViewPointViewModel.UserViewPoint = vp;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private ObservableCollection<Component> GetElements(string viewpoint_Guid)
        {
            var components = new ObservableCollection<Component>();
            try
            {
                switch (ViewPointViewModel.SelectedOption)
                {
                    case ComponentOption.SelectedElements:
                        var selectedIds = m_app.ActiveUIDocument.Selection.GetElementIds();
                        foreach (var eId in selectedIds)
                        {
                            var element = ActiveDoc.GetElement(eId);
                            if (null != element)
                            {
                                var comp = new Component();
                                comp.Guid = Guid.NewGuid().ToString();
                                comp.IfcGuid = element.IfcGUID();
                                comp.Selected = false;
                                comp.Visible = true;
                                comp.OriginatingSystem = m_app.Application.VersionName;
                                comp.AuthoringToolId = element.Id.IntegerValue.ToString();
                                comp.ViewPointGuid = viewpoint_Guid;
                                comp.ElementName = element.Name;

                                components.Add(comp);
                            }
                        }
                        break;
                    case Utils.ComponentOption.OnlyVisible:
                        var currentView = ActiveDoc.ActiveView;
                        var collector = new FilteredElementCollector(ActiveDoc, currentView.Id);
                        ICollection<Element> elements = collector.ToElements();
                        foreach (var element in elements)
                        {
                            var comp = new Component();
                            comp.Guid = Guid.NewGuid().ToString();
                            comp.IfcGuid = element.IfcGUID();
                            comp.Selected = false;
                            comp.Visible = true;
                            comp.OriginatingSystem = m_app.Application.VersionName;
                            comp.AuthoringToolId = element.Id.IntegerValue.ToString();
                            comp.ViewPointGuid = viewpoint_Guid;
                            comp.ElementName = element.Name;

                            components.Add(comp);
                        }

                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return components;
        }

        private PerspectiveCamera GetPerspectiveCamera(View3D view, string viewpoint_Guid)
        {
            var camera = new PerspectiveCamera();
            try
            {
                var viewCenter = view.Origin;
                double zoomValue = 45;

                var orientation = RevitUtils.ConvertBasePoint(ActiveDoc, viewCenter, view.ViewDirection, view.UpDirection, false);

                var c = orientation.EyePosition;
                var vi = orientation.ForwardDirection;
                var up = orientation.UpDirection;

                camera.Guid = Guid.NewGuid().ToString();
                camera.ViewPointGuid = viewpoint_Guid;

                var viewPoint = new Schemas.Point
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = c.X.ToMeters(),
                    Y = c.Y.ToMeters(),
                    Z = c.Z.ToMeters()
                };
                camera.CameraViewPoint = viewPoint;

                var upVector = new Direction
                {
                    Guid = Guid.NewGuid().ToString(),
                    X=up.X.ToMeters(),
                    Y=up.Y.ToMeters(),
                    Z=up.Z.ToMeters()
                };
                camera.CameraUpVector = upVector;

                var direction = new Direction
                {
                    Guid = Guid.NewGuid().ToString(),
                    X= -(vi.X.ToMeters()),
                    Y=-(vi.Y.ToMeters()),
                    Z=-(vi.Z.ToMeters())
                };
                camera.CameraDirection = direction;

                camera.FieldOfView = zoomValue;
            }
            catch (Exception)
            {
                // ignored
            }

            return camera;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="viewpoint_Guid"></param>
        /// <returns></returns>
        private OrthogonalCamera GetOrthogonalCamera(View3D view, string viewpoint_Guid)
        {
            var camera = new OrthogonalCamera();
            try
            {

                var uiView = FindDefaultUIView(view); if (null == uiView) { return camera; }
                var topLeft = uiView.GetZoomCorners()[0];
                var bottomRight = uiView.GetZoomCorners()[1];

                var x = (topLeft.X + bottomRight.X) / 2;
                var y = (topLeft.Y + bottomRight.Y) / 2;
                var z = (topLeft.Z + bottomRight.Z) / 2;

                var viewCenter = new XYZ(x, y, z);

                var diagVector = topLeft.Subtract(bottomRight);
                var dist = topLeft.DistanceTo(bottomRight) / 2;

                var zoomValue = dist * Math.Sin(diagVector.AngleTo(view.RightDirection)).ToMeters();

                var orientation = RevitUtils.ConvertBasePoint(ActiveDoc, viewCenter, view.ViewDirection, view.UpDirection, false);

                var c = orientation.EyePosition;
                var vi = orientation.ForwardDirection;
                var up = orientation.UpDirection;

                camera.Guid = Guid.NewGuid().ToString();
                camera.ViewPointGuid = viewpoint_Guid;
                var viewPoint = new Schemas.Point
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = c.X.ToMeters(),
                    Y = c.Y.ToMeters(),
                    Z = c.Z.ToMeters()
                };
                camera.CameraViewPoint = viewPoint;

                var upVector = new Direction
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = up.X.ToMeters(),
                    Y = up.Y.ToMeters(),
                    Z = up.Z.ToMeters()
                };
                camera.CameraUpVector = upVector;

                var direction = new Direction
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = -(vi.X.ToMeters()),
                    Y = -(vi.Y.ToMeters()),
                    Z = -(vi.Z.ToMeters())
                };
                camera.CameraDirection = direction;

                camera.ViewToWorldScale = zoomValue;
            }
            catch (Exception)
            {
                // ignored
            }

            return camera;
        }
       
        public string GetName()
        {
            return "BCF Event Handler";
        }
    }

    public enum RequestId
    {
        None = 0,
        SetViewPointView = 1,
        ApplyViews = 2,
        HighlightElement = 3,
        IsolateElement = 4,
        PlaceSectionBox = 5,
        WriteParameters = 6,
        StoreToolSettings = 7,
        ExportImage = 8
    }

    public class Request
    {
        private int m_request = (int)RequestId.None;

        public RequestId Take()
        {
            return (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.None);
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref m_request, (int)request);
        }
    }

    public enum BCFParameters
    {
        BCF_Action,
        BCF_Author,
        BCF_Comment,
        BCF_Date,
        BCF_Name,
        BCF_Responsibility,
        BCF_Topic,
        BCF_TopicStatus,
        BCF_TopicType,
        BCF_AssignedTo
    }
}
