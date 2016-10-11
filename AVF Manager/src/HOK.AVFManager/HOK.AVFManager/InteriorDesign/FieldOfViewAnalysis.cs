using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using HOK.AVFManager.GenericClasses;
using System.Windows.Forms;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using System.IO;
using Autodesk.Revit.UI;

namespace HOK.AVFManager.InteriorDesign
{
    public class FieldOfViewAnalysis
    {
        private Document doc;
        private SettingProperties settings;
        private ToolStripProgressBar progressBar;
        private SpatialFieldManager sfm;
        private FamilyInstance sphere = null;
        private bool overwriteResult = false;
        private List<string> unitNames = new List<string>();
        private List<double> multipliers = new List<double>();
        private Dictionary<int/*roomId*/, Face> roomFaces = new Dictionary<int, Face>();
        private Dictionary<int/*floorId*/, Face> floorFaces = new Dictionary<int, Face>();
        
        private Dictionary<int/*wallId*/, Face> wallFaces = new Dictionary<int, Face>();

        public List<string> UnitNames { get { return unitNames; } set { unitNames = value; } }
        public List<double> Multipliers { get { return multipliers; } set { multipliers = value; } }

        public FieldOfViewAnalysis(Document document, SettingProperties settingProperties, ToolStripProgressBar toolStripProgressBar, SpatialFieldManager fieldManager)
        {
            doc = document;
            settings = settingProperties;
            progressBar = toolStripProgressBar;
            sfm = fieldManager;
        }

        public FamilyInstance FindPointOfView()
        {
            sphere = null;
            try
            {
                if (null != settings.ReferenceElements)
                {
                    sphere = settings.ReferenceElements.First() as FamilyInstance;
                }
                else
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    collector.WherePasses(new ElementClassFilter(typeof(FamilyInstance)));
                    var sphereElements = from element in collector where element.Name == "PointOfView" select element;
                    if (sphereElements.Count() == 0)
                    {
                        Family family = null;
                        FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                        collector2.WherePasses(new ElementClassFilter(typeof(Family)));
                        var sphereFamily = from element in collector2 where element.Name == "PointOfView" select element;
                        if (sphereFamily.Count()==0)
                        {
                            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                            string fileName = Path.GetDirectoryName(currentAssembly) + "/Resources/PointOfView.rfa";
                            doc.LoadFamily(fileName, out family);
                        }

                        MessageBox.Show("No reference object was found.\n Please insert a family instance Generic Models >> PointOfView.\n Otherwise, pick a window element as a reference object.", "No Reference Object", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    }
                    else
                    {
                        sphere = sphereElements.Cast<FamilyInstance>().First<FamilyInstance>();
                    }
                }
                return sphere;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a point obect of view\n" + ex.Message, "FieldOfViewAnalysis:FindPointOfView", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return sphere;
            }
        }

