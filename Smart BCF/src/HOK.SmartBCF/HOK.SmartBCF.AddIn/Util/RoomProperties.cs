using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.AddIn.Util
{
    public class RoomProperties
    {
        private Room roomElement = null;
        private ElementId roomId = ElementId.InvalidElementId;
        private string uniqueId = "";
        private string ifcGuid = "";
        private string ifcProjectGuid = "";
        private string roomNumber = "";
        private string roomName = "";
        private Transform transformValue = Transform.Identity;
        private bool isLinked = false;

        public Room RoomElement { get { return roomElement; } set { roomElement = value; } }
        public ElementId RoomId { get { return roomId; } set { roomId = value; } }
        public string UniqueId { get { return uniqueId; } set { uniqueId = value; } }
        public string IfcGuid { get { return ifcGuid; } set { ifcGuid = value; } }
        public string IfcProjectGuid { get { return ifcProjectGuid; } set { ifcProjectGuid = value; } }
        public string RoomNumber { get { return roomNumber; } set { roomNumber = value; } }
        public string RoomName { get { return roomName; } set { roomName = value; } }
        public Transform TransformValue { get { return transformValue; } set { transformValue = value; } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; } }

        public RoomProperties(Room room, RevitLinkProperties link)
        {
            roomElement = room;
            roomId = room.Id;
            uniqueId = room.UniqueId;
            ifcGuid = room.IfcGUID();
            ifcProjectGuid = link.IfcProjectGuid;
            transformValue = link.TransformValue;
            isLinked = link.IsLinked;
            roomNumber = room.Number;
            roomName = room.Name;
           
            //roomSolid = GetRoomSolid();
        }

        private Solid GetRoomSolid()
        {
            Solid solid = null;
            try
            {
                if (null != roomElement.ClosedShell)
                {
                    GeometryElement geoElem = roomElement.ClosedShell;
                    if (null != geoElem)
                    {
                        //geoElem = geoElem.GetTransformed(Transform.Identity);
                        foreach (GeometryObject geoObj in geoElem)
                        {
                            solid = geoObj as Solid;
                            if (null != solid)
                            {
                                if (solid.Volume > 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return solid;
        }
    }
}
