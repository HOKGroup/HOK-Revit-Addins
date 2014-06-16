using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HOK.RoomsToMass.ParameterAssigner;
using System.IO;

namespace HOK.RoomsToMass.ToMass
{
    public partial class Form_FloorMass : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document doc;
        private MassCreator massCreator;
        private INIDataManager floorDataManager;
        private Form_ProgressBar progressForm;
        private List<Element> collectedFloors = new List<Element>();
        private Dictionary<int/*floorId*/, FloorProperties> floorDictionary = new Dictionary<int, FloorProperties>();
        private List<string> levelNames = new List<string>();

        private List<int> placedFloors = new List<int>();
        private List<int> floorDiscrepancy = new List<int>();
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();
        private string originalDefFile = "";

        public Form_FloorMass(UIApplication uiapp, List<Element> floors)
        {
            try
            {
                m_app = uiapp;
                doc = m_app.ActiveUIDocument.Document;
                collectedFloors = floors;
                InitializeComponent();
                this.Text = "Create Extruded Mass - v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //to restore user setting for shared parameter file
                originalDefFile = m_app.Application.SharedParametersFilename;

                floorDataManager = new INIDataManager(m_app, MassCategory.Floors);
                placedFloors = floorDataManager.PlacedFloors;
                floorDiscrepancy = floorDataManager.FloorDiscrepancy;
                defDictionary = floorDataManager.DefDictionary;
               
                CollectFloors();
                DisplayFloorData();

                massCreator = new MassCreator(m_app);
                massCreator.MassFolder = floorDataManager.MassFolder;
                massCreator.FloorDictionary = floorDictionary;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start running the Mass Tool.\n" + ex.Message, "Mass From Area", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectFloors()
        {
            try
            {
                progressForm = new Form_ProgressBar();
                progressForm.LabelText = "Collecting Information from the Revit Project..";
                progressForm.LabelCount = collectedFloors.Count + " floors found";
                progressForm.Show();
                progressForm.MaxValue = collectedFloors.Count;
                progressForm.Refresh();

                foreach (Floor floor in collectedFloors)
                {
                    progressForm.PerformStep();
                    FloorProperties fp = new FloorProperties(doc, floor);
                    if (!floorDictionary.ContainsKey(fp.ID))
                    {
                        floorDictionary.Add(fp.ID, fp);
                        if (!levelNames.Contains(fp.Level)) { levelNames.Add(fp.Level); }
                    }
                }
            }
            catch (Exception ex)
            {
                progressForm.Close();
                MessageBox.Show("Failed to collect Floors data. \n" + ex.Message, "Form_FloorMass:CollectFloors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DisplayFloorData()
        {
            try
            {
                progressForm.LabelText = "Preparing Components to be Displayed..";
                progressForm.CurValue = 1;
                progressForm.Refresh();

                foreach (int floorId in floorDictionary.Keys)
                {
                    progressForm.PerformStep();
                    FloorProperties fp = floorDictionary[floorId];

                    int index = dataGridViewFloor.Rows.Add();
                    DataGridViewRow row = dataGridViewFloor.Rows[index];
                    row.Tag = fp;
                    row.Cells[0].Value = false;
                    row.Cells[1].Value = fp.TypeName;
                    row.Cells[2].Value = fp.Level;
                    row.Cells[3].Value = fp.Phase;
                    row.Cells[4].Value = fp.DesignOption;
                    row.Cells[5].Value = fp.Comments;
                    row.Cells[6].Value = 10;//default height

                    if (placedFloors.Contains(fp.ID))
                    {
                        row.Cells[0].Value = false;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = fp.ID + ": Mass family is already placed in the project.";
                        }
                    }
                    if (floorDiscrepancy.Contains(fp.ID))
                    {
                        row.Cells[0].Value = true;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = fp.ID + ": Floor boundary lines have been changed.";
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
                progressForm.Close();
                MessageBox.Show("Failed to display floor data.\n" + ex.Message, "Form_CreateMass:DisplayFloorData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void SaveHeightValues()
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewFloor.Rows)
                {
                    if (null != row.Tag && null != row.Cells[7].Value)
                    {
                        FloorProperties fp = row.Tag as FloorProperties;
                        fp.Height = Convert.ToDouble(row.Cells[7].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save height values.\n" + ex.Message, "Form_FloorMass:SaveHeightValues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewFloor.Rows)
            {
                row.Cells[0].Value = false;
            }
        }

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewFloor.Rows)
            {
                row.Cells[0].Value = true;
            }
        }

        private void menuViewElement_Click(object sender, EventArgs e)
        {
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Show Elements");
                try
                {
                    UIDocument uidoc = m_app.ActiveUIDocument;
                    List<ElementId> elementIds = new List<ElementId>();

#if RELEASE2013||RELEASE2014
                    SelElementSet newSelection = SelElementSet.Create();

                    foreach (DataGridViewRow row in dataGridViewFloor.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            FloorProperties fp = row.Tag as FloorProperties;
                            ElementId elementId = new ElementId(fp.ID);
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
                    foreach (DataGridViewRow row in dataGridViewFloor.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            FloorProperties fp = row.Tag as FloorProperties;
                            ElementId elementId = new ElementId(fp.ID);
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
                    MessageBox.Show("Failed to view elements.\n" + ex.Message, "Form_FloorMass: menuViewElement_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void checkBoxHeight_CheckedChanged(object sender, EventArgs e)
        {
            double height = 0;
            if (checkBoxHeight.Checked)
            {
                if (textBoxHeight.Text.Length < 1 || !double.TryParse(textBoxHeight.Text, out height))
                {
                    checkBoxHeight.Checked = false;
                    MessageBox.Show("Please enter a valid number for the default height of Floor.", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    double.TryParse(textBoxHeight.Text, out height);
                    foreach (DataGridViewRow row in dataGridViewFloor.Rows)
                    {
                        if (!row.ReadOnly)
                        {
                            row.Cells[7].Value = height;
                        }
                    }
                }
            }
        }

        private void textBoxHeight_TextChanged(object sender, EventArgs e)
        {
            checkBoxHeight.Checked = false;
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

        private void bttnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                List<int> checkedRows = new List<int>();
                for (int i = 0; i < dataGridViewFloor.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dataGridViewFloor.Rows[i].Cells[0].Value))
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

                    SaveHeightValues();
                    Dictionary<int, FloorProperties> createdFloors = new Dictionary<int, FloorProperties>();
                    Dictionary<int, MassProperties> placedMasses = new Dictionary<int, MassProperties>();

                    statusLabel.Text = "Updating Masses . . .";
                    toolStripProgressBar.Maximum = dataGridViewFloor.Rows.Count;
                    toolStripProgressBar.Visible = true;
                    StringBuilder resultMessage = new StringBuilder();

                    foreach (int index in checkedRows)
                    {
                        toolStripProgressBar.PerformStep();
                        DataGridViewRow row = dataGridViewFloor.Rows[index];
                        if (null != row.Tag)
                        {
                            FloorProperties fp = row.Tag as FloorProperties;
                            MassProperties mp = new MassProperties();
                            mp.HostElementId = fp.ID;

                            if (placedFloors.Contains(fp.ID))
                            {
                                FamilyInstance instance = MassUtils.FindMassById(doc, fp.ID);
                                if (null != instance)
                                {
                                    mp.MassFamilyInstance = instance;
                                    
                                    if (!placedMasses.ContainsKey(fp.ID)) { placedMasses.Add(fp.ID, mp); }

                                    resultMessage.AppendLine(fp.ID + "\t" + fp.TypeName + "\t" + fp.Level);
                                }
                                continue;
                            }

                            if (floorDiscrepancy.Contains(fp.ID))
                            {
                                FamilyInstance instance = MassUtils.FindMassById(doc, fp.ID);
                                if (null != instance)
                                {
                                    using (Transaction trans = new Transaction(doc))
                                    {
                                        trans.Start("Delete Eelemnt");
                                        doc.Delete(instance.Id); 
                                        trans.Commit();
                                    }
                                }
                            }

                            FamilyInstance familyInstance = massCreator.CreateFamily(fp);
                            if (null != familyInstance)
                            {
                                mp.MassFamilyInstance = familyInstance;
                                if (!placedMasses.ContainsKey(fp.ID)) { placedMasses.Add(fp.ID, mp); }
                            }

                            createdFloors.Add(fp.ID, fp);
                            resultMessage.AppendLine(fp.ID + "\t" + fp.TypeName + "\t" + fp.Level);
                        }
                    }

                    foreach (int floorId in placedFloors)
                    {
                        FamilyInstance instance = MassUtils.FindMassById(doc, floorId);
                        MassProperties mp = new MassProperties();
                        if (null != instance)
                        {
                            mp.MassFamilyInstance = instance;
                            if (!placedMasses.ContainsKey(floorId)) { placedMasses.Add(floorId, mp); }
                        }
                    }

                    statusLabel.Text = "Done";
                    floorDataManager.CreatedFloors = createdFloors;
                    floorDataManager.WriteINI();

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
                            string header = "Floor Masses are sucessfully created. \n[Floor ID], [Floor Type], [Floor Level]\n ";
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
                    MessageBox.Show("Please select at leaset one Floor item to proceed.", "Empty Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create mass floors.\n" + ex.Message, "Form_CreateMass:BttnCreate_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void menuUncheckElement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewFloor.SelectedRows)
            {
                if (!row.ReadOnly)
                {
                    row.Cells[0].Value = false ;
                }
            }
        }

        private void menuCheckElement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewFloor.SelectedRows)
            {
                if (!row.ReadOnly)
                {
                    row.Cells[0].Value = true;
                }
            }
        }

        private void menuViewElement_Click_1(object sender, EventArgs e)
        {
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Show Elements");
                try
                {
                    UIDocument uidoc = m_app.ActiveUIDocument;
                    List<ElementId> elementIds = new List<ElementId>();
#if RELEASE2013||RELEASE2014
                    SelElementSet newSelection = SelElementSet.Create();

                    foreach (DataGridViewRow row in dataGridViewFloor.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            FloorProperties fp = row.Tag as FloorProperties;
                            ElementId elementId = new ElementId(fp.ID);
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
                    foreach (DataGridViewRow row in dataGridViewFloor.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            FloorProperties fp = row.Tag as FloorProperties;
                            ElementId elementId = new ElementId(fp.ID);
                            if (elementId != ElementId.InvalidElementId)
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
                    MessageBox.Show("Failed to view elements.\n" + ex.Message, "Form_FloorMass: menuViewElement_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void bttnParameter_Click(object sender, EventArgs e)
        {
            Form_Parameters formParameters = new Form_Parameters(m_app, MassCategory.Floors);
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

    }
}