        public bool AnalysisByElements()
        {
            bool result = false;
            try
            {
                FindFloorFaces();
                if (floorFaces.Count > 0)
                {
                    progressBar.Maximum = floorFaces.Count * 30;
                    result = MeasureDistanceOnFloor();
                }

                FindRoomFaces();
                if (roomFaces.Count > 0)
                {
                    progressBar.Maximum = roomFaces.Count;
                    result = MeasureDistanceOnRoom();
                }
                SetCurrentStyle();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to analyze elements. \n" + ex.Message, "FieldOfViewAnalysis : AnalysisByElements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private void FindRoomFaces()
        {
            Options options = new Options();
            options.ComputeReferences = true;
            options.IncludeNonVisibleObjects = true;

            foreach (Element element in settings.SelectedElements)
            {
                Room room = element as Room;
                GeometryElement geomElem = null;
                if (null != room)
                {
                    geomElem = room.get_Geometry(options);
                }
                if (null != geomElem)
                {
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        Solid solid = geomObj as Solid;
                        if (solid != null)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                if (AnalysisHelper.DetermineFaceDirection(face, DisplayingFaces.Bottom))
                                {
                                    //roomFaces.Add(room.Id.IntegerValue, CreateGeometry(face));
                                    roomFaces.Add(room.Id.IntegerValue, face);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Face CreateGeometry(Face bottomFace)
        {
            Face newFace = null;
            Solid extrusion = null;
            EdgeArrayArray edgeArrayArray = new EdgeArrayArray();
            edgeArrayArray = bottomFace.EdgeLoops;
            List<Curve> curves = new List<Curve>();
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    curves.Add(edge.AsCurve());
                }
                CurveLoop profile = CurveLoop.Create(curves);
                List<CurveLoop> loops = new List<CurveLoop>();
                loops.Add(profile);
                
                XYZ direction = new XYZ(0, 0, 1);
                extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(loops, direction, 5);
            }

            if (null != extrusion)
            {
                foreach (Face face in extrusion.Faces)
                {
                    if (AnalysisHelper.DetermineFaceDirection(face, DisplayingFaces.Bottom))
                    {
                        newFace = face;
                    }
                }
            }

            return newFace;
        }

        private void FindFloorFaces()
        {
            Options options = new Options();
            options.ComputeReferences = true;

            foreach (Element element in settings.SelectedElements)
            {
                Floor floor = element as Floor;
                if (null != floor)
                {
                    GeometryElement geomElem = floor.get_Geometry(options);
                    if (null != geomElem)
                    {
                        foreach (GeometryObject geomObj in geomElem)
                        {
                            Solid solid = geomObj as Solid;
                            if (solid != null)
                            {
                                foreach (Face face in solid.Faces)
                                {
                                    if (AnalysisHelper.DetermineFaceDirection(face, DisplayingFaces.Top))
                                    {
                                        if (!floorFaces.ContainsKey(floor.Id.IntegerValue))
                                        {
                                            floorFaces.Add(floor.Id.IntegerValue, face);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Display on the floor
        private bool MeasureDistanceOnFloor()
        {
            bool result = false;
            try
            {
                //find walls located inside the selected rooms
                FindWallFacesOnFloor();

                //Find point of view from the sphere element
                if (null != sphere)
                {
                    LocationPoint location = sphere.Location as LocationPoint;
                    XYZ pointXYZ = location.Point;

                    int resultIndex = FindIndexOfResult(sfm);
                    bool firstLoop = true;
                    foreach (int floorId in floorFaces.Keys)
                    {
                        Face floorFace = floorFaces[floorId];
                        XYZ vectorZ = new XYZ(0, 0, 1);
#if RELEASE2013
                        Transform transform = Transform.get_Translation(vectorZ);
#else
                        Transform transform = Transform.CreateTranslation(vectorZ);
#endif

                        int index = sfm.AddSpatialFieldPrimitive(floorFace,transform);

                        List<double> doubleList = new List<double>();
                        IList<UV> uvPoints = new List<UV>();
                        IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                        BoundingBoxUV bb = floorFace.GetBoundingBox();
                        for (double u = bb.Min.U; u < bb.Max.U; u = u + (bb.Max.U - bb.Min.U) / 30)
                        {
                            for (double v = bb.Min.V; v < bb.Max.V; v = v + (bb.Max.V - bb.Min.V) / 30)
                            {
                                UV uvPoint = new UV(u, v);
                                uvPoints.Add(uvPoint);
                                XYZ faceXYZ = floorFace.Evaluate(uvPoint);
#if RELEASE2013
                                Line line = doc.Application.Create.NewLine(pointXYZ, faceXYZ, true);
#else
                                Line line = Line.CreateBound(pointXYZ, faceXYZ);
#endif
                                double dblValue = 1 ;

                                IntersectionResultArray resultArray;
                                foreach (int wallId in wallFaces.Keys)
                                {
                                    Face wallFace = wallFaces[wallId];
                                    SetComparisonResult compResult = wallFace.Intersect(line, out resultArray);
                                    //if intersects with walls the level of visibility will have negative values
                                    if (compResult==SetComparisonResult.Overlap && null!=resultArray)
                                    {
                                        if (dblValue > 0)
                                        {
                                            dblValue = 0 - resultArray.Size;
                                        }
                                        else
                                        {
                                            dblValue = dblValue - resultArray.Size;
                                        }
                                    }
                                }
                                
                                doubleList.Add(dblValue);
                                valList.Add(new ValueAtPoint(doubleList));
                                doubleList.Clear();
                            }
                            progressBar.PerformStep();
                        }
                        
                        FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPoints);
                        FieldValues values = new FieldValues(valList);

                        AnalysisResultSchema resultSchema = new AnalysisResultSchema(settings.LegendTitle, settings.LegendDescription);
                        resultSchema.SetUnits(unitNames, multipliers);
                        if (unitNames.Contains(settings.Units)) { resultSchema.CurrentUnits = unitNames.IndexOf(settings.Units); }

                        if (overwriteResult) { sfm.SetResultSchema(resultIndex, resultSchema); }
                        else if (firstLoop) { resultIndex = sfm.RegisterResult(resultSchema); firstLoop = false; }
                        else { sfm.SetResultSchema(resultIndex, resultSchema); }

                        sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to measure distance from the point element to the points on floor face.\n" + ex.Message, "FieldOfViewAnalysis:MeasureDistanceOnFloor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }
        
        //Display on the Room
        private bool MeasureDistanceOnRoom()
        {
            bool result = false;
            try
            {
                //find walls located inside the selected rooms
                FindWallFacesOnRoom();

                if (null != sphere)
                {
                    LocationPoint location = sphere.Location as LocationPoint;
                    XYZ pointXYZ = location.Point;
                    XYZ vectorZ = new XYZ(0, 0, 1);
#if RELEASE2013
                    Transform transform = Transform.get_Translation(vectorZ);
#else
                    Transform transform = Transform.CreateTranslation(vectorZ);
#endif

                    int resultIndex = FindIndexOfResult(sfm);
                    bool firstLoop = true;
                    foreach (int roomId in roomFaces.Keys)
                    {
                        Face roomFace=roomFaces[roomId];
                        int index = sfm.AddSpatialFieldPrimitive(roomFace, transform);
                        List<double> doubleList = new List<double>();
                        IList<UV> uvPoints = new List<UV>();
                        IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                        BoundingBoxUV boundingBox = roomFace.GetBoundingBox();
                        for (double u = boundingBox.Min.U; u < boundingBox.Max.U; u = u + (boundingBox.Max.U - boundingBox.Min.U) / 15)
                        {
                            for (double v = boundingBox.Min.V; v < boundingBox.Max.V; v = v + (boundingBox.Max.V - boundingBox.Min.V) / 15)
                            {
                                UV uvPoint = new UV(u, v);
                                XYZ xyzPoint = roomFace.Evaluate(uvPoint);
                                uvPoints.Add(uvPoint);
#if RELEASE2013
                                Line line = doc.Application.Create.NewLine(pointXYZ, xyzPoint, true);
#else
                                Line line = Line.CreateBound(pointXYZ, xyzPoint);
#endif

                                //double dblValue = 1 / (line.ApproximateLength);
                                double dblValue = 1;

                                IntersectionResultArray resultArray;
                                foreach (int wallId in wallFaces.Keys)
                                {
                                    Face wallFace = wallFaces[wallId];
                                    SetComparisonResult compResult = wallFace.Intersect(line, out resultArray);
                                    if (compResult == SetComparisonResult.Overlap && null != resultArray)
                                    {
                                        if (dblValue > 0)
                                        {
                                            dblValue = 0 - resultArray.Size;
                                        }
                                        else
                                        {
                                            dblValue = dblValue - resultArray.Size;
                                        }
                                    }
                                }
                                doubleList.Add(dblValue);
                                valList.Add(new ValueAtPoint(doubleList));
                                doubleList.Clear();
                            }
                        }

                        FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPoints);
                        FieldValues values = new FieldValues(valList);

                        AnalysisResultSchema resultSchema = new AnalysisResultSchema(settings.LegendTitle, settings.LegendDescription);
                        resultSchema.SetUnits(unitNames, multipliers);
                        if (unitNames.Contains(settings.Units)) { resultSchema.CurrentUnits = unitNames.IndexOf(settings.Units); }

                        if (overwriteResult) { sfm.SetResultSchema(resultIndex, resultSchema); }
                        else if (firstLoop) { resultIndex = sfm.RegisterResult(resultSchema); firstLoop = false; }
                        else { sfm.SetResultSchema(resultIndex, resultSchema); }

                        sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
                        progressBar.PerformStep();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to measure distance from the point element to the points on Room face.\n" + ex.Message, "FieldOfViewAnalysis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        //accumulative values on grid points
        private bool MeasureAccDistance()
        {
            bool result = false;
            try
            {
                //find walls located inside the selected rooms
                FindWallFacesOnRoom();

                //Find point of view from the sphere element
                FamilyInstance sphere = FindPointOfView();
                if (null != sphere)
                {
                    LocationPoint location = sphere.Location as LocationPoint;
                    XYZ pointXYZ = location.Point;
                    XYZ vectorZ = new XYZ(0, 0, 1);
#if RELEASE2013
                    Transform transform = Transform.get_Translation(vectorZ);
#else
                    Transform transform = Transform.CreateTranslation(vectorZ);
#endif

                    int resultIndex = FindIndexOfResult(sfm);
                    bool firstLoop = true;
                    foreach (int roomId in roomFaces.Keys)
                    {
                        Face roomFace = roomFaces[roomId];
                        int index = sfm.AddSpatialFieldPrimitive(roomFace, transform);
                        List<double> doubleList = new List<double>();
                        IList<UV> uvPoints = new List<UV>();
                        IList<ValueAtPoint> valList = new List<ValueAtPoint>();
                        BoundingBoxUV boundingBox = roomFace.GetBoundingBox();
                        for (double u = boundingBox.Min.U; u < boundingBox.Max.U; u = u + (boundingBox.Max.U - boundingBox.Min.U) / 15)
                        {
                            for (double v = boundingBox.Min.V; v < boundingBox.Max.V; v = v + (boundingBox.Max.V - boundingBox.Min.V) / 15)
                            {
                                UV uvPoint = new UV(u, v);
                                XYZ xyzPoint = roomFace.Evaluate(uvPoint);
                                uvPoints.Add(uvPoint);
#if RELEASE2013
                                Line line = doc.Application.Create.NewLine(pointXYZ, xyzPoint, true);
#else
                                Line line = Line.CreateBound(pointXYZ, xyzPoint);
#endif
                                //double dblValue = 1 / (line.ApproximateLength);
                                double dblValue = 1;

                                IntersectionResultArray resultArray;
                                foreach (int wallId in wallFaces.Keys)
                                {
                                    Face wallFace = wallFaces[wallId];
                                    SetComparisonResult compResult = wallFace.Intersect(line, out resultArray);
                                    if (compResult == SetComparisonResult.Overlap && null != resultArray)
                                    {
                                        if (dblValue > 0)
                                        {
                                            dblValue = 0 - resultArray.Size;
                                        }
                                        else
                                        {
                                            dblValue = -resultArray.Size;
                                        }
                                    }
                                }
                                doubleList.Add(dblValue);
                                valList.Add(new ValueAtPoint(doubleList));
                                doubleList.Clear();
                            }
                        }

                        FieldDomainPointsByUV domainPoints = new FieldDomainPointsByUV(uvPoints);
                        FieldValues values = new FieldValues(valList);

                        AnalysisResultSchema resultSchema = new AnalysisResultSchema(settings.LegendTitle, settings.LegendDescription);
                        resultSchema.SetUnits(unitNames, multipliers);
                        if (unitNames.Contains(settings.Units)) { resultSchema.CurrentUnits = unitNames.IndexOf(settings.Units); }

                        if (overwriteResult) { sfm.SetResultSchema(resultIndex, resultSchema); }
                        else if (firstLoop) { resultIndex = sfm.RegisterResult(resultSchema); firstLoop = false; }
                        else { sfm.SetResultSchema(resultIndex, resultSchema); }

                        sfm.UpdateSpatialFieldPrimitive(index, domainPoints, values, resultIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to measure distance from the point element to the points on Room face.\n" + ex.Message, "FieldOfViewAnalysis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private void FindWallFacesOnRoom()
        {
            try
            {
                foreach (Element element in settings.SelectedElements)
                {
                    Room room = element as Room;
                    if (room.GetBoundarySegments(new SpatialElementBoundaryOptions()) != null)
                    {
                        foreach (IList<Autodesk.Revit.DB.BoundarySegment> boundarySegments in room.GetBoundarySegments(new SpatialElementBoundaryOptions()))
                        {
                            if (boundarySegments.Count > 0)
                            {
                                foreach (Autodesk.Revit.DB.BoundarySegment boundarySegment in boundarySegments)
                                {
#if RELEASE2017

                                    if (boundarySegment.ElementId == ElementId.InvalidElementId) { continue; }
                                    Wall boundaryWall = doc.GetElement(boundarySegment.ElementId) as Wall;
                                    if (boundaryWall != null)
                                    {
                                        if (!wallFaces.ContainsKey(boundaryWall.Id.IntegerValue))
                                        {
                                            Face sideFace = GetSideWallFace(boundaryWall);
                                            if (null != sideFace)
                                            {
                                                wallFaces.Add(boundaryWall.Id.IntegerValue, sideFace);
                                            }
                                        }
                                    }
#else
                                    
                                    if (null == boundarySegment.Element) { continue; }

                                    if (boundarySegment.Element.Category.Name == "Walls")
                                    {
                                        Wall boundaryWall = boundarySegment.Element as Wall;
                                        if (boundaryWall != null)
                                        {
                                            if (!wallFaces.ContainsKey(boundaryWall.Id.IntegerValue))
                                            {
                                                Face sideFace = GetSideWallFace(boundaryWall);
                                                if (null != sideFace)
                                                {
                                                    wallFaces.Add(boundaryWall.Id.IntegerValue, sideFace);
                                                }
                                            }
                                        }
                                    }
#endif
                                }
                            }
                        }
                    }
                }

                List<Wall> wallList = new List<Wall>();
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                wallList = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().OfType<Wall>().ToList();

                foreach (Wall wall in wallList)
                {
                    if (wall.WallType.Kind == WallKind.Curtain) { continue; } //regard as a transparent wall
                    if (wallFaces.ContainsKey(wall.Id.IntegerValue)) { continue; }

                    LocationCurve wallLocation = wall.Location as LocationCurve;
                    Curve wallCurve = wallLocation.Curve;
#if RELEASE2013
                    XYZ startPoint = wallCurve.get_EndPoint(0);
                    XYZ endPoint = wallCurve.get_EndPoint(1);
#else
                    XYZ startPoint = wallCurve.GetEndPoint(0);
                    XYZ endPoint = wallCurve.GetEndPoint(1);
#endif

                    foreach (Element element in settings.SelectedElements)
                    {
                        Room room = element as Room;
                        if (room.IsPointInRoom(startPoint) || room.IsPointInRoom(endPoint))
                        {
                            if (!wallFaces.ContainsKey(wall.Id.IntegerValue))
                            {
                                Face sideFace = GetSideWallFace(wall);
                                if (null != sideFace)
                                {
                                    wallFaces.Add(wall.Id.IntegerValue, sideFace);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a face of a wall object\n"+ex.Message, "FieldOfViewAnalysis:FindWallFaces", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //find walls located on the floor
        private void FindWallFacesOnFloor()
        {
            List<Wall> wallList = new List<Wall>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            wallList = collector.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().OfType<Wall>().ToList();

            foreach (Wall wall in wallList)
            {
                LocationCurve wallLocation = wall.Location as LocationCurve;
                Curve wallCurve = wallLocation.Curve;

                foreach (Element element in settings.SelectedElements)
                {
                    Floor floor = element as Floor;
#if RELEASE2013
                    if (floor.Level.Id == wall.Level.Id && !wallFaces.ContainsKey(wall.Id.IntegerValue))
                    {
                        Face sideFace = GetSideWallFace(wall);
                        if (null != sideFace)
                        {
                            wallFaces.Add(wall.Id.IntegerValue, sideFace);
                        }
                    }
#else
                     if (floor.LevelId == wall.LevelId && !wallFaces.ContainsKey(wall.Id.IntegerValue))
                    {
                        Face sideFace = GetSideWallFace(wall);
                        if (null != sideFace)
                        {
                            wallFaces.Add(wall.Id.IntegerValue, sideFace);
                        }
                    }
#endif

                }
            }
        }

        private Face GetSideWallFace(Wall wall)
        {
            Face sideFace=null;
            double area = 0;
            Options options = new Options();
            options.ComputeReferences = true;

            GeometryElement geomElem = wall.get_Geometry(options);
            if (null != geomElem)
            {
                foreach (GeometryObject geomObj in geomElem)
                {
                    Solid solid = geomObj as Solid;
                    if (solid != null)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face.Area > area)
                            {
                                sideFace = face;
                                area = face.Area;
                            }
                        }
                    }
                }
            }
            return sideFace;
        }

        private void SetCurrentStyle()
        {
            ElementId displayStyleId = AnalysisDisplayStyle.FindByName(doc, settings.DisplayStyle);
            if (null != displayStyleId)
            {
                doc.ActiveView.AnalysisDisplayStyleId = displayStyleId;
            }
        }

        private int FindIndexOfResult(SpatialFieldManager sfm)
        {
            int index = 0;
            overwriteResult = false;
            IList<int> regIndices = sfm.GetRegisteredResults();

            foreach (int i in regIndices)
            {
                AnalysisResultSchema result = sfm.GetResultSchema(i);
                if (result.Name == settings.LegendTitle)
                {
                    index = i;
                    overwriteResult = true;
                    break;
                }
            }
            return index;
        }
    }
}
