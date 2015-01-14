using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using HOK.Utilities.RoomUpdater;

namespace HOK.Utilities.RoomElevation
{
    public class ElevationCreator
    {
        private UIApplication m_app;
        private Document m_doc;

        private Room m_room = null;
        private Wall m_wall = null;
        private LinkedInstanceProperties roomLink = null;
        private LinkedInstanceProperties wallLink = null;
        private ViewPlan m_viewPlan = null;
        private ViewFamilyType m_viewFamilyType = null;
        private int m_viewScale = 0;
        private string[] suffix = new string[] { "-A", "-B", "-C", "-D" };
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();

        public Dictionary<int, LinkedInstanceProperties> LinkedDocuments { get { return linkedDocuments; } set { linkedDocuments = value; } }

        public ElevationCreator(UIApplication uiapp, Room room, Wall wall, ViewPlan viewPlan, ViewFamilyType viewFamilyType, int scale)
        {
            m_doc = uiapp.ActiveUIDocument.Document;
            m_room = room;
            m_wall = wall;
            m_viewPlan = viewPlan;
            m_viewFamilyType = viewFamilyType;
            m_viewScale = scale;
        }

        public bool CreateElevation()
        {
            bool result = false;
            try
            {
                ElevationMarker marker = null;
                XYZ markerLocation = null;
                List<ViewSection> elevationViews = new List<ViewSection>();

                using (Transaction trans = new Transaction(m_doc, "Elevation Creator"))
                {
                    trans.Start();
                    try
                    {
                        LocationPoint locationPoint = m_room.Location as LocationPoint;
                        markerLocation = locationPoint.Point;
#if RELEASE2014||RELEASE2015
                        if (m_room.Document.IsLinked)
                        {
                            var documents = from doc in linkedDocuments.Values where doc.DocumentTitle == m_room.Document.Title select doc;
                            if (documents.Count() > 0)
                            {
                                LinkedInstanceProperties lip = documents.First();
                                roomLink = lip;
                                markerLocation = lip.TransformValue.OfPoint(markerLocation);
                            }
                        }
#endif
                        
                        marker = ElevationMarker.CreateElevationMarker(m_doc, m_viewFamilyType.Id, markerLocation, m_viewScale);
                        trans.Commit();

                        if (null != marker)
                        {
                            
                            string viewName = "Room" + m_room.Number + "-Elevation";
                            int viewCount = marker.MaximumViewCount < suffix.Length ? marker.MaximumViewCount : suffix.Length;
                            for (int i = 0; i < viewCount; i++)
                            {
                                trans.Start();
                                ViewSection viewElevation = marker.CreateElevation(m_doc, m_viewPlan.Id, i);
                                viewElevation.Name = viewName + suffix[i];

#if RELEASE2013 || RELEASE2014
                                Parameter param = viewElevation.get_Parameter("Title on Sheet");
#elif RELEASE2015
                                Parameter param = viewElevation.LookupParameter("Title on Sheet");
#endif
                                if (null != param)
                                {
                                    param.Set(m_room.Name);
                                }
                               
                                trans.Commit();

                                if (i == 0 && null != viewElevation)
                                {
                                    trans.Start();
                                    bool rotated = RotateMarker(marker, markerLocation);
                                    trans.Commit();
                                }

                                elevationViews.Add(viewElevation);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create an elevation view.\n" + ex.Message, "Create Elevation View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                if (null != marker && null != markerLocation)
                {
                    if (elevationViews.Count > 0)
                    {
                        if (ModifyCropBox(elevationViews))
                        {
                            result = true;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create an elevation view.\nRoom Name: "+m_room.Name+"\nWall Name: "+m_wall.Name+"\n"+ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        public bool RotateMarker(ElevationMarker marker, XYZ markerPoint)
        {
            bool rotated = false;
            LocationCurve locationCurve = m_wall.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
#if RELEASE2014||RELEASE2015
            if (m_wall.Document.IsLinked)
            {
                var documents = from doc in linkedDocuments.Values where doc.DocumentTitle == m_wall.Document.Title select doc;
                if (documents.Count() > 0)
                {
                    LinkedInstanceProperties lip = documents.First();
                    wallLink = lip;
                    curve = curve.CreateTransformed(wallLink.TransformValue);
                }
            }
#endif
            IntersectionResult intersectionResult = curve.Project(markerPoint);
            XYZ intersectionPoint = intersectionResult.XYZPoint;
            if (null != intersectionPoint)
            {
                XYZ directionV = intersectionPoint - markerPoint;
                double angle = XYZ.BasisY.AngleTo(directionV);
                angle = angle % (Math.PI/2);

                if (XYZ.BasisY.CrossProduct(directionV).Z < 0)
                {
                    angle = -angle;
                }

                XYZ point1 = markerPoint;
                XYZ point2 = new XYZ(markerPoint.X, markerPoint.Y, markerPoint.Z + 10);
#if RELEASE2013
                Line axis = m_app.Application.Create.NewLineBound(point1, point2);
#elif RELEASE2014 || RELEASE2015
                Line axis = Line.CreateBound(point1, point2);
#endif

                ElementTransformUtils.RotateElement(m_doc, marker.Id, axis, angle);
                rotated = true;
            }
            return rotated;
        }

        public bool ModifyCropBox(List<ViewSection> elevationViews)
        {
            bool result = false;
            try
            {
                List<XYZ> vertices = new List<XYZ>();
                GeometryElement geomElement = m_room.ClosedShell;
                if (null != geomElement)
                {
#if RELEASE2014||RELEASE2015
                    if (null != roomLink)
                    {
                        geomElement = geomElement.GetTransformed(roomLink.TransformValue);
                    }
#endif
                    foreach (GeometryObject geomObject in geomElement)
                    {
                        if (geomObject is Solid)
                        {
                            Solid solid = geomObject as Solid;
                            foreach (Edge edge in solid.Edges)
                            {
                                Curve curve = edge.AsCurve();
#if RELEASE2013
                                vertices.Add(curve.get_EndPoint(0));
                                vertices.Add(curve.get_EndPoint(1));
#else
                                vertices.Add(curve.GetEndPoint(0));
                                vertices.Add(curve.GetEndPoint(1));
#endif
                            }
                        }
                    }
                }

                if (vertices.Count > 0)
                {
                    
                    foreach (ViewSection elevationView in elevationViews)
                    {
                        List<XYZ> verticesInView = new List<XYZ>();
                        BoundingBoxXYZ bb = elevationView.CropBox;
                        if (null != bb)
                        {
                            Transform transform = bb.Transform;
                            Transform transformInverse = transform.Inverse;

                            foreach (XYZ vertex in vertices)
                            {
                                verticesInView.Add(transformInverse.OfPoint(vertex));
                            }

                            double xMin = 0, yMin = 0, xMax = 0, yMax = 0, zMin=0, zMax=0;
                            bool first = true;
                            foreach (XYZ p in verticesInView)
                            {
                                if (first)
                                {
                                    xMin = p.X;
                                    yMin = p.Y;
                                    zMin = p.Z;
                                    xMax = p.X;
                                    yMax = p.Y;
                                    zMax = p.Z;
                                    first = false;
                                }
                                else
                                {
                                    if (xMin > p.X) { xMin = p.X; }
                                    if (yMin > p.Y) { yMin = p.Y; }
                                    if (zMin > p.Z) { zMin = p.Z; }
                                    if (xMax < p.X) { xMax = p.X; }
                                    if (yMax < p.Y) { yMax = p.Y; }
                                    if (zMax < p.Z) { zMax = p.Z; }
                                }
                            }

                            using (Transaction trans = new Transaction(m_doc, "Set Crop Box"))
                            {
                                trans.Start();
                                try
                                {
                                    elevationView.CropBoxActive = false;

                                    bb.Max = new XYZ(xMax, yMax, -zMin);
                                    bb.Min = new XYZ(xMin, yMin, 0);
                                    elevationView.CropBox = bb;
                                    elevationView.CropBoxActive = true;
                                    elevationView.CropBoxVisible = true;

                                    trans.Commit();
                                    result = true;
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
                MessageBox.Show("Failed to modify the crop region in this view.\n"+ex.Message, "Modify Crop Box", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }
    }
}
