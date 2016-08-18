using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.SmartBCF.AddIn.Util;
using HOK.SmartBCF.Schemas;
using System;
using System.Collections.Generic;
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
        BCF_Topic
    }

    public class BCFHandler : IExternalEventHandler
    {
        private UIApplication m_app = null;
        private Document m_doc = null;
        private View3D bcfView = null;
        private UIView bcfUIView = null;
        private Request m_request = new Request();
        private ComponentViewModel viewModel = null;
        private string addinDefinitionFile = "";
        private string databaseFile = "";
       
        public Document ActiveDoc { get { return m_doc; } set { m_doc = value; } }
        public View3D BCFView { get { return bcfView; } set { bcfView = value; } }
        public UIView BCFUIView { get { return bcfUIView; } set { bcfUIView = value; } }
        public Request Request { get { return m_request; } }
        public ComponentViewModel ViewModel { get { return viewModel; } set { viewModel = value; } }
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
                        CleanViewSettings(true, true, true);
                        if (viewModel.IsHighlightChecked)
                        {
                            HighlightElements();
                        }
                        if (ViewModel.IsIsolateChecked)
                        {
                            IsolateElement();
                        }
                        else if (viewModel.IsSectionBoxChecked)
                        {
                            PlaceSectionBox();
                        }
                        else
                        {
                            SetViewPointView();
                        }

                        break;
                    case RequestId.HighlightElement:
                        if (null != bcfView)
                        {
                            CleanViewSettings(true, false, false);
                            if (viewModel.IsHighlightChecked)
                            {
                                HighlightElements();
                            }
                        }
                        
                        break;
                    case RequestId.IsolateElement:
                        if (null != bcfView)
                        {
                            CleanViewSettings(false, true, true);

                            m_app.ActiveUIDocument.ActiveView = bcfView;
                            if (ViewModel.IsIsolateChecked) 
                            { 
                                IsolateElement(); 
                            }
                            else
                            {
                                SetViewPointView();
                            }
                        }
                        break;
                    case RequestId.PlaceSectionBox:
                        if (null != bcfView)
                        {
                            CleanViewSettings(false, true, true);

                            m_app.ActiveUIDocument.ActiveView = bcfView;
                            if (ViewModel.IsSectionBoxChecked) 
                            { 
                                PlaceSectionBox(); 
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
                        CleanViewSettings(true, true, true);

                        bool viewSet = SetViewPointView();
                        
                        break;
                    case RequestId.StoreToolSettings:
                        StoreToolSettings();
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

#if RELEASE2013||RELEASE2014
                        SelElementSet selElements = SelElementSet.Create();
                        selElements.Add(viewModel.SelectedComponent.RvtElement);
                       
                        uidoc.Selection.Elements = selElements;
                        //uidoc.ShowElements(selElements);

#elif RELEASE2015 || RELEASE2016

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

        private bool IsolateElement()
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
#if RELEASE2014||RELEASE2015||RELEASE2016
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

        private bool PlaceSectionBox()
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

#if RELEASE2013
                            bcfView.SectionBox = offsetBox;
                            bcfView.SectionBox.Enabled = true;
#else
                            bcfView.SetSectionBox(offsetBox);
                            bcfView.GetSectionBox().Enabled = true;
#endif
                            bcfUIView.ZoomAndCenterRectangle(offsetBox.Min, offsetBox.Max);

#if RELEASE2014||RELEASE2015||RELEASE2016
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
#if RELEASE2013||RELEASE2014
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
                VisualizationInfo visInfo = viewModel.SelectedMarkup.SelectedViewpoint.VisInfo;
                if (null == bcfView)
                {
                    bcfView = CreateDefaultView();
                }
                if (null == bcfUIView && null!= bcfView)
                {
                    m_app.ActiveUIDocument.ActiveView = bcfView;
                    bcfUIView = FindDefaultUIView();
                }

                if (null != bcfView && null != bcfUIView)
                {
                    bool orientationSet = SetViewOrientation(visInfo);
                    bool boundingBoxSet = SetViewPointBoundingBox();

                    updated = orientationSet && boundingBoxSet;
                    m_app.ActiveUIDocument.ActiveView = bcfView;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return updated;
        }

        private bool SetViewOrientation(VisualizationInfo visInfo)
        {
            bool orientationSet = false;

            bool isPerspective = (visInfo.PerspectiveCamera.FieldOfView != 0) ? true : false;
            HOK.SmartBCF.Schemas.Point viewPoint = (isPerspective) ? visInfo.PerspectiveCamera.CameraViewPoint : visInfo.OrthogonalCamera.CameraViewPoint;
            HOK.SmartBCF.Schemas.Direction direction = (isPerspective) ? visInfo.PerspectiveCamera.CameraDirection : visInfo.OrthogonalCamera.CameraDirection;
            HOK.SmartBCF.Schemas.Direction upVector = (isPerspective) ? visInfo.PerspectiveCamera.CameraUpVector : visInfo.OrthogonalCamera.CameraUpVector;

            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Set Orientation");
                try
                {
                    ViewOrientation3D orientation = new ViewOrientation3D(new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z), new XYZ(upVector.X, upVector.Y, upVector.Z), new XYZ(direction.X, direction.Y, direction.Z));
                    bcfView.SetOrientation(orientation);
                    if (!isPerspective) { bcfView.Scale = (int)visInfo.OrthogonalCamera.ViewToWorldScale; }

                    trans.Commit();
                    orientationSet = true;
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    string message = ex.Message;
                }
            }
            return orientationSet;
        }

        private bool SetViewPointBoundingBox()
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

#if RELEASE2013
                   bcfView.SectionBox = offsetBoundingBox;
#else
                    bcfView.SetSectionBox(offsetBoundingBox);
                    bcfView.GetSectionBox().Enabled = true;
#endif
                    bcfUIView.ZoomAndCenterRectangle(offsetBoundingBox.Min, offsetBoundingBox.Max);

#if RELEASE2014||RELEASE2015||RELEASE2016
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

        private bool CleanViewSettings(bool removeHighlight, bool removeIsolate, bool removeSectionBox)
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
#if RELEASE2013||RELEASE2014
                        SelElementSet selElements = SelElementSet.Create();
                        uidoc.Selection.Elements = selElements;
#elif RELEASE2015 || RELEASE2016
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
#if RELEASE2013
                    bcfView.SectionBox.Enabled = false;
#else
                        bcfView.GetSectionBox().Enabled = false;
#endif
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

        private View3D CreateDefaultView()
        {
            View3D view3D = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<View3D> view3ds = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                var viewfound = from view in view3ds where view.IsTemplate == false && view.IsPerspective == false && view.ViewName.Contains("SmartBCF") select view;
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
                                view3D.Name = "SmartBCF";
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

        private UIView FindDefaultUIView()
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

        public string GetName()
        {
            return "BCF Event Handler";
        }
    }

    public enum RequestId : int
    {
        None = 0, SetViewPointView = 1, ApplyViews = 2, HighlightElement = 3, IsolateElement = 4, PlaceSectionBox = 5, WriteParameters = 6, StoreToolSettings = 7
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
