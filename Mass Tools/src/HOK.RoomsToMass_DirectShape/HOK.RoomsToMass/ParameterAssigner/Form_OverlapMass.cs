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
using HOK.Core.Utilities;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public partial class Form_OverlapMass : System.Windows.Forms.Form
    {
        private Dictionary<int/*elementId*/, ElementProperties> intersectingElements = new Dictionary<int, ElementProperties>();
        private Dictionary<int, MassProperties> massDictionary = new Dictionary<int, MassProperties>();
        private List<string> intersectingCategories = new List<string>();
        private List<int> massIds = new List<int>();
        private double determinant = 0;
        private UIApplication m_app;
        private bool splitOption = false;
        private string[] categoryNames = new string[] { "Columns", "Conduits", "Ducts", "Floors", "Pipes","Roofs", "Structural Columns", "Structural Framing", "Walls" };
        private List<string> categoriesToSplit = new List<string>();
        private Dictionary<int/*hostId*/, Dictionary<int/*elementId*/, int/*index*/>> hostMaps = new Dictionary<int, Dictionary<int,int>>();
        private Dictionary<int/*elemntId*/, ElementProperties> unassignedElements = new Dictionary<int, ElementProperties>();

        public Dictionary<int, ElementProperties> IntersectingElements { get { return intersectingElements; } set { intersectingElements = value; } }
        public List<string> CategoriesToSplit { get { return categoriesToSplit; } set { categoriesToSplit = value; } }
        public Dictionary<int, ElementProperties> UnassignedElements { get { return unassignedElements; } set { unassignedElements = value; } }

        public Form_OverlapMass(UIApplication uiapp, bool splitOn, Dictionary<int, ElementProperties> elements, Dictionary<int, MassProperties> masses, List<int> massIdList, List<string> categoryList)
        {
            m_app = uiapp;
            splitOption = splitOn;
            intersectingElements = elements;
            massDictionary = masses;
            massIds = massIdList;
            intersectingCategories = categoryList;

            InitializeComponent();
            checkBoxHost.Checked = true;
            if (splitOn)
            {
                labelDescription.Text = "Select a mass that will cut intersecting elements.";
                splitContainer1.Panel1Collapsed = false;
                DisplayCategories(); //display categories and elements that belongs to them.
            }
            else
            {
                labelDescription.Text = "Define a mass element that will propagate parameters to the intersecting element.";
                splitContainer1.Panel1Collapsed = true;
            }
            
            DisplayIntersectingMasses();

            textBoxRatio.Text = "0.7";
            SetDeterminant(double.Parse(textBoxRatio.Text));
            CreateHostMap();
        }

        private void Form_OverlapMass_Load(object sender, EventArgs e)
        {
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(buttonDetermine, "Set the minimum ratio value that will filter out intersecting elements.");
        }

        private void DisplayCategories()
        {
            try
            {
                foreach (string category in intersectingCategories)
                {
                    if (categoryNames.Contains(category))
                    {
                        ListViewItem item = new ListViewItem(category);
                        item.Name = category;
                        item.Tag = category;
                        item.Checked = true;
                        listViewCategories.Items.Add(item);
                        categoriesToSplit.Add(category);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void DisplayIntersectingMasses()
        {
            try
            {
                if (massIds.Count > 1)
                {
                    int colWidth = (massIds.Count - 1) * 120;
                    if (splitOption)
                    {
                        colWidth += 120;
                    }
                    
                    this.Size = new Size(this.Size.Width+colWidth, this.Size.Height);
                }

                if (!splitOption)
                {
                    dataGridViewElement.Columns[0].Visible = false;
                    bttnCheckAll.Enabled = false;
                    bttnCheckNone.Enabled = false;
                }

                foreach (int massId in massIds)
                {
                    if (massDictionary.ContainsKey(massId))
                    {
                        DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                        column.Name = massId.ToString();
                        column.HeaderText = massDictionary[massId].MassName;
                        column.ToolTipText = massId.ToString();
                        column.ReadOnly = true;
                        column.Width = 110;

                        dataGridViewElement.Columns.Add(column);
                    }
                }

                DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn();
                comboColumn.Name = "ColumnSelection";
                comboColumn.HeaderText = "Selection";
                comboColumn.Width = 110;
                comboColumn.FlatStyle = FlatStyle.Flat;
                comboColumn.AutoComplete = true;
                dataGridViewElement.Columns.Add(comboColumn);

                //hidden massId column to create elementId-massId map
                DataGridViewTextBoxColumn massIdColumn = new DataGridViewTextBoxColumn();
                massIdColumn.Name = "MassId";
                massIdColumn.HeaderText = "MassId";
                massIdColumn.Visible = false;
                dataGridViewElement.Columns.Add(massIdColumn);

                int index = 0;
                foreach (int elementId in intersectingElements.Keys)
                {
                    ElementProperties ep = intersectingElements[elementId];
                    index = dataGridViewElement.Rows.Add();
                    dataGridViewElement.Rows[index].Tag = ep;
                    dataGridViewElement.Rows[index].Cells[0].Value = true;
                    dataGridViewElement.Rows[index].Cells[1].Value = ep.ElementId.ToString();
                    dataGridViewElement.Rows[index].Cells[2].Value = ep.ElementName;
                    dataGridViewElement.Rows[index].Cells[3].Value = ep.CategoryName;

                    List<string> massNames = new List<string>();
                    massNames.Add("<Unassigned>");
                    double maxVal = 0;
                    string selectedName = "";
                    int maxId = 0;
                    foreach (int massId in ep.MassContainers.Keys)
                    {
                        if (massDictionary.ContainsKey(massId))
                        {
                            string massName = massDictionary[massId].MassName;
                            if (ep.OpverappingMaps.Count > 0)
                            {
                                if (ep.OpverappingMaps.ContainsKey(massId))
                                {
                                    double ratio = ep.OpverappingMaps[massId];

                                    dataGridViewElement.Rows[index].Cells[massId.ToString()].Value = Math.Round(ratio, 2);
                                    if (ratio > maxVal)
                                    {
                                        maxVal = ratio;
                                        selectedName = massName;
                                        maxId = massId;
                                    }
                                }
                            }
                            massNames.Add(massName);
                        }
                    }

                    DataGridViewComboBoxCell comboCell = dataGridViewElement.Rows[index].Cells["ColumnSelection"] as DataGridViewComboBoxCell;
                    comboCell.DataSource = massNames;
                    if (ep.OpverappingMaps.Count > 0) { comboCell.Value = selectedName; }
                    else { comboCell.Value = massNames[0]; maxId = 0; }
                    
                    dataGridViewElement.Rows[index].Cells["MassId"].Value = maxId;
                }
            }
            catch(Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public void SetDeterminant(double determinantVal)
        {
            try
            {
                determinant=determinantVal;
                textBoxRatio.Text = determinant.ToString();
                foreach (DataGridViewRow row in dataGridViewElement.Rows)
                {
                    row.Visible = true;
                }

                foreach (DataGridViewRow row in dataGridViewElement.Rows)
                {
                    for (int i = 3; i < row.Cells.Count - 2; i++)
                    {
                        double ratio = 0;
                        if (null != row.Cells[i].Value)
                        {
                            if (double.TryParse(row.Cells[i].Value.ToString(), out ratio))
                            {
                                if (ratio > determinantVal)
                                {
                                    row.Visible = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public void SetFollowByHost(bool followHost)
        {
            checkBoxHost.Checked = followHost;
        }

        private void CreateHostMap()
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewElement.Rows)
                {
                    if (null != row.Tag)
                    {
                        ElementProperties ep = row.Tag as ElementProperties;
                        if (ep.HostElementId != 0)
                        {
                            if (hostMaps.ContainsKey(ep.HostElementId))
                            {
                                hostMaps[ep.HostElementId].Add(ep.ElementId, row.Index);
                            }
                            else
                            {
                                Dictionary<int, int> elementMap = new Dictionary<int, int>();
                                elementMap.Add(ep.ElementId, row.Index);
                                hostMaps.Add(ep.HostElementId, elementMap);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void dataGridViewElement_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                System.Windows.Forms.ComboBox combo = e.Control as System.Windows.Forms.ComboBox;
                if (null != combo)
                {
                    combo.SelectedIndexChanged -= new EventHandler(ComboBox_SelectedIndexChanged);
                    combo.SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var currentCell = dataGridViewElement.CurrentCellAddress;
                var sendingCB = sender as DataGridViewComboBoxEditingControl;
                DataGridViewTextBoxCell cell = (DataGridViewTextBoxCell)dataGridViewElement.Rows[currentCell.Y].Cells["MassId"];
                int selectedIndex = ((System.Windows.Forms.ComboBox)sender).SelectedIndex;
                
                int selectedMassId = 0;
                if (null != dataGridViewElement.Rows[currentCell.Y].Tag && selectedIndex!=-1)
                {
                    if (selectedIndex == 0) //mass name: <None>
                    {
                        cell.Value = selectedMassId;
                    }
                    else if (selectedIndex > 0)
                    {
                        ElementProperties ep = dataGridViewElement.Rows[currentCell.Y].Tag as ElementProperties;
                        selectedMassId = ep.MassContainers.Keys.ToList()[selectedIndex - 1];
                        cell.Value = selectedMassId;

                        if (checkBoxHost.Checked)
                        {
                            if (hostMaps.ContainsKey(ep.ElementId))
                            {
                                foreach (int elementId in hostMaps[ep.ElementId].Keys)
                                {
                                    int rowIndex = hostMaps[ep.ElementId][elementId];
                                    DataGridViewCell textCell = dataGridViewElement.Rows[rowIndex].Cells["MassId"];
                                    textCell.Value = selectedMassId;
                                    DataGridViewComboBoxCell comboCell = dataGridViewElement.Rows[rowIndex].Cells["ColumnSelection"] as DataGridViewComboBoxCell;
                                    comboCell.Value = massDictionary[selectedMassId].MassName;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void showElementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UIDocument uidoc = m_app.ActiveUIDocument;
                Document m_doc = uidoc.Document;
                List<ElementId> elementIds = new List<ElementId>();

#if RELEASE2013||RELEASE2014
                SelElementSet newSelection = SelElementSet.Create();

                foreach (DataGridViewRow row in dataGridViewElement.SelectedRows)
                {
                    if (null != row.Tag)
                    {
                        ElementProperties ep = row.Tag as ElementProperties;
                        ElementId elementId = new ElementId(ep.ElementId);
                        elementIds.Add(elementId);
                        newSelection.Add(ep.ElementObj);
                    }
                }
                uidoc.ShowElements(elementIds);
                uidoc.Selection.Elements = newSelection;

#else
                Selection selection = uidoc.Selection;

                foreach (DataGridViewRow row in dataGridViewElement.SelectedRows)
                {
                    if (null != row.Tag)
                    {
                        ElementProperties ep = row.Tag as ElementProperties;
                        ElementId elementId = new ElementId(ep.ElementId);
                        elementIds.Add(elementId);
                    }
                }
                uidoc.ShowElements(elementIds);
                selection.SetElementIds(elementIds);
#endif
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void dataGridViewElement_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    int rowSelected = e.RowIndex;
                    if (e.RowIndex > -1 && e.RowIndex < dataGridViewElement.Rows.Count)
                    {
                        contextMenuStripView.Enabled = true;
                        dataGridViewElement.Rows[rowSelected].Selected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void dataGridViewElement_MouseDown(object sender, MouseEventArgs e)
        {
            contextMenuStripView.Enabled = false;
        }

        private void listViewCategories_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            try
            {
                categoriesToSplit = new List<string>();
                foreach (ListViewItem item in listViewCategories.Items)
                {
                    if (null != item.Tag && item.Checked)
                    {
                        categoriesToSplit.Add(item.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void buttonDetermine_Click(object sender, EventArgs e)
        {
            try
            {
                string ratio = textBoxRatio.Text;
                double doubleRatio = 0;

                if (double.TryParse(ratio, out doubleRatio))
                {
                    if (doubleRatio > 0 && doubleRatio < 1)
                    {
                        SetDeterminant(doubleRatio);
                    }
                    else if (doubleRatio == 0)
                    {
                        SetDeterminant(doubleRatio);
                    }
                    else if (doubleRatio == 1)
                    {
                        SetDeterminant(doubleRatio);
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid determinant value for ratio.\n The value should be between 0 and 1.", "Invalid Ratio Determinant", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid determinant value for ratio.\n The value should be between 0 and 1.", "Invalid Ratio Determinant", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckValidation())
                {
                    foreach (DataGridViewRow row in dataGridViewElement.Rows)
                    {
                        if (null != row.Tag)
                        {
                            ElementProperties ep = row.Tag as ElementProperties;
                            intersectingElements.Remove(ep.ElementId);

                            bool selected = Convert.ToBoolean(row.Cells[0].Value);
                            if (selected)
                            {
                                int massId = int.Parse(row.Cells["MassId"].Value.ToString());
                                ep.SelectedMassId = massId;
                                intersectingElements.Add(ep.ElementId, ep);

                                if (massId == 0)
                                {
                                    unassignedElements.Add(ep.ElementId, ep);
                                }
                            }
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private bool CheckValidation()
        {
            bool result = true;
            try
            {
                foreach (DataGridViewRow row in dataGridViewElement.Rows)
                {
                    int massId = int.Parse(row.Cells["MassId"].Value.ToString());
                    if (massId == 0)
                    {
                        row.Cells["ColumnSelection"].Selected = true;
                        //result = false;
                    }
                }

                if (!result)
                {
                    MessageBox.Show("Please select a mass that will cut the intersecting element.", "Missing Mass Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                result = false;
            }
            return result;
        }

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewElement.Rows)
                {
                    row.Cells[0].Value = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all.\n" + ex.Message, "Form_OverlapMass:bttnCheckAll_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dataGridViewElement.Rows)
                {
                    row.Cells[0].Value = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all.\n" + ex.Message, "Form_OverlapMass:bttnCheckAll_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
