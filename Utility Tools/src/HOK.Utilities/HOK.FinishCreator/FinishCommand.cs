using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.FinishCreator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class FinishCommand : IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                UIDocument uidoc = m_app.ActiveUIDocument;
                string title = "Finish Creator v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                TaskDialog mainDialog = new TaskDialog(title);
                mainDialog.MainInstruction = "Select a Finish Type";
                mainDialog.MainContent = "Start selecting rooms to create floors or ceilings and click Finish on the options bar.\n The windowed area will filter out Room category only.";
                
                mainDialog.AllowCancellation = true;
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Floors");
                mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Ceilings");
                
                TaskDialogResult tResult = mainDialog.Show();
                if (TaskDialogResult.CommandLink1 == tResult)
                {
                    //floor
                    IList<Reference> selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to create floors which follow their area boundary. Click Finish on the options bar when you're done selecting rooms.");
                    List<Element> selectedRooms = new List<Element>();
                    foreach (Reference reference in selectedElement)
                    {
                        Element element = m_doc.GetElement(reference.ElementId);
                        if (null != element)
                        {
                            selectedRooms.Add(element);
                        }
                    }

                    if (selectedRooms.Count > 0)
                    {
                        FloorCreator floorCreator = new FloorCreator(m_app, selectedRooms);
                        floorCreator.CreateFloor();
                    }
                }
                else if (TaskDialogResult.CommandLink2 == tResult)
                {
                    //ceiling
                    IList<Reference> selectedElement = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to create floors which follow their area boundary. Click Finish on the options bar when you're done selecting rooms.");
                    List<Room> selectedRooms = new List<Room>();
                    foreach (Reference reference in selectedElement)
                    {
                        Room room = m_doc.GetElement(reference.ElementId) as Room;
                        if (null != room)
                        {
                            selectedRooms.Add(room);
                        }
                    }

                    if (selectedRooms.Count > 0)
                    {
                        CeilingCreator ceilingCreator = new CeilingCreator(m_app, selectedRooms);
                        if (ceilingCreator.CreateCeiling())
                        {
                            int ceilingCount = 0;
                            foreach (int roomid in ceilingCreator.CreatedCeilings.Keys)
                            {
                                ceilingCount += ceilingCreator.CreatedCeilings[roomid].Count;
                            }

                            MessageBox.Show(ceilingCount.ToString() + " ceiling finishes are created in " + ceilingCreator.CreatedCeilings.Count + " rooms.", "Ceiling Finishes Created", MessageBoxButtons.OK, MessageBoxIcon.Information); 
                        }
                    }
                }

                

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot start creating floors.\n" + ex.Message, "Create Floors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Result.Failed;
            }
        }
    }

    public class RoomElementFilter : ISelectionFilter
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
