using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using ARUP.IssueTracker.Classes;
using ARUP.IssueTracker.Revit.Classes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ARUP.IssueTracker.Classes.BCF2;

namespace ARUP.IssueTracker.Revit.Entry
{
    /// <summary>
    /// Obfuscation Ignore for External Interface
    /// </summary>
    [Obfuscation(Exclude = true, ApplyToMembers = false)]
    public class ExtOpenView : IExternalEventHandler
    {

        //public Tuple<ViewOrientation3D, double, string, string> touple;
        public VisualizationInfo v;

        /// <summary>
        /// External Event Implementation
        /// </summary>
        /// <param name="app"></param>
        public void Execute(UIApplication app)
        {

            try
            {

                UIDocument uidoc = app.ActiveUIDocument;
                Document doc = uidoc.Document;
                SelElementSet m_elementsToHide = SelElementSet.Create();

                List<ElementId> elementsToBeIsolated = new List<ElementId>();
                List<ElementId> elementsToBeHidden = new List<ElementId>();
                List<ElementId> elementsToBeSelected = new List<ElementId>();


                // IS ORTHOGONAL
                if (v.OrthogonalCamera != null)
                {
                    if (v.OrthogonalCamera.ViewToWorldScale == null || v.OrthogonalCamera.CameraViewPoint == null || v.OrthogonalCamera.CameraUpVector == null || v.OrthogonalCamera.CameraDirection == null)
                        return;
                    //type = "OrthogonalCamera";
                    var zoom = UnitUtils.ConvertToInternalUnits(v.OrthogonalCamera.ViewToWorldScale, DisplayUnitType.DUT_METERS);
                    var CameraDirection = ARUP.IssueTracker.Revit.Classes.Utils.GetXYZ(v.OrthogonalCamera.CameraDirection.X, v.OrthogonalCamera.CameraDirection.Y, v.OrthogonalCamera.CameraDirection.Z);
                    var CameraUpVector = ARUP.IssueTracker.Revit.Classes.Utils.GetXYZ(v.OrthogonalCamera.CameraUpVector.X, v.OrthogonalCamera.CameraUpVector.Y, v.OrthogonalCamera.CameraUpVector.Z);
                    var CameraViewPoint = ARUP.IssueTracker.Revit.Classes.Utils.GetXYZ(v.OrthogonalCamera.CameraViewPoint.X, v.OrthogonalCamera.CameraViewPoint.Y, v.OrthogonalCamera.CameraViewPoint.Z);
                    var orient3d = ARUP.IssueTracker.Revit.Classes.Utils.ConvertBasePoint(doc, CameraViewPoint, CameraDirection, CameraUpVector, true);


                    View3D orthoView = null;
                    //if active view is 3d ortho use it
                    if (doc.ActiveView.ViewType == ViewType.ThreeD)
                    {
                        View3D ActiveView3D = doc.ActiveView as View3D;
                        if (!ActiveView3D.IsPerspective)
                            orthoView = ActiveView3D;
                    }
                    if (orthoView == null)
                    {
                        IEnumerable<View3D> viewcollector3D = get3DViews(doc);
                        //try to use default 3D view
                        if (viewcollector3D.Any() && viewcollector3D.Where(o => o.Name == "{3D}" || o.Name == "BCFortho").Any())
                            orthoView = viewcollector3D.Where(o => o.Name == "{3D}" || o.Name == "BCFortho").First();


                    }
                    using (Transaction trans = new Transaction(uidoc.Document))
                    {
                        if (trans.Start("Open orthogonal view") == TransactionStatus.Started)
                        {
                            //create a new 3d ortho view 
                            if (orthoView == null)
                            {
                                orthoView = View3D.CreateIsometric(doc, getFamilyViews(doc).First().Id);
                                orthoView.Name = "BCFortho";
                            }

                            orthoView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                            orthoView.SetOrientation(orient3d);
                            trans.Commit();
                        }
                    }
                    uidoc.ActiveView = orthoView;
                    //adjust view rectangle

                    // **** CUSTOM VALUE FOR TEKLA **** //
                    // double x = touple.Item2
                    // **** CUSTOM VALUE FOR TEKLA **** //
                    double customZoomValue = (MyProjectSettings.Get("useDefaultZoom", doc.PathName) == "1") ? 1 : 2.5;
                    double x = zoom / customZoomValue;
                    XYZ m_xyzTl = uidoc.ActiveView.Origin.Add(uidoc.ActiveView.UpDirection.Multiply(x)).Subtract(uidoc.ActiveView.RightDirection.Multiply(x));
                    XYZ m_xyzBr = uidoc.ActiveView.Origin.Subtract(uidoc.ActiveView.UpDirection.Multiply(x)).Add(uidoc.ActiveView.RightDirection.Multiply(x));
                    uidoc.GetOpenUIViews().First().ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);
                }

                else if (v.PerspectiveCamera != null)
                {
                    if (v.PerspectiveCamera.FieldOfView == null || v.PerspectiveCamera.CameraViewPoint == null || v.PerspectiveCamera.CameraUpVector == null || v.PerspectiveCamera.CameraDirection == null)
                        return;

                    var zoom = v.PerspectiveCamera.FieldOfView;
                    double z1 = 18 / Math.Tan(zoom / 2 * Math.PI / 180);//focale 1
                    double z = 18 / Math.Tan(25 / 2 * Math.PI / 180);//focale, da controllare il 18, vedi PDF
                    double factor = z1 - z;

                    var CameraDirection = ARUP.IssueTracker.Revit.Classes.Utils.GetXYZ(v.PerspectiveCamera.CameraDirection.X, v.PerspectiveCamera.CameraDirection.Y, v.PerspectiveCamera.CameraDirection.Z);
                    var CameraUpVector = ARUP.IssueTracker.Revit.Classes.Utils.GetXYZ(v.PerspectiveCamera.CameraUpVector.X, v.PerspectiveCamera.CameraUpVector.Y, v.PerspectiveCamera.CameraUpVector.Z);
                    XYZ oldO = ARUP.IssueTracker.Revit.Classes.Utils.GetXYZ(v.PerspectiveCamera.CameraViewPoint.X, v.PerspectiveCamera.CameraViewPoint.Y, v.PerspectiveCamera.CameraViewPoint.Z);
                    var CameraViewPoint = (oldO.Subtract(CameraDirection.Divide(factor)));
                    var orient3d = ARUP.IssueTracker.Revit.Classes.Utils.ConvertBasePoint(doc, CameraViewPoint, CameraDirection, CameraUpVector, true);



                    View3D perspView = null;

                    IEnumerable<View3D> viewcollector3D = get3DViews(doc);
                    if (viewcollector3D.Any() && viewcollector3D.Where(o => o.Name == "BCFpersp").Any())
                        perspView = viewcollector3D.Where(o => o.Name == "BCFpersp").First();
                    using (Transaction trans = new Transaction(uidoc.Document))
                    {
                        if (trans.Start("Open perspective view") == TransactionStatus.Started)
                        {
                            if (null == perspView)
                            {
                                perspView = View3D.CreatePerspective(doc, getFamilyViews(doc).First().Id);
                                perspView.Name = "BCFpersp";
                            }

                            perspView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                            perspView.SetOrientation(orient3d);

                            // turn on the far clip plane with standard parameter API 
                            if (perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).HasValue)
                            {
                                Parameter m_farClip = perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
                                m_farClip.Set(1);

                            }
                            // reset far clip offset
                            if (perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).HasValue)
                            {
                                Parameter m_farClipOffset = perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR);
                                m_farClipOffset.SetValueString("35");
                            }
                            // turn off the far clip plane with standard parameter API 
                            if (perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).HasValue)
                            {
                                Parameter m_farClip = perspView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR);
                                m_farClip.Set(0);

                            }
                            perspView.CropBoxActive = true;
                            perspView.CropBoxVisible = true;

