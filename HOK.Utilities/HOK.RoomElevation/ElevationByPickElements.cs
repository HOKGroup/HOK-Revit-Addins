using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.Core.Utilities;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.RoomElevation
{
    public class ElevationByPickElements
    {
        private UIApplication m_app;
        private UIDocument uidoc;
        private Document m_doc;
        private ElevationCreatorSettings toolSettings;
        private Dictionary<long, LinkedInstanceProperties> linkedDocuments = new Dictionary<long, LinkedInstanceProperties>();
        private Dictionary<long, RoomElevationProperties> roomDictionary = new Dictionary<long, RoomElevationProperties>();

        public ElevationByPickElements(UIApplication uiapp, ElevationCreatorSettings settings, Dictionary<long,LinkedInstanceProperties> linkedInstances, Dictionary<long, RoomElevationProperties> roomProperties)
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
                XYZ pickPoint = null;
                if (hostRoom)
                {
                    selectedRoom = uidoc.Selection.PickObject(ObjectType.Element, new RoomElementFilter(), "Select a room from the host model to create an elevation view.");
                    if (null != selectedRoom)
                    {
                        roomElement = m_doc.GetElement(selectedRoom.ElementId) as Room;
                    }
                }
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

                if (null != selectedRoom)
                {
                    pickPoint = uidoc.Selection.PickPoint("Pick a point to locate the elevation mark.");
                    if (null != pickPoint)
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

                        if (null != roomElement && null != wallElement && null != pickPoint)
                        {
                            long roomId = GetElementIdValue(roomElement.Id);
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
                            elevationCreator.PickPoint = pickPoint;

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
}
