using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.FinishCreator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FinishCommand : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public override void Execute()
        {
            try
            {
                m_app = Context.UiApplication;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-FinishCreator", Application.VersionNumber));

                var uidoc = m_app.ActiveUIDocument;
                var title = "Finish Creator v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var mainDialog = new Autodesk.Revit.UI.TaskDialog(title)
                {
                    MainInstruction = "Select a Finish Type",
                    MainContent =
                        "Start selecting rooms to create floors or ceilings and click Finish on the options bar.",
                    AllowCancellation = true
                };

                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Floors");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Floors from Linked Rooms");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Ceilings");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Ceilings from Linked Rooms");
                
                var tResult = mainDialog.Show();
                if (TaskDialogResult.CommandLink1 == tResult)
                {
                    //floor
                    var selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to create floors with their room boundary. Click Finish on the options bar when you're done selecting rooms.");
                    var selectedRooms = new List<Element>();
                    foreach (var reference in selectedElement)
                    {
                        var element = m_doc.GetElement(reference.ElementId);
                        if (null != element)
                        {
                            selectedRooms.Add(element);
                        }
                    }

                    if (selectedRooms.Any())
                    {
                        var floorCreator = new FloorCreator(m_app, selectedRooms);
                        floorCreator.CreateFloorFromRoom();
                    }
                }
                else if (TaskDialogResult.CommandLink2 == tResult)
                {
                    //floor from linked element
                    var selectedElement = uidoc.Selection.PickObjects(ObjectType.LinkedElement, "Select linked rooms to create floors with their room boundary. Click Finish on the options bar when you're done selecting rooms.");
                    var selectedRooms = new List<LinkedRoomProperties>();
                    foreach (var reference in selectedElement)
                    {
                        var linkInstance = m_doc.GetElement(reference.ElementId) as RevitLinkInstance; // Link Instance
                        if (null != linkInstance)
                        {
                            var linkedRoom = new LinkedRoomProperties(linkInstance, reference.LinkedElementId);
                            if (null != linkedRoom.LinkedRoom)
                            {
                                selectedRooms.Add(linkedRoom);
                            }
                        }
                    }

                    if (selectedRooms.Count > 0)
                    {
                        var floorCreator = new FloorCreator(m_app, selectedRooms);
                        floorCreator.CreateFloorFromLink();
                    }
                }
                else if (TaskDialogResult.CommandLink3 == tResult)
                {
                    //ceiling
                    var selectedElement = uidoc.Selection.PickObjects(
                        ObjectType.Element, 
                        new RoomElementFilter(), 
                        "Select rooms to create ceilings with their room boundary. Click Finish on the options bar when you're done selecting rooms.");
                    var selectedRooms = new List<Room>();
                    foreach (var reference in selectedElement)
                    {
                        var room = m_doc.GetElement(reference.ElementId) as Room;
                        if (null != room)
                        {
                            selectedRooms.Add(room);
                        }
                    }

                    if (selectedRooms.Any())
                    {
                        var ceilingCreator = new CeilingCreator(m_app, selectedRooms);
                        if (ceilingCreator.CreateCeilingFromRoom())
                        {
                            var ceilingCount = 0;
                            foreach (var roomid in ceilingCreator.CreatedCeilings.Keys)
                            {
                                ceilingCount += ceilingCreator.CreatedCeilings[roomid].Count;
                            }

                            MessageBox.Show(ceilingCount + " ceiling finishes are created in " + ceilingCreator.CreatedCeilings.Count + " rooms.", "Ceiling Finishes Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else if (TaskDialogResult.CommandLink4 == tResult)
                {
                    //ceiling
                    var selectedElement = uidoc.Selection.PickObjects(ObjectType.LinkedElement,  "Select linked rooms to create ceilings with their room boundary. Click Finish on the options bar when you're done selecting rooms.");
                    var selectedRooms = new List<LinkedRoomProperties>();
                    foreach (var reference in selectedElement)
                    {
                        var linkInstance = m_doc.GetElement(reference.ElementId) as RevitLinkInstance; // Link Instance
                        if (null != linkInstance)
                        {
                            var linkedRoom = new LinkedRoomProperties(linkInstance, reference.LinkedElementId);
                            if (null != linkedRoom.LinkedRoom)
                            {
                                selectedRooms.Add(linkedRoom);
                            }
                        }
                    }

                    if (selectedRooms.Count > 0)
                    {
                        var ceilingCreator = new CeilingCreator(m_app, selectedRooms);
                        if (ceilingCreator.CreateCeilingFromLink())
                        {
                            var ceilingCount = 0;
                            foreach (var roomid in ceilingCreator.CreatedCeilings.Keys)
                            {
                                ceilingCount += ceilingCreator.CreatedCeilings[roomid].Count;
                            }

                            MessageBox.Show(ceilingCount + " ceiling finishes are created in " + ceilingCreator.CreatedCeilings.Count + " rooms.", "Ceiling Finishes Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                Log.AppendLog(LogMessageType.INFO, "Ended");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }

    public class LinkedRoomProperties
    {
        public RevitLinkInstance LinkInstance { get; set; }
        public Transform TransformValue { get; set; } = Transform.Identity;
        public ElementId LinkedRoomId { get; set; }
        public Room LinkedRoom { get; set; }

        public LinkedRoomProperties(RevitLinkInstance instance, ElementId linkElementId)
        {
            LinkInstance = instance;
            LinkedRoomId = linkElementId;
            if (null == LinkInstance.GetTotalTransform()) return;

            TransformValue = LinkInstance.GetTotalTransform();
            GetLinkedRoom();
        }

        private void GetLinkedRoom()
        {
            try
            {
                var linkedDoc = LinkInstance.GetLinkDocument();
                LinkedRoom = linkedDoc.GetElement(LinkedRoomId) as Room;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
    }
}
