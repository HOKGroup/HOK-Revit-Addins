using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;


namespace HOK.Utilities.DoorRoom
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

        private Dictionary<int, DoorProperties> doorDictionary = new Dictionary<int, DoorProperties>();
        private Dictionary<int, LinkedRoomProperties> linkedRoomDictionary = new Dictionary<int, LinkedRoomProperties>();

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
                            List<Room> rooms = CollectLinkedRooms();
                            result = CopyLinkedRoomData(doorInstances, rooms);
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
                using (Transaction trans = new Transaction(m_doc, "copy room data"))
                {
                    foreach (FamilyInstance door in doorInstances)
                    {
                        try
                        {
                            trans.Start();
#if RELEASE2015
                            DoorProperties dp = AssignToFromRoom(door);
                            if (null != dp.ToRoom)
                            {
                                Parameter toParam = door.LookupParameter(toRoomNumber);
                                if (null != toParam)
                                {
                                    toParam.Set(dp.ToRoom.Number);
                                }
                                toParam = door.LookupParameter(toRoomName);
                                if (null != toParam)
                                {
                                    toParam.Set(dp.ToRoom.Name);
                                }
                            }
                            if (null != dp.FromRoom)
                            {
                                Parameter fromParam = door.LookupParameter(fromRoomNumber);
                                if (null != fromParam)
                                {
                                    fromParam.Set(dp.FromRoom.Number);
                                }
                                fromParam = door.LookupParameter(fromRoomName);
                                if (null != fromParam)
                                {
                                    fromParam.Set(dp.FromRoom.Name);
                                }
                            }
#else
                            DoorProperties dp = AssignToFromRoom(door);
                            if (null != dp.ToRoom)
                            {
                                Parameter toParam = door.get_Parameter(toRoomNumber);
                                if (null != toParam)
                                {
                                    toParam.Set(dp.ToRoom.Number);
                                }
                                toParam = door.get_Parameter(toRoomName);
                                if (null != toParam)
                                {
                                    toParam.Set(dp.ToRoom.Name);
                                }
                            }
                            if (null != dp.FromRoom)
                            {
                                Parameter fromParam = door.get_Parameter(fromRoomNumber);
                                if (null != fromParam)
                                {
                                    fromParam.Set(dp.FromRoom.Number);
                                }
                                fromParam = door.get_Parameter(fromRoomName);
                                if (null != fromParam)
                                {
                                    fromParam.Set(dp.FromRoom.Name);
                                }
                            }
#endif
                            trans.Commit();
                        }
                        catch(Exception ex)
                        {
                            trans.RollBack();
                            result = false;
                            strBuilder.AppendLine(door.Id.IntegerValue+"\t"+door.Name+": "+ex.Message);
                        }
                    }
                    if (strBuilder.Length > 0)
                    {
                        MessageBox.Show("The following doors have been skipped due to some issues.\n\n" + strBuilder.ToString(), "Skipped Door Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy system room data to shared parameters.\n"+ex.Message, "Copy Room Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
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
       
        private bool CopyLinkedRoomData(List<FamilyInstance> doorInstances, List<Room> roomInstances)
        {
            bool result = true;
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                using (Transaction trans = new Transaction(m_doc, "Set Parameter"))
                {
                    foreach (FamilyInstance door in doorInstances)
                    {
                        trans.Start();
                        try
                        {
                            DoorProperties dp = new DoorProperties(door);
                            if (null != dp.FromPoint && null != dp.ToPoint)
                            {
                                //fromRoom
                                var fromRooms = from room in roomInstances where room.IsPointInRoom(dp.FromPoint) select room;
                                if (fromRooms.Count() > 0)
                                {
                                    dp.FromRoom = fromRooms.First();
                                }

                                if (null != dp.FromRoom)
                                {
#if RELEASE2015
                                    Parameter fParam = door.LookupParameter(fromRoomNumber);
                                    if (null != fParam)
                                    {
                                        fParam.Set(dp.FromRoom.Number);
                                    }
                                    fParam = door.LookupParameter(fromRoomName);
                                    if (null != fParam)
                                    {
                                        fParam.Set(dp.FromRoom.Name);
                                    }
#else
                                    Parameter fParam = door.get_Parameter(fromRoomNumber);
                                    if (null != fParam)
                                    {
                                        fParam.Set(dp.FromRoom.Number);
                                    }
                                    fParam = door.get_Parameter(fromRoomName);
                                    if (null != fParam)
                                    {
                                        fParam.Set(dp.FromRoom.Name);
                                    }
#endif
                                }

                                //toRoom
                                var toRooms = from room in roomInstances where room.IsPointInRoom(dp.ToPoint) select room;
                                if (toRooms.Count() > 0)
                                {
                                    dp.ToRoom = toRooms.First();
                                }

                                if (null != dp.ToRoom)
                                {
#if RELEASE2015
                                    Parameter tParam = door.LookupParameter(toRoomNumber);
                                    if (null != tParam)
                                    {
                                        tParam.Set(dp.ToRoom.Number);
                                    }
                                    tParam = door.LookupParameter(toRoomName);
                                    if (null != tParam)
                                    {
                                        tParam.Set(dp.ToRoom.Name);
                                    }
#else
                                     Parameter tParam = door.get_Parameter(toRoomNumber);
                                    if (null != tParam)
                                    {
                                        tParam.Set(dp.ToRoom.Number);
                                    }
                                    tParam = door.get_Parameter(toRoomName);
                                    if (null != tParam)
                                    {
                                        tParam.Set(dp.ToRoom.Name);
                                    }
#endif
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
                            result = false;
                            strBuilder.AppendLine(door.Id.IntegerValue + "\t" + door.Name + ": " + ex.Message);
                        }
                    }
                    if (strBuilder.Length > 0)
                    {
                        MessageBox.Show("The following doors have been skipped due to some issues.\n\n" + strBuilder.ToString(), "Skipped Door Elements", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect door data.\n"+ex.Message, "Collect Door Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private List<Room> CollectLinkedRooms()
        {
            List<Room> linkedRooms = new List<Room>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                collector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType();
                List<RevitLinkInstance> revitLinkInstances = collector.ToElements().Cast<RevitLinkInstance>().ToList();

                Dictionary<int/*typeId*/, RevitLinkType> linkTypes = new Dictionary<int, RevitLinkType>();
                foreach (RevitLinkInstance instance in revitLinkInstances)
                {
                    ElementId typeId = instance.GetTypeId();
                    RevitLinkType linkType = m_doc.GetElement(typeId) as RevitLinkType;
                    string linkName = linkType.Name;

                    foreach (Document document in m_app.Application.Documents)
                    {
                        if (linkName.Contains(document.Title))
                        {
                            FilteredElementCollector linkedCollector = new FilteredElementCollector(document);
                            List<Room> rooms = linkedCollector.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements().Cast<Room>().ToList();
                            linkedRooms.AddRange(rooms);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect linked rooms.\n"+ex.Message, "Collect Linked Rooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return linkedRooms;
        }
        
    }

    public class DoorProperties
    {
        private FamilyInstance doorInstance = null;
        private int doorId = 0;
        private Room toRoom = null;
        private Room fromRoom = null;
        private XYZ toPoint = null;
        private XYZ fromPoint = null;

        public FamilyInstance DoorInstance { get { return doorInstance; } set { doorInstance = value; } }
        public int DoorId { get { return doorId; } set { doorId = value; } }
        public Room ToRoom { get { return toRoom; } set { toRoom = value; } }
        public Room FromRoom { get { return fromRoom; } set { fromRoom = value; } }
        public XYZ ToPoint { get { return toPoint; } set { toPoint = value; } }
        public XYZ FromPoint { get { return fromPoint; } set { fromPoint = value; } }

        public DoorProperties(FamilyInstance instance)
        {
            doorInstance = instance;
            doorId = instance.Id.IntegerValue;
            CreateDoorPoints();
        }

        public void CreateDoorPoints()
        {
            try
            {
                LocationPoint location = doorInstance.Location as LocationPoint;
                if (null != location)
                {
                    XYZ insertionPoint = location.Point;
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
        private int roomId = 0;

        public Room RoomObject { get { return roomObject; } set { roomObject = value; } }
        public int RoomId { get { return roomId; } set { roomId = value; } }

        public LinkedRoomProperties(Room room)
        {
            roomObject = room;
            roomId = room.Id.IntegerValue;
        }
    }
}
