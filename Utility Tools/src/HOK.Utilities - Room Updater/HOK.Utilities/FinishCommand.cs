using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HOK.Utilities.FinishCreator;
using System.Windows.Forms;

namespace HOK.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class FinishCommand:IExternalCommand
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
                DialogResult dr = MessageBox.Show("Start selecting rooms to create floors and click Finish on the options bar. The windowed area will filter out Room category only.", "Select Rooms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dr == DialogResult.OK)
                {
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
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot start creating floors.\n" + ex.Message, "Create Floors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Result.Failed;
            }
        }
    }
}
