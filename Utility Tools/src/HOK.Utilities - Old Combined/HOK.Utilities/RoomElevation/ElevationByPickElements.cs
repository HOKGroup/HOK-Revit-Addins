using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.Utilities.RoomElevation
{
    public class ElevationByPickElements
    {
        private UIApplication m_app = null;
        private UIDocument uidoc = null;
        private Document m_doc = null;
        private ElevationCreatorSettings toolSettings = null;
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();
        private Dictionary<int, RoomElevationProperties> roomDictionary = new Dictionary<int, RoomElevationProperties>();

        public ElevationByPickElements(UIApplication uiapp, ElevationCreatorSettings settings, Dictionary<int,LinkedInstanceProperties> linkedInstances, Dictionary<int, RoomElevationProperties> roomProperties)
        {
            m_app = uiapp;
            uidoc = m_app.ActiveUIDocument;
            m_doc = uidoc.Document;
            toolSettings = settings;
            linkedDocuments = linkedInstances;
            roomDictionary = roomProperties;
        }

        public void StartSelection()
        {
            try
            {
                bool hostRoom = toolSettings.IsLinkedRoom ? false : true;
                bool hostWall= toolSettings.IsLInkedWall ? false: true;

                SelectRoomAndWall(hostRoom, hostWall);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select elements.\n"+ex.Message, "Elevation Creator: Start Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectRoomAndWall(bool hostRoom, bool hostWall)
        {
            try
            {
                Reference selectedRoom = null;
                Room roomElement = null;
                if (hostRoom)
                {
                    selectedRoom = uidoc.Selection.PickObject(ObjectType.Element, new RoomSelectionFilter(), "Select a room from the host model to create an elevation view.");
                    if (null != selectedRoom)
                    {
                        roomElement = m_doc.GetElement(selectedRoom.ElementId) as Room;
                    }
                }
#if RELEASE2014||RELEASE2015
                else
                {
                    selectedRoom = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Select a room from linked models to create an elevation view.");
                    if (null != selectedRoom)
                    {
                        foreach (LinkedInstanceProperties lip in linkedDocuments.Values)
                        {
                            Element element = lip.LinkedDocument.GetElement(selectedRoom.LinkedElementId);
                            if (null != element)
                            {
                                roomElement = element as Room; break;
                            }
                        }
                    }
                }
#endif

                if (null != selectedRoom)
                {

                    Reference selectedWall = null;
                    Wall wallElement = null;
                    if (hostWall)
                    {
                        selectedWall = uidoc.Selection.PickObject(ObjectType.Element, new WallSelectionFilter(), "Select a wall from the host model to rotate an elevation view perpendicular to the wall.");
                        if (null != selectedWall)
                        {
                            wallElement = m_doc.GetElement(selectedWall.ElementId) as Wall;
                        }
                    }
#if RELEASE2014||RELEASE2015
                    else
                    {
                        selectedWall = uidoc.Selection.PickObject(ObjectType.LinkedElement, "Select a wall from linked models to rotate an elevation view perpendicular to the wall.");
                        if (null != selectedWall)
                        {
                            foreach (LinkedInstanceProperties lip in linkedDocuments.Values)
                            {
                                Element element = lip.LinkedDocument.GetElement(selectedWall.LinkedElementId);
                                if (null != element)
                                {
                                    wallElement = element as Wall; break;
                                }
                            }
                        }
                    }
#endif

                    if (null != roomElement && null != wallElement)
                    {
                        int roomId = roomElement.Id.IntegerValue;
                        RoomElevationProperties rep = null;
                        if (roomDictionary.ContainsKey(roomId))
                        {
                            rep = roomDictionary[roomId];
                        }
                        else
                        {
                            rep = new RoomElevationProperties(roomElement);
                        }

                        ElevationCreator elevationCreator = new ElevationCreator(m_app, rep, wallElement, toolSettings, linkedDocuments);
                        if (elevationCreator.CheckExisting())
                        {
                            if (elevationCreator.CreateElevationByWall())
                            {
                                MessageBoxResult dr = MessageBox.Show("Would you like to continue to select a room and a wall for the elevation view?", "Select a Room and a Wall", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (dr == MessageBoxResult.Yes)
                                {
                                    SelectRoomAndWall(hostRoom, hostWall);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occured while selecting rooms and walls.\n" + ex.Message, "Select Rooms and Walls", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    public class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (null != elem.Category)
            {
                if (elem.Category.Name == "Walls")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    public class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (null != elem.Category)
            {
                if (elem.Category.Name == "Rooms")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
