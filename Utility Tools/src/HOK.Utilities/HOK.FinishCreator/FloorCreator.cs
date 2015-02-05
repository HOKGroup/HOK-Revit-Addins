using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace HOK.FinishCreator
{
    public class FloorCreator
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private List<Element> selectedRooms = new List<Element>();
        private List<Floor> createdFloors = new List<Floor>();

        public List<Floor> CreatedFloors { get { return createdFloors; } set { createdFloors = value; } }

        public FloorCreator(UIApplication application, List<Element> selectedElements)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            selectedRooms = selectedElements;
        }

        public void CreateFloor()
        {
            try
            {
                foreach (Element element in selectedRooms)
                {
                    Room room = element as Room;
                    EdgeArrayArray edgeArrayArray = GetRoomBoundaries(room);
                    List<CurveArray> curveArrayList = CreateProfiles(edgeArrayArray);
                    FloorType floorType = FindFloorType(room);
                    Floor newFloor = null;

                    Transaction trans = new Transaction(m_doc);
                    trans.Start("Create Floor");

#if RELEASE2013
                    if (null != floorType && null != room.Level)
                    {
                        newFloor = m_doc.Create.NewFloor(curveArrayList[0], floorType, room.Level, false);
                    }
#else
                    if (null != floorType && ElementId.InvalidElementId != room.LevelId)
                    {
                        Level roomLevel = m_doc.GetElement(room.LevelId) as Level;
                        newFloor = m_doc.Create.NewFloor(curveArrayList[0], floorType, roomLevel, false);
                    }
#endif
                    else
                    {
                        newFloor = m_doc.Create.NewFloor(curveArrayList[0], false);
                    }
                    trans.Commit();

                    trans = new Transaction(m_doc);
                    trans.Start("Create Openings");
                    if (null != newFloor && curveArrayList.Count > 1)
                    {
                        for (int i = 1; i < curveArrayList.Count; i++)
                        {
                            Opening opening = m_doc.Create.NewOpening(newFloor, curveArrayList[i], false);
                        }
                    }
                    trans.Commit();

                    trans = new Transaction(m_doc);
                    trans.Start("Move Floors");
                    if (null != newFloor)
                    {
                        Location location = newFloor.Location;
                        double thickness = GetFloorThickness(newFloor);
                        XYZ translationVec = new XYZ(0, 0, thickness);
                        bool moved = location.Move(translationVec);
                    }
                    trans.Commit();

                    if (null != newFloor)
                    {
                        createdFloors.Add(newFloor);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create floors from the selected room.\n" + ex.Message, "Create Floor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private EdgeArrayArray GetRoomBoundaries(Room room)
        {
            EdgeArrayArray edgeArrayArray = new EdgeArrayArray();
            try
            {
                GeometryElement geomElem = room.ClosedShell;
                if (geomElem != null)
                {
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        Solid solid = geomObj as Solid;
                        if (solid != null)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                UV uv = new UV(0, 0);
                                XYZ normal = face.ComputeNormal(uv);
                                if (normal.IsAlmostEqualTo(new XYZ(0, 0, -1))) //bottom face
                                {
                                    edgeArrayArray = face.EdgeLoops;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot get room boundaries.\n" + ex.Message, "Get Room Boundaries", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return edgeArrayArray;
        }

        private List<CurveArray> CreateProfiles(EdgeArrayArray edgeArrayArray)
        {
            List<CurveArray> curveArrayList = new List<CurveArray>();
            try
            {
                double maxCircum = 0;

                foreach (EdgeArray edgeArray in edgeArrayArray)
                {
                    CurveArray curveArray = new CurveArray();
                    Curve curve = null;
                    List<XYZ> pointList = new List<XYZ>();
                    bool first = true;
                    double circumference = 0;

                    foreach (Edge edge in edgeArray)
                    {
                        circumference += edge.ApproximateLength;
                        int pointCount = edge.Tessellate().Count;
                        if (pointCount > 2)//edge from a circular face 
                        {
                            IList<XYZ> tPoints = edge.Tessellate();
                            tPoints.RemoveAt(tPoints.Count - 1);
                            foreach (XYZ point in tPoints)
                            {
                                pointList.Add(point);
                            }
                        }
                        else if (pointCount == 2)
                        {
                            curve = edge.AsCurve();
#if RELEASE2013
                            XYZ point = curve.get_EndPoint(0);
#else
                            XYZ point = curve.GetEndPoint(0);
#endif

                            if (first)
                            {
                                pointList.Add(point); first = false;
                            }
                            else if (pointList[pointList.Count - 1].DistanceTo(point) > 0.0026)
                            {
                                pointList.Add(point);
                            }
                        }
                    }

                    if (maxCircum == 0) { maxCircum = circumference; }
                    else if (maxCircum < circumference) { maxCircum = circumference; }

                    int num = pointList.Count;
                    if (num > 0)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            if (i == num - 1)
                            {
#if RELEASE2013
                                curve = m_doc.Application.Create.NewLineBound(pointList[i], pointList[0]);
#else
                                curve = Line.CreateBound(pointList[i], pointList[0]);
#endif
                            }
                            else
                            {
#if RELEASE2013
                                curve = m_doc.Application.Create.NewLineBound(pointList[i], pointList[i + 1]);
#else
                                curve = Line.CreateBound(pointList[i], pointList[i + 1]);
#endif
                            }
                            curveArray.Append(curve);
                        }

                        if (maxCircum == circumference) { curveArrayList.Insert(0, curveArray); }
                        else { curveArrayList.Add(curveArray); }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create profiles for floors.\n" + ex.Message, "Create Profiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return curveArrayList;
        }


        private FloorType FindFloorType(Room room)
        {
            FloorType floorType = null;
            try
            {
                string typeName = room.get_Parameter(BuiltInParameter.ROOM_FINISH_FLOOR).AsString();
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                ElementClassFilter classFilter = new ElementClassFilter(typeof(FloorType));
                collector.WherePasses(classFilter);

                var query = from element in collector
                            where element.Name == typeName
                            select element;

                List<FloorType> floorTypes = query.Cast<FloorType>().ToList<FloorType>();
                if (floorTypes.Count > 0)
                {
                    floorType = floorTypes[0];
                }

                if (floorTypes.Count == 0)
                {
                    floorType = CreateFloorType(room, typeName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find the floor type.\n" + ex.Message, "Find Floor Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return floorType;
        }

        private FloorType CreateFloorType(Room room, string typeName)
        {
            FloorType newFloorType = null;
            Transaction trans = new Transaction(m_doc);
            try
            {
                trans.Start("Create a Floor Type");

                if (string.IsNullOrEmpty(typeName))
                {
                    typeName = "Floor Finish";
                    Parameter param = room.get_Parameter(BuiltInParameter.ROOM_FINISH_FLOOR);
                    param.Set(typeName);
                }

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                ElementClassFilter classFilter = new ElementClassFilter(typeof(FloorType));
                collector.WherePasses(classFilter);

                List<FloorType> floorTypes = collector.Cast<FloorType>().ToList<FloorType>();

                var query = from floorType in floorTypes
                            where floorType.Name == typeName
                            select floorType;

                List<FloorType> existingFloorTypes = query.Cast<FloorType>().ToList();
                if (existingFloorTypes.Count > 0)
                {
                    newFloorType = existingFloorTypes[0];
                }
                else
                {
                    if (floorTypes.Count > 0)
                    {
                        foreach (FloorType floorType in floorTypes)
                        {
                            if (floorType.IsFoundationSlab) { continue; }
                            newFloorType = floorType.Duplicate(typeName) as FloorType;
                            break;
                        }
                    }

                    if (null != newFloorType)
                    {
                        CompoundStructure compoundStructure = newFloorType.GetCompoundStructure();
                        double layerThickness = 0.020833;
                        int layerIndex = compoundStructure.GetFirstCoreLayerIndex();
                        compoundStructure.SetLayerFunction(layerIndex, MaterialFunctionAssignment.Finish1);
                        compoundStructure.SetLayerWidth(layerIndex, layerThickness);

                        for (int i = compoundStructure.LayerCount - 1; i > -1; i--)
                        {
                            if (i == layerIndex) { continue; }
                            else
                            {
                                compoundStructure.DeleteLayer(i);
                            }
                        }
                        compoundStructure.StructuralMaterialIndex = 0; //only single layer is remained
                        newFloorType.SetCompoundStructure(compoundStructure);
                    }
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create a floor type.\n" + ex.Message, "CreateFloorType", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                trans.RollBack();
            }
            return newFloorType;
        }

        private double GetFloorThickness(Floor floor)
        {
            double thickness = 0;
            try
            {
                thickness = floor.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot get floor thickness.\n" + ex.Message, "Get Floor Thickness", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return thickness;
        }
    }
}
