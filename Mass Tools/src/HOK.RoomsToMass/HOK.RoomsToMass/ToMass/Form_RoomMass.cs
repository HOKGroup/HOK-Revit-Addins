using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using System.IO;
using HOK.RoomsToMass.ParameterAssigner;

namespace HOK.RoomsToMass.ToMass
{
    public partial class Form_RoomMass : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document doc;
        private MassCreator massCreator;
        private INIDataManager roomDataManager;
        private Form_ProgressBar progressForm;
        private List<Element> collectedRooms = new List<Element>();
        private Dictionary<int/*roomId*/, RoomProperties> roomDictionary = new Dictionary<int, RoomProperties>();
        private List<string> departmentNames = new List<string>();
       
        private List<int> placedRooms = new List<int>();
        private List<int> roomDiscrepancy = new List<int>();
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();
        private string originalDefFile = "";
       
        public Form_RoomMass(UIApplication uiapp, List<Element> rooms)
        {
            try
            {
                m_app = uiapp;
                doc = m_app.ActiveUIDocument.Document;

                collectedRooms = rooms;
                InitializeComponent();
                this.Text = "Create Extruded Mass - v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //to restore user setting for shared parameter file
                originalDefFile = m_app.Application.SharedParametersFilename;

                roomDataManager = new INIDataManager(m_app, MassCategory.Rooms);
                placedRooms = roomDataManager.PlacedRooms;
                roomDiscrepancy = roomDataManager.RoomDiscrepancy;
                defDictionary = roomDataManager.DefDictionary;

                CollectRooms();
                DisplayRoomData();

                massCreator = new MassCreator(m_app);
                massCreator.MassFolder = roomDataManager.MassFolder;
                massCreator.RoomDictionary = roomDictionary;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start running the Mass Tool.\n"+ex.Message, "Mass From Room", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectRooms()
        {
            try
            {
                progressForm = new Form_ProgressBar();
                progressForm.LabelText = "Collecting Information from the Revit Project..";
                progressForm.LabelCount = collectedRooms.Count + " rooms found";
                progressForm.Show();
                progressForm.MaxValue = collectedRooms.Count;
                progressForm.Refresh();

                foreach (Room room in collectedRooms)
                {
                    progressForm.PerformStep();
                    RoomProperties rp = new RoomProperties(doc, room);
                    if (!roomDictionary.ContainsKey(rp.ID) && null != room.Location)
                    {
                        roomDictionary.Add(rp.ID, rp);
                        if (!departmentNames.Contains(rp.Department)) { departmentNames.Add(rp.Department); }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Rooms data. \n" + ex.Message, "Form_RoomMass:CollectRooms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                progressForm.Close();
            }
        }

        private void DisplayRoomData()
        {
            try
            {
                progressForm.LabelText = "Preparing Components to be Displayed..";
                progressForm.CurValue = 1;
                progressForm.Refresh();
               
                foreach (int roomId in roomDictionary.Keys)
                {
                    progressForm.PerformStep();
                    RoomProperties rp = roomDictionary[roomId];

                    int index = dataGridViewRoom.Rows.Add();
                    DataGridViewRow row = dataGridViewRoom.Rows[index];
                    row.Tag = rp;
                    row.Cells[0].Value = false;
                    row.Cells[1].Value = rp.Number;
                    row.Cells[2].Value = rp.Name;
                    row.Cells[3].Value = rp.Department;
                    row.Cells[4].Value = rp.Level;
                    row.Cells[5].Value = rp.Phase;
                    row.Cells[6].Value = rp.DesignOption;

                    if (placedRooms.Contains(rp.ID))
                    {
                        //row.Cells[0].ReadOnly = true;
                        row.Cells[0].Value = false;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = rp.ID + ": Mass family is already placed in the project.";
                        }
                    }
                    if (roomDiscrepancy.Contains(rp.ID))
                    {
                        row.Cells[0].Value = true;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = rp.ID + ": Room boundary lines have been changed.";
                        }
                    }
                }

                if (defDictionary.Count > 0)
                {
                    string parameters = "";
                    foreach (string defName in defDictionary.Keys)
                    {
                        parameters += "[" + defName + "]   ";
                    }
                    richTextBoxParameters.Text = parameters;
                }

                progressForm.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display room data.\n" + ex.Message, "Form_CreateMass:DisplayRoomData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                progressForm.Close();
            }
        }

        private void bttnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                List<int> checkedRows = new List<int>();
                for (int i = 0; i < dataGridViewRoom.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dataGridViewRoom.Rows[i].Cells[0].Value))
                    {
                        checkedRows.Add(i);
                    }
                }

