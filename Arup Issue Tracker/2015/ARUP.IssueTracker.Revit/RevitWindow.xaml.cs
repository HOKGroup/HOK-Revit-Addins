using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;
using System.IO;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using ARUP.IssueTracker.Revit.Entry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Classes.BCF2;
using System.ComponentModel;
using ARUP.IssueTracker.Windows;
using System.Windows.Controls;
using System.Text;

namespace ARUP.IssueTracker.Revit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RevitWindow : Window
    {
        private ExternalEvent m_ExEvent;
        private ExtOpenView m_Handler;
        public UIApplication uiapp;
        private CommentController commentController;
        private ComponentController componentController;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_uiapp"></param>
        /// <param name="exEvent"></param>
        /// <param name="handler"></param>
        public RevitWindow(UIApplication _uiapp, ExternalEvent exEvent, ExtOpenView handler)
        {
            try
            {
                InitializeComponent();

                m_ExEvent = exEvent;
                m_Handler = handler;
                uiapp = _uiapp;

                mainPan.jiraPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueJira);
                mainPan.bcfPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueBCF);

                mainPan.bcfPan.open3dViewEvent += new RoutedEventHandler(Open3dViewBCF);
                mainPan.jiraPan.open3dViewEvent += new RoutedEventHandler(Open3dViewJira);

                // for ICommentController callback
                commentController = new CommentController(this);
                commentController.client = AuthoringTool.Revit;
                mainPan.jiraPan.AddCommBtn.Tag = commentController;
                mainPan.bcfPan.AddCommBtn.Tag = commentController;

                // for IComponentController
                componentController = new ComponentController(this);
                mainPan.componentController = this.componentController;

                // for open 3d view and show components
                //mainPan.jiraPan.open3dView.Visibility = System.Windows.Visibility.Visible;
                //mainPan.jiraPan.showComponents.Visibility = System.Windows.Visibility.Visible;
                //mainPan.bcfPan.isShowBcfFirstViewpointButtons = true;

            }

            catch (Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }

        }

        /// <summary>
        /// Add Issue to Jira
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddIssueJira(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
                Tuple<Markup, Issue> tup = AddIssue(path, false);
                if (tup == null)
                    return;
                Markup issue = tup.Item1;
                Issue issueJira = tup.Item2;

                List<Markup> issues = new List<Markup>();
                List<Issue> issuesJira = new List<Issue>();

                issues.Add(issue);
                issuesJira.Add(issueJira);

                if (issue != null)
                    mainPan.doUploadIssue(issues, path, true, mainPan.jiraPan.projIndex, issuesJira);
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }

        }

        /// <summary>
        /// Add Issue to BCF
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddIssueBCF(object sender, EventArgs e)
        {
            try
            {
                Tuple<Markup, Issue> tup = AddIssue(mainPan.jira.Bcf.TempPath, true);
                if (tup == null)
                    return;
                Markup issue = tup.Item1;

                if (issue != null)
                {
                    mainPan.jira.Bcf.Issues.Add(issue);
                    mainPan.jira.Bcf.HasBeenSaved = false;
                    mainPan.bcfPan.issueList.SelectedIndex = mainPan.jira.Bcf.Issues.Count - 1;
                }
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        /// <summary>
        /// Add Issue
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isBcf"></param>
        /// <returns></returns>
        private Tuple<Markup, Issue> AddIssue(string path, bool isBcf)
        {
            try
            {
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                if (!(uidoc.ActiveView is View3D || uidoc.ActiveView is ViewSheet || uidoc.ActiveView is ViewPlan || uidoc.ActiveView is ViewSection || uidoc.ActiveView is ViewDrafting))
                {
                    MessageBox.Show("I'm sorry,\nonly 3D and 2D views are supported.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                Markup issue = new Markup(DateTime.Now);

                string folderIssue = Path.Combine(path, issue.Topic.Guid);
                if (!Directory.Exists(folderIssue))
                    Directory.CreateDirectory(folderIssue);

                var types = new ObservableCollection<Issuetype>();
                var assignees = new List<User>();
                var components = new ObservableCollection<IssueTracker.Classes.Component>();
                var priorities = new ObservableCollection<Priority>();
                var noCom = true;
                var noPrior = true;
                var noAssign = true;

                if (!isBcf)
                {
                    types = mainPan.jira.TypesCollection;
                    assignees = mainPan.getAssigneesIssue();
                    components = mainPan.jira.ComponentsCollection;
                    priorities = mainPan.jira.PrioritiesCollection;
                    noCom =
                        mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.components ==
                        null;
                    noPrior =
                        mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.priority ==
                        null;
                    noAssign =
                        mainPan.jira.ProjectsCollection[mainPan.jiraPan.projIndex].issuetypes[0].fields.assignee ==
                        null;

                }

                AddIssueRevit air = new AddIssueRevit(uidoc, folderIssue, types, assignees, components, priorities, noCom, noPrior, noAssign);
                air.Title = "Add Jira Issue";
                if (!isBcf)
                {
                    air.JiraFieldsBox.Visibility = System.Windows.Visibility.Visible;
                    air.VerbalStatus.Visibility = System.Windows.Visibility.Collapsed;
                    air.BcfFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    air.JiraFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
                    air.BcfFieldsBox.Visibility = System.Windows.Visibility.Visible;
                }

                air.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                air.ShowDialog();
                if (air.DialogResult.HasValue && air.DialogResult.Value)
                {
                    ViewPoint vp = new ViewPoint(true);
                    vp.SnapshotPath = Path.Combine(folderIssue, "snapshot.png");
                    int elemCheck = 2;
                    if (air.all.IsChecked.Value)
                        elemCheck = 0;
                    else if (air.selected.IsChecked.Value)
                        elemCheck = 1;
                    vp.VisInfo = generateViewpoint(elemCheck);

                    //Add annotations for description with snapshot/viewpoint
                    StringBuilder descriptionText = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(air.CommentBox.Text))
                    {
                        descriptionText.AppendLine(air.CommentBox.Text);
                    }
                    if (!isBcf)
                    {
                        if (vp.VisInfo != null)
                        {
                            descriptionText.AppendLine(string.Format("<Viewpoint>[^{0}]</Viewpoint>", "viewpoint.bcfv"));
                        }
                        if (!string.IsNullOrWhiteSpace(vp.SnapshotPath))
                        {
                            descriptionText.AppendLine(string.Format("<Snapshot>[^{0}]</Snapshot>", "snapshot.png"));
                            descriptionText.AppendLine(string.Format("!{0}|width=200!", "snapshot.png"));
                        }
                    }

                    Issue issueJira = new Issue();
                    if (!isBcf)
                    {
                        issueJira.fields = new Fields();
                        issueJira.fields.description = descriptionText.ToString().Trim();
                        issueJira.fields.issuetype = (Issuetype)air.issueTypeCombo.SelectedItem;
                        issueJira.fields.priority = (Priority)air.priorityCombo.SelectedItem;
                        if (!string.IsNullOrEmpty(air.ChangeAssign.Content.ToString()) &&
                            air.ChangeAssign.Content.ToString() != "none")
                        {
                            issueJira.fields.assignee = new User();
                            issueJira.fields.assignee.name = air.ChangeAssign.Content.ToString();
                        }

                        if (air.SelectedComponents != null && air.SelectedComponents.Any())
                        {
                            issueJira.fields.components = air.SelectedComponents;
                        }
                    }

                    issue.Viewpoints.Add(vp);
                    issue.Topic.Title = air.TitleBox.Text;
                    issue.Topic.Description = descriptionText.ToString().Trim();
                    issue.Topic.AssignedTo = air.BcfAssignee.Text;
                    issue.Topic.CreationAuthor = MySettings.Get("username");
                    issue.Topic.Priority = air.BcfPriority.Text;
                    issue.Topic.TopicStatus = air.VerbalStatus.Text;
                    issue.Topic.TopicType = air.BcfIssueType.Text;

                    issue.Header[0].IfcProject = ExporterIFCUtils.CreateProjectLevelGUID(doc,
                        Autodesk.Revit.DB.IFC.IFCProjectLevelGUIDType.Project);
                    string projFilename = (doc.PathName != null && doc.PathName != "")
                        ? System.IO.Path.GetFileName(doc.PathName)
                        : "";
                    issue.Header[0].Filename = projFilename;
                    issue.Header[0].Date = DateTime.Now;

                    return new Tuple<Markup, Issue>(issue, issueJira);
                }
                else
                {
                    mainPan.DeleteDirectory(folderIssue);
                }

            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
            return null;

        }

        /// <summary>
        /// Open 3D View - BCF
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open3dViewBCF(object sender, EventArgs e)
        {
            try
            {
                VisualizationInfo VisInfo = (VisualizationInfo)((Button)sender).Tag;
                doOpen3DView(VisInfo);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        /// <summary>
        /// Open 3D View - Jira
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open3dViewJira(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = (string)((Button)sender).Tag;
                VisualizationInfo v = mainPan.getVisInfo(url);
                if (v == null)
                {
                    MessageBox.Show("Failed to open the viewpoint", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                doOpen3DView(v);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }

        /// <summary>
        /// Open a 3D View
        /// </summary>
        /// <param name="v"></param>
        private void doOpen3DView(VisualizationInfo v)
        {
            try
            {
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                if (uidoc.ActiveView.ViewType == ViewType.ThreeD)
                {
                    View3D view3D = (View3D)uidoc.ActiveView;
                    if (view3D.IsPerspective) //ORTHO
                    {
                        MessageBox.Show("This operation is not allowed in a Perspective View.\nPlease close the current window(s) and retry.",
                            "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                }
                m_Handler.v = v;

                //var touple = GetViewCoordinates(doc, v);
                //if (touple == null)
                //{
                //    MessageBox.Show("Viewpoint not formatted correctly.", "Viewpoint Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                //m_Handler.touple = touple;

                m_ExEvent.Raise();
            }
            catch (System.Exception ex1)
            {
                TaskDialog.Show("Error!", "exception: " + ex1);
            }
        }


        #region viewpoint operations

        /// <summary>
        /// Generate Viewpoint
        /// </summary>
        /// <param name="elemCheck"></param>
        /// <returns></returns>
        public VisualizationInfo generateViewpoint(int elemCheck)
        {

            try
            {
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                VisualizationInfo v = new VisualizationInfo();

                XYZ centerIMP = new XYZ();
                string type = "";
                double zoomValue = 1;

                if (uidoc.ActiveView.ViewType != ViewType.ThreeD) //is a 2D view
                {
                    XYZ TL = uidoc.GetOpenUIViews()[0].GetZoomCorners()[0];
                    XYZ BR = uidoc.GetOpenUIViews()[0].GetZoomCorners()[1];
                    v.SheetCamera = new SheetCamera();
                    v.SheetCamera.SheetID = uidoc.ActiveView.Id.IntegerValue;
                    v.SheetCamera.TopLeft = new IssueTracker.Classes.BCF2.Point(TL.X, TL.Y, TL.Z);
                    v.SheetCamera.BottomRight = new IssueTracker.Classes.BCF2.Point(BR.X, BR.Y, BR.Z);
                }
                else
                {
                    View3D view3D = (View3D)uidoc.ActiveView;
                    if (!view3D.IsPerspective) //IS ORTHO
                    {
                        XYZ TL = uidoc.GetOpenUIViews()[0].GetZoomCorners()[0];
                        XYZ BR = uidoc.GetOpenUIViews()[0].GetZoomCorners()[1];

                        double xO = (TL.X + BR.X) / 2;
                        double yO = (TL.Y + BR.Y) / 2;
                        double zO = (TL.Z + BR.Z) / 2;
                        //converto to METERS
                        centerIMP = new XYZ(xO, yO, zO);
                        double dist = TL.DistanceTo(BR) / 2; //custom sectet value to get solibri zoom value from Corners of Revit UiView
                        XYZ diagVector = TL.Subtract(BR);
                        // **** CUSTOM VALUE FOR TEKLA **** //
                        //zoomValue = UnitUtils.ConvertFromInternalUnits(dist * Math.Sin(diagVector.AngleTo(view3D.RightDirection)), DisplayUnitType.DUT_METERS);
                        // **** CUSTOM VALUE FOR TEKLA **** //
                        double customZoomValue = (MyProjectSettings.Get("useDefaultZoom", doc.PathName) == "1") ? 1 : 2.5;

                        zoomValue = UnitUtils.ConvertFromInternalUnits(dist * Math.Sin(diagVector.AngleTo(view3D.RightDirection)), DisplayUnitType.DUT_METERS) * customZoomValue;
                        type = "OrthogonalCamera";
                    }
                    else // it is a perspective view
                    {
                        centerIMP = uidoc.ActiveView.Origin;
                        type = "PerspectiveCamera";
                        zoomValue = 45;
                    }
                    ViewOrientation3D t = ConvertBasePoint(centerIMP, uidoc.ActiveView.ViewDirection, uidoc.ActiveView.UpDirection, false);
                    //ViewOrientation3D t = new ViewOrientation3D(centerIMP, uidoc.ActiveView.UpDirection, uidoc.ActiveView.ViewDirection);
                    XYZ c = t.EyePosition;
                    XYZ vi = t.ForwardDirection;
                    XYZ up = t.UpDirection;

                    if (type == "OrthogonalCamera")
                    {
                        v.OrthogonalCamera = new OrthogonalCamera();
                        v.OrthogonalCamera.CameraViewPoint.X = UnitUtils.ConvertFromInternalUnits(c.X, DisplayUnitType.DUT_METERS);
                        v.OrthogonalCamera.CameraViewPoint.Y = UnitUtils.ConvertFromInternalUnits(c.Y, DisplayUnitType.DUT_METERS);
                        v.OrthogonalCamera.CameraViewPoint.Z = UnitUtils.ConvertFromInternalUnits(c.Z, DisplayUnitType.DUT_METERS);
                        v.OrthogonalCamera.CameraUpVector.X = UnitUtils.ConvertFromInternalUnits(up.X, DisplayUnitType.DUT_METERS);
                        v.OrthogonalCamera.CameraUpVector.Y = UnitUtils.ConvertFromInternalUnits(up.Y, DisplayUnitType.DUT_METERS);
                        v.OrthogonalCamera.CameraUpVector.Z = UnitUtils.ConvertFromInternalUnits(up.Z, DisplayUnitType.DUT_METERS);
                        v.OrthogonalCamera.CameraDirection.X = UnitUtils.ConvertFromInternalUnits(vi.X, DisplayUnitType.DUT_METERS) * -1;
                        v.OrthogonalCamera.CameraDirection.Y = UnitUtils.ConvertFromInternalUnits(vi.Y, DisplayUnitType.DUT_METERS) * -1;
                        v.OrthogonalCamera.CameraDirection.Z = UnitUtils.ConvertFromInternalUnits(vi.Z, DisplayUnitType.DUT_METERS) * -1;
                        v.OrthogonalCamera.ViewToWorldScale = zoomValue;
                    }
                    else
                    {
                        v.PerspectiveCamera = new PerspectiveCamera();
                        v.PerspectiveCamera.CameraViewPoint.X = UnitUtils.ConvertFromInternalUnits(c.X, DisplayUnitType.DUT_METERS);
                        v.PerspectiveCamera.CameraViewPoint.Y = UnitUtils.ConvertFromInternalUnits(c.Y, DisplayUnitType.DUT_METERS);
                        v.PerspectiveCamera.CameraViewPoint.Z = UnitUtils.ConvertFromInternalUnits(c.Z, DisplayUnitType.DUT_METERS);
                        v.PerspectiveCamera.CameraUpVector.X = UnitUtils.ConvertFromInternalUnits(up.X, DisplayUnitType.DUT_METERS);
                        v.PerspectiveCamera.CameraUpVector.Y = UnitUtils.ConvertFromInternalUnits(up.Y, DisplayUnitType.DUT_METERS);
                        v.PerspectiveCamera.CameraUpVector.Z = UnitUtils.ConvertFromInternalUnits(up.Z, DisplayUnitType.DUT_METERS);
                        v.PerspectiveCamera.CameraDirection.X = UnitUtils.ConvertFromInternalUnits(vi.X, DisplayUnitType.DUT_METERS) * -1;
                        v.PerspectiveCamera.CameraDirection.Y = UnitUtils.ConvertFromInternalUnits(vi.Y, DisplayUnitType.DUT_METERS) * -1;
                        v.PerspectiveCamera.CameraDirection.Z = UnitUtils.ConvertFromInternalUnits(vi.Z, DisplayUnitType.DUT_METERS) * -1;
                        v.PerspectiveCamera.FieldOfView = zoomValue;
                    }

                    // handle section box if enabled
                    if (view3D.IsSectionBoxActive)
                    {
                        BoundingBoxXYZ sectionBox = view3D.GetSectionBox();

                        // Note that the section box can be rotated and transformed.  
                        // So the min/max corners coordinates relative to the model must be computed via the transform.
                        Transform trf = sectionBox.Transform;

                        XYZ max = sectionBox.Max; //Maximum coordinates (upper-right-front corner of the box before transform is applied).
                        XYZ min = sectionBox.Min; //Minimum coordinates (lower-left-rear corner of the box before transform is applied).

                        // Transform the min and max to model coordinates
                        XYZ maxInModelCoords = trf.OfPoint(max);
                        XYZ minInModelCoords = trf.OfPoint(min);

                        // Convert to project unit
                        DisplayUnitType lengthUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
                        maxInModelCoords = new XYZ(UnitUtils.ConvertFromInternalUnits(maxInModelCoords.X, lengthUnitType),
                                                   UnitUtils.ConvertFromInternalUnits(maxInModelCoords.Y, lengthUnitType),
                                                   UnitUtils.ConvertFromInternalUnits(maxInModelCoords.Z, lengthUnitType));
                        minInModelCoords = new XYZ(UnitUtils.ConvertFromInternalUnits(minInModelCoords.X, lengthUnitType),
                                                   UnitUtils.ConvertFromInternalUnits(minInModelCoords.Y, lengthUnitType),
                                                   UnitUtils.ConvertFromInternalUnits(minInModelCoords.Z, lengthUnitType));

                        // Convert to shared coordinates
                        maxInModelCoords = ARUP.IssueTracker.Revit.Classes.Utils.ConvertToFromSharedCoordinate(doc, maxInModelCoords, false);
                        minInModelCoords = ARUP.IssueTracker.Revit.Classes.Utils.ConvertToFromSharedCoordinate(doc, minInModelCoords, false);


                        // Add to BCF clipping planes
                        v.ClippingPlanes = BcfAdapter.GetClippingPlanesFromBoundingBox
                        (
                            maxInModelCoords.X, maxInModelCoords.Y, maxInModelCoords.Z,
                            minInModelCoords.X, minInModelCoords.Y, minInModelCoords.Z
                        );

                    }

                }



                //COMPONENTS PART
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
                System.Collections.Generic.ICollection<ElementId> collection = null;

                if (elemCheck == 0)
                    collection = collector.ToElementIds();
                else if (elemCheck == 1)
                    collection = uidoc.Selection.GetElementIds();

                if (null != collection && collection.Any())
                {
                    v.Components = new List<IssueTracker.Classes.BCF2.Component>();
                    foreach (var eId in collection)
                    {
                        Guid guid = ExportUtils.GetExportId(doc, eId);
                        string ifcguid = IfcGuid.ToIfcGuid(guid).ToString();
                        v.Components.Add(new ARUP.IssueTracker.Classes.BCF2.Component(doc.Application.VersionName, eId.ToString(), ifcguid));

                    }
                }

                return v;

            }
            catch (System.Exception ex1)
            {
                TaskDialog.Show("Error!", "exception: " + ex1);
            }
            return null;
        }

        //MOVES THE CAMERA ACCORDING TO THE PROJECT BASE LOCATION +++++++++++++++++++++++++++++++++++++++++++ BASE POINT ++++++++++++++++++++

        /// <summary>
        /// changes the coordinates accordingly to the project base location to an absolute location (for BCF export)
        /// if the value negative is set to true, does the opposite (for opening BCF views)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="view"></param>
        /// <param name="up"></param>
        /// <param name="negative"></param>
        /// <returns></returns>
        private ViewOrientation3D ConvertBasePoint(XYZ c, XYZ view, XYZ up, bool negative)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ProjectBasePoint);
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //System.Collections.Generic.IEnumerable<Element> elements = collector.WherePasses(filter).ToElements();

            double angle = 0;
            double x = 0;
            double y = 0;
            double z = 0;

            //VERY IMPORTANT
            //BuiltInParameter.BASEPOINT_EASTWEST_PARAM is the value of the BASE POINT LOCATION
            //position is the location of the BPL related to Revit's absolute origini!
            //if BPL is set to 0,0,0 not always it corresponds to Revit's origin

            ProjectLocation projectLocation = doc.ActiveProjectLocation;
            XYZ origin = new XYZ(0, 0, 0);
            ProjectPosition position = projectLocation.get_ProjectPosition(origin);

            int i = (negative) ? -1 : 1;
            //foreach (Element element in elements)
            //{
            //    MessageBox.Show(UnitUtils.ConvertFromInternalUnits(position.EastWest, DisplayUnitType.DUT_METERS).ToString() + "  " + element.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsValueString() + "\n" +
            //        UnitUtils.ConvertFromInternalUnits(position.NorthSouth, DisplayUnitType.DUT_METERS).ToString() + "  " + element.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsValueString() + "\n" +
            //        UnitUtils.ConvertFromInternalUnits(position.Elevation, DisplayUnitType.DUT_METERS).ToString() + "  " + element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsValueString() + "\n" +
            //        position.Angle.ToString() + "  " + element.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsDouble().ToString());
            //}
            x = i * position.EastWest;
            y = i * position.NorthSouth;
            z = i * position.Elevation;
            angle = i * position.Angle;

            //bool includeZ = MyProjectSettings.Get("includeZ", doc.PathName) == "1";

            if (negative) // I do the addition BEFORE
            {

                //if (includeZ)
                c = new XYZ(c.X + x, c.Y + y, c.Z + z); //INCLUDE Z coord (for NAVIS)
                // else
                // c = new XYZ(c.X + x, c.Y + y, c.Z); // EXCLUDE Z

            }

            //rotation
            double centX = (c.X * Math.Cos(angle)) - (c.Y * Math.Sin(angle));
            double centY = (c.X * Math.Sin(angle)) + (c.Y * Math.Cos(angle));

            XYZ newC = new XYZ();
            if (negative) // already done the addition
                newC = new XYZ(centX, centY, c.Z);
            else // I do the addition AFTERWARDS
            {
                // if (includeZ)
                newC = new XYZ(centX + x, centY + y, c.Z + z); //INCLUDE Z coord (for NAVIS)
                // else
                // newC = new XYZ(centX + x, centY + y, c.Z); // EXCLUDE Z
            }


            double viewX = (view.X * Math.Cos(angle)) - (view.Y * Math.Sin(angle));
            double viewY = (view.X * Math.Sin(angle)) + (view.Y * Math.Cos(angle));
            XYZ newView = new XYZ(viewX, viewY, view.Z);

            double upX = (up.X * Math.Cos(angle)) - (up.Y * Math.Sin(angle));
            double upY = (up.X * Math.Sin(angle)) + (up.Y * Math.Cos(angle));

            XYZ newUp = new XYZ(upX, upY, up.Z);
            return new ViewOrientation3D(newC, newUp, newView);
        }

        /// <summary>
        /// returns XYZ and ZOOM/FOV value
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public Tuple<ViewOrientation3D, double, string, string> GetViewCoordinates(Document doc, VisualizationInfo viewport)
        {
            string type = ""; //change if i force ortho mode
            double zoom = 0; //fov or worldscale

            XYZ CameraDirection = new XYZ();
            XYZ CameraUpVector = new XYZ();
            XYZ CameraViewPoint = new XYZ();
            //retrive the force perspective value


            // IS ORTHOGONAL
            if (viewport.OrthogonalCamera != null)
            {
                if (viewport.OrthogonalCamera.ViewToWorldScale == null || viewport.OrthogonalCamera.CameraViewPoint == null || viewport.OrthogonalCamera.CameraUpVector == null || viewport.OrthogonalCamera.CameraDirection == null)
                    return null;
                type = "OrthogonalCamera";
                zoom = UnitUtils.ConvertToInternalUnits(viewport.OrthogonalCamera.ViewToWorldScale, DisplayUnitType.DUT_METERS);
                CameraDirection = GetXYZ(viewport.OrthogonalCamera.CameraDirection.X, viewport.OrthogonalCamera.CameraDirection.Y, viewport.OrthogonalCamera.CameraDirection.Z);
                CameraUpVector = GetXYZ(viewport.OrthogonalCamera.CameraUpVector.X, viewport.OrthogonalCamera.CameraUpVector.Y, viewport.OrthogonalCamera.CameraUpVector.Z);
                CameraViewPoint = GetXYZ(viewport.OrthogonalCamera.CameraViewPoint.X, viewport.OrthogonalCamera.CameraViewPoint.Y, viewport.OrthogonalCamera.CameraViewPoint.Z);


            }

            else if (viewport.PerspectiveCamera != null)
            {
                if (viewport.PerspectiveCamera.FieldOfView == null || viewport.PerspectiveCamera.CameraViewPoint == null || viewport.PerspectiveCamera.CameraUpVector == null || viewport.PerspectiveCamera.CameraDirection == null)
                    return null;

                type = "PerspectiveCamera";
                zoom = viewport.PerspectiveCamera.FieldOfView;
                double z1 = 18 / Math.Tan(zoom / 2 * Math.PI / 180);//focale 1
                double z = 18 / Math.Tan(25 / 2 * Math.PI / 180);//focale, da controllare il 18, vedi PDF
                double factor = z1 - z;

                CameraDirection = GetXYZ(viewport.PerspectiveCamera.CameraDirection.X, viewport.PerspectiveCamera.CameraDirection.Y, viewport.PerspectiveCamera.CameraDirection.Z);
                CameraUpVector = GetXYZ(viewport.PerspectiveCamera.CameraUpVector.X, viewport.PerspectiveCamera.CameraUpVector.Y, viewport.PerspectiveCamera.CameraUpVector.Z);
                XYZ oldO = GetXYZ(viewport.PerspectiveCamera.CameraViewPoint.X, viewport.PerspectiveCamera.CameraViewPoint.Y, viewport.PerspectiveCamera.CameraViewPoint.Z);
                CameraViewPoint = (oldO.Subtract(CameraDirection.Divide(factor)));
            }
            else
                return null;
            // CHAGE VALUES ACCORDING TO BASEPOINT 
            //THIS WAS the one with DOC
            ViewOrientation3D orient3d = ConvertBasePoint(CameraViewPoint, CameraDirection, CameraUpVector, true);

            return new Tuple<ViewOrientation3D, double, string, string>(orient3d, zoom, type, "New View");
        }

        /// <summary>
        /// Get XYZ
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private XYZ GetXYZ(double x, double y, double z)
        {

            XYZ myXYZ = new XYZ(
              UnitUtils.ConvertToInternalUnits(x, DisplayUnitType.DUT_METERS),
              UnitUtils.ConvertToInternalUnits(y, DisplayUnitType.DUT_METERS),
              UnitUtils.ConvertToInternalUnits(z, DisplayUnitType.DUT_METERS));
            return myXYZ;
        }

        #endregion

        /// <summary>
        /// passing event to the user control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = mainPan.onClosing(e);
        }

    }
}