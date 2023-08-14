using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.RoomsToMass.ToMass
{
    public class RoomProperties
    {
        private Room m_room = null;
        private string roomDocument = "";
        private ElementId roomElementId = ElementId.InvalidElementId;
        private long roomId = -1;
        private string roomUniqueId = "";
        private string roomName = "";
        private string roomNumber = "";
        private string roomDepartment = "";
        private double roomHeight = 0;
        private double userHeight = 10;
        private ElementId levelId = ElementId.InvalidElementId;
        private string levelName = "";
        private string designOption = "";
        private ElementId phaseId = ElementId.InvalidElementId;
        private string phaseName = "";
        private Solid roomSolid = null;
        private XYZ roomSolidCentroid = null;
        private Face bottomFace = null;
        private IList<CurveLoop> roomProfiles = new List<CurveLoop>();
        private Transform roomTransform = Autodesk.Revit.DB.Transform.Identity;

        private MassProperties linked3dMass = null;
        private MassProperties linked2dMass = null;
        private bool linked3d = false;
        private bool linked2d = false;
        private bool modifiedHost = false;

        //for datagrid
        private bool isSelected = false;
        private string tooltip = "The linked mass does not exist.";

        public Room RoomElement { get { return m_room; } set { m_room = value; } }
        public string RoomDocument { get { return roomDocument; } set { roomDocument = value; } }
        public ElementId RoomElementId { get { return roomElementId; } set { roomElementId = value; } }
        public long RoomId { get { return roomId; } set { roomId = value; } }
        public string RoomUniqueId { get { return roomUniqueId; } set { roomUniqueId = value; } }
        public string RoomName { get { return roomName; } set { roomName = value; } }
        public string RoomNumber { get { return roomNumber; } set { roomNumber = value; } }
        public string RoomDepartment { get { return roomDepartment; } set { roomDepartment = value; } }
        public double RoomHeight { get { return roomHeight; } set { roomHeight = value; } }
        public double UserHeight { get { return userHeight; } set { userHeight = value; } }
        public ElementId LevelId { get { return levelId; } set { levelId = value; } }
        public string LevelName { get { return levelName; } set { levelName = value; } }
        public string RoomDesignOption { get { return designOption; } set { designOption = value; } }
        public ElementId PhaseId { get { return phaseId; } set { phaseId = value; } }
        public string PhaseName { get { return phaseName; } set { phaseName = value; } }
        public Solid RoomSolid { get { return roomSolid; } set { roomSolid = value; } }
        public XYZ RoomSolidCentroid { get { return roomSolidCentroid; } set { roomSolidCentroid = value; } }
        public Face BottomFace { get { return bottomFace; } set { bottomFace = value; } }
        public IList<CurveLoop> RoomProfiles { get { return roomProfiles; } set { roomProfiles = value; } }
        public Transform RoomTransform {  get { return roomTransform; } set {  roomTransform = value; } }

        public MassProperties Linked3dMass { get { return linked3dMass; } set { linked3dMass = value; } }
        public MassProperties Linked2dMass { get { return linked2dMass; } set { linked2dMass = value; } }
        public bool Linked3d { get { return linked3d; } set { linked3d = value; } }
        public bool Linked2d { get { return linked2d; } set { linked2d = value; } }
        public bool ModifiedHost { get { return modifiedHost; } set { modifiedHost = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }
        public string ToolTip { get { return tooltip; } set { tooltip = value; } }

        public RoomProperties(Room room)
        {
            m_room = room;
            roomDocument = m_room.Document.Title;

            GetRoomInfo();
        }

        public RoomProperties(RoomProperties rp)
        {
            this.RoomElement = rp.RoomElement;
            this.RoomDocument = rp.RoomDocument;
            this.RoomId = rp.RoomId;
            this.RoomUniqueId = rp.RoomUniqueId;
            this.RoomElementId = rp.RoomElementId;
            this.RoomName = rp.RoomName;
            this.RoomNumber = rp.RoomNumber;
            this.RoomDepartment = rp.RoomDepartment;
            this.RoomHeight = rp.RoomHeight;
            this.UserHeight = rp.UserHeight;
            this.LevelId = rp.LevelId;
            this.LevelName = rp.LevelName;
            this.RoomDesignOption = rp.RoomDesignOption;
            this.PhaseId = rp.PhaseId;
            this.PhaseName = rp.PhaseName;
            this.RoomSolid = rp.RoomSolid;
            this.RoomSolidCentroid = rp.RoomSolidCentroid;
            this.RoomProfiles = rp.RoomProfiles;
            this.RoomTransform = rp.RoomTransform;
            this.Linked3dMass = rp.Linked3dMass;
            this.Linked2dMass = rp.Linked2dMass;
            this.Linked3d = rp.Linked3d;
            this.Linked2d = rp.Linked2d;
            this.ModifiedHost = rp.ModifiedHost;
            this.IsSelected = rp.IsSelected;
            this.ToolTip = rp.ToolTip;
        }
        

        private void GetRoomInfo()
        {
            try
            {
                roomElementId = m_room.Id;
                roomId = GetElementIdValue(m_room.Id);
                roomUniqueId = m_room.UniqueId;

                Parameter nameParam = m_room.get_Parameter(BuiltInParameter.ROOM_NAME);
                if (null != nameParam)
                {
                    roomName = nameParam.AsString();
                }

                roomNumber = m_room.Number;

                Parameter departmentParam = m_room.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT);
                if (null != departmentParam)
                {
                    roomDepartment = departmentParam.AsString();
                }

                roomHeight = m_room.UnboundedHeight;
                userHeight = Math.Round(roomHeight,2);
                levelId = m_room.LevelId;
                Level level = m_room.Document.GetElement(levelId) as Level;
                if (null != level)
                {
                    levelName = level.Name;
                }

                if (null != m_room.DesignOption)
                {
                    designOption = m_room.DesignOption.Name;
                }

                Parameter phaseParam = m_room.get_Parameter(BuiltInParameter.ROOM_PHASE);
                if (null != phaseParam)
                {
                    phaseId = phaseParam.AsElementId();
                    phaseName = phaseParam.AsValueString();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        public void GetRoomGeometry(Transform linkTransform, SpatialElementGeometryCalculator calculator)
        {
            try
            {
                if (SpatialElementGeometryCalculator.CanCalculateGeometry(m_room))
                {
                    var results = calculator.CalculateSpatialElementGeometry(m_room);
                    if (null != results)
                    {
                        Solid solid = results.GetGeometry();
                        if (null != solid)
                        {
                            if (solid.Volume > 0)
                            {
                                roomSolid = SolidUtils.CreateTransformed(solid, linkTransform);
                            }
                        }
                    }
                }

                if (null != roomSolid)
                {
                    roomSolidCentroid = roomSolid.ComputeCentroid();
                    foreach (Face face in roomSolid.Faces)
                    {
                        XYZ normal = face.ComputeNormal(new UV(0, 0));
                        if (normal.Z < 0)
                        {
                            if (Math.Abs(normal.Z) > Math.Abs(normal.X) && Math.Abs(normal.Z) > Math.Abs(normal.Y))
                            {
                                bottomFace = face; break;
                            }
                        }
                    }
                    if (null != bottomFace)
                    {
                        roomProfiles = bottomFace.GetEdgesAsCurveLoops();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }
    }

   



}