                if (checkedRows.Count > 0)
                {
                    double height = 0;
                    if (checkBoxHeight.Checked)
                    {
                        if (!ValidateHeight(out height))
                        {
                            MessageBox.Show("Please enter a valid room height.", "Room Height Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        else if (height == 0)
                        {
                            MessageBox.Show("Please enter a valid room height.", "Room Height Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    massCreator.DefDictionary = defDictionary;

                    Dictionary<int, RoomProperties> createdRooms = new Dictionary<int, RoomProperties>(); //created rooms at this run
                    Dictionary<int, MassProperties> placedMasses = new Dictionary<int, MassProperties>();

                    statusLabel.Text = "Updating Masses . . .";
                    toolStripProgressBar.Maximum = checkedRows.Count;
                    toolStripProgressBar.Visible = true;

                    StringBuilder resultMessage = new StringBuilder();

                    foreach (int index in checkedRows)
                    {
                        toolStripProgressBar.PerformStep();
                        DataGridViewRow row = dataGridViewRoom.Rows[index];
                        if (null != row.Tag)
                        {
                            RoomProperties rp = row.Tag as RoomProperties;
                            MassProperties mp = new MassProperties();
                            mp.HostElementId = rp.ID;
                            if (placedRooms.Contains(rp.ID))
                            {
                                FamilyInstance instance = MassUtils.FindMassById(doc, rp.ID);
                                if (null != instance)
                                {
                                    mp.MassFamilyInstance = instance;
                                    if (!placedMasses.ContainsKey(rp.ID)) { placedMasses.Add(rp.ID, mp); }

                                    resultMessage.AppendLine(rp.ID + "\t" + rp.Number + "\t" + rp.Name);
                                }
                                continue;
                            }

                            if (roomDiscrepancy.Contains(rp.ID))
                            {
                                FamilyInstance instance = MassUtils.FindMassById(doc, rp.ID);
                                if (null != instance)
                                {
                                    using (Transaction trans = new Transaction(doc))
                                    {
                                        trans.Start("Delete Element");
                                        doc.Delete(instance.Id);
                                        trans.Commit();
                                    }
                                }
                            }
                            rp.IsDefaultHeight = checkBoxHeight.Checked;
                            rp.DefaultHeight = height;

                            FamilyInstance familyInstance = massCreator.CreateFamily(rp);
                            if (null != familyInstance)
                            {
                                mp.MassFamilyInstance = familyInstance;
                                if (!placedMasses.ContainsKey(rp.ID)) { placedMasses.Add(rp.ID, mp); }
                            }

                            createdRooms.Add(rp.ID, rp);
                            resultMessage.AppendLine(rp.ID + "\t" + rp.Number + "\t" + rp.Name);
                        }
                    }
                    //to include placed masses from the previous run into the snapshot.png
                    foreach (int roomId in placedRooms)
                    {
                        FamilyInstance instance = MassUtils.FindMassById(doc, roomId);
                        MassProperties mp = new MassProperties();
                        if (null != instance)
                        {
                            mp.MassFamilyInstance = instance;
                            if (!placedMasses.ContainsKey(roomId)) { placedMasses.Add(roomId, mp); }
                        }
                    }

                    statusLabel.Text = "Done";
                    roomDataManager.CreatedRooms = createdRooms;
                    roomDataManager.WriteINI();
                    if (massCreator.FailureMessage.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show("Errors occured while creating extrusion forms.\n" + massCreator.FailureMessage.ToString(), "Warning Messages", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        if (dr == DialogResult.OK)
                        {
                            using (Transaction trans = new Transaction(doc))
                            {
                                trans.Start("Set Shared Parameters");
                                m_app.Application.SharedParametersFilename = originalDefFile;
                                trans.Commit();
                            }
                            
                            this.Close();
                        }
                    }
                    else
                    {
                        if (resultMessage.Length > 0)
                        {
                            string header = "Room Masses are sucessfully updated. \n[Room ID], [Room Number], [Room Name]\n";
                            MessageBoxForm messageForm = new MessageBoxForm("Completion Messages", header + resultMessage.ToString(), "", false, false);
                            if (DialogResult.OK == messageForm.ShowDialog())
                            {
                                using (Transaction trans = new Transaction(doc))
                                {
                                    trans.Start("Set Shared Parameters");
                                    m_app.Application.SharedParametersFilename = originalDefFile;
                                    trans.Commit();
                                }
                                this.Close();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select at leaset one Room item to proceed.", "Empty Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create mass rooms.\n" + ex.Message, "Form_CreateMass:BttnCreate_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewRoom.Rows)
            {
                row.Cells[0].Value = false;
            }
        }

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewRoom.Rows)
            {
                row.Cells[0].Value = true;
            }
        }

        private void menuViewElement_Click(object sender, EventArgs e)
        {
            using (Transaction trans = new Transaction(doc))
            {
                try
                {
                    trans.Start("Show Elements");
                    UIDocument uidoc = m_app.ActiveUIDocument;
                    List<ElementId> elementIds = new List<ElementId>();

#if RELEASE2013||RELEASE2014
                    SelElementSet newSelection = SelElementSet.Create();

                    foreach (DataGridViewRow row in dataGridViewRoom.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            RoomProperties rp = row.Tag as RoomProperties;
                            ElementId elementId = new ElementId(rp.ID);
                            Element element = doc.GetElement(elementId);
                            if (null != element)
                            {
                                newSelection.Add(element);
                                elementIds.Add(elementId);
                            }
                        }
                    }
                    uidoc.ShowElements(elementIds);
                    uidoc.Selection.Elements = newSelection;
#elif RELEASE2015
                    Selection selection = uidoc.Selection;

                    foreach (DataGridViewRow row in dataGridViewRoom.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            RoomProperties rp = row.Tag as RoomProperties;
                            ElementId elementId = new ElementId(rp.ID);
                            Element element = doc.GetElement(elementId);
                            if (null != element)
                            {
                                elementIds.Add(elementId);
                            }
                        }
                    }
                    uidoc.ShowElements(elementIds);
                    selection.SetElementIds(elementIds);
#endif
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to view elements.\n" + ex.Message, "Form_RoomMass: menuViewElement_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void menuCheckElement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewRoom.SelectedRows)
            {
                row.Cells[0].Value = true;
            }
        }

        private void menuUncheckElement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewRoom.SelectedRows)
            {
                row.Cells[0].Value = false;
            }
        }

        private void bttnParameter_Click(object sender, EventArgs e)
        {
            Form_Parameters formParameters = new Form_Parameters(m_app,MassCategory.Rooms);
            formParameters.DefDictionary = defDictionary;
            formParameters.DisplayInfo();

            if (formParameters.ShowDialog() == DialogResult.OK)
            {
                defDictionary = formParameters.DefDictionary;
                formParameters.Close();
                string parameters = "";
                foreach (string defName in defDictionary.Keys)
                {
                    parameters += "[" + defName + "]   ";
                }
                richTextBoxParameters.Text = parameters;
            }
        }

        private void checkBoxHeight_CheckedChanged(object sender, EventArgs e)
        {
            double height = 0;
            if (!ValidateHeight(out height))
            {
                MessageBox.Show("Please enter a valid room height.", "Room Height Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool ValidateHeight(out double height)
        {
            bool valid = false;
            height = 0;
            if (string.IsNullOrEmpty(textBoxHeight.Text))
            {
                return false;
            }
            else if (double.TryParse(textBoxHeight.Text, out height))
            {
                return true;
            }
            return valid;
        }

    }
}
