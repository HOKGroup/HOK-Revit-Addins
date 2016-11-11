using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;

//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Navigation;

//Add two new namespaces
using ARUP.IssueTracker.Classes;
using Autodesk.Navisworks.Api;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using ComApiBridge = Autodesk.Navisworks.Api.ComApi;
using ComApi = Autodesk.Navisworks.Api.Interop.ComApi;
using Com = Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api.DocumentParts;
using Autodesk.Navisworks.Api.Data;
using System.Data;
using System.Xml.Serialization;
using ARUP.IssueTracker.Classes.BCF2;

namespace ARUP.IssueTracker.Navisworks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NavisWindow : UserControl
    {
        List<SavedViewpoint> _savedViewpoints = new List<SavedViewpoint>();
        //const double Feet = 3.2808;
        List<ModelItem> _elementList;
        readonly Document _oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;
        FolderItem topFolder;

        public NavisWindow()
        {
            InitializeComponent();
            mainPan.bcfPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueBCF);
            mainPan.jiraPan.AddIssueBtn.Click += new RoutedEventHandler(AddIssueJira);

            mainPan.bcfPan.open3dViewEvent += new RoutedEventHandler(Open3dViewBCF);
            mainPan.jiraPan.open3dViewEvent += new RoutedEventHandler(Open3dViewJira);

            mainPan.bcfPan.AddIssueBtn.ToolTip = "Add a new issue or update an existing one";
            mainPan.bcfPan.AddIssueBtnText.Text = "Add/Update Issue";
            mainPan.jiraPan.AddIssueBtn.ToolTip = "Add a new issue or update an existing one";
            mainPan.jiraPan.AddIssueBtnText.Text = "Add/Update Issue";

            //enable create saved viewpoint button
            mainPan.bcfPan.CreateSavedViewpointBtn.Visibility = System.Windows.Visibility.Visible;
            mainPan.jiraPan.CreateSavedViewpointBtn.Visibility = System.Windows.Visibility.Visible;
            mainPan.jiraPan.CreateSavedViewpointBtn.Click += new RoutedEventHandler(CreateSavedViewpointJira);
            mainPan.bcfPan.CreateSavedViewpointBtn.Click += new RoutedEventHandler(CreateSavedViewpointBCF);

            CheckSavedViewpointTopFolder();

        }
        private void CheckSavedViewpointTopFolder()
        {
            //check if the top folder exists
            int indexOfExistingTopFolder = _oDoc.SavedViewpoints.ToSavedItemCollection().IndexOfDisplayName("Issues from BCF/Jira");
            if (indexOfExistingTopFolder >= 0)
            {
                topFolder = _oDoc.SavedViewpoints.ToSavedItemCollection().ElementAt(indexOfExistingTopFolder) as FolderItem;
            }
            else
            {
                using (Transaction trans = new Transaction(_oDoc, "Create Jira/BCF Folder"))
                {
                    topFolder = new FolderItem() { DisplayName = "Issues from BCF/Jira", Guid = Guid.NewGuid() };
                    _oDoc.SavedViewpoints.AddCopy(topFolder);
                    trans.Commit();

                    // Note that we add a copy of the folder item here
                    // Need to find and assign the top folder again
                    indexOfExistingTopFolder = _oDoc.SavedViewpoints.ToSavedItemCollection().IndexOfDisplayName("Issues from BCF/Jira");
                    if (indexOfExistingTopFolder >= 0)
                    {
                        topFolder = _oDoc.SavedViewpoints.ToSavedItemCollection().ElementAt(indexOfExistingTopFolder) as FolderItem;
                    }
                }
            }
        }
        private void AddIssueJira(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
                Tuple<List<Markup>, List<Issue>> tup = AddIssue(path, false);
                if (tup == null)
                    return;
                List<Markup> issues = tup.Item1;
                List<Issue> issuesJira = tup.Item2;

                if (issues != null && issues.Any())
                    mainPan.doUploadIssue(issues, path, true, mainPan.jiraPan.projIndex, issuesJira);
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }

        }
        private void AddIssueBCF(object sender, EventArgs e)
        {
            try
            {

                //mainPan.NewBCF(null, null);
                //if (mainPan.jira.Bcf != null && !mainPan.jira.Bcf.HasBeenSaved && mainPan.jira.Bcf.Issues.Any())
                //    return;
                Tuple<List<Markup>, List<Issue>> tup = AddIssue(mainPan.jira.Bcf.TempPath, true);
                if (tup == null)
                    return;
                List<Markup> issues = tup.Item1;
                //int typeInt = tup.Item2;
                if (issues != null && issues.Any())
                {
                    int newIssueCounter = 0;
                    int updateIssueCounter = 0;
                    int unchangedIssueCounter = 0;

                    foreach (var i in issues)
                    {
                        int indexOfExistingIssue = mainPan.jira.Bcf.Issues.ToList().FindIndex(issue => issue.Topic.Guid == i.Topic.Guid);
                        if (indexOfExistingIssue >= 0) // Update an exisiting issue with new comments
                        {
                            int originalCommentNumber = mainPan.jira.Bcf.Issues[indexOfExistingIssue].Comment.Count;

                            foreach (ARUP.IssueTracker.Classes.BCF2.Comment newComment in i.Comment)
                            {
                                if (!newComment.Comment1.Contains("CachedId"))
                                {
                                    mainPan.jira.Bcf.Issues[indexOfExistingIssue].Comment.Insert(0, newComment);
                                }
                            }

                            if (mainPan.jira.Bcf.Issues[indexOfExistingIssue].Comment.Count == originalCommentNumber)
                            {
                                unchangedIssueCounter++;
                            }
                            else
                            {
                                updateIssueCounter++;
                            }
                        }
                        else // Create a new issue 
                        {
                            mainPan.jira.Bcf.Issues.Add(i);
                            newIssueCounter++;
                        }
                    }

                    string msg = string.Format("{0} new issue(s) added, {1} issue(s) updated, and {2} issue(s) unchanged.", newIssueCounter, updateIssueCounter, unchangedIssueCounter);
                    MessageBox.Show(msg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    //mainPan.jira.Bcf.Issues = new System.Collections.ObjectModel.ObservableCollection<IssueBCF>(issues);
                    mainPan.jira.Bcf.HasBeenSaved = false;
                    //string projFilename = !string.IsNullOrEmpty(_oDoc.FileName) ? System.IO.Path.GetFileNameWithoutExtension(_oDoc.FileName) : "New BCF Report";
                    //mainPan.jira.Bcf.Filename = projFilename;
                }
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private Tuple<List<Markup>, List<Issue>> AddIssue(string path, bool isBcf)
        {
            try
            {
                // set image export settings
                ComApi.InwOaPropertyVec options = ComBridge.State.GetIOPluginOptions("lcodpimage");
                // configure the option "export.image.format" to export png and image size
                foreach (ComApi.InwOaProperty opt in options.Properties())
                {
                    if (opt.name == "export.image.format")
                        opt.value = "lcodpexpng";
                    if (opt.name == "export.image.width")
                        opt.value = 1600;
                    if (opt.name == "export.image.height")
                        opt.value = 900;

                }

                _savedViewpoints = new List<SavedViewpoint>();

                foreach (SavedItem oSI in _oDoc.SavedViewpoints.ToSavedItemCollection())
                {
                    RecurseItems(oSI);
                }

                var types = new ObservableCollection<Issuetype>();
                var assignees = new List<User>();
                var components = new ObservableCollection<ARUP.IssueTracker.Classes.Component>();
                var priorities = new ObservableCollection<Priority>();
                var noCom = true;
                var noPrior = true;
                var noAssign = true;

                if (!isBcf)
                {
                    types = mainPan.jira.TypesCollection;
                    assignees = mainPan.getAssigneesProj();
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

                // Key: saved viewpoints; Value: BCF/Jira issue title (if it has)
                Dictionary<SavedViewpoint, string> savedViewpointsAndIssueTitles = new Dictionary<SavedViewpoint, string>();
                _savedViewpoints.ForEach(sv =>
                {
                    string originalIssueTitle = null;
                    if (sv.Parent != null)
                    {
                        if (sv.Parent.Parent != null)
                        {
                            if (sv.Parent.Parent.DisplayName == "Issues from BCF/Jira")
                            {
                                originalIssueTitle = sv.Parent.DisplayName;
                            }
                        }
                    }
                    savedViewpointsAndIssueTitles.Add(sv, originalIssueTitle);
                });

                AddIssueNavis ain = new AddIssueNavis(savedViewpointsAndIssueTitles, types, assignees, components, priorities, noCom, noPrior, noAssign);
                if (!isBcf)
                {
                    ain.JiraFieldsBox.Visibility = System.Windows.Visibility.Visible;
                    ain.BcfFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    ain.JiraFieldsBox.Visibility = System.Windows.Visibility.Collapsed;
                    ain.BcfFieldsBox.Visibility = System.Windows.Visibility.Visible;
                }
                ain.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                ain.ShowDialog();
                if (ain.DialogResult.HasValue && ain.DialogResult.Value)
                {
                    int elemCheck = 2;
                    if (ain.all.IsChecked.Value)
                        elemCheck = 0;
                    else if (ain.selected.IsChecked.Value)
                        elemCheck = 1;

                    List<SavedViewpoint> savedViewpointsImport = new List<SavedViewpoint>();

                    for (int i = 0; i < ain.issueList.SelectedItems.Count; i++)
                    {
                        int index = ain.issueList.Items.IndexOf(ain.issueList.SelectedItems[i]);
                        savedViewpointsImport.Add(_savedViewpoints[index]);
                    }
                    if (!savedViewpointsImport.Any())
                        return null;
                    //get selection only once!
                    if (elemCheck == 1)
                        _elementList = _oDoc.CurrentSelection.SelectedItems.Where(o => o.InstanceGuid != Guid.Empty).ToList<ModelItem>();

                    List<Markup> issues = new List<Markup>();
                    List<Issue> issuesJira = new List<Issue>();
                    foreach (var sv in savedViewpointsImport)
                    {
                        Issue issueJira = new Issue();
                        Markup issue = new Markup(DateTime.Now);

                        // Check if this is a saved viewpoint which was previosly created from Jira/BCF
                        string jiraIssueGuid = GetMetadata(MetadataTables.JiraIssue, sv.Parent.Guid.ToString());
                        if (!string.IsNullOrEmpty(jiraIssueGuid))  // update an existing Jira (or BCF exported from Jira) issue
                        {
                            issue.Topic.Guid = jiraIssueGuid;
                        }
                        if (!isBcf)  // new Jira issue
                        {
                            issueJira.fields = new Fields();
                            issueJira.fields.issuetype = (Issuetype)ain.issueTypeCombo.SelectedItem;
                            issueJira.fields.priority = (Priority)ain.priorityCombo.SelectedItem;
                            if (!string.IsNullOrEmpty(ain.ChangeAssign.Content.ToString()) &&
                                ain.ChangeAssign.Content.ToString() != "none")
                            {
                                issueJira.fields.assignee = new User();
                                issueJira.fields.assignee.name = ain.ChangeAssign.Content.ToString();
                            }

                            if (ain.SelectedComponents != null && ain.SelectedComponents.Any())
                            {
                                issueJira.fields.components = ain.SelectedComponents;
                            }
                        }

                        string folderIssue = Path.Combine(path, issue.Topic.Guid);
                        if (!Directory.Exists(folderIssue))
                            Directory.CreateDirectory(folderIssue);

                        ViewPoint vp = new ViewPoint(true);
                        // set the currtent saved viewpoint and then generate sna and BCF viewpoint
                        _oDoc.SavedViewpoints.CurrentSavedViewpoint = sv;
                        vp.VisInfo = generateViewpoint(sv.Viewpoint, elemCheck);
                        vp.SnapshotPath = Path.Combine(folderIssue, "snapshot.png");
                        issue.Viewpoints.Add(vp);
                        generateSnapshot(folderIssue);

                        string originalIssueTitle = null;
                        // Check if this is a saved viewpoint which was previosly created from BCF
                        if (isBcf && sv.Parent != null)
                        {
                            string originalBcfTopic = GetMetadata(MetadataTables.BcfTopic, sv.Parent.Guid.ToString());
                            if (!string.IsNullOrEmpty(originalBcfTopic))
                            {
                                var serializer = new XmlSerializer(typeof(ARUP.IssueTracker.Classes.BCF2.Topic));
                                using (TextReader reader = new StringReader(originalBcfTopic))
                                {
                                    ARUP.IssueTracker.Classes.BCF2.Topic topic = serializer.Deserialize(reader) as ARUP.IssueTracker.Classes.BCF2.Topic;
                                    if (topic != null)
                                    {
                                        issue.Topic.Guid = topic.Guid;
                                    }
                                }
                            }

                            // Check if it has an original BCF issue title
                            if (savedViewpointsAndIssueTitles.ContainsKey(sv))
                            {
                                originalIssueTitle = savedViewpointsAndIssueTitles[sv];
                            }
                        }

                        issue.Topic.Title = string.IsNullOrEmpty(originalIssueTitle) ? sv.DisplayName : originalIssueTitle;
                        issue.Topic.AssignedTo = ain.BcfAssignee.Text;
                        issue.Topic.CreationAuthor = MySettings.Get("username");
                        issue.Topic.Priority = ain.BcfPriority.Text;
                        issue.Topic.TopicStatus = ain.BcfStatus.Text;
                        issue.Topic.TopicType = ain.BcfIssueType.Text;
                        issue.Header[0].IfcProject = "";
                        string projFilename = !string.IsNullOrEmpty(_oDoc.FileName) ? System.IO.Path.GetFileName(_oDoc.FileName) : "";
                        issue.Header[0].Filename = projFilename;
                        issue.Header[0].Date = DateTime.Now;

                        //comment
                        if (sv.Comments.Any())
                        {
                            foreach (var comm in sv.Comments)
                            {
                                if (!comm.Body.Contains("CachedId") || isBcf)
                                {
                                    string commentMetadata = string.Empty;
                                    commentMetadata += comm.Body + Environment.NewLine;
                                    commentMetadata += string.Format("<Navisworks:OriginalAuthor>{0}</Navisworks:OriginalAuthor>{1}", comm.Author, Environment.NewLine);
                                    commentMetadata += string.Format("<Navisworks:CommentStatus>{0}</Navisworks:CommentStatus>{1}", comm.Status, Environment.NewLine);
                                    commentMetadata += string.Format("<Navisworks:CreationDate>{0}</Navisworks:CreationDate>{1}", comm.CreationDate, Environment.NewLine);
                                    var c = new ARUP.IssueTracker.Classes.BCF2.Comment
                                    {
                                        Comment1 = commentMetadata,
                                        Topic = new CommentTopic { Guid = issue.Topic.Guid }
                                    };
                                    ;
                                    c.Date = DateTime.Now;
                                    c.VerbalStatus = comm.Status.ToString();
                                    c.Author = (string.IsNullOrWhiteSpace(mainPan.jira.Self.displayName)) ? MySettings.Get("BCFusername") : mainPan.jira.Self.displayName;
                                    issue.Comment.Add(c);
                                }
                            }
                        }

                        issues.Add(issue);
                        issuesJira.Add(issueJira);
                    } // end foreach
                    return new Tuple<List<Markup>, List<Issue>>(issues, issuesJira);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        public void generateSnapshot(string folderIssue)
        {
            try
            {
                string snapshot = Path.Combine(folderIssue, "snapshot.png");

                // get the state of COM
                ComApi.InwOpState10 oState = ComBridge.State;
                // get the IO plugin for image
                ComApi.InwOaPropertyVec options = oState.GetIOPluginOptions("lcodpimage");

                //export the viewpoint to the image
                oState.DriveIOPlugin("lcodpimage", snapshot, options);
                System.Drawing.Bitmap oBitmap = new System.Drawing.Bitmap(snapshot);
                System.IO.MemoryStream ImageStream = new System.IO.MemoryStream();
                oBitmap.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                oBitmap.Dispose();
                //IM.postAttach(issueKey, File.ReadAllBytes(snapshot), IM.ConvertToBytes(v), g);
                //   postAttach2(issueKey, ConvertToBytes(v), "viewpoint.bcfv");
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private VisualizationInfo generateViewpoint(Viewpoint oVP, int elemCheck)
        {
            double units = GetGunits();
            VisualizationInfo v = new VisualizationInfo();
            try
            {

                Vector3D vi = getViewDir(oVP);
                Vector3D up = getViewUp(oVP);
                Point3D center = new Point3D(oVP.Position.X / units, oVP.Position.Y / units, oVP.Position.Z / units);
                double zoomValue = 1;


                oVP = oVP.CreateCopy();
                if (!oVP.HasFocalDistance)
                    oVP.FocalDistance = 1;

                if (oVP.Projection == ViewpointProjection.Orthographic) //IS ORTHO
                {
                    // **** CUSTOM VALUE FOR TEKLA **** //
                    // otherwise = 1
                    // **** CUSTOM VALUE FOR TEKLA **** //


                    double dist = oVP.VerticalExtentAtFocalDistance / 2 / units;
                    zoomValue = 3.125 * dist / (up.Length * 1.25);
                    //zoomValue = Math.Tan(oVP.HeightField / 2) * oVP.FarPlaneDistance / _feet *1.25;
                    //  MessageBox.Show(oVP.HeightField.ToString() + "  " + oVP.FarPlaneDistance.ToString() + "   " + zoomValue + "   " + oVP.HasFocalDistance.ToString() + "   " + oVP.VerticalExtentAtFocalDistance.ToString());
                    v.OrthogonalCamera = new OrthogonalCamera();
                    v.OrthogonalCamera.CameraViewPoint.X = center.X;
                    v.OrthogonalCamera.CameraViewPoint.Y = center.Y;
                    v.OrthogonalCamera.CameraViewPoint.Z = center.Z;
                    v.OrthogonalCamera.CameraUpVector.X = up.X;
                    v.OrthogonalCamera.CameraUpVector.Y = up.Y;
                    v.OrthogonalCamera.CameraUpVector.Z = up.Z;
                    v.OrthogonalCamera.CameraDirection.X = vi.X;
                    v.OrthogonalCamera.CameraDirection.Y = vi.Y;
                    v.OrthogonalCamera.CameraDirection.Z = vi.Z;
                    v.OrthogonalCamera.ViewToWorldScale = zoomValue;
                }
                else // it is a perspective view
                {
                    double f = oVP.FocalDistance;
                    //there is an issue when using vewpoint generated from clashes
                    //their VerticalExtentAtFocalDistance is correct but the HorizontalExtentAtFocalDistance is too small!
                    //so results that the aspect ratio in that case is <1. In which I try to get an approximate valut of the HorizontalExtentAtFocalDistance
                    // by multiplying the vercial by 1.35
                    //double hfov = (oVP.AspectRatio < 1) ? oVP.VerticalExtentAtFocalDistance * 1.23245 / 2 : oVP.HorizontalExtentAtFocalDistance / 2;
                    //double angle = Math.Atan(hfov * (1 / f)) * 2;
                    //double angled = (180 * angle / Math.PI);


                    //NAVIS USES HFOV
                    //double vfov = oVP.VerticalExtentAtFocalDistance / 2;
                    //double fov = Math.Sqrt(hfov * hfov + vfov * vfov);
                    //double angle = Math.Atan(fov*(1/f)) *2;
                    //double angled = (180 * angle / Math.PI);
                    //MessageBox.Show(angled.ToString() + "   " + oVP.FarDistance + "   " +f + "   " + oVP.NearDistance );// + "\n zoom" + zoom);

                    zoomValue = f;

                    v.PerspectiveCamera = new PerspectiveCamera();
                    v.PerspectiveCamera.CameraViewPoint.X = center.X;
                    v.PerspectiveCamera.CameraViewPoint.Y = center.Y;
                    v.PerspectiveCamera.CameraViewPoint.Z = center.Z;
                    v.PerspectiveCamera.CameraUpVector.X = up.X;
                    v.PerspectiveCamera.CameraUpVector.Y = up.Y;
                    v.PerspectiveCamera.CameraUpVector.Z = up.Z;
                    v.PerspectiveCamera.CameraDirection.X = vi.X;
                    v.PerspectiveCamera.CameraDirection.Y = vi.Y;
                    v.PerspectiveCamera.CameraDirection.Z = vi.Z;
                    v.PerspectiveCamera.FieldOfView = zoomValue;
                }



                if (elemCheck == 0)//visible (0)
                    _elementList = _oDoc.Models.First.RootItem.DescendantsAndSelf.Where(o => o.InstanceGuid != Guid.Empty && ChechHidden(o.AncestorsAndSelf) && o.FindFirstGeometry() != null && !o.FindFirstGeometry().Item.IsHidden).ToList<ModelItem>();

                if (null != _elementList && _elementList.Any() && elemCheck != 2)//not if none (2)
                {
                    v.Components = new List<Classes.BCF2.Component>();
                    string appname = Autodesk.Navisworks.Api.Application.Title;
                    foreach (var eId in _elementList)
                    {
                        string ifcguid = IfcGuid.ToIfcGuid(eId.InstanceGuid).ToString();
                        v.Components.Add(new ARUP.IssueTracker.Classes.BCF2.Component(appname, "", ifcguid));

                    }
                }

                // handling section planes
                v.ClippingPlanes = GetCurrentSectionPlanes().ToArray();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return v;
        }

        private bool ChechHidden(ModelItemEnumerableCollection items)
        {
            if (items.Any(o => o.IsHidden))
                return false; //an anchestor is hidden, so it the item
            return true; // all anchestors are visible


        }
        private void RecurseItems(SavedItem oSI)
        {
            try
            {
                Autodesk.Navisworks.Api.GroupItem group = oSI as Autodesk.Navisworks.Api.GroupItem;
                if (null != group) // is a group
                {
                    Autodesk.Navisworks.Api.SavedViewpointAnimation animation = group as Autodesk.Navisworks.Api.SavedViewpointAnimation;
                    if (animation == null) // and not an animation
                    {
                        foreach (SavedItem oSII in group.Children)
                        {
                            RecurseItems(oSII);
                        }
                    }
                }
                else
                {
                    _savedViewpoints.Add((SavedViewpoint)oSI);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void Open3dViewBCF(object sender, EventArgs e)
        {
            try
            {
                VisualizationInfo VisInfo = (VisualizationInfo)((Button)sender).Tag;
                Open3DView(VisInfo);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        private void Open3dViewJira(object sender, EventArgs e)
        {
            try
            {
                string url = (string)((Button)sender).Tag;
                VisualizationInfo v = mainPan.getVisInfo(url);
                if (null != v)
                    Open3DView(v);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
        }
        // for creating saved viewpoints using BCF 2.0 VisualizationInfo
        private Viewpoint GetViewpointFromVisualizationInfo(VisualizationInfo visInfo)
        {
            try
            {
                Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double> tuple = null;
                tuple = GetViewCoordinatesFromBcf2VisInfo(visInfo);

                if (tuple == null)
                {
                    MessageBox.Show("Viewpoint not formatted correctly.", "Viewpoint Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

                // get current viewpoint
                // Viewpoint oCurVP = oDoc.vi.CurrentViewpoint.ToViewpoint;
                // get copy viewpoint
                Viewpoint oCopyVP = new Viewpoint();



                oCopyVP.AlignDirection(tuple.Item3);
                oCopyVP.AlignUp(tuple.Item2);
                oCopyVP.Projection = tuple.Item4;



                // **** CUSTOM VALUE FOR TEKLA **** //
                // otherwise = 1
                // **** CUSTOM VALUE FOR TEKLA **** //
                const double TEKLA = 1.25;

                double x = tuple.Item5 / TEKLA;


                if (oCopyVP.Projection == ViewpointProjection.Orthographic)
                {

                    oCopyVP.Position = tuple.Item1;
                    oCopyVP.FocalDistance = 1;
                    //top center point of view
                    Point3D xyzTL = oCopyVP.Position.Add(tuple.Item2.Multiply(x));
                    oCopyVP.SetExtentsAtFocalDistance(1, xyzTL.DistanceTo(oCopyVP.Position));
                }
                else
                {
                    //double angle = tuple.Item5 * Math.PI / 180;
                    // MessageBox.Show(tuple.Item5.ToString() + "  " +(Math.Tan(angle / 2)*2).ToString());
                    oCopyVP.FocalDistance = tuple.Item5;
                    //oCopyVP.SetExtentsAtFocalDistance(Math.Tan(angle / 2) * 2, Math.Tan(angle / 2) * 2 / oCopyVP.AspectRatio);
                    oCopyVP.Position = tuple.Item1;
                }

                //SavedViewpoint sv = new SavedViewpoint(oCopyVP);
                //sv.DisplayName = "test view";
                //sv.Guid = Guid.NewGuid();
                //oDoc.SavedViewpoints.AddCopy(sv);
                return oCopyVP;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }
        private void Open3DView(VisualizationInfo v)
        {
            try
            {
                //    {
                //      
                Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double> tuple = GetViewCoordinatesFromBcf2VisInfo(v);

                if (tuple == null)
                {
                    MessageBox.Show("Viewpoint not formatted correctly.", "Viewpoint Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Document oDoc = Autodesk.Navisworks.Api.Application.ActiveDocument;

                // get current viewpoint
                // Viewpoint oCurVP = oDoc.vi.CurrentViewpoint.ToViewpoint;
                // get copy viewpoint
                Viewpoint oCopyVP = new Viewpoint();



                oCopyVP.AlignDirection(tuple.Item3);
                oCopyVP.AlignUp(tuple.Item2);
                oCopyVP.Projection = tuple.Item4;

                // adjust render style and lighting to imporve speed of loading
                oCopyVP.RenderStyle = ViewpointRenderStyle.Shaded;
                oCopyVP.Lighting = ViewpointLighting.Headlight;

                // **** CUSTOM VALUE FOR TEKLA **** //
                // otherwise = 1
                // **** CUSTOM VALUE FOR TEKLA **** //
                const double TEKLA = 1.25;

                double x = tuple.Item5 / TEKLA;


                if (oCopyVP.Projection == ViewpointProjection.Orthographic)
                {

                    oCopyVP.Position = tuple.Item1;
                    oCopyVP.FocalDistance = 1;
                    //top center point of view
                    Point3D xyzTL = oCopyVP.Position.Add(tuple.Item2.Multiply(x));
                    oCopyVP.SetExtentsAtFocalDistance(1, xyzTL.DistanceTo(oCopyVP.Position));
                }
                else
                {
                    //double angle = tuple.Item5 * Math.PI / 180;
                    // MessageBox.Show(tuple.Item5.ToString() + "  " +(Math.Tan(angle / 2)*2).ToString());
                    oCopyVP.FocalDistance = tuple.Item5;
                    //oCopyVP.SetExtentsAtFocalDistance(Math.Tan(angle / 2) * 2, Math.Tan(angle / 2) * 2 / oCopyVP.AspectRatio);
                    oCopyVP.Position = tuple.Item1;
                }

                //SavedViewpoint sv = new SavedViewpoint(oCopyVP);
                //sv.DisplayName = "test view";
                //sv.Guid = Guid.NewGuid();
                //oDoc.SavedViewpoints.AddCopy(sv);
                oDoc.CurrentViewpoint.CopyFrom(oCopyVP);

                // apply BCF clipping planes
                if (v.ClippingPlanes != null)
                {
                    for (int i = 0; i < v.ClippingPlanes.Count(); i++)
                    {
                        CreateSectionPlane(i, v.ClippingPlanes[i]);
                    }
                }

                //Avoid handling too many elements
                if (v.Components.Count > 100)
                {
                    var result = MessageBox.Show("Too many elements attached. It may take for a while to isolate/select them. Do you want to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        oDoc.Models.ResetAllHidden();
                        return;
                    }
                }

                if (v.Components != null && v.Components.Any())
                {
                    // Use search engine
                    Search searchForVisibility = new Search();
                    searchForVisibility.Selection.SelectAll();
                    searchForVisibility.Locations = SearchLocations.DescendantsAndSelf;

                    Search searchForSelection = new Search();
                    searchForSelection.Selection.SelectAll();
                    searchForSelection.Locations = SearchLocations.DescendantsAndSelf;

                    // handle visibility
                    if (v.Components.Any(ele => ele.Visible == false))
                    {
                        v.Components.ForEach(o =>
                        {
                            Guid instanceGuid = IfcGuid.FromIfcGUID(o.IfcGuid);
                            // Set up the search condition:
                            SearchCondition condition = SearchCondition.HasPropertyByDisplayName("Item", "GUID")
                                .EqualValue(VariantData.FromDisplayString(instanceGuid.ToString()));

                            if (o.Visible == false)
                            {
                                // it's an OR condition among groups
                                List<SearchCondition> searchConditionGroup = new List<SearchCondition>();
                                searchConditionGroup.Add(condition);
                                searchForVisibility.SearchConditions.AddGroup(searchConditionGroup);
                            }
                        });

                        // Get the collection of model items that satisfy the search condition:
                        ModelItemCollection searchResultVisibility = searchForVisibility.FindAll(_oDoc, false);

                        if (searchResultVisibility.Count > 0)
                        {
                            oDoc.Models.ResetAllHidden();

                            // hide attached
                            oDoc.Models.SetHidden(searchResultVisibility, true);
                        }
                    }
                    else
                    {
                        v.Components.ForEach(o =>
                        {
                            Guid instanceGuid = IfcGuid.FromIfcGUID(o.IfcGuid);
                            // Set up the search condition:
                            SearchCondition condition = SearchCondition.HasPropertyByDisplayName("Item", "GUID")
                                .EqualValue(VariantData.FromDisplayString(instanceGuid.ToString()));

                            // it's an OR condition among groups
                            List<SearchCondition> searchConditionGroup = new List<SearchCondition>();
                            searchConditionGroup.Add(condition);
                            searchForVisibility.SearchConditions.AddGroup(searchConditionGroup);
                        });

                        // Get the collection of model items that satisfy the search condition:
                        ModelItemCollection searchResultVisibility = searchForVisibility.FindAll(_oDoc, false);

                        if (searchResultVisibility.Count > 0)
                        {
                            oDoc.Models.ResetAllHidden();
                            oDoc.CurrentSelection.Clear();

                            //select attached elements
                            oDoc.CurrentSelection.AddRange(searchResultVisibility);

                            // hide unselected
                            HideUnselected();
                        }
                    }

                    // handle selection
                    oDoc.CurrentSelection.Clear();

                    if (v.Components.Any(ele => ele.Selected == true))
                    {
                        v.Components.ForEach(o =>
                        {
                            if (o.Selected)
                            {
                                Guid instanceGuid = IfcGuid.FromIfcGUID(o.IfcGuid);
                                SearchCondition condition = SearchCondition.HasPropertyByDisplayName("Item", "GUID")
                                    .EqualValue(VariantData.FromDisplayString(instanceGuid.ToString()));

                                List<SearchCondition> searchConditionGroup = new List<SearchCondition>();
                                searchConditionGroup.Add(condition);
                                searchForSelection.SearchConditions.AddGroup(searchConditionGroup);
                            }
                        });

                        ModelItemCollection searchResultSelection = searchForSelection.FindAll(_oDoc, false);
                        if (searchResultSelection.Count > 0)
                        {
                            oDoc.CurrentSelection.AddRange(searchResultSelection);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }
        public void CreateSectionPlane(int index, ClippingPlane cp)
        {
            ComApi.InwOpState10 state;
            state = ComApiBridge.ComApiBridge.State;

            // create a geometry vector as the normal of section plane 
            ComApi.InwLUnitVec3f sectionPlaneNormal =
                (ComApi.InwLUnitVec3f)state.ObjectFactory(
                Autodesk.Navisworks.Api.Interop.ComApi.nwEObjectType.eObjectType_nwLUnitVec3f,
                null,
                null);

            sectionPlaneNormal.SetValue(-cp.Direction.X, -cp.Direction.Y, -cp.Direction.Z);

            // create a geometry plane 
            ComApi.InwLPlane3f sectionPlane =
                (ComApi.InwLPlane3f)state.ObjectFactory
                (Autodesk.Navisworks.Api.Interop.ComApi.nwEObjectType.eObjectType_nwLPlane3f,
                null,
                null);

            //get collection of sectioning planes 
            ComApi.InwClippingPlaneColl2 clipColl =
                (ComApi.InwClippingPlaneColl2)state.CurrentView.ClippingPlanes();

            // create a new sectioning plane
            // it forces creation of planes up to this index.
            clipColl.CreatePlane(index + 1);

            // get the last sectioning plane which are what we created
            ComApi.InwOaClipPlane cliPlane =
                (ComApi.InwOaClipPlane)state.CurrentView.ClippingPlanes()[index + 1];

            //assign the geometry vector with the plane
            double distance = -cp.Direction.X * cp.Location.X - cp.Direction.Y * cp.Location.Y - cp.Direction.Z * cp.Location.Z;
            sectionPlane.SetValue(sectionPlaneNormal, distance * GetGunits()); // convert meter (BCF) to feet (Navis COM)

            // ask the sectioning plane uses the new geometry plane 
            cliPlane.Plane = sectionPlane;
            cliPlane.Alignment = ComApi.nwEClipPlaneAlignment.eAlignment_NONE;

            // enable this sectioning plane 
            cliPlane.Enabled = true;
        }
        public List<ClippingPlane> GetCurrentSectionPlanes()
        {
            List<ClippingPlane> sectionPlanes = new List<ClippingPlane>();

            ComApi.InwOpState10 state;
            state = ComApiBridge.ComApiBridge.State;

            //get collection of sectioning planes 
            ComApi.InwClippingPlaneColl2 clipColl =
                (ComApi.InwClippingPlaneColl2)state.CurrentView.ClippingPlanes();

            if (clipColl.Count <= 1)
            {
                return sectionPlanes;
            }

            foreach (ComApi.InwOaClipPlane p in clipColl)
            {
                ClippingPlane sp = new ClippingPlane();

                // get the normal of the section plane (and convert from feet to meter)
                double normalX = -p.Plane.GetNormal().data1;
                double normalY = -p.Plane.GetNormal().data2;
                double normalZ = -p.Plane.GetNormal().data3;
                sp.Direction = new Direction() { X = normalX, Y = normalY, Z = normalZ };

                // get one point on the plane as a location for BCF
                if (normalX != 0 && normalY != 0 && normalZ != 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = 0, Y = 0, Z = -p.Plane.distance() / normalZ / GetGunits() };
                }
                else if (normalX == 0 && normalY == 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = 0, Y = 0, Z = -p.Plane.distance() / normalZ / GetGunits() };
                }
                else if (normalX == 0 && normalZ == 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = 0, Y = -p.Plane.distance() / normalY / GetGunits(), Z = 0 };
                }
                else if (normalZ == 0 && normalY == 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = -p.Plane.distance() / normalX / GetGunits(), Y = 0, Z = 0 };
                }
                else if (normalX == 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = 0, Y = 0, Z = -p.Plane.distance() / normalZ / GetGunits() };
                }
                else if (normalY == 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = 0, Y = 0, Z = -p.Plane.distance() / normalZ / GetGunits() };
                }
                else if (normalZ == 0)
                {
                    sp.Location = new ARUP.IssueTracker.Classes.BCF2.Point() { X = -p.Plane.distance() / normalX / GetGunits(), Y = 0, Z = 0 };
                }

                sectionPlanes.Add(sp);
            }

            return sectionPlanes;
        }
        public void HideUnselected()
        {
            //Create hidden collection
            ModelItemCollection hidden = new ModelItemCollection();

            //create a store for the visible items
            ModelItemCollection visible = new ModelItemCollection();

            //Add all the items that are visible to the visible collection
            foreach (ModelItem item in Autodesk.Navisworks.Api.Application.ActiveDocument.CurrentSelection.SelectedItems)
            {
                if (item.AncestorsAndSelf != null)
                    visible.AddRange(item.AncestorsAndSelf);
                if (item.Descendants != null)
                    visible.AddRange(item.Descendants);
            }

            //mark as invisible all the siblings of the visible items as well as the visible items
            foreach (ModelItem toShow in visible)
            {
                if (toShow.Parent != null)
                {
                    hidden.AddRange(toShow.Parent.Children);
                }
            }

            //remove the visible items from the collection
            foreach (ModelItem toShow in visible)
            {
                hidden.Remove(toShow);
            }

            //hide the remaining items
            Autodesk.Navisworks.Api.Application.ActiveDocument.Models.
               SetHidden(hidden, true);
        }
        public Tuple<Point3D, Vector3D, Vector3D, Autodesk.Navisworks.Api.ViewpointProjection, double> GetViewCoordinatesFromBcf2VisInfo(ARUP.IssueTracker.Classes.BCF2.VisualizationInfo viewport)
        {
            try
            {
                double units = GetGunits();

                Point3D Position = new Point3D();
                Vector3D VectorUp = new Vector3D();
                Vector3D VectorTo = new Vector3D();
                ViewpointProjection vp = ViewpointProjection.Perspective;
                double zoom = 0;
                if (viewport.OrthogonalCamera != null)
                {
                    if (viewport.OrthogonalCamera.CameraViewPoint == null || viewport.OrthogonalCamera.CameraUpVector == null || viewport.OrthogonalCamera.CameraDirection == null)
                        return null;

                    vp = ViewpointProjection.Orthographic;
                    zoom = units * viewport.OrthogonalCamera.ViewToWorldScale;
                    Position = GetXYZ(viewport.OrthogonalCamera.CameraViewPoint.X, viewport.OrthogonalCamera.CameraViewPoint.Y, viewport.OrthogonalCamera.CameraViewPoint.Z);
                    VectorUp = GetXYZ(viewport.OrthogonalCamera.CameraUpVector.X, viewport.OrthogonalCamera.CameraUpVector.Y, viewport.OrthogonalCamera.CameraUpVector.Z).ToVector3D().Normalize();
                    VectorTo = GetXYZ(viewport.OrthogonalCamera.CameraDirection.X, viewport.OrthogonalCamera.CameraDirection.Y, viewport.OrthogonalCamera.CameraDirection.Z).ToVector3D().Normalize();
                }
                else if (viewport.PerspectiveCamera != null)
                {
                    if (viewport.PerspectiveCamera.CameraViewPoint == null || viewport.PerspectiveCamera.CameraUpVector == null || viewport.PerspectiveCamera.CameraDirection == null)
                        return null;

                    zoom = viewport.PerspectiveCamera.FieldOfView;
                    Position = GetXYZ(viewport.PerspectiveCamera.CameraViewPoint.X, viewport.PerspectiveCamera.CameraViewPoint.Y, viewport.PerspectiveCamera.CameraViewPoint.Z);
                    VectorUp = GetXYZ(viewport.PerspectiveCamera.CameraUpVector.X, viewport.PerspectiveCamera.CameraUpVector.Y, viewport.PerspectiveCamera.CameraUpVector.Z).ToVector3D().Normalize();
                    VectorTo = GetXYZ(viewport.PerspectiveCamera.CameraDirection.X, viewport.PerspectiveCamera.CameraDirection.Y, viewport.PerspectiveCamera.CameraDirection.Z).ToVector3D().Normalize();

                }
                else
                    return null;

                return new Tuple<Point3D, Vector3D, Vector3D, ViewpointProjection, double>(Position, VectorUp, VectorTo, vp, zoom);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }
        private Point3D GetXYZ(double x, double y, double z)
        {
            double units = GetGunits();

            Point3D myXYZ = new Point3D(x * units, y * units, z * units);
            return myXYZ;
        }

        private void CreateSavedViewpointJira(object sender, EventArgs e)
        {
            CheckSavedViewpointTopFolder();
            using (Transaction trans = new Transaction(_oDoc, "Save Viewpoints from Jira"))
            {
                CreateSavedViewpoint(true);
                trans.Commit();
            }
        }

        private void CreateSavedViewpointBCF(object sender, EventArgs e)
        {
            CheckSavedViewpointTopFolder();
            using (Transaction trans = new Transaction(_oDoc, "Save Viewpoints from BCF"))
            {
                CreateSavedViewpoint(false);
                trans.Commit();
            }
        }

        private int CreateSavedViewpoint(bool isJira)
        {
            try
            {
                //temp containers
                List<Autodesk.Navisworks.Api.FolderItem> folderItems = new List<Autodesk.Navisworks.Api.FolderItem>();

                int errors = 0;

                if (isJira)
                {
                    if (mainPan.jiraPan.issueList.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("Please select an issue.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        return -1;
                    }
                    foreach (object t in mainPan.jiraPan.issueList.SelectedItems)
                    {
                        int index = mainPan.jiraPan.issueList.Items.IndexOf(t);
                        Issue issue = mainPan.jira.IssuesCollection[index];

                        string issueFolderName = string.Format("{0}: {1}", issue.key, issue.fields.summary);

                        if (string.IsNullOrWhiteSpace(issue.viewpoint))
                        {
                            errors++;
                            continue;
                        }
                        // Save viewpoint file
                        string viewpointPath = string.Empty;
                        try
                        {
                            string ReportFolder = Path.Combine(Path.GetTempPath(), "BCFtemp");
                            if (!Directory.Exists(ReportFolder))
                                Directory.CreateDirectory(ReportFolder);
                            string issueFolder = Path.Combine(ReportFolder, issue.fields.guid);
                            if (!Directory.Exists(issueFolder))
                                Directory.CreateDirectory(issueFolder);
                            viewpointPath = Path.Combine(issueFolder, "viewpoint.bcfv");
                            mainPan.saveSnapshotViewpoint(issue.viewpoint, viewpointPath);
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            continue;
                        }
                        if (File.Exists(viewpointPath))
                        {
                            try
                            {
                                ARUP.IssueTracker.Classes.BCF2.VisualizationInfo issueViewpoint = ARUP.IssueTracker.Classes.BCF2.BcfContainer.DeserializeViewpoint(viewpointPath);
                                if (issueViewpoint == null)
                                {
                                    errors++;
                                    continue;
                                }
                                Viewpoint navisworksViewpoint = GetViewpointFromVisualizationInfo(issueViewpoint);
                                if (navisworksViewpoint == null)
                                {
                                    errors++;
                                    continue;
                                }
                                else
                                {
                                    Guid folderGuid = Guid.NewGuid();
                                    FolderItem issueFolder = new FolderItem() { DisplayName = issueFolderName, Guid = folderGuid };

                                    //save issue guid to DB
                                    SaveMetadata(MetadataTables.JiraIssue, folderGuid.ToString(), issue.fields.guid);

                                    // save issue metadata to a folder comment
                                    string issueMetadata = string.Empty;
                                    if (!string.IsNullOrWhiteSpace(issue.fields.summary))
                                        issueMetadata += string.Format("<Jira:Summary>{0}</Jira:Summary>{1}", issue.fields.summary, Environment.NewLine);
                                    if (!string.IsNullOrWhiteSpace(issue.fields.description))
                                        issueMetadata += string.Format("<Jira:Description>{0}</Jira:Description>{1}", issue.fields.description, Environment.NewLine);
                                    if (issue.fields.issuetype != null)
                                        issueMetadata += string.Format("<Jira:Type>{0}</Jira:Type>{1}", issue.fields.issuetype.name, Environment.NewLine);
                                    if (issue.fields.status != null)
                                        issueMetadata += string.Format("<Jira:Status>{0}</Jira:Status>{1}", issue.fields.status.name, Environment.NewLine);
                                    if (issue.fields.priority != null)
                                        issueMetadata += string.Format("<Jira:Priority>{0}</Jira:Priority>{1}", issue.fields.priority.name, Environment.NewLine);
                                    if (issue.fields.creator != null)
                                        issueMetadata += string.Format("<Jira:Creator>{0}</Jira:Creator>{1}", issue.fields.creator.displayName, Environment.NewLine);
                                    if (!string.IsNullOrWhiteSpace(issue.fields.created))
                                        issueMetadata += string.Format("<Jira:Created>{0}</Jira:Created>{1}", issue.fields.created, Environment.NewLine);
                                    if (issue.fields.assignee != null)
                                        issueMetadata += string.Format("<Jira:Assignee>{0}</Jira:Assignee>{1}", issue.fields.assignee.displayName, Environment.NewLine);
                                    if (!string.IsNullOrWhiteSpace(issue.fields.updated))
                                        issueMetadata += string.Format("<Jira:Updated>{0}</Jira:Updated>{1}", issue.fields.updated, Environment.NewLine);

                                    issueFolder.Comments.Add(new Autodesk.Navisworks.Api.Comment(issueMetadata, Autodesk.Navisworks.Api.CommentStatus.New, issue.fields.creator.displayName));

                                    // save issue comments to Navis comments
                                    SavedViewpoint savedViewpoint = new SavedViewpoint(navisworksViewpoint);
                                    if (issue.fields.comment.total > 0)
                                    {
                                        foreach (Comment2 jiraComment in issue.fields.comment.comments)
                                        {
                                            if (!string.IsNullOrWhiteSpace(jiraComment.body))
                                            {
                                                string commentMetadata = string.Empty;
                                                commentMetadata += jiraComment.body;
                                                commentMetadata += string.Format("{0}<Jira:Updated>{1}</Jira:Updated>", Environment.NewLine, jiraComment.updated);
                                                commentMetadata += string.Format("{0}<Jira:CachedId>{1}</Jira:CachedId>", Environment.NewLine, jiraComment.id);
                                                Autodesk.Navisworks.Api.Comment navisworksComment = new Autodesk.Navisworks.Api.Comment(commentMetadata, Autodesk.Navisworks.Api.CommentStatus.New, jiraComment.updateAuthor.displayName);
                                                savedViewpoint.Comments.Add(navisworksComment);
                                            }
                                        }
                                    }
                                    Guid savedViewpointGuid = Guid.NewGuid();
                                    savedViewpoint.DisplayName = "Snapshot 1";
                                    savedViewpoint.Guid = savedViewpointGuid;

                                    //save viewpoint to DB
                                    SaveMetadata(MetadataTables.Viewpoint, savedViewpointGuid.ToString(), File.ReadAllText(viewpointPath));

                                    //save all viewpoints to the issue folder
                                    issueFolder.Children.Add(savedViewpoint);
                                    //save the issue folder to folder collection
                                    folderItems.Add(issueFolder);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                                errors++;
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    if (mainPan.bcfPan.issueList.SelectedItems.Count == 0)
                    {
                        MessageBox.Show("Please select an issue.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                        return -1;
                    }
                    foreach (object t in mainPan.bcfPan.issueList.SelectedItems)
                    {
                        int index = mainPan.bcfPan.issueList.Items.IndexOf(t);
                        Markup issueBcf = mainPan.jira.Bcf.Issues[index];

                        if (issueBcf.Viewpoints.Count == 0)
                        {
                            errors++;
                            continue;
                        }

                        string issueFolderName = string.Format("{0} ({1})", issueBcf.Topic.Title, issueBcf.Topic.Guid);

                        try
                        {
                            VisualizationInfo issueViewpoint = issueBcf.Viewpoints[0].VisInfo;
                            if (issueViewpoint == null)
                            {
                                errors++;
                                continue;
                            }
                            Viewpoint navisworksViewpoint = GetViewpointFromVisualizationInfo(issueViewpoint);
                            if (navisworksViewpoint == null)
                            {
                                errors++;
                                continue;
                            }
                            else
                            {
                                Guid folderGuid = Guid.NewGuid();
                                FolderItem issueFolder = new FolderItem() { DisplayName = issueFolderName, Guid = folderGuid };

                                //save issue guid to DB
                                SaveMetadata(MetadataTables.BcfTopic, folderGuid.ToString(), ToXML(issueBcf.Topic));

                                // save issue metadata to the issue folder
                                string issueMetadata = string.Empty;
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.Title))
                                    issueMetadata += string.Format("<BCF2:Title>{0}</BCF2:Title>{1}", issueBcf.Topic.Title, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.Description))
                                    issueMetadata += string.Format("<BCF2:Description>{0}</BCF2:Description>{1}", issueBcf.Topic.Description, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.TopicType))
                                    issueMetadata += string.Format("<BCF2:TopicType>{0}</BCF2:TopicType>{1}", issueBcf.Topic.TopicType, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.TopicStatus))
                                    issueMetadata += string.Format("<BCF2:TopicStatus>{0}</BCF2:TopicStatus>{1}", issueBcf.Topic.TopicStatus, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.Priority))
                                    issueMetadata += string.Format("<BCF2:Priority>{0}</BCF2:Priority>{1}", issueBcf.Topic.Priority, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.CreationAuthor))
                                    issueMetadata += string.Format("<BCF2:CreationAuthor>{0}</BCF2:CreationAuthor>{1}", issueBcf.Topic.CreationAuthor, Environment.NewLine);
                                if (issueBcf.Topic.CreationDateSpecified)
                                    issueMetadata += string.Format("<BCF2:CreationDate>{0}</BCF2:CreationDate>{1}", issueBcf.Topic.CreationDate.ToString(), Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.AssignedTo))
                                    issueMetadata += string.Format("<BCF2:AssignedTo>{0}</BCF2:AssignedTo>{1}", issueBcf.Topic.AssignedTo, Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.ModifiedAuthor))
                                    issueMetadata += string.Format("<BCF2:ModifiedAuthor>{0}</BCF2:ModifiedAuthor>{1}", issueBcf.Topic.ModifiedAuthor, Environment.NewLine);
                                if (issueBcf.Topic.ModifiedDateSpecified)
                                    issueMetadata += string.Format("<BCF2:ModifiedDate>{0}</BCF2:ModifiedDate>{1}", issueBcf.Topic.ModifiedDate.ToString(), Environment.NewLine);
                                if (!string.IsNullOrWhiteSpace(issueBcf.Topic.ReferenceLink))
                                    issueMetadata += string.Format("<BCF2:ReferenceLink>{0}</BCF2:ReferenceLink>{1}", issueBcf.Topic.ReferenceLink.ToString(), Environment.NewLine);

                                if (!string.IsNullOrWhiteSpace(issueMetadata))
                                    issueFolder.Comments.Add(new Autodesk.Navisworks.Api.Comment(issueMetadata, Autodesk.Navisworks.Api.CommentStatus.New, issueBcf.Topic.CreationAuthor));

                                // save issue comments to Navis comment
                                SavedViewpoint savedViewpoint = new SavedViewpoint(navisworksViewpoint);
                                if (issueBcf.Comment.Count > 0)
                                {
                                    foreach (ARUP.IssueTracker.Classes.BCF2.Comment bcf2Comment in issueBcf.Comment)
                                    {
                                        if (!string.IsNullOrWhiteSpace(bcf2Comment.Comment1))
                                        {
                                            string commentMetadata = string.Empty;
                                            commentMetadata += bcf2Comment.Comment1;
                                            commentMetadata += string.Format("{0}<BCF2:Author>{1}</BCF2:Author>", Environment.NewLine, bcf2Comment.Author);
                                            commentMetadata += string.Format("{0}<BCF2:Date>{1}</BCF2:Date>", Environment.NewLine, bcf2Comment.Date);
                                            commentMetadata += string.Format("{0}<BCF2:Status>{1}</BCF2:Status>", Environment.NewLine, bcf2Comment.Status);
                                            if (!string.IsNullOrWhiteSpace(bcf2Comment.VerbalStatus))
                                                commentMetadata += string.Format("{0}<BCF2:VerbalStatus>{1}</BCF2:VerbalStatus>", Environment.NewLine, bcf2Comment.VerbalStatus);
                                            if (!string.IsNullOrWhiteSpace(bcf2Comment.ModifiedAuthor))
                                                commentMetadata += string.Format("{0}<BCF2:ModifiedAuthor>{1}</BCF2:ModifiedAuthor>", Environment.NewLine, bcf2Comment.ModifiedAuthor);
                                            if (bcf2Comment.ModifiedDateSpecified)
                                                commentMetadata += string.Format("{0}<BCF2:ModifiedDate>{1}</BCF2:ModifiedDate>", Environment.NewLine, bcf2Comment.ModifiedDate);
                                            commentMetadata += string.Format("{0}<BCF2:CachedId>{1}</BCF2:CachedId>", Environment.NewLine, bcf2Comment.Guid);
                                            Autodesk.Navisworks.Api.Comment navisworksComment = new Autodesk.Navisworks.Api.Comment(commentMetadata, Autodesk.Navisworks.Api.CommentStatus.New, bcf2Comment.Author);
                                            savedViewpoint.Comments.Add(navisworksComment);

                                            //save cached id and comment to DB
                                            SaveMetadata(MetadataTables.BcfComment, bcf2Comment.Guid, ToXML(bcf2Comment));
                                        }
                                    }
                                }
                                Guid savedViewpointGuid = Guid.NewGuid();
                                savedViewpoint.DisplayName = "Snapshot 1";
                                savedViewpoint.Guid = savedViewpointGuid;

                                //save viewpoint to DB
                                SaveMetadata(MetadataTables.Viewpoint, savedViewpointGuid.ToString(), ToXML(issueBcf.Viewpoints[0].VisInfo));

                                //save all viewpoints to the issue folder
                                issueFolder.Children.Add(savedViewpoint);
                                //save the issue folder to folder collection
                                folderItems.Add(issueFolder);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            continue;
                        }
                    }
                }

                // add new issues
                folderItems.ForEach(f => _oDoc.SavedViewpoints.AddCopy(this.topFolder, f));

                if (errors != 0)
                {
                    MessageBox.Show(errors + " viewpoints(s) were not generated because of missing files or wrong formats of viewpoint.bcfv.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                return -1;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return -1;
            }

        }

        /// <summary>
        /// Save metadata to embedded document database when creating saved viewpoints from Jira/BCF
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id">GUID or comment ID</param>
        /// <param name="metadata">Serialized XML or GUID</param>
        private void SaveMetadata(MetadataTables tableName, string id, string metadata)
        {
            //string text = string.Format("{0} - {1} - {2}", tableName.ToString(), id, metadata);
            //MessageBox.Show(text);
            //get document database 
            DocumentDatabase database = Autodesk.Navisworks.Api.Application.ActiveDocument.Database;

            // use transaction. The type for creation is Reset 
            NavisworksTransaction trans = database.BeginTransaction(DatabaseChangedAction.Reset);

            //setup SQL command  
            NavisworksCommand cmd = trans.Connection.CreateCommand();
            //creation of SQL syntax
            string sql = "CREATE TABLE IF NOT EXISTS " + tableName.ToString() + "(" +
                        "id TEXT," +
                        "metadata TEXT)";

            cmd.CommandText = sql;

            // do the job
            cmd.ExecuteNonQuery();
            //submit the transaction
            trans.Commit();


            // fill in the data
            trans = database.BeginTransaction(DatabaseChangedAction.Edited);
            cmd = trans.Connection.CreateCommand();
            cmd.Parameters.AddWithValue("@p1", id);
            cmd.Parameters.AddWithValue("@p2", metadata);

            //build the SQL text
            string insert_sql = "INSERT INTO " + tableName.ToString() + "(id, metadata)" + " VALUES(@p1, @p2);";
            cmd.CommandText = insert_sql;
            //execute SQL
            cmd.ExecuteNonQuery();
            trans.Commit();

            //database.Dispose();

        }

        private string GetMetadata(MetadataTables tableName, string id)
        {
            try
            {
                //get document database
                DocumentDatabase database = Autodesk.Navisworks.Api.Application.ActiveDocument.Database;

                //create adaptor to retrieve data from the data source.
                NavisworksDataAdapter dataAdapter = new NavisworksDataAdapter("SELECT id, metadata FROM " + tableName, database.Value);

                DataTable table = new DataTable();
                dataAdapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    if (row["id"].ToString() == id)
                        return row["metadata"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public string ToXML(object o)
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(o.GetType());
            serializer.Serialize(stringwriter, o);
            return stringwriter.ToString();
        }

        //public void setVisibility(XDocument v)
        //{
        //    try
        //    {
        //        if (v.Element("VisualizationInfo").Elements("Components").Any() && v.Element("VisualizationInfo").Elements("Components").Elements("Component").Any())
        //        {
        //            System.Collections.Generic.IEnumerable<string> guidList = from item in v.Element("VisualizationInfo").Elements("Components").Elements("Component")
        //                                                                      select (string)item.Attribute("IfcGuid");
        //            System.Collections.Generic.IEnumerable<string> authList = from item in v.Element("VisualizationInfo").Elements("Components").Elements("Component")
        //                                                                      select (string)item.Element("AuthoringToolId");


        //            //COMPONENTS PART
        //            ModelItemCollection hidden = new ModelItemCollection();

        //            //mark as invisible all the siblings of the visible items as well as the visible items
        //            foreach (var item in Autodesk.Navisworks.Api.Application.ActiveDocument.Models.First.RootItem.DescendantsAndSelf.Where(o => o.InstanceGuid != Guid.Empty))
        //            {
        //                string ifcguid = IfcGuid.ToIfcGuid(item.InstanceGuid).ToString();
        //                if (!guidList.Contains(ifcguid))//there is not its ifcguid, check for authoring tool ID
        //                {
        //                    if (null != item.PropertyCategories.FindCategoryByDisplayName("Element ID")) //has an auto tool id
        //                    {
        //                        string t = item.PropertyCategories.FindCategoryByDisplayName("Element ID")
        //                            .Properties.FindPropertyByDisplayName("Value").Value.ToDisplayString();
        //                        if (!authList.Contains(ifcguid))//does not have its auth tool id
        //                            hidden.Add(item);
        //                    }
        //                    else //has no auth tool id
        //                        hidden.Add(item);

        //                }
        //            }
        //            Autodesk.Navisworks.Api.Application.ActiveDocument.Models.ResetAllHidden();
        //            Autodesk.Navisworks.Api.Application.ActiveDocument.CurrentSelection.Clear();
        //            Autodesk.Navisworks.Api.Application.ActiveDocument.Models.SetHidden(hidden, true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }

        //}
        #region math rotations and stuff
        private Vector3D getViewDir(Viewpoint oVP)
        {
            double units = GetGunits();

            Rotation3D oRot = oVP.Rotation;
            // calculate view direction
            Rotation3D oNegtiveZ = new Rotation3D(0, 0, -1, 0);
            Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
            Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
            // get view direction
            Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

            return oViewDir.Normalize();
        }
        private Vector3D getViewUp(Viewpoint oVP)
        {
            double units = GetGunits();

            Rotation3D oRot = oVP.Rotation;
            // calculate view direction
            Rotation3D oNegtiveZ = new Rotation3D(0, 1, 0, 0);
            Rotation3D otempRot = MultiplyRotation3D(oNegtiveZ, oRot.Invert());
            Rotation3D oViewDirRot = MultiplyRotation3D(oRot, otempRot);
            // get view direction
            Vector3D oViewDir = new Vector3D(oViewDirRot.A, oViewDirRot.B, oViewDirRot.C);

            return oViewDir.Normalize();
        }

        // help function: Multiply two Rotation3D
        private Rotation3D MultiplyRotation3D(
            Rotation3D r2,
            Rotation3D r1)
        {

            Rotation3D oRot =
                new Rotation3D(r2.D * r1.A + r2.A * r1.D +
                                    r2.B * r1.C - r2.C * r1.B,
                                r2.D * r1.B + r2.B * r1.D +
                                    r2.C * r1.A - r2.A * r1.C,
                                r2.D * r1.C + r2.C * r1.D +
                                    r2.A * r1.B - r2.B * r1.A,
                                r2.D * r1.D - r2.A * r1.A -
                                    r2.B * r1.B - r2.C * r1.C);

            oRot.Normalize();

            return oRot;

        }
        private double GetGunits()
        {
            string units = _oDoc.Units.ToString();
            double factor = 1;
            switch (units)
            {
                case "Centimeters":
                    factor = 100;
                    break;
                case "Feet":
                    factor = 3.28084;
                    break;
                case "Inches":
                    factor = 39.3701;
                    break;
                case "Kilometers":
                    factor = 0.001;
                    break;
                case "Meters":
                    factor = 1;
                    break;
                case "Micrometers":
                    factor = 1000000;
                    break;
                case "Miles":
                    factor = 0.000621371;
                    break;
                case "Millimeters":
                    factor = 1000;
                    break;
                case "Mils":
                    factor = 39370.0787;
                    break;
                case "Yards":
                    factor = 1.09361;
                    break;
                default:
                    MessageBox.Show("Units " + units + " not recognized.");
                    factor = 1;
                    break;
            }
            return factor;
        }

        #endregion
    }
}
