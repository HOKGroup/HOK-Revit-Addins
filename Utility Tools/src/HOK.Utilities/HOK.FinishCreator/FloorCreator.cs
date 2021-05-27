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
        private readonly List<Element> selectedRooms = new List<Element>();
        private List<LinkedRoomProperties> selectedLinkedRooms = new List<LinkedRoomProperties>();
        private List<Floor> createdFloors = new List<Floor>();

        public List<Floor> CreatedFloors { get { return createdFloors; } set { createdFloors = value; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="application"></param>
        /// <param name="selectedElements"></param>
        public FloorCreator(UIApplication application, List<Element> selectedElements)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            selectedRooms = selectedElements;
        }

        public FloorCreator(UIApplication application, List<LinkedRoomProperties> selectedLinkedElements)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            selectedLinkedRooms = selectedLinkedElements;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateFloorFromRoom()
        {
            using (var tg = new TransactionGroup(m_doc, "Create Floors"))
            {
                tg.Start();
                try
                {
                    foreach (var element in selectedRooms)
                    {
                        var room = element as Room;
                        var edgeArrayArray = GetRoomBoundaries(room, Transform.Identity);
                        var curveArrayList = CreateProfiles(edgeArrayArray);
                        var floorType = FindFloorType(room);
                        var newFloor = CreateNewFloor(room, curveArrayList, floorType);

                        if (null != newFloor)
                        {
                            createdFloors.Add(newFloor);
                        }
                    }

                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Cannot create floors from the selected room.\n" + ex.Message, "Create Floors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void CreateFloorFromLink()
        {
            using (var tg = new TransactionGroup(m_doc))
            {
                tg.Start("Create Floors");
                try
                {
                    foreach (var lrp in selectedLinkedRooms)
                    {
                        var room = lrp.LinkedRoom;
                        var edgeArrayArray = GetRoomBoundaries(room, lrp.TransformValue);
                        var curveArrayList = CreateProfiles(edgeArrayArray);
                        var floorType = FindFloorType(room);
                        var newFloor = CreateNewFloor(room, curveArrayList, floorType);


                        if (null != newFloor)
                        {
                            createdFloors.Add(newFloor);
                        }
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    MessageBox.Show("Cannot create floors from the selected linked room.\n" + ex.Message, "Create Floors from Linked Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private Floor CreateNewFloor(Room room, List<CurveArray> curveArrayList, FloorType floorType)
        {
            Floor newFloor = null;
            try
            {
                using (var trans = new Transaction(m_doc))
                {
                    trans.Start("Create Floor");
                    try
                    {
                        if (null != floorType && ElementId.InvalidElementId != room.LevelId)
                        {
                            var roomLevel = m_doc.GetElement(room.LevelId) as Level;
#if RELEASE2022
                            var loopProfile = new List<CurveLoop>(1);
                            List<Curve> curves = new List<Curve>();
                            foreach (Curve curve in curveArrayList[0])
                            {
                                curves.Add(curve);
                            }
                            CurveLoop curveLoop = CurveLoop.Create(curves);
                            loopProfile.Add(curveLoop);
                            newFloor = Floor.Create(m_doc, loopProfile, floorType.Id, roomLevel.Id);
#else
                            newFloor = m_doc.Create.NewFloor(curveArrayList[0], floorType, roomLevel, false);
#endif
                        }
                        else
                        {
#if RELEASE2022
                            throw new Exception("FloorTypeId or RoomLevelId was not valid");
#else
                            newFloor = m_doc.Create.NewFloor(curveArrayList[0], false);
#endif
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Cannot create floor from a room.\nRoom ElementId:" + room.Id.IntegerValue + "\n\n" + ex.Message, "Create Floor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                using (var trans = new Transaction(m_doc))
                {
                    trans.Start("Create Openings");
                    try
                    {
                        if (null != newFloor && curveArrayList.Count > 1)
                        {
                            for (var i = 1; i < curveArrayList.Count; i++)
                            {
                                var opening = m_doc.Create.NewOpening(newFloor, curveArrayList[i], false);
                            }
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Cannot create openings.\nRoom ElementId:" + room.Id.IntegerValue + "\n\n" + ex.Message, "Create Openings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                using (var trans = new Transaction(m_doc))
                {
                    trans.Start("Move Floors");
                    try
                    {
                        if (null != newFloor)
                        {
                            var location = newFloor.Location;
                            var thickness = GetFloorThickness(newFloor);
                            var translationVec = new XYZ(0, 0, thickness);
                            var moved = location.Move(translationVec);
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        MessageBox.Show("Cannot move floors.\nRoom ElementId:" + room.Id.IntegerValue + "\n\n" + ex.Message, "Move Floors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return newFloor;
        }

        private EdgeArrayArray GetRoomBoundaries(Room room, Transform transformValue)
        {
            var edgeArrayArray = new EdgeArrayArray();
            try
            {
                var geomElem = room.ClosedShell;
                if (geomElem != null)
                {
                    geomElem = geomElem.GetTransformed(transformValue);
                    foreach (var geomObj in geomElem)
                    {
                        var solid = geomObj as Solid;
                        if (solid != null)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                var uv = new UV(0, 0);
                                var normal = face.ComputeNormal(uv);
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
            var curveArrayList = new List<CurveArray>();
            try
            {
                double maxCircum = 0;

                foreach (EdgeArray edgeArray in edgeArrayArray)
                {
                    var curveArray = new CurveArray();
                    Curve curve = null;
                    var pointList = new List<XYZ>();
                    var first = true;
                    double circumference = 0;

                    foreach (Edge edge in edgeArray)
                    {
                        circumference += edge.ApproximateLength;
                        var pointCount = edge.Tessellate().Count;
                        if (pointCount > 2)//edge from a circular face 
                        {
                            var tPoints = edge.Tessellate();
                            tPoints.RemoveAt(tPoints.Count - 1);
                            foreach (var point in tPoints)
                            {
                                pointList.Add(point);
                            }
                        }
                        else if (pointCount == 2)
                        {
                            curve = edge.AsCurve();
                            var point = curve.GetEndPoint(0);

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

                    var num = pointList.Count;
                    if (num > 0)
                    {
                        for (var i = 0; i < num; i++)
                        {
                            if (i == num - 1)
                            {
                                curve = Line.CreateBound(pointList[i], pointList[0]);
                            }
                            else
                            {
                                curve = Line.CreateBound(pointList[i], pointList[i + 1]);
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
                var typeName = room.get_Parameter(BuiltInParameter.ROOM_FINISH_FLOOR).AsString();
                var collector = new FilteredElementCollector(m_doc);
                var classFilter = new ElementClassFilter(typeof(FloorType));
                collector.WherePasses(classFilter);

                var query = from element in collector
                            where element.Name == typeName
                            select element;

                var floorTypes = query.Cast<FloorType>().ToList<FloorType>();
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
            using (var trans = new Transaction(m_doc))
            {
                trans.Start("Create Floor Type");
                try
                {

                    if (string.IsNullOrEmpty(typeName))
                    {
                        typeName = "Floor Finish";
                        if (!room.Document.IsLinked)
                        {
                            var param = room.get_Parameter(BuiltInParameter.ROOM_FINISH_FLOOR);
                            param.Set(typeName);
                        }
                    }

                    var collector = new FilteredElementCollector(m_doc);
                    var classFilter = new ElementClassFilter(typeof(FloorType));
                    collector.WherePasses(classFilter);

                    var floorTypes = collector.Cast<FloorType>().ToList<FloorType>();

                    var query = from floorType in floorTypes
                                where floorType.Name == typeName
                                select floorType;

                    var existingFloorTypes = query.Cast<FloorType>().ToList();
                    if (existingFloorTypes.Count > 0)
                    {
                        newFloorType = existingFloorTypes[0];
                    }
                    else
                    {
                        if (floorTypes.Count > 0)
                        {
                            foreach (var floorType in floorTypes)
                            {
                                if (floorType.IsFoundationSlab) { continue; }
                                newFloorType = floorType.Duplicate(typeName) as FloorType;
                                break;
                            }
                        }

                        if (null != newFloorType)
                        {
                            var compoundStructure = newFloorType.GetCompoundStructure();
                            var layerThickness = 0.020833;
                            var layerIndex = compoundStructure.GetFirstCoreLayerIndex();
                            compoundStructure.SetLayerFunction(layerIndex, MaterialFunctionAssignment.Finish1);
                            compoundStructure.SetLayerWidth(layerIndex, layerThickness);

                            for (var i = compoundStructure.LayerCount - 1; i > -1; i--)
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
                    MessageBox.Show("Cannot create a floor type.\n" + ex.Message, "Create Floor Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
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
