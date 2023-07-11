using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;
using static HOK.Core.Utilities.ElementIdExtension;


namespace HOK.DoorRoom
{
    public enum DoorLinkType
    {
        FindFromLink,
        CopyFromHost,
        None
    }
    public class DoorLinkManager
    {
        private UIApplication m_app;
        private Document m_doc;
        private DoorLinkType doorLinkType=DoorLinkType.None;
        
        private const string  toRoomNumber="ToRoomNumber";
        private const string toRoomName="ToRoomName";
        private const string fromRoomNumber="FromRoomNumber";
        private const string fromRoomName = "FromRoomName";

        private Dictionary<long, DoorProperties> doorDictionary = new Dictionary<long, DoorProperties>();
        
        public DoorLinkManager(UIApplication uiapp, DoorLinkType linkType)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            doorLinkType = linkType;
            CollectDoors();
        }

        private void CollectDoors()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<FamilyInstance>  doorInstances = collector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType().ToElements().Cast<FamilyInstance>().ToList();

                if (doorInstances.Count > 0)
                {
                    bool result = false;
                    switch (doorLinkType)
                    {
                        case DoorLinkType.CopyFromHost:
                            result = CopyRoomData(doorInstances);
                            if (result)
                            {
                                MessageBox.Show("System room data in all door elements are successfully copied in shared parameters.", "Completion Message - Door Link ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            break;
                        case DoorLinkType.FindFromLink:
                            result = CopyLinkedRoomData(doorInstances);
                            if (result)
                            {
                                MessageBox.Show("Room data from linked model are successfully propagated to shared parameters in door elements.", "Completion Message - Door Link", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Door elements don't exist in the current Revit model.", "Doors Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect door elements.\n" + ex.Message, "Collect Doors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CopyRoomData(List<FamilyInstance> doorInstances)
        {
            bool result = true;
            try
            {
                StringBuilder strBuilder = new StringBuilder();

                using (TransactionGroup tg = new TransactionGroup(m_doc, "Copy Room Data"))
                {
                    tg.Start();
                    try
                    {
                        foreach (FamilyInstance door in doorInstances)
                        {
                            using (Transaction trans = new Transaction(m_doc, "Copy Room"))
                            {
                                trans.Start();
                                try
                                {
                                    DoorProperties dp = AssignToFromRoom(door);
                                    if (null != dp.ToRoom)
                                    {
                                        string roomNumber = GetRoomNumber(dp.ToRoom);
                                        string roomName = GetRoomName(dp.ToRoom);
                                        Parameter toParam = door.LookupParameter(toRoomNumber);
                                        if (null != toParam)
                                        {
                                            toParam.Set(roomNumber);
                                        }
                                        toParam = door.LookupParameter(toRoomName);
                                        if (null != toParam)
                                        {
                                            toParam.Set(roomName);
                                        }
                                    }
                                    if (null != dp.FromRoom)
                                    {
                                        string roomNumber = GetRoomNumber(dp.FromRoom);
                                        string roomName = GetRoomName(dp.FromRoom);
                                        Parameter fromParam = door.LookupParameter(fromRoomNumber);
                                        if (null != fromParam)
                                        {
                                            fromParam.Set(roomNumber);
                                        }
                                        fromParam = door.LookupParameter(fromRoomName);
                                        if (null != fromParam)
                                        {
                                            fromParam.Set(roomName);
                                        }
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    result = false;
                                    strBuilder.AppendLine(door.Id + "\t" + door.Name + ": " + ex.Message);
                                    trans.RollBack();
                                }
                            }

                        }
                        tg.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                        tg.RollBack();
                    }
                }

                if (strBuilder.Length > 0)
                {
                    MessageBox.Show("The following doors have been skipped due to some issues.\n\n" + strBuilder.ToString(), "Skipped Door Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy system room data to shared parameters.\n"+ex.Message, "Copy Room Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private string GetRoomNumber(Room room)
        {
            string roomNumber = "";
            try
            {
                Parameter parameter = room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
                if (null != parameter)
                {
                    roomNumber = parameter.AsString();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return roomNumber;
        }

        private string GetRoomName(Room room)
        {
            string roomName = "";
            try
            {
                Parameter parameter = room.get_Parameter(BuiltInParameter.ROOM_NAME);
                if (null != parameter)
                {
                    roomName = parameter.AsString();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return roomName;
        }

        //sometimes Revit internal rooms cannot capture flipped doors correctly.
        private DoorProperties AssignToFromRoom(FamilyInstance door)
        {
            DoorProperties dp = new DoorProperties(door);
            try
            {
                if (null != dp.FromPoint && null != dp.ToPoint)
                {
                    Room roomA = door.ToRoom;
                    Room roomB = door.FromRoom;

                    if (null != roomA)
                    {
                        if (roomA.IsPointInRoom(dp.ToPoint))
                        {
                            dp.ToRoom = roomA;
                        }
                        else if (roomA.IsPointInRoom(dp.FromPoint))
                        {
                            dp.FromRoom = roomA;
                        }
                    }
                    if (null != roomB)
                    {
                        if (roomB.IsPointInRoom(dp.ToPoint))
                        {
                            dp.ToRoom = roomB;
                        }
                        else if (roomB.IsPointInRoom(dp.FromPoint))
                        {
                            dp.FromRoom = roomB;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to assign To and From values in door properties.\n"+ex.Message, "Assign To and From Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dp;
        }

        private bool CopyLinkedRoomData(List<FamilyInstance> doorInstances)
        {
            bool result = true;
            try
            {
                Dictionary<long, LinkedInstanceProperties> linkedInstanceDictionary = CollectLinkedInstances();

                StringBuilder strBuilder = new StringBuilder();
                using (TransactionGroup tg = new TransactionGroup(m_doc, "Set Door Parameters"))
                {
                    tg.Start();
                    try
                    {

                        foreach (FamilyInstance door in doorInstances)
                        {
                            using (Transaction trans = new Transaction(m_doc, "Set Parameter"))
                            {
                                trans.Start();
                                try
                                {
                                    DoorProperties dp = new DoorProperties(door);
                                    if (null != dp.FromPoint && null != dp.ToPoint)
                                    {
                                        GeometryElement geomElem = door.get_Geometry(new Options());
                                        XYZ direction = door.FacingOrientation;

                                        Dictionary<long, LinkedRoomProperties> linkedRooms = new Dictionary<long, LinkedRoomProperties>();
                                        foreach (LinkedInstanceProperties lip in linkedInstanceDictionary.Values)
                                        {
                                            GeometryElement trnasformedElem = geomElem.GetTransformed(lip.TransformValue.Inverse);
                                            BoundingBoxXYZ bb = trnasformedElem.GetBoundingBox();
                                            //extended bounding box
                                           
                                            XYZ midPt = 0.5 * (bb.Min + bb.Max);
                                            XYZ extMin = bb.Min + (bb.Min - midPt).Normalize();
                                            XYZ extMax = bb.Max + (bb.Max - midPt).Normalize();

                                            Outline outline = new Outline(extMin, extMax);

                                            BoundingBoxIntersectsFilter bbIntersectFilter = new BoundingBoxIntersectsFilter(outline);
                                            BoundingBoxIsInsideFilter bbInsideFilter = new BoundingBoxIsInsideFilter(outline);
                                            LogicalOrFilter orFilter = new LogicalOrFilter(bbIntersectFilter, bbInsideFilter);
                                            FilteredElementCollector collector = new FilteredElementCollector(lip.LinkedDocument);
                                            List<Room> roomList = collector.OfCategory(BuiltInCategory.OST_Rooms).WherePasses(orFilter).WhereElementIsNotElementType().ToElements().Cast<Room>().ToList();
                                            if (roomList.Count > 0)
                                            {
                                                foreach (Room room in roomList)
                                                {
                                                    LinkedRoomProperties lrp = new LinkedRoomProperties(room);
                                                    lrp.LinkedInstance = lip;
                                                    if (!linkedRooms.ContainsKey(lrp.RoomId))
                                                    {
                                                        linkedRooms.Add(lrp.RoomId, lrp);
                                                    }
                                                }
                                            }
                                        }

                                        LinkedRoomProperties fromRoom = null;
                                        LinkedRoomProperties toRoom = null;

                                        if (linkedRooms.Count > 0)
                                        {
                                            foreach (LinkedRoomProperties lrp in linkedRooms.Values)
                                            {
                                                XYZ tFrom = lrp.LinkedInstance.TransformValue.Inverse.OfPoint(dp.FromPoint);
                                                XYZ tTo = lrp.LinkedInstance.TransformValue.Inverse.OfPoint(dp.ToPoint);

                                                if (lrp.RoomObject.IsPointInRoom(tFrom))
                                                {
                                                    dp.FromRoom = lrp.RoomObject;
                                                    fromRoom = lrp;
                                                }
                                                if (lrp.RoomObject.IsPointInRoom(tTo))
                                                {
                                                    dp.ToRoom = lrp.RoomObject;
                                                    toRoom = lrp;
                                                }
                                            }
                                        }

                                        if (null != fromRoom)
                                        {
                                            Parameter fParam = door.LookupParameter(fromRoomNumber);
                                            if (null != fParam)
                                            {
                                                fParam.Set(fromRoom.RoomNumber);
                                            }
                                            fParam = door.LookupParameter(fromRoomName);
                                            if (null != fParam)
                                            {
                                                fParam.Set(fromRoom.RoomName);
                                            }
                                        }


                                        if (null != toRoom)
                                        {
                                            Parameter tParam = door.LookupParameter(toRoomNumber);
                                            if (null != tParam)
                                            {
                                                tParam.Set(toRoom.RoomNumber);
                                            }
                                            tParam = door.LookupParameter(toRoomName);
                                            if (null != tParam)
                                            {
                                                tParam.Set(toRoom.RoomName);
                                            }
                                        }

                                        if (!doorDictionary.ContainsKey(dp.DoorId))
                                        {
                                            doorDictionary.Add(dp.DoorId, dp);
                                        }
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    trans.RollBack();
                                    string message = ex.Message;
                                    result = false;
                                    strBuilder.AppendLine(GetElementIdValue(door.Id) + "\t" + door.Name + ": " + ex.Message);
                                }
                            }
                        }
                        tg.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        tg.RollBack();
                        string message = ex.Message;
                    }

                    if (strBuilder.Length > 0)
                    {
                        MessageBox.Show("The following doors have been skipped due to some issues.\n\n" + strBuilder.ToString(), "Skipped Door Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect door data.\n" + ex.Message, "Collect Door Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private Dictionary<long, LinkedInstanceProperties> CollectLinkedInstances()
        {
            Dictionary<long, LinkedInstanceProperties> linkedInstanceDictionary = new Dictionary<long, LinkedInstanceProperties>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                List<RevitLinkInstance> revitLinkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();

                Dictionary<int/*typeId*/, RevitLinkType> linkTypes = new Dictionary<int, RevitLinkType>();
                foreach (RevitLinkInstance instance in revitLinkInstances)
                {
                    if (null == instance.GetLinkDocument()) { continue; }
                    LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);

                    FilteredElementCollector linkedCollector = new FilteredElementCollector(lip.LinkedDocument);
                    List<Room> rooms = linkedCollector.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements().Cast<Room>().ToList();
                    if (rooms.Count > 0)
                    {
                        if (!linkedInstanceDictionary.ContainsKey(lip.InstanceId))
                        {
                            linkedInstanceDictionary.Add(lip.InstanceId, lip);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect linked instances.\n"+ex.Message, "Collect Linked Instances", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return linkedInstanceDictionary;
        }
    }

    public class DoorProperties
    {
        private FamilyInstance doorInstance = null;
        private long doorId = 0;
        private Room toRoom = null;
        private Room fromRoom = null;
        private XYZ toPoint = null;
        private XYZ fromPoint = null;

        public FamilyInstance DoorInstance { get { return doorInstance; } set { doorInstance = value; } }
        public long DoorId { get { return doorId; } set { doorId = value; } }
        public Room ToRoom { get { return toRoom; } set { toRoom = value; } }
        public Room FromRoom { get { return fromRoom; } set { fromRoom = value; } }
        public XYZ ToPoint { get { return toPoint; } set { toPoint = value; } }
        public XYZ FromPoint { get { return fromPoint; } set { fromPoint = value; } }

        public DoorProperties(FamilyInstance instance)
        {
            doorInstance = instance;
            doorId = GetElementIdValue(instance.Id);

            CreateDoorPoints();
        }

        public void CreateDoorPoints()
        {
            try
            {
                BoundingBoxXYZ bb = doorInstance.get_BoundingBox(null);
                if (null != bb)
                {
                    XYZ insertionPoint = new XYZ(0.5 * (bb.Min.X + bb.Max.X), 0.5 * (bb.Min.Y + bb.Max.Y), 0.5 * (bb.Min.Z + bb.Max.Z));
                    XYZ direction = doorInstance.FacingOrientation;
                    double offset = 3;

                    toPoint = insertionPoint + offset * direction.Normalize();
                    fromPoint = insertionPoint + offset * direction.Negate().Normalize();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }

    public class LinkedRoomProperties
    {
        private Room roomObject = null;
        private long roomId = 0;
        private string roomName = "";
        private string roomNumber = "";
        private LinkedInstanceProperties lip = null;

        public Room RoomObject { get { return roomObject; } set { roomObject = value; } }
        public long RoomId { get { return roomId; } set { roomId = value; } }
        public string RoomName { get { return roomName; } set { roomName = value; } }
        public string RoomNumber { get { return roomNumber; } set { roomNumber = value; } }
        public LinkedInstanceProperties LinkedInstance { get { return lip; } set { lip = value; } }

        public LinkedRoomProperties(Room room)
        {
            roomObject = room;
            roomId = GetElementIdValue(room.Id);
            Parameter param = room.get_Parameter(BuiltInParameter.ROOM_NAME);
            if (null != param)
            {
                roomName = param.AsString();
            }
            param = room.get_Parameter(BuiltInParameter.ROOM_NUMBER);
            if (null != param)
            {
                roomNumber = param.AsString();
            }
        }
    }

    public class LinkedInstanceProperties
    {
        private RevitLinkInstance m_instance = null;
        private long instanceId = -1;
        private Document linkedDocument = null;
        private string documentTitle = "";
        private Autodesk.Revit.DB.Transform transformValue = null;

        public RevitLinkInstance Instance { get { return m_instance; } set { m_instance = value; } }
        public long InstanceId { get { return instanceId; } set { instanceId = value; } }
        public Document LinkedDocument { get { return linkedDocument; } set { linkedDocument = value; } }
        public string DocumentTitle { get { return documentTitle; } set { documentTitle = value; } }
        public Autodesk.Revit.DB.Transform TransformValue { get { return transformValue; } set { transformValue = value; } }

        public LinkedInstanceProperties(RevitLinkInstance instance)
        {
            m_instance = instance;
            instanceId = GetElementIdValue(instance.Id);
            linkedDocument = instance.GetLinkDocument();
            documentTitle = linkedDocument.Title;
            transformValue = instance.GetTotalTransform();
        }
    }
}
