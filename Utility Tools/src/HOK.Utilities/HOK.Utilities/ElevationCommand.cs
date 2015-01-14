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
using HOK.Utilities.RoomElevation;
using HOK.Utilities.RoomUpdater;

namespace HOK.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    
    public class ElevationCommand:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private UIDocument uidoc;
        private ViewPlan viewPlan = null;
        private ViewFamilyType viewElevationFamilyType = null;
        private Dictionary<int, LinkedInstanceProperties> linkedDocuments = new Dictionary<int, LinkedInstanceProperties>();
        private int viewScale = 0;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                uidoc = m_app.ActiveUIDocument;

                if (CheckPrerequisites())
                {
                    TaskDialog tDialog = new TaskDialog("Room Elevation Creator");
                    tDialog.MainInstruction = "Select a model source of selecting elements.";
                    tDialog.MainContent = "Room elements and wall elements can be selected either from the host model or linked models.";
                    tDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Rooms and Walls in the Host Model.");
#if RELEASE2014||RELEASE2015
                    tDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Rooms and Walls in Linked Models.");
                    tDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Rooms in Linked Models, Walls in the Host Model.");
                    tDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Rooms in the Host Model, Walls in Linked Models.");
#endif

                    tDialog.CommonButtons = TaskDialogCommonButtons.Close;
                    TaskDialogResult tResult = tDialog.Show();
                    switch (tResult)
                    {
                        case TaskDialogResult.CommandLink1:
                            //elements from the host model
                            DialogResult dr1 = MessageBox.Show("Start selecting a room and a wall element to create an elevation view.", "Select a Room and a Wall", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (dr1 == DialogResult.OK)
                            {
                                SelectRoomAndWall(true, true);
                            }
                            break;
                        case TaskDialogResult.CommandLink2:
                            //elements from linked models
                            DialogResult dr2 = MessageBox.Show("Start selecting a room and a wall element to create an elevation view.", "Select a Room and a Wall", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (dr2 == DialogResult.OK)
                            {
                                SelectRoomAndWall(false, false);
                            }
                            break;
                        case TaskDialogResult.CommandLink3:
                            //elements from linked models
                            DialogResult dr3 = MessageBox.Show("Start selecting a room and a wall element to create an elevation view.", "Select a Room and a Wall", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (dr3 == DialogResult.OK)
                            {
                                SelectRoomAndWall(false, true);
                            }
                            break;
                        case TaskDialogResult.CommandLink4:
                            //elements from linked models
                            DialogResult dr4 = MessageBox.Show("Start selecting a room and a wall element to create an elevation view.", "Select a Room and a Wall", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (dr4 == DialogResult.OK)
                            {
                                SelectRoomAndWall(true, false);
                            }
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization Problem:\n"+ex.Message, "Room Elevation Creator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Result.Succeeded;
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
                     
                    if (null!=roomElement && null!=wallElement)
                    {
                        ElevationCreator elevationCreator = new ElevationCreator(m_app, roomElement, wallElement, viewPlan, viewElevationFamilyType, viewScale);
                        elevationCreator.LinkedDocuments = linkedDocuments;
                        bool created = elevationCreator.CreateElevation();
                        if (created)
                        {
                            DialogResult dr = MessageBox.Show("Would you like to continue to select a room and a wall for the elevation view?", "Select a Room and a Wall", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dr == DialogResult.Yes)
                            {
                                SelectRoomAndWall(hostRoom, hostWall);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Errors occured while selecting rooms and walls.\n"+ex.Message, "Select Rooms and Walls", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        

        private bool CheckPrerequisites()
        {
            bool result = false;
            try
            {
                //Viewplan
                Autodesk.Revit.DB.View activeView = m_doc.ActiveView;
                viewPlan = activeView as ViewPlan;
                if (null != viewPlan)
                {
                    viewScale = viewPlan.Scale;

                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<ViewFamilyType> viewTypeList = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
                    var viewTypes = from vft in viewTypeList where vft.ViewFamily == ViewFamily.Elevation select vft;
                    if (viewTypes.Count() > 0)
                    {
                        var interiorTypes = from vft in viewTypes where vft.Name.Contains("Interior") select vft;
                        if (interiorTypes.Count() > 0)
                        {
                            viewElevationFamilyType = interiorTypes.First();
                        }
                        else
                        {
                            viewElevationFamilyType = viewTypes.First();
                        }
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("A valid view family type for Elevation has not been loaded in this project.", "View Family Type Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }
                }
                else
                {
                    MessageBox.Show("Please open a plan view that elevation markers are visible.", "Open a Plan View", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    result = false;
                }

                FilteredElementCollector linkCollector = new FilteredElementCollector(m_doc);
                List<RevitLinkInstance> linkInstances = linkCollector.OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().Cast<RevitLinkInstance>().ToList();
                foreach (RevitLinkInstance instance in linkInstances)
                {
                    LinkedInstanceProperties lip = new LinkedInstanceProperties(instance);
                    if (null != lip.LinkedDocument)
                    {
                        if (!linkedDocuments.ContainsKey(lip.InstanceId))
                        {
                            linkedDocuments.Add(lip.InstanceId, lip);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check prereauisites.\n"+ex.Message, "Check Prerequisites", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
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
