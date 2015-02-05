using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace HOK.Utilities.RoomElevation
{
    public class ElevationCreator
    {
        private UIApplication m_app;
        private Document m_doc;

        private Room m_room = null;
        private RoomElevationProperties roomProperties = null;
        private Wall m_wall = null;
        private LinkedInstanceProperties roomLink = null;
        private LinkedInstanceProperties wallLink = null;
        private ViewPlan m_viewPlan = null;
        private ViewFamilyType m_viewFamilyType = null;
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();
        private ElevationCreatorSettings toolSettings = null;

        public ElevationCreatorSettings ToolSettings { get { return toolSettings; } set { toolSettings = value; } }
        public RoomElevationProperties RoomProperties { get { return roomProperties; } set { roomProperties = value; } }

        public ElevationCreator(UIApplication uiapp, RoomElevationProperties rep, Wall wall, ElevationCreatorSettings settings, Dictionary<int, LinkedInstanceProperties> linkedInstances)
        {
            //create by pick elements
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
            roomProperties = rep;
            m_room = roomProperties.RoomObj;
            m_wall = wall;
            toolSettings = settings;
            linkedDocuments = linkedInstances;
            m_viewPlan = toolSettings.ActiveViewPlan;
            m_viewFamilyType = toolSettings.ElevationFamilyType;
            
        }

        public ElevationCreator(UIApplication uiapp, RoomElevationProperties rep, ElevationCreatorSettings settings, Dictionary<int, LinkedInstanceProperties> linkedInstances)
        {
            //create by room list
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
            roomProperties = rep;
            m_room = roomProperties.RoomObj;
            toolSettings = settings;
            linkedDocuments = linkedInstances;
            m_viewPlan = toolSettings.ActiveViewPlan;
            m_viewFamilyType = toolSettings.ElevationFamilyType;

        }

        public bool CheckExisting()
        {
            bool result = false;
            try
            {
                //delete existing
                if (roomProperties.ElevationViews.Count > 0)
                {
                    ElementId markId = new ElementId(roomProperties.ElevationMarkId);
                    if (markId != ElementId.InvalidElementId)
                    {
                        using (Transaction trans = new Transaction(m_doc, "Delete Elevation Mark"))
                        {
                            trans.Start();
                            FailureHandlingOptions failureOptions = trans.GetFailureHandlingOptions();
                            failureOptions.SetFailuresPreprocessor(new DeleteViewsPreprocessor());
                            try
                            {
                                m_doc.Delete(markId);
                                trans.Commit(failureOptions);
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                    roomProperties.ElevationMarkId = -1;
                    roomProperties.ElevationViews = new Dictionary<int, ElevationViewProperties>();
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check existing views.\n"+ex.Message , "Elevation Creator: CheckExisting", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }


        public bool CreateElevationByList()
        {
            bool result = false;
            try
            {
                ElevationMarker marker = null;
                XYZ markerLocation = null;
                Dictionary<int, ElevationViewProperties> elevationViews = new Dictionary<int, ElevationViewProperties>();

                ApplyTemplateSettings();

                using (Transaction trans = new Transaction(m_doc, "Create Elevation Marker"))
                {
                    trans.Start();
                    try
                    {
                        BoundingBoxXYZ bbBox = m_room.get_BoundingBox(null);
                        markerLocation = new XYZ((bbBox.Max.X + bbBox.Min.X) / 2, (bbBox.Max.Y + bbBox.Min.Y) / 2, bbBox.Min.Z);

                        //LocationPoint locationPoint = m_room.Location as LocationPoint;
                        //markerLocation = locationPoint.Point;
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

                        marker = ElevationMarker.CreateElevationMarker(m_doc, m_viewFamilyType.Id, markerLocation, toolSettings.CustomScale);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create an elevation marker.\n" + ex.Message, "Elevation Creator: Create Elevation Marker", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (null != marker)
                {
                    using (Transaction trans = new Transaction(m_doc, "Create Elevation Views"))
                    {
                        trans.Start();
                        try
                        {
                            string prefix = toolSettings.PrefixText;
                            string intermediateText = GetRoomParameterValue(m_room, toolSettings.IntermediateText);
                            string suffix = GetRoomParameterValue(m_room, toolSettings.SuffixText);

                            int viewCount = marker.MaximumViewCount < 4 ? marker.MaximumViewCount : 4;

                            for (int i = 0; i < viewCount; i++)
                            {
                                string indexText = "";
                                if (i == 0 && toolSettings.DIsSelected) { indexText = "D"; }
                                else if (i == 1 && toolSettings.AIsSelected) { indexText = "A"; }
                                else if (i == 2 && toolSettings.BIsSelected) { indexText = "B"; }
                                else if (i == 3 && toolSettings.CIsSelected) { indexText = "C"; }
                                else { continue; }

                                ViewSection viewElevation = marker.CreateElevation(m_doc, m_viewPlan.Id, i);
                                viewElevation.Name = GetViewName(prefix, intermediateText, indexText, suffix);
                                if (toolSettings.ViewTemplateId != -1)
                                {
                                    viewElevation.ViewTemplateId = new ElementId(toolSettings.ViewTemplateId);
                                }

#if RELEASE2013 || RELEASE2014
                                Parameter param = viewElevation.get_Parameter("Title on Sheet");
#elif RELEASE2015
                                Parameter param = viewElevation.LookupParameter("Title on Sheet");
#endif
                                if (null != param)
                                {
                                    param.Set(m_room.Name);
                                }

                               

                                ElevationViewProperties viewProperties = new ElevationViewProperties(viewElevation);
                                if (!elevationViews.ContainsKey(viewProperties.ViewId))
                                {
                                    elevationViews.Add(viewProperties.ViewId, viewProperties);
                                }
                            }
                            roomProperties.ElevationMarkId = marker.Id.IntegerValue;
                            roomProperties.ElevationViews = elevationViews;
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.RollBack();
                            MessageBox.Show("Failed to create elevation views.\n" + ex.Message, "Elevation Creator: Create Elevation Views", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }

                if (null != marker && null != markerLocation)
                {
                    if (ModifyCropBox(roomProperties))
                    {
                        if (ElevationCreatorDataStorageUtil.StoreRoomElevationProperties(m_doc, roomProperties))
                        {
                            //update extensible storage
                            result = true;
                        }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create elevation by room lists.\n"+ex.Message, "Elevation Creator: CreateElevationByList", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        public bool CreateElevationByWall()
        {
            bool result = false;
            try
            {
                ElevationMarker marker = null;
                XYZ markerLocation = null;
                Dictionary<int, ElevationViewProperties> elevationViews = new Dictionary<int, ElevationViewProperties>();
                ApplyTemplateSettings();

                using (Transaction trans = new Transaction(m_doc, "Elevation Creator"))
                {
                    trans.Start();
                    try
                    {
                        BoundingBoxXYZ bbBox = m_room.get_BoundingBox(null);
                        markerLocation = new XYZ((bbBox.Max.X + bbBox.Min.X) / 2, (bbBox.Max.Y + bbBox.Min.Y) / 2, bbBox.Min.Z);
                        
                        //LocationPoint locationPoint = m_room.Location as LocationPoint;
                        //markerLocation = locationPoint.Point;
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
                        
                        marker = ElevationMarker.CreateElevationMarker(m_doc, m_viewFamilyType.Id, markerLocation, toolSettings.CustomScale);
                        trans.Commit();

                        if (null != marker)
                        {
                            string prefix = toolSettings.PrefixText;
                            string intermediateText = GetRoomParameterValue(m_room, toolSettings.IntermediateText);
                            string suffix = GetRoomParameterValue(m_room, toolSettings.SuffixText);

                            int viewCount = marker.MaximumViewCount < suffix.Length ? marker.MaximumViewCount : suffix.Length;

                            double rotationalAngle = 0;
                            Dictionary<int, string> indexLabels = GetMarkerLabel(markerLocation, out rotationalAngle);
                            bool firstView = true;
                            foreach (int index in indexLabels.Keys)
                            {
                                if (index < marker.MaximumViewCount)
                                {
                                    trans.Start();
                                    ViewSection viewElevation = marker.CreateElevation(m_doc, m_viewPlan.Id, index);
                                    viewElevation.Name = GetViewName(prefix, intermediateText, indexLabels[index], suffix);
                                    if (toolSettings.ViewTemplateId != -1)
                                    {
                                        viewElevation.ViewTemplateId = new ElementId(toolSettings.ViewTemplateId);
                                    }

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

                                    if (firstView && null != viewElevation)
                                    {
                                        trans.Start();
                                        try
                                        {
                                            bool rotated = RotateMarker(marker, markerLocation, rotationalAngle);
                                            firstView = false;
                                            trans.Commit();
                                        }
                                        catch(Exception ex)
                                        {
                                            trans.RollBack();
                                            MessageBox.Show("Failed to rotate the elevation marker.\n"+ex.Message , "Elevation Creator : RotateMarker", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }

                                    ElevationViewProperties viewProperties = new ElevationViewProperties(viewElevation);
                                    viewProperties.WallId = m_wall.Id.IntegerValue;
                                    if (!elevationViews.ContainsKey(viewProperties.ViewId))
                                    {
                                        elevationViews.Add(viewProperties.ViewId, viewProperties);
                                    }
                                }
                            }
                            roomProperties.ElevationMarkId = marker.Id.IntegerValue;
                            roomProperties.ElevationViews = elevationViews;
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Failed to create an elevation view.\n" + ex.Message, "Create Elevation View", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                if (null != marker && null != markerLocation)
                {
                    if (ModifyCropBox(roomProperties))
                    {
                        if (ElevationCreatorDataStorageUtil.StoreRoomElevationProperties(m_doc, roomProperties))
                        {
                            //update extensible storage
                            result = true;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create an elevation view.\nRoom Name: "+m_room.Name+"\nWall Name: "+m_wall.Name+"\n"+ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private void ApplyTemplateSettings()
        {
            try
            {
                if (toolSettings.ViewTemplateId != -1)
                {
                    ViewSection viewSection = m_doc.GetElement(new ElementId(toolSettings.ViewTemplateId)) as ViewSection;
                    if (null != viewSection)
                    {
                        ICollection<ElementId> parameterIds = viewSection.GetTemplateParameterIds();
                        int viewScaleImperial = (int)BuiltInParameter.VIEW_SCALE_PULLDOWN_IMPERIAL;
                        int viewScaleMetric = (int)BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC;
                        int viewScale = (int)BuiltInParameter.VIEW_SCALE;

                        var selectedIds = from paramId in parameterIds where paramId.IntegerValue == viewScaleImperial || paramId.IntegerValue == viewScaleMetric || paramId.IntegerValue == viewScale select paramId;
                        if (selectedIds.Count() > 0)
                        {
                            using (Transaction trans = new Transaction(m_doc, "SetViewTemplate"))
                            {
                                trans.Start();
                                try
                                {
                                    if (toolSettings.ScaleByTemplate)
                                    {
                                        toolSettings.CustomScale = viewSection.Scale;
                                    }
                                    else
                                    {
                                        viewSection.SetNonControlledTemplateParameterIds(selectedIds.ToList());
                                    }
                                   
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    MessageBox.Show("Failed to include or exclude View Scale parameter from the view template.\n"+ex.Message, "View Template", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply template settings.\n"+ex.Message, "Elevation Creator : ApplyTemplateSettings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetRoomParameterValue(Room room, string parameterName)
        {
            string parameterValue = parameterName;
            try
            {
#if RELEASE2013 || RELEASE2014
                Parameter param = room.get_Parameter(parameterName);
#elif RELEASE2015
                Parameter param = room.LookupParameter(parameterName);
#endif
                if (null != param)
                {
                    if (param.StorageType == StorageType.String)
                    {
                        parameterValue = param.AsString();
                    }
                    else
                    {
                        parameterValue = param.AsValueString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the room parameter value for the view name.\n"+ex.Message, "Elevation Creator: GetRoomParameterValue", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return parameterValue;
        }

        private string GetViewName(string prefix, string intermediateText, string indexText, string suffix)
        {
            string viewName = "";
            try
            {
                if (toolSettings.PrefixSelected)
                {
                    viewName = prefix;
                }
                if (toolSettings.IntermediateSelected)
                {
                    viewName += intermediateText;
                }

                if (toolSettings.ElevationSelected)
                {
                    if (!string.IsNullOrEmpty(viewName))
                    {
                        viewName += "-Elevation";
                    }
                    else
                    {
                        viewName += "Elevation";
                    }
                }

                if (toolSettings.ABCDSelected)
                {
                    if (!string.IsNullOrEmpty(viewName))
                    {
                        viewName += "-" + indexText;
                    }
                    else
                    {
                        viewName += indexText;
                    }
                }

                if (toolSettings.SuffixSelected)
                {
                    if (!string.IsNullOrEmpty(viewName))
                    {
                        viewName += " (" + suffix+")";
                    }
                    else
                    {
                        viewName += "(" + suffix + ")";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a view name.\n"+ex.Message, "Elevation Creator : GetViewName", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return viewName;
        }

        private Dictionary<int/*index*/, string/*labelText*/> GetMarkerLabel(XYZ markerPoint, out double rotationalAngle)
        {
            Dictionary<int, string> indexDictionary = new Dictionary<int, string>();
            rotationalAngle = 0;
            try
            {
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
#elif   RELEASE2013
                
#endif

                IntersectionResult intersectionResult = curve.Project(markerPoint);
                XYZ intersectionPoint = intersectionResult.XYZPoint;
                if (null != intersectionPoint)
                {
                    XYZ directionV = intersectionPoint - markerPoint;
                    double angle = XYZ.BasisY.AngleTo(directionV);
                    rotationalAngle = angle % (Math.PI / 2);

                    bool counterClockwise = true;
                    if (XYZ.BasisY.CrossProduct(directionV).Z < 0)
                    {
                        rotationalAngle = -rotationalAngle;
                        counterClockwise = false;
                    }

                    if (counterClockwise)
                    {
                        if (angle <(0.5*Math.PI))
                        {
                            if (toolSettings.AIsSelected) { indexDictionary.Add(1, "A"); }
                            if (toolSettings.BIsSelected) { indexDictionary.Add(2, "B"); }
                            if (toolSettings.CIsSelected) { indexDictionary.Add(3, "C"); }
                            if (toolSettings.DIsSelected) { indexDictionary.Add(0, "D"); }

                        }
                        else if (angle >= (0.5*Math.PI) || angle < Math.PI)
                        {
                            if (toolSettings.AIsSelected) { indexDictionary.Add(0, "A"); }
                            if (toolSettings.BIsSelected) { indexDictionary.Add(1, "B"); }
                            if (toolSettings.CIsSelected) { indexDictionary.Add(2, "C"); }
                            if (toolSettings.DIsSelected) { indexDictionary.Add(3, "D"); }
                        }
                    }
                    else
                    {
                        if (angle < (0.5*Math.PI))
                        {
                            if (toolSettings.AIsSelected) { indexDictionary.Add(1, "A"); }
                            if (toolSettings.BIsSelected) { indexDictionary.Add(2, "B"); }
                            if (toolSettings.CIsSelected) { indexDictionary.Add(3, "C"); }
                            if (toolSettings.DIsSelected) { indexDictionary.Add(0, "D"); }
                        }
                        else if (angle >= (0.5*Math.PI) || angle < Math.PI)
                        {
                            if (toolSettings.AIsSelected) { indexDictionary.Add(2, "A"); }
                            if (toolSettings.BIsSelected) { indexDictionary.Add(3, "B"); }
                            if (toolSettings.CIsSelected) { indexDictionary.Add(0, "C"); }
                            if (toolSettings.DIsSelected) { indexDictionary.Add(1, "D"); }
                        }
                    }

                    if (angle == Math.PI)
                    {
                        if (toolSettings.AIsSelected) { indexDictionary.Add(3, "A"); }
                        if (toolSettings.BIsSelected) { indexDictionary.Add(0, "B"); }
                        if (toolSettings.CIsSelected) { indexDictionary.Add(1, "C"); }
                        if (toolSettings.DIsSelected) { indexDictionary.Add(2, "D"); }
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to get the index for marker labels.\n"+ex.Message, "Elevation Creator: Get Marker Label", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return indexDictionary;
        }

        public bool RotateMarker(ElevationMarker marker, XYZ markerPoint, double angle)
        {
            bool rotated = false;
            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to rotate the elevation marker.\n"+ex.Message, "Elevation Creator: RotateMarker", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            return rotated;
        }

        public bool ModifyCropBox(RoomElevationProperties rep)
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
                    if (rep.ElevationViews.Count > 0)
                    {
                        foreach (ElevationViewProperties evp in rep.ElevationViews.Values)
                        {
                            ViewSection elevationView = evp.ViewObj;
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

                                double xMin = 0, yMin = 0, xMax = 0, yMax = 0, zMin = 0, zMax = 0;
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
                                        int spacing = toolSettings.SpaceAround;

                                        bb.Max = new XYZ(xMax + spacing, yMax + spacing, -zMin);
                                        bb.Min = new XYZ(xMin - spacing, yMin - spacing, 0);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to modify the crop region in this view.\n"+ex.Message, "Modify Crop Box", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }
    }
}
