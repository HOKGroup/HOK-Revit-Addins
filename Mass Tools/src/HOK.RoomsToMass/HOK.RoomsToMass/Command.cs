using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using HOK.RoomsToMass.ToMass;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HOK.RoomsToMass
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    class Command:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private List<Element> selectedRooms = new List<Element>();
        private List<Element> selectedAreas = new List<Element>();
        private List<Element> selectedFloors = new List<Element>();
        private bool roomSelected = false;
        private bool areaSelected = false;
        private bool floorSelected = false;
        private bool roomExist = false;
        private bool areaExist = false;
        private bool floorExist = false;
      
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                UIDocument uidoc = m_app.ActiveUIDocument;
                Selection selection = uidoc.Selection;

                foreach(ElementId elementId in selection.GetElementIds())
                {
                    Element element = m_doc.GetElement(elementId);
                    if (null != element)
                    {
                        if (element.Category.Name == "Rooms") { selectedRooms.Add(element); }
                        else if (element.Category.Name == "Areas") { selectedAreas.Add(element); }
                        else if (element.Category.Name == "Floors") { selectedFloors.Add(element); }
                    }
                }

                if (selectedRooms.Count > 0) { roomSelected = true; }
                if (selectedAreas.Count > 0) { areaSelected = true; }
                if (selectedFloors.Count > 0) { floorSelected = true; }

                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                if (collector.OfCategory(BuiltInCategory.OST_Rooms).ToElements().Count > 0) { roomExist = true; }
                collector = new FilteredElementCollector(m_doc);
                if (collector.OfCategory(BuiltInCategory.OST_Areas).ToElements().Count > 0) { areaExist = true; }
                collector = new FilteredElementCollector(m_doc);
                if (collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements().Count > 0) { floorExist = true; }

                Form_Command commandForm = new Form_Command(m_app);
                commandForm.RoomSelected = roomSelected;
                commandForm.AreaSelected = areaSelected;
                commandForm.FloorSelected = floorSelected;
                commandForm.RoomExist = roomExist;
                commandForm.AreaExist = areaExist;
                commandForm.FloorExist = floorExist;

                if (commandForm.ShowDialog() == DialogResult.OK)
                {
                    string userSelect = commandForm.UserSelect;
                    commandForm.Close();

                    switch (userSelect)
                    {
                        case "Rooms":
                            List<Element> roomList = new List<Element>();
                            FilteredElementCollector collector1 = new FilteredElementCollector(m_doc);
                            roomList = collector1.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements().ToList();
                            
                            Form_RoomMass formRoomMass = new Form_RoomMass(m_app, roomList);
                            formRoomMass.ShowDialog();
                            break;
                        case "SelectedRooms":
                            Form_RoomMass formRoomMass2 = new Form_RoomMass(m_app, selectedRooms);
                            formRoomMass2.ShowDialog();
                            break;
                        case"Areas":
                            List<Element> areaList = new List<Element>();
                            FilteredElementCollector collector2 = new FilteredElementCollector(m_doc);
                            areaList = collector2.OfCategory(BuiltInCategory.OST_Areas).WhereElementIsNotElementType().ToElements().ToList();

                            Form_AreaMass formAreaMass = new Form_AreaMass(m_app, areaList);
                            formAreaMass.ShowDialog();
                            break;
                        case "SelectedAreas":
                            Form_AreaMass formAreaMass2 = new Form_AreaMass(m_app, selectedAreas);
                            formAreaMass2.ShowDialog();
                            break;
                        case "Floors":
                            List<Element> floorList = new List<Element>();
                            FilteredElementCollector collector3 = new FilteredElementCollector(m_doc);
                            floorList = collector3.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements().ToList();

                            Form_FloorMass formFloorMass = new Form_FloorMass(m_app, floorList);
                            formFloorMass.ShowDialog();
                            break;
                        case "SelectedFloors":
                            Form_FloorMass formFloorMass2 = new Form_FloorMass(m_app, selectedFloors);
                            formFloorMass2.ShowDialog();
                            break;
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: cannot start Rooms to Mass.\n" + ex.Message, "Rooms to Mass");
                return Result.Failed;
            }
            
        }
    }
}