                            trans.Commit();
                        }
                    }
                    uidoc.ActiveView = perspView;
                }
                else if (v.SheetCamera != null)//sheet
                {
                    //using (Transaction trans = new Transaction(uidoc.Document))
                    //{
                    //    if (trans.Start("Open sheet view") == TransactionStatus.Started)
                    //    {
                    IEnumerable<View> viewcollectorSheet = getSheets(doc, v.SheetCamera.SheetID);
                    if (!viewcollectorSheet.Any())
                    {
                        MessageBox.Show("No Sheet with Id=" + v.SheetCamera.SheetID + " found.");
                        return;
                    }
                    uidoc.ActiveView = viewcollectorSheet.First();
                    uidoc.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                    uidoc.RefreshActiveView();

                    //        trans.Commit();
                    //    }
                    //}
                    XYZ m_xyzTl = new XYZ(v.SheetCamera.TopLeft.X, v.SheetCamera.TopLeft.Y,
                                v.SheetCamera.TopLeft.Z);
                    XYZ m_xyzBr = new XYZ(v.SheetCamera.BottomRight.X, v.SheetCamera.BottomRight.Y,
                                v.SheetCamera.BottomRight.Z);
                    uidoc.GetOpenUIViews().First().ZoomAndCenterRectangle(m_xyzTl, m_xyzBr);

                }
                else
                {
                    return;
                }

                //apply BCF clipping planes to Revit section box
                View3D view3D = doc.ActiveView as View3D;
                if (view3D != null)
                {
                    if (v.ClippingPlanes != null)
                    {
                        if (v.ClippingPlanes.Count() > 0)
                        {
                            var result = getBoundingBoxFromClippingPlanes(doc, v.ClippingPlanes);

                            if (result != null)
                            {
                                BoundingBoxXYZ computedBox = result.Item1;
                                Transform rotate = result.Item2;

                                using (Transaction trans = new Transaction(uidoc.Document))
                                {
                                    if (trans.Start("Apply Section Box") == TransactionStatus.Started)
                                    {
                                        view3D.IsSectionBoxActive = true;
                                        view3D.SetSectionBox(computedBox);

                                        if (rotate != null)
                                        {
                                            // Transform the View3D's section box with the rotation transform
                                            computedBox.Transform = computedBox.Transform.Multiply(rotate);

                                            // Set the section box back to the view (requires an open transaction)
                                            view3D.SetSectionBox(computedBox);
                                        }
                                    }
                                    trans.Commit();
                                }
                            }
                        }
                    }
                }

                //select/hide elements
                if (v.Components != null && v.Components.Any())
                {
                    if (v.Components.Count > 100)
                    {
                        var result = MessageBox.Show("Too many elements attached. It may take for a while to isolate/select them. Do you want to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if(result == MessageBoxResult.No)
                        {
                            uidoc.RefreshActiveView();
                            return;
                        }
                    }

                    FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
                    System.Collections.Generic.ICollection<ElementId> collection = collector.ToElementIds();
                    foreach (var e in v.Components)
                    {
                        var bcfguid = IfcGuid.FromIfcGUID(e.IfcGuid);
                        int authoringToolId = string.IsNullOrWhiteSpace(e.AuthoringToolId) ? -1 : int.Parse(e.AuthoringToolId);
                        var ids = collection.Where(o => bcfguid == ExportUtils.GetExportId(doc, o) | authoringToolId == Convert.ToInt32(doc.GetElement(o).UniqueId.Substring(37), 16));
                        if (ids.Any())
                        {
                            // handle visibility
                            if (e.Visible)
                                elementsToBeIsolated.Add(ids.First());
                            else
                                elementsToBeHidden.Add(ids.First());

                            // handle selection
                            if (e.Selected)
                                elementsToBeSelected.Add(ids.First());
                        }
                    }

                    if (elementsToBeHidden.Count > 0)
                    {
                        using (Transaction trans = new Transaction(uidoc.Document))
                        {
                            if (trans.Start("Hide Elements") == TransactionStatus.Started)
                            {
                                uidoc.ActiveView.HideElementsTemporary(elementsToBeHidden);
                            }
                            trans.Commit();
                        }
                    }
                    else if (elementsToBeIsolated.Count > 0)
                    {
                        using (Transaction trans = new Transaction(uidoc.Document))
                        {
                            if (trans.Start("Isolate Elements") == TransactionStatus.Started)
                            {
                                uidoc.ActiveView.IsolateElementsTemporary(elementsToBeIsolated);
                            }
                            trans.Commit();
                        }
                    }

                    if (elementsToBeSelected.Count > 0)
                    {
                        using (Transaction trans = new Transaction(uidoc.Document))
                        {
                            if (trans.Start("Select Elements") == TransactionStatus.Started)
                            {
                                SelElementSet selectedElements = SelElementSet.Create();
                                elementsToBeSelected.ForEach(id => {
                                    selectedElements.Add(doc.GetElement(id));                                
                                });

                                uidoc.Selection.Elements = selectedElements;
                            }
                            trans.Commit();
                        }
                    }
                }

                uidoc.RefreshActiveView();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error!", "exception: " + ex);
            }
        }

        /// <summary>
        /// Calculate the max point and the min point of Revit section box based on BCF clipping planes
        /// </summary>
        /// <param name="clippingPlanes">clipping planes from BCF viewpoint</param>
        /// <returns>1: max, 2: min</returns>
        private Tuple<BoundingBoxXYZ, Transform> getBoundingBoxFromClippingPlanes(Document doc, ClippingPlane[] clippingPlanes)
        {
            const double tolerance = 0.0000001;

            if (clippingPlanes.Count() != 6)
            {
                return null;
            }

            try
            {
                List<ClippingPlane> cPlanes = clippingPlanes.ToList();
                double maxZ, minZ;

                // checking z direction normals
                List<XYZ> zPoints = new List<XYZ>();
                List<ClippingPlane> xyClipppingPlanes = new List<ClippingPlane>();
                foreach (ClippingPlane cp in cPlanes)
                {
                    XYZ zDirection = new XYZ(0, 0, 1);
                    XYZ normal = new XYZ(cp.Direction.X, cp.Direction.Y, cp.Direction.Z);
                    if (normal.IsAlmostEqualTo(zDirection, tolerance) || normal.IsAlmostEqualTo(-zDirection, tolerance))
                    {
                        zPoints.Add(new XYZ(cp.Location.X, cp.Location.Y, cp.Location.Z));
                    }
                    else
                    {
                        xyClipppingPlanes.Add(cp);
                    }
                }
                if (zPoints.Count != 2)
                {
                    return null;
                }
                else
                {
                    maxZ = zPoints[0].Z > zPoints[1].Z ? zPoints[0].Z : zPoints[1].Z;
                    minZ = zPoints[0].Z < zPoints[1].Z ? zPoints[0].Z : zPoints[1].Z;
                    maxZ = UnitUtils.ConvertToInternalUnits(maxZ, DisplayUnitType.DUT_METERS);
                    minZ = UnitUtils.ConvertToInternalUnits(minZ, DisplayUnitType.DUT_METERS);
                }

                // check if the remaining 4 points are on XY plane
                //if (!xyClipppingPlanes.TrueForAll(cp => (cp.Location.Z < tolerance && cp.Location.Z > -tolerance)))
                //{
                //    return null;
                //}

                // find out lines orthorgonal to self-normal
                List<Autodesk.Revit.DB.Line> linesToBeIntersected = new List<Autodesk.Revit.DB.Line>();
                foreach (ClippingPlane cp in xyClipppingPlanes)
                {
                    XYZ planeNormal = new XYZ(cp.Direction.X, cp.Direction.Y, cp.Direction.Z);
                    ClippingPlane othorgonalPlane = xyClipppingPlanes.Find(c => !(
                        new XYZ(c.Direction.X, c.Direction.Y, c.Direction.Z).IsAlmostEqualTo(planeNormal, tolerance)
                        ||
                        new XYZ(c.Direction.X, c.Direction.Y, c.Direction.Z).IsAlmostEqualTo(-planeNormal, tolerance)
                    ));
                    XYZ othorgonalNormal = new XYZ(othorgonalPlane.Direction.X, othorgonalPlane.Direction.Y, othorgonalPlane.Direction.Z);
                    XYZ planeOrigin = new XYZ(cp.Location.X, cp.Location.Y, 0);

                    linesToBeIntersected.Add(Autodesk.Revit.DB.Line.CreateUnbound(planeOrigin, othorgonalNormal));
                }

                // get intersection results
                List<XYZ> intersectedPoints = new List<XYZ>();
                foreach (Autodesk.Revit.DB.Line line1 in linesToBeIntersected)
                {
                    foreach (Autodesk.Revit.DB.Line line2 in linesToBeIntersected)
                    {
                        if (line1 != line2)
                        {
                            // calculate intersection points
                            double a1 = line1.Direction.Y;
                            double b1 = line1.Direction.X;
                            double a2 = line2.Direction.Y;
                            double b2 = line2.Direction.X;

                            // if not parallel
                            double delta = b1 * a2 - a1 * b2;
                            if (delta > tolerance || delta < -tolerance)
                            {
                                double c1 = a1 * line1.Origin.X - b1 * line1.Origin.Y;
                                double c2 = a2 * line2.Origin.X - b2 * line2.Origin.Y;

                                double deltaX = b1 * c2 - b2 * c1;
                                double deltaY = a1 * c2 - a2 * c1;

                                double intersectionX = deltaX / delta;
                                double intersectionY = deltaY / delta;
                                intersectedPoints.Add(new XYZ(intersectionX, intersectionY, 0));
                            }
                        }
                    }
                }

                // find rightmost, leftmost, topmost, and bottommost points
                XYZ rightmost = intersectedPoints[0];
                XYZ leftmost = intersectedPoints[0];
                XYZ topmost = intersectedPoints[0];
                XYZ bottommost = intersectedPoints[0]; // for non-rotated section box only
                if (intersectedPoints.Count < 4)
                {
                    return null;
                }
                else
                {
                    foreach (XYZ p in intersectedPoints)
                    {
                        if (p.X > rightmost.X)
                            rightmost = p;
                        if (p.X < leftmost.X)
                            leftmost = p;
                        if (p.Y > topmost.Y)
                            topmost = p;
                        if (p.Y < bottommost.Y)
                            bottommost = p;
                    }
                }

                // change the coordinate system from Project to Shared
                rightmost = ConvertToInteranlAndSharedCoordinate(doc, rightmost);
                leftmost = ConvertToInteranlAndSharedCoordinate(doc, leftmost);
                topmost = ConvertToInteranlAndSharedCoordinate(doc, topmost);
                bottommost = ConvertToInteranlAndSharedCoordinate(doc, bottommost);

                // create diagonal and rotation vector
                XYZ horizontalBase = new XYZ(-1, 0, 0);
                Autodesk.Revit.DB.Line diagonal = Autodesk.Revit.DB.Line.CreateBound(rightmost, leftmost);
                Autodesk.Revit.DB.Line rotationBase = !rightmost.IsAlmostEqualTo(topmost, tolerance) ?
                                                        Autodesk.Revit.DB.Line.CreateBound(rightmost, topmost) :
                                                        Autodesk.Revit.DB.Line.CreateUnbound(new XYZ(0, 0, 0), horizontalBase);

                // return these two guys
                BoundingBoxXYZ bBox = new BoundingBoxXYZ();
                Transform originalTransform = null;

                // compute a correct section box depending on two conditions                
                if (rightmost.IsAlmostEqualTo(topmost, tolerance) ||
                    leftmost.IsAlmostEqualTo(bottommost, tolerance) ||
                    horizontalBase.IsAlmostEqualTo(rotationBase.Direction, tolerance) ||
                    horizontalBase.IsAlmostEqualTo(-rotationBase.Direction, tolerance))
                {  //non-rotated section box

                    XYZ max = new XYZ(
                        rightmost.X,
                        topmost.Y,
                        maxZ
                    );
                    XYZ min = new XYZ(
                        leftmost.X,
                        bottommost.Y,
                        minZ
                    );

                    bBox.Max = max;
                    bBox.Min = min;
                }
                else //rotated section box
                {
                    // calculate rotation angle
                    double angle = horizontalBase.AngleTo(rotationBase.Direction);

                    // create transform
                    Transform transform = Transform.CreateRotationAtPoint(new XYZ(0, 0, 1), angle, rightmost);

                    // rotate it then get the rotated bounding box projection point (i.e., min. of rorated section box)
                    XYZ rotatedMin = diagonal.CreateTransformed(transform).GetEndPoint(1);

                    // create rotated section box with max and min 
                    XYZ max = new XYZ(
                        rightmost.X,
                        rightmost.Y,
                        maxZ
                    );
                    XYZ min = new XYZ(
                        rotatedMin.X,
                        rotatedMin.Y,
                        minZ
                    );

                    bBox.Max = max;
                    bBox.Min = min;

                    // rotate back to the original position
                    originalTransform = Transform.CreateRotationAtPoint(new XYZ(0, 0, 1), -angle, max);
                }

                return new Tuple<BoundingBoxXYZ, Transform>(bBox, originalTransform);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }

        }

        private XYZ ConvertToInteranlAndSharedCoordinate(Document doc, XYZ p)
        {
            DisplayUnitType lengthUnitType = DisplayUnitType.DUT_METERS;
            p = new XYZ(
                        UnitUtils.ConvertToInternalUnits(p.X, lengthUnitType),
                        UnitUtils.ConvertToInternalUnits(p.Y, lengthUnitType),
                        UnitUtils.ConvertToInternalUnits(p.Z, lengthUnitType)
                    );
            p = ARUP.IssueTracker.Revit.Classes.Utils.ConvertToFromSharedCoordinate(doc, p, true);

            return p;
        }

        private System.Collections.Generic.IEnumerable<ViewFamilyType> getFamilyViews(Document doc)
        {

            return from elem in new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                   let type = elem as ViewFamilyType
                   where type.ViewFamily == ViewFamily.ThreeDimensional
                   select type;
        }
        private IEnumerable<View3D> get3DViews(Document doc)
        {
            return from elem in new FilteredElementCollector(doc).OfClass(typeof(View3D))
                   let view = elem as View3D
                   select view;
        }
        private IEnumerable<View> getSheets(Document doc, int id)
        {
            ElementId eid = new ElementId(id);
            return from elem in new FilteredElementCollector(doc).OfClass(typeof(View))
                   let view = elem as View
                   where view.Id == eid
                   select view;
        }
        private string ToBcfViewName(string name)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            name = rgx.Replace(name, "");
            return "BCF-" + name;
        }



        public string GetName()
        {
            return "Open 3D View";
        }
        // returns XYZ and ZOOM/FOV value


        public static string MakeUniqueFileName(string file, System.Collections.Generic.IEnumerable<View3D> views)
        {
            string fn;

            for (int i = 0; ; ++i)
            {
                fn = file + i.ToString();
                System.Collections.Generic.IEnumerable<View3D> m_nviews = from elem in views
                                                                          let type = elem as View3D
                                                                          where type.Name == fn
                                                                          select type;

                if (m_nviews.Count() == 0)
                    return fn;
            }
        }

        private bool convertToBool(string s)
        {
            return s != "" && Convert.ToBoolean(s);
        }
    }

}