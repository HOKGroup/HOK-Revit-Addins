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
    public partial class Form_AreaMass : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document doc;
        private MassCreator massCreator;
        private INIDataManager iniDataManager;
        private Form_ProgressBar progressForm;
        private List<Element> collectedAreas = new List<Element>();
        private Dictionary<int/*iniId*/, AreaProperties> areaDictionary = new Dictionary<int, AreaProperties>();
        private List<string> areaTypeNames = new List<string>();

        private List<int> placedAreas = new List<int>();
        private List<int> areaDiscrepancy = new List<int>();
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();
        private string originalDefFile = "";

        public Form_AreaMass(UIApplication uiapp, List<Element> areas)
        {
            try
            {
                m_app = uiapp;
                doc = m_app.ActiveUIDocument.Document;
                collectedAreas = areas;
                InitializeComponent();
                this.Text = "Create Extruded Mass - v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                //to restore user setting for shared parameter file
                originalDefFile = m_app.Application.SharedParametersFilename;

                iniDataManager = new INIDataManager(m_app, MassCategory.Areas);
                placedAreas = iniDataManager.PlacedAreas;
                areaDiscrepancy = iniDataManager.AreaDiscrepancy;
                defDictionary = iniDataManager.DefDictionary;

                CollectAreas();
                DisplayAreaData();

                massCreator = new MassCreator(m_app);
                massCreator.AreaDictionary = areaDictionary;
                massCreator.MassFolder = iniDataManager.MassFolder;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start running the Mass Tool.\n"+ex.Message, "Mass From Area", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectAreas()
        {
            try
            {
                progressForm = new Form_ProgressBar();
                progressForm.LabelText = "Collecting Information from the Revit Project..";
                progressForm.LabelCount = collectedAreas.Count + " areas found";
                progressForm.Show();
                progressForm.MaxValue = collectedAreas.Count;
                progressForm.Refresh();

                foreach (Area area in collectedAreas)
                {
                    progressForm.PerformStep();
                    AreaProperties ap = new AreaProperties(doc, area);
                    if (!areaDictionary.ContainsKey(ap.ID) && !ap.CurveArrArray.IsEmpty)
                    {
                        areaDictionary.Add(ap.ID, ap);
                        if (!areaTypeNames.Contains(ap.AreaType)) { areaTypeNames.Add(ap.AreaType); }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect Areas data. \n" + ex.Message, "Form_AreaMass:CollectAreas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                progressForm.Close();
            }
        }

        private void DisplayAreaData()
        {
            try
            {
                progressForm.LabelText = "Preparing Components to be Displayed..";
                progressForm.CurValue = 1;
                progressForm.Refresh();

                
                foreach (int areaId in areaDictionary.Keys)
                {
                    progressForm.PerformStep();
                    AreaProperties ap = areaDictionary[areaId];

                    int index = dataGridViewArea.Rows.Add();
                    DataGridViewRow row = dataGridViewArea.Rows[index];
                    row.Tag = ap;
                    row.Cells[0].Value = false;
                    row.Cells[1].Value = ap.Number;
                    row.Cells[2].Value = ap.Name;
                    row.Cells[3].Value = ap.Level;
                    row.Cells[4].Value = ap.DesignOption;
                    row.Cells[5].Value = ap.AreaType;
                    row.Cells[6].Value = 10; //default height=10

                    if (placedAreas.Contains(ap.ID))
                    {
                        //row.ReadOnly = true;
                        row.Cells[0].Value = false;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = ap.ID + ": Mass family is already placed in the project.";
                        }
                    }
                    if (areaDiscrepancy.Contains(ap.ID))
                    {
                        row.Cells[0].Value = true;
                        row.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.ToolTipText = ap.ID + ": Area boundary lines have been changed.";
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
                MessageBox.Show("Failed to display area data.\n" + ex.Message, "Form_AreaMass:DisplayAreaData", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                progressForm.Close();
            }
        }

        private void SaveHeightValues()
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewArea.Rows)
                {
                    if (null != row.Tag && null!=row.Cells[8].Value)
                    {
                        AreaProperties ap = row.Tag as AreaProperties;
                        ap.Height = Convert.ToDouble(row.Cells[8].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save height values.\n" + ex.Message, "Form_AreaMass:SaveHeightValues", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                List<int> checkedRows = new List<int>();
                for (int i = 0; i < dataGridViewArea.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dataGridViewArea.Rows[i].Cells[0].Value))
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
                            MessageBox.Show("Please enter a valid area height.", "Area Height Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        else if (height == 0)
                        {
                            MessageBox.Show("Please enter a valid area height.", "Area Height Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    massCreator.DefDictionary = defDictionary;

                    SaveHeightValues();
                    Dictionary<int, AreaProperties> createdAreas = new Dictionary<int, AreaProperties>();
                    Dictionary<int, MassProperties> placedMasses = new Dictionary<int, MassProperties>();

                    statusLabel.Text = "Updating Masses . . .";
                    toolStripProgressBar.Maximum = checkedRows.Count;
                    toolStripProgressBar.Visible = true;

                    StringBuilder resultMessage = new StringBuilder();

                    foreach (int index in checkedRows)
                    {
                        toolStripProgressBar.PerformStep();
                        DataGridViewRow row = dataGridViewArea.Rows[index];
                        if (null != row.Tag)
                        {
                            AreaProperties ap = row.Tag as AreaProperties;
                            MassProperties mp = new MassProperties();
                            mp.HostElementId = ap.ID;
                            if (placedAreas.Contains(ap.ID))
                            {
                                FamilyInstance instance = MassUtils.FindMassById(doc, ap.ID);
                                if (null != instance)
                                {
                                    mp.MassFamilyInstance = instance;
                                   
                                    if (!placedMasses.ContainsKey(ap.ID)) { placedMasses.Add(ap.ID, mp); }
                                    resultMessage.AppendLine(ap.ID + "\t" + ap.Number + "\t" + ap.Name);
                                }
                                continue;
                            }

                            if (areaDiscrepancy.Contains(ap.ID))
                            {
                                FamilyInstance instance = MassUtils.FindMassById(doc, ap.ID);
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

                            FamilyInstance familyInstance = massCreator.CreateFamily(ap);
                            if (null != familyInstance)
                            {
                                mp.MassFamilyInstance = familyInstance;
                                if (!placedMasses.ContainsKey(ap.ID)) { placedMasses.Add(ap.ID, mp); }
                            }
                            createdAreas.Add(ap.ID, ap);
                            resultMessage.AppendLine(ap.ID + "\t" + ap.Number + "\t" + ap.Name);
                        }
                    }

                    foreach (int areaId in placedAreas)
                    {
                        FamilyInstance instance = MassUtils.FindMassById(doc, areaId);
                        MassProperties mp = new MassProperties();
                        if (null != instance)
                        {
                            mp.MassFamilyInstance = instance;
                            if (!placedMasses.ContainsKey(areaId)) { placedMasses.Add(areaId, mp); }
                        }
                    }

                    statusLabel.Text = "Done";
                    iniDataManager.CreatedAreas = createdAreas;
                    iniDataManager.WriteINI();

                    if (massCreator.FailureMessage.Length > 0)
                    {
                        DialogResult dr = MessageBox.Show("Errors occured while creating extrusion forms.\n" + massCreator.FailureMessage.ToString(), "Warning Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            string header = "Area Masses are sucessfully created. \n[Area ID], [Area Number], [Area Name]\n";
                            MessageBoxForm messageForm = new MessageBoxForm("Completion Messages", header + resultMessage.ToString(), "", false, false);
                            if (DialogResult.OK == messageForm.ShowDialog())
                            {
                                using (Transaction trans = new Transaction(doc))
                                {
                                    trans.Start("Set Shared parameters");
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
                    MessageBox.Show("Please select at leaset one Area item to proceed.", "Empty Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            foreach (DataGridViewRow row in dataGridViewArea.Rows)
            {
                row.Cells[0].Value = false;
            }
        }

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewArea.Rows)
            {
                row.Cells[0].Value = true;
            }
        }

        private void checkBoxHeight_CheckedChanged(object sender, EventArgs e)
        {
            double height=0;
            if (checkBoxHeight.Checked)
            {
                if (textBoxHeight.Text.Length < 1 || !double.TryParse(textBoxHeight.Text, out height))
                {
                    checkBoxHeight.Checked = false;
                    MessageBox.Show("Please enter a valid number for the default height of Areas", "Invalid Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    double.TryParse(textBoxHeight.Text, out height);
                    foreach (DataGridViewRow row in dataGridViewArea.Rows)
                    {
                        if (!row.ReadOnly)
                        {
                            row.Cells[7].Value = height;
                        }
                    }
                }
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

        private void textBoxHeight_TextChanged(object sender, EventArgs e)
        {
            checkBoxHeight.Checked = false;
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

                    foreach (DataGridViewRow row in dataGridViewArea.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            AreaProperties ap = row.Tag as AreaProperties;
                            ElementId elementId = new ElementId(ap.ID);
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
                    foreach (DataGridViewRow row in dataGridViewArea.SelectedRows)
                    {
                        if (null != row.Tag)
                        {
                            AreaProperties ap = row.Tag as AreaProperties;
                            ElementId elementId = new ElementId(ap.ID);
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
                    MessageBox.Show("Failed to view elements.\n" + ex.Message, "Form_AreaMass: menuViewElement_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        private void menuCheckElement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewArea.SelectedRows)
            {
                row.Cells[0].Value = true;
            }
        }

        private void menuUncheckElement_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewArea.SelectedRows)
            {
                row.Cells[0].Value = false;
            }
        }

        private void bttnParameter_Click(object sender, EventArgs e)
        {
            Form_Parameters formParameters = new Form_Parameters(m_app, MassCategory.Areas);
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
