using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.SmartBCF.AddIn.Util;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SmartBCF.AddIn
{
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

    public class BCFHandler : IExternalEventHandler
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private View3D bcfOrthoView = null;
        private View3D bcfPersView = null;
        private UIView bcfUIView = null;
        private Request m_request = new Request();
        private ComponentViewModel viewModel = null;
        private AddViewModel addViewModel = null;

        private string addinDefinitionFile = "";
        private string databaseFile = "";
       
        public Document ActiveDoc { get { return m_doc; } set { m_doc = value; } }
        public View3D BCFOrthoView { get { return bcfOrthoView; } set { bcfOrthoView = value; } }
        public View3D BCFPersView { get { return bcfPersView; } set { bcfPersView = value; } }
        public UIView BCFUIView { get { return bcfUIView; } set { bcfUIView = value; } }
        public Request Request { get { return m_request; } }
        public ComponentViewModel ViewModel { get { return viewModel; } set { viewModel = value; } }
        public AddViewModel ViewPointViewModel { get { return addViewModel; } set { addViewModel = value; } }

        public string DatabaseFile { get { return databaseFile; } set { databaseFile = value; } }

        public BCFHandler(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;

            addinDefinitionFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Resources/Addins Shared Parameters.txt";
            //collect information of linked instances
        }

        public void Execute(UIApplication app)
        {
            m_doc = app.ActiveUIDocument.Document;
            try
            {
                switch (Request.Take())
                {
                    case RequestId.ApplyViews:
                        //when component selection changed
                        if (null != bcfOrthoView)
                        {
                            CleanViewSettings(bcfOrthoView, true, true, true);
                            
                            SetDefaultView();
                            if (viewModel.IsHighlightChecked)
                            {
                                HighlightElements();
                            }
                            if (ViewModel.IsIsolateChecked)
                            {
                                IsolateElement(bcfOrthoView);
                            }
                            else if (viewModel.IsSectionBoxChecked)
                            {
                                PlaceSectionBox(bcfOrthoView);
                            }
                        }
                        break;
                    case RequestId.HighlightElement:
                         if (null != bcfOrthoView)
                        {
                            CleanViewSettings(bcfOrthoView, true, false, false);
                            if (viewModel.IsHighlightChecked)
                            {
                                HighlightElements();
                            }
                        }
                        break;
                    case RequestId.IsolateElement:
                        if (null != bcfOrthoView)
                        {
                            CleanViewSettings(bcfOrthoView, false, true, true);

                            SetDefaultView();
                            if (ViewModel.IsIsolateChecked)
                            {
                                IsolateElement(bcfOrthoView);
                            }
                            else
                            {
                                SetViewPointView();
                            }
                        }
                        break;
                    case RequestId.PlaceSectionBox:
                         if (null != bcfOrthoView)
                         {
                             CleanViewSettings(bcfOrthoView, false, true, true);

                             SetDefaultView();
                             if (ViewModel.IsSectionBoxChecked)
                             {
                                 PlaceSectionBox(bcfOrthoView);
                             }
                             else
                             {
                                 SetViewPointView();
                             }
                         }
                        break;
                    case RequestId.WriteParameters:
                        WriteParameters();
                        break;
                    case RequestId.SetViewPointView:
                        //when markup topic changed
                        bool viewSet = SetViewPointView();
                        
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

        private bool HighlightElements()
        {
            bool highlighted = false;
            if (null != viewModel.SelectedComponent)
            {
                try
                {
                    UIDocument uidoc = new UIDocument(m_doc);

#if RELEASE2014
                        SelElementSet selElements = SelElementSet.Create();
                        selElements.Add(viewModel.SelectedComponent.RvtElement);
                       
                        uidoc.Selection.Elements = selElements;
                        //uidoc.ShowElements(selElements);

#elif RELEASE2015 || RELEASE2016 ||RELEASE2017

                    List<ElementId> selectedIds = new List<ElementId>();
                    selectedIds.Add(viewModel.SelectedComponent.ElementId);

                    uidoc.Selection.SetElementIds(selectedIds);
                    //uidoc.ShowElements(selectedIds);
#endif
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
            
            return highlighted;
        }

        private bool IsolateElement(View3D bcfView)
        {
            bool isolated = false;

            RevitComponent selectedComponent = viewModel.SelectedComponent;
            if (null != selectedComponent)
            {
                using (Transaction trans = new Transaction(m_doc))
                {
                    trans.Start("Isolate View");
                    try
                    {
                        bcfView.IsolateElementTemporary(viewModel.SelectedComponent.ElementId);

                        Element element = viewModel.SelectedComponent.RvtElement;
                        if (null != element)
                        {
                            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
                            if (null != boundingBox)
                            {
                                bcfUIView.ZoomAndCenterRectangle(boundingBox.Min, boundingBox.Max);
#if RELEASE2014||RELEASE2015||RELEASE2016||RELEASE2017
                                bcfUIView.Zoom(0.8);
#endif
                            }
                        }
                        trans.Commit();
                        isolated = true;
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        string message = ex.Message;
                    }
                }
            }
            return isolated;
        }

        private bool PlaceSectionBox(View3D bcfView)
        {
            bool placed = false;
            RevitComponent selectedComponent = viewModel.SelectedComponent;
            if (null != selectedComponent)
            {
                using (Transaction trans = new Transaction(m_doc))
                {
                    trans.Start("Set Section Box");
                    try
                    {
                        Element element = selectedComponent.RvtElement;
                        if (null != element)
                        {
                            Transform transform = selectedComponent.TransformValue;
                            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
                            XYZ minXYZ = transform.OfPoint(boundingBox.Min);
                            XYZ maxXYZ = transform.OfPoint(boundingBox.Max);
                            

                            BoundingBoxXYZ offsetBox = new BoundingBoxXYZ();
                            offsetBox.Min = new XYZ(minXYZ.X - 3, minXYZ.Y - 3, minXYZ.Z - 3);
                            offsetBox.Max = new XYZ(maxXYZ.X + 3, maxXYZ.Y + 3, maxXYZ.Z + 3);

                            bcfView.SetSectionBox(offsetBox);
                            bcfView.GetSectionBox().Enabled = true;

                            bcfUIView.ZoomAndCenterRectangle(offsetBox.Min, offsetBox.Max);

#if RELEASE2014||RELEASE2015||RELEASE2016||RELEASE2017
                            bcfUIView.Zoom(0.8);
#endif
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
            return placed;
        }

        private bool WriteParameters()
        {
            bool updated = false;
            try
            {
                RevitComponent selectedComponent = viewModel.SelectedComponent;
                if (null != selectedComponent)
                {
                    if (!selectedComponent.IsLinked)
                    {
                        Element element = selectedComponent.RvtElement;
                        if (null != element)
                        {
                            using (Transaction trans = new Transaction(m_doc))
                            {
                                trans.Start("Write Parameters");
                                try
                                {
                                    bool updatedName = WriteParameter(element, BCFParameters.BCF_Name, viewModel.SelectedBCF.ZipFileName);
                                    bool updatedTopic = WriteParameter(element, BCFParameters.BCF_Topic, viewModel.SelectedMarkup.Topic.Title);
                                    bool updatedTopicType = WriteParameter(element, BCFParameters.BCF_TopicType, viewModel.SelectedMarkup.Topic.TopicType.ToString());
                                    bool updatedTopicStatus = WriteParameter(element, BCFParameters.BCF_TopicStatus, viewModel.SelectedMarkup.Topic.TopicStatus.ToString());
                                    bool updatedAssignedTo = WriteParameter(element, BCFParameters.BCF_AssignedTo, viewModel.SelectedMarkup.Topic.AssignedTo);
                                    bool updatedComment = WriteParameter(element, BCFParameters.BCF_Comment, viewModel.SelectedComment.Comment1);
                                    bool updatedAuthor = WriteParameter(element, BCFParameters.BCF_Author, viewModel.SelectedComment.ModifiedAuthor);
                                    bool updatedDate = WriteParameter(element, BCFParameters.BCF_Date, viewModel.SelectedComment.ModifiedDate.ToString());
                                    bool updatedAction = WriteParameter(element, BCFParameters.BCF_Action, selectedComponent.Action.ParameterValue);
                                    bool updatedResponsibility = WriteParameter(element, BCFParameters.BCF_Responsibility, selectedComponent.Responsibility.ParameterValue);

                                    trans.Commit();
                                    updated = true;
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    string message = ex.Message;
                                }
                            }
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        private bool WriteParameter(Element element, BCFParameters bcfParam, string value)
        {
            bool updated = false;
            try
            {
#if RELEASE2014
                 Parameter param = element.get_Parameter(bcfParam.ToString());
#else
                Parameter param = element.LookupParameter(bcfParam.ToString());
#endif
                if (null != param)
                {
                    if (!param.IsReadOnly)
                    {
                        updated = param.Set(value);
                    }
                }
                else
                {
                    bool insertedParam = InsertBinding(bcfParam, element.Category);
                    if (insertedParam)
                    {
                        updated = WriteParameter(element, bcfParam, value);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        private bool InsertBinding(BCFParameters bcfParam, Category category)
        {
            bool inserted = false;
            try
            {
                DefinitionBindingMapIterator iter = m_doc.ParameterBindings.ForwardIterator();
                Definition definitionFound = null;
                while (iter.MoveNext())
                {
                    Definition definition = iter.Key;
                    ElementBinding elemBinding = (ElementBinding)iter.Current;
                    if (definition.Name == bcfParam.ToString())
                    {
                        definitionFound = definition;
                    }
                }

                if (category.AllowsBoundParameters)
                {
                    if (null != definitionFound)
                    {
                        ElementBinding elementBinding = (ElementBinding)m_doc.ParameterBindings.get_Item(definitionFound);
                        if (null != elementBinding)
                        {
                            CategorySet catset = elementBinding.Categories;
                            catset.Insert(category);

                            InstanceBinding binding = m_app.Application.Create.NewInstanceBinding(catset);
                            inserted = m_doc.ParameterBindings.ReInsert(definitionFound, binding);
                        }
                    }
                    else
                    {
                       
                        string originalDefinitionFile = m_app.Application.SharedParametersFilename;
                        m_app.Application.SharedParametersFilename = addinDefinitionFile;
                        DefinitionFile definitionFile = m_app.Application.OpenSharedParameterFile();
                        foreach (DefinitionGroup group in definitionFile.Groups)
                        {
                            if (group.Name == "HOK BCF")
                            {
                                foreach (Definition definition in group.Definitions)
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
                            CategorySet catset = m_app.Application.Create.NewCategorySet();
                            catset.Insert(category);

                            InstanceBinding binding = m_app.Application.Create.NewInstanceBinding(catset);
                            inserted = m_doc.ParameterBindings.Insert(definitionFound, binding);
                        }

                        m_app.Application.SharedParametersFilename = originalDefinitionFile;
                    }
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return inserted;
        }

        private bool SetViewPointView()
        {
            bool updated = false;
            try
            {
                //refresh background view
                if (null == bcfPersView)
                {
                    bcfPersView = CreateDefaultPersView();
                }
                if (null == bcfOrthoView)
                {
                    bcfOrthoView = CreateDefaultOrthoView();
                }

                SetDefaultView();
                VisualizationInfo visInfo = viewModel.SelectedMarkup.SelectedViewpoint.VisInfo;
                if (viewModel.RvtComponents.Count > 0)
                {
                    //create orthoview to iterate elements
                    CleanViewSettings(bcfOrthoView, true, true, true);

                    bcfUIView = FindDefaultUIView(bcfOrthoView);

                    if (visInfo.IsPersepective)
                    {
                        updated = SetOrthogonalView(bcfOrthoView, visInfo.PerspectiveCamera);
                    }
                    else
                    {
                        updated = SetOrthogonalView(bcfOrthoView, visInfo.OrthogonalCamera);
                    }
                }
                else
                {
                    if (visInfo.IsPersepective && null != bcfPersView)
                    {
                        //PerspectiveCamera
                        CleanViewSettings(bcfPersView, true, true, true);

                        updated = SetPerspectiveView(bcfPersView, visInfo.PerspectiveCamera);
                        m_app.ActiveUIDocument.ActiveView = bcfPersView;
                        bcfUIView = FindDefaultUIView(bcfPersView);    
                    }
                    else if (!visInfo.IsPersepective && null != bcfOrthoView)
                    {
                        //OrthogonalCamera
                        CleanViewSettings(bcfOrthoView, true, true, true);

                        bcfUIView = FindDefaultUIView(bcfOrthoView);

                        updated = SetOrthogonalView(bcfOrthoView, visInfo.OrthogonalCamera);
                    }
                }

                m_app.ActiveUIDocument.RefreshActiveView();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        public bool SetDefaultView()
        {
            bool result = false;
            try
            {
                if (null != bcfOrthoView)
                {
                    m_app.ActiveUIDocument.ActiveView = bcfOrthoView;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private bool SetOrthogonalView(View3D bcfView, OrthogonalCamera camera)
        {
            bool result = false;
            try
            {
                double zoom = camera.ViewToWorldScale.ToFeet();
                XYZ direction = RevitUtils.GetRevitXYZ(camera.CameraDirection);
                XYZ upVector = RevitUtils.GetRevitXYZ(camera.CameraUpVector);
                XYZ viewPoint = RevitUtils.GetRevitXYZ(camera.CameraViewPoint);
                ViewOrientation3D orientation = RevitUtils.ConvertBasePoint(m_doc, viewPoint, direction, upVector, true);

                using (var trans = new Transaction(m_doc))
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
                        string message = ex.Message;
                    }
                }

                XYZ m_xyzTl = bcfView.Origin.Add(bcfView.UpDirection.Multiply(zoom)).Subtract(bcfView.RightDirection.Multiply(zoom));
                XYZ m_xyzBr = bcfView.Origin.Subtract(bcfView.UpDirection.Multiply(zoom)).Add(bcfView.RightDirection.Multiply(zoom));
                bcfUIView.ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);

                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        //orthogonalview by PerspectiveCamera
        private bool SetOrthogonalView(View3D bcfView, PerspectiveCamera camera)
        {
            bool result = false;
            try
            {
                XYZ direction = RevitUtils.GetRevitXYZ(camera.CameraDirection);
                XYZ upVector = RevitUtils.GetRevitXYZ(camera.CameraUpVector);
                XYZ viewPoint = RevitUtils.GetRevitXYZ(camera.CameraViewPoint);
                ViewOrientation3D orientation = RevitUtils.ConvertBasePoint(m_doc, viewPoint, direction, upVector, true);

                using (var trans = new Transaction(m_doc))
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
                        string message = ex.Message;
                    }
                }

                bool boundingBoxSet = SetViewPointBoundingBox(bcfView);

                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private bool SetPerspectiveView(View3D bcfView,  PerspectiveCamera camera)
        {
            bool result = false;
            try
            {
                double zoom = camera.FieldOfView;
                XYZ direction = RevitUtils.GetRevitXYZ(camera.CameraDirection);
                XYZ upVector = RevitUtils.GetRevitXYZ(camera.CameraUpVector);
                XYZ viewPoint = RevitUtils.GetRevitXYZ(camera.CameraViewPoint);
                ViewOrientation3D orientation = RevitUtils.ConvertBasePoint(m_doc, viewPoint, direction, upVector, true);


                using (var trans = new Transaction(m_doc))
                {
                    trans.Start("Set Orientation");
                    try
                    {
#if RELEASE2015|| RELEASE2016 || RELEASE2017
                        if (bcfView.CanResetCameraTarget())
                        {
                            bcfView.ResetCameraTarget();
                        }
#endif

                        bcfView.SetOrientation(orientation);

                        if (bcfView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).HasValue)
                        {
                            Parameter m_farClip = bcfView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
                            m_farClip.Set(0); 
                        }

                        bcfView.CropBoxActive = true;
                        bcfView.CropBoxVisible = true;

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        string message = ex.Message;
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private bool SetViewPointBoundingBox(View3D bcfView)
        {
            bool boundingBoxSet = false;

            if (viewModel.RvtComponents.Count == 0) { return boundingBoxSet; }

            BoundingBoxXYZ offsetBoundingBox = new BoundingBoxXYZ();

            using (Transaction trans = new Transaction(m_doc))
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
                    bool firstLoop = true;

                    foreach (RevitComponent rvtComp in viewModel.RvtComponents)
                    {
                        Element element = rvtComp.RvtElement;
                        if (null != element)
                        {
                            BoundingBoxXYZ bb = element.get_BoundingBox(null);
                            if (null != bb)
                            {
                                Transform transform = rvtComp.TransformValue;

                                XYZ minXYZ = transform.OfPoint(bb.Min);
                                XYZ maxXYZ = transform.OfPoint(bb.Max);

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

                    bcfUIView.ZoomAndCenterRectangle(offsetBoundingBox.Min, offsetBoundingBox.Max);

#if RELEASE2014||RELEASE2015||RELEASE2016||RELEASE2017
                    bcfUIView.Zoom(0.8);
#endif

                    trans.Commit();
                    boundingBoxSet = true;
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    string message = ex.Message;
                }
            }
            return boundingBoxSet;
        }

        private bool CleanViewSettings(View3D bcfView, bool removeHighlight, bool removeIsolate, bool removeSectionBox)
        {
            bool cleaned = false;
            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Clean Views");
                try
                {
                    UIDocument uidoc = new UIDocument(m_doc);
                    if (removeHighlight)
                    {
                        //remove selection
#if RELEASE2014
                        SelElementSet selElements = SelElementSet.Create();
                        uidoc.Selection.Elements = selElements;
#elif RELEASE2015 || RELEASE2016||RELEASE2017
                        uidoc.Selection.SetElementIds(new List<ElementId>());
#endif
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
                    cleaned = true;
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    string message = ex.Message;
                }
            }
            return cleaned;
        }

        private View3D CreateDefaultOrthoView()
        {
            View3D view3D = null;
            try
            {
                string viewName = "SmartBCF - Orthogonal - " + Environment.UserName;
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == false && view.ViewName == viewName select view;
                if (viewfound.Count() > 0)
                {
                    view3D = viewfound.First();
                }
                else
                {
                    ElementId viewFamilyTypeId = GetViewFamilyTypeId();
                    if (viewFamilyTypeId != ElementId.InvalidElementId)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Create View");
                            try
                            {
                                view3D = View3D.CreateIsometric(m_doc, viewFamilyTypeId);
                                view3D.Name = viewName;
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return view3D;
        }

        private View3D CreateDefaultPersView()
        {
            View3D view3D = null;
            try
            {
                string viewName = "SmartBCF - Perspective - " + Environment.UserName;
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                //by the limitation of perspective view, create isometric instead
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == true && view.ViewName == viewName select view;
                if (viewfound.Count() > 0)
                {
                    view3D = viewfound.First();
                }
                else
                {
                    ElementId viewFamilyTypeId = GetViewFamilyTypeId();
                    if (viewFamilyTypeId != ElementId.InvalidElementId)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Create View");
                            try
                            {
                                view3D = View3D.CreatePerspective(m_doc, viewFamilyTypeId);
                                view3D.Name = viewName;
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string messge = ex.Message;
            }
            return view3D;
        }

        private UIView FindDefaultUIView(View3D bcfView)
        {
            UIView uiview = null;
            try
            {
                UIDocument uidoc = new UIDocument(m_doc);
                IList<UIView> uiviews = uidoc.GetOpenUIViews();
                var viewFound = from view in uiviews where view.ViewId == bcfView.Id select view;
                if (viewFound.Count() > 0)
                {
                    uiview = viewFound.First();
                }   
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return uiview;
        }

        private ElementId GetViewFamilyTypeId()
        {
            ElementId viewTypeId = ElementId.InvalidElementId;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewFamilyType> viewFamilyTypes = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
                var vTypes = from vType in viewFamilyTypes where vType.ViewFamily == ViewFamily.ThreeDimensional select vType;
                if (vTypes.Count() > 0)
                {
                    ViewFamilyType vfType = vTypes.First();
                    viewTypeId = vfType.Id;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return viewTypeId;
        }

        private bool StoreToolSettings()
        {
            bool stored = false;
            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Store Settings");
                try
                {
                    stored = DataStorageUtil.UpdateLinkedDatabase(m_doc, databaseFile);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    string message = ex.Message;
                }
            }
            return stored;
        }

        private bool ExportImage()
        {
            bool exported = false;
            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Export Image");
                try
                {
                    
                    string imgFileName = Path.Combine(Path.GetTempPath(), "SmartBCF", Path.GetTempFileName() + ".png");
                    if (File.Exists(imgFileName))
                    {
                        File.Delete(imgFileName);
                    }

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

                    m_doc.ExportImage(options);
                    trans.Commit();

                    bool defined = SetViewPoint(imgFileName);
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    string message = ex.Message;
                }
            }
            return exported;
        }

        private bool SetViewPoint(string imageFile)
        {
            bool result = false;
            try
            {
                int count = 5;
                while (!File.Exists(imageFile))
                {
                    count--;
                    if (count < 0) { break; }
                    System.Threading.Thread.Sleep(100);
                }
                
                if (File.Exists(imageFile))
                {
                    ViewPoint vp = new ViewPoint();
                    vp.Guid = Guid.NewGuid().ToString();
                    vp.TopicGuid = addViewModel.SelectedMarkup.Topic.Guid;
                    vp.Snapshot = Path.GetFileName(imageFile);
                    vp.SnapshotImage = ImageUtil.GetImageArray(imageFile);

                    VisualizationInfo visInfo = new VisualizationInfo();
                    visInfo.ViewPointGuid = vp.Guid;
                    //update components
                    visInfo.Components = GetElements(vp.Guid);

                    //update camera
                    View3D currentView = m_doc.ActiveView as View3D;
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
                    addViewModel.UserViewPoint = vp;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private ObservableCollection<Component> GetElements(string viewpoint_Guid)
        {
            ObservableCollection<Component> components = new ObservableCollection<Component>();
            try
            {
                switch (addViewModel.SelectedOption)
                {
                    case Utils.ComponentOption.SelectedElements:
                        ICollection<ElementId> selectedIds = m_app.ActiveUIDocument.Selection.GetElementIds();
                        foreach (ElementId eId in selectedIds)
                        {
                            Element element = m_doc.GetElement(eId);
                            if (null != element)
                            {
                                Component comp = new Component();
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
                        View currentView = m_doc.ActiveView;
                        FilteredElementCollector collector = new FilteredElementCollector(m_doc, currentView.Id);
                        ICollection<Element> elements = collector.ToElements();
                        foreach (Element element in elements)
                        {
                            Component comp = new Component();
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
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return components;
        }

        private PerspectiveCamera GetPerspectiveCamera(View3D view, string viewpoint_Guid)
        {
            PerspectiveCamera camera = new PerspectiveCamera();
            try
            {
                XYZ viewCenter = view.Origin;
                double zoomValue = 45;

                ViewOrientation3D orientation = RevitUtils.ConvertBasePoint(m_doc, viewCenter, view.ViewDirection, view.UpDirection, false);

                XYZ c = orientation.EyePosition;
                XYZ vi = orientation.ForwardDirection;
                XYZ up = orientation.UpDirection;

                camera.Guid = Guid.NewGuid().ToString();
                camera.ViewPointGuid = viewpoint_Guid;

                HOK.SmartBCF.Schemas.Point viewPoint = new Schemas.Point()
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = c.X.ToMeters(),
                    Y = c.Y.ToMeters(),
                    Z = c.Z.ToMeters()
                };
                camera.CameraViewPoint = viewPoint;

                Direction upVector = new Direction()
                {
                    Guid = Guid.NewGuid().ToString(),
                    X=up.X.ToMeters(),
                    Y=up.Y.ToMeters(),
                    Z=up.Z.ToMeters()
                };
                camera.CameraUpVector = upVector;

                Direction direction = new Direction()
                {
                    Guid = Guid.NewGuid().ToString(),
                    X= -(vi.X.ToMeters()),
                    Y=-(vi.Y.ToMeters()),
                    Z=-(vi.Z.ToMeters())
                };
                camera.CameraDirection = direction;

                camera.FieldOfView = zoomValue;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return camera;
        }

        private OrthogonalCamera GetOrthogonalCamera(View3D view, string viewpoint_Guid)
        {
            OrthogonalCamera camera = new OrthogonalCamera();
            try
            {

                UIView uiView = FindDefaultUIView(view); if (null == uiView) { return camera; }
                XYZ topLeft = uiView.GetZoomCorners()[0];
                XYZ bottomRight = uiView.GetZoomCorners()[1];

                double x = (topLeft.X + bottomRight.X) / 2;
                double y = (topLeft.Y + bottomRight.Y) / 2;
                double z = (topLeft.Z + bottomRight.Z) / 2;

                XYZ viewCenter = new XYZ(x, y, z);

                XYZ diagVector = topLeft.Subtract(bottomRight);
                double dist = topLeft.DistanceTo(bottomRight) / 2;

                double zoomValue = dist * Math.Sin(diagVector.AngleTo(view.RightDirection)).ToMeters();

                ViewOrientation3D orientation = RevitUtils.ConvertBasePoint(m_doc, viewCenter, view.ViewDirection, view.UpDirection, false);

                XYZ c = orientation.EyePosition;
                XYZ vi = orientation.ForwardDirection;
                XYZ up = orientation.UpDirection;

                camera.Guid = Guid.NewGuid().ToString();
                camera.ViewPointGuid = viewpoint_Guid;
                HOK.SmartBCF.Schemas.Point viewPoint = new Schemas.Point()
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = c.X.ToMeters(),
                    Y = c.Y.ToMeters(),
                    Z = c.Z.ToMeters()
                };
                camera.CameraViewPoint = viewPoint;

                Direction upVector = new Direction()
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = up.X.ToMeters(),
                    Y = up.Y.ToMeters(),
                    Z = up.Z.ToMeters()
                };
                camera.CameraUpVector = upVector;

                Direction direction = new Direction()
                {
                    Guid = Guid.NewGuid().ToString(),
                    X = -(vi.X.ToMeters()),
                    Y = -(vi.Y.ToMeters()),
                    Z = -(vi.Z.ToMeters())
                };
                camera.CameraDirection = direction;

                camera.ViewToWorldScale = zoomValue;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return camera;
        }

       
        public string GetName()
        {
            return "BCF Event Handler";
        }
    }

    public enum RequestId : int
    {
        None = 0, SetViewPointView = 1, ApplyViews = 2, HighlightElement = 3, IsolateElement = 4, PlaceSectionBox = 5, WriteParameters = 6, StoreToolSettings = 7, ExportImage = 8
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

}
