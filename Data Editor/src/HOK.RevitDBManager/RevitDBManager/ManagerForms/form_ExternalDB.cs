using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Access.Dao;
using RevitDBManager.Classes;
using Autodesk.Revit.DB;
using System.Diagnostics;

namespace RevitDBManager.ManagerForms
{
    public partial class form_ExternalDB : System.Windows.Forms.Form
    {
        private string selectedCategory = "";
        private string extDBPath = "";
        private DataGridView extDataGridView;
        private Microsoft.Office.Interop.Access.Application oAccess;
        private Database daoDB;
        private string selectedTable = "";
        private LinkedParameter linkedParameter;
        private bool isEdit = false;
        private int rowIndex = 0;
        private Dictionary<string/*category*/, Dictionary<int, ParamProperties>> typeCatParamSettings = new Dictionary<string, Dictionary<int, ParamProperties>>();
        private Dictionary<string/*tableName*/, List<string>/*fieldNames*/> fieldNames = new Dictionary<string, List<string>>();
        private ListViewItem selectedParam;
        private ListViewItem selectedField;

        #region Property
        public bool IsEdit { get { return isEdit; } set { isEdit = value; } }
        public int RowIndex { get { return rowIndex; } set { rowIndex = value; } }
        public Dictionary<string/*category*/, Dictionary<int, ParamProperties>> TypeCatParamSettings { get { return typeCatParamSettings; } set { typeCatParamSettings = value; } }
        public LinkedParameter LinkedParameter { get { return linkedParameter; } set { linkedParameter = value; } }
        #endregion

        public form_ExternalDB(string selCategory, string extDB, DataGridView dataGridView)
        {
            selectedCategory = selCategory;
            extDBPath = extDB;
            extDataGridView = dataGridView;

            oAccess = new Microsoft.Office.Interop.Access.Application();
            oAccess.OpenCurrentDatabase(extDBPath);
            daoDB = oAccess.CurrentDb();
            oAccess.Visible = false;
            InitializeComponent();
        }

        private void form_ExternalDB_Load(object sender, EventArgs e)
        {
            textBoxCategory.Text = selectedCategory;
            lblExtDB.Text = "External Database: " + extDBPath;

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(this.bttnAddControl, "Add a Controlling Parameter and a Field.");
            toolTip.SetToolTip(this.bttnAddUpdate, "Add a Updating Parameter and a Field");

            
            FillTableList();
            comboBoxTable.SelectedIndex = 0;

            if (isEdit)
            {
                textBoxParameter.Text = linkedParameter.ControlParameter;
                textBoxField.Text = linkedParameter.ControlField;
                textBoxField.Tag = linkedParameter.TableName;

                foreach (string updateParam in linkedParameter.UpdateParameterField.Keys)
                {
                    ListViewItem item = new ListViewItem(updateParam);
                    item.Name = updateParam;
                    item.SubItems.Add(linkedParameter.UpdateParameterField[updateParam]);
                    listViewUpdate.Items.Add(item);
                }

                int selIndex = 0;
                for (int index = 0; index < comboBoxTable.Items.Count; index++)
                {
                    if (comboBoxTable.Items[index].ToString() == linkedParameter.TableName)
                    {
                        selIndex = index;
                        break;
                    }
                }
                comboBoxTable.SelectedIndex = selIndex;
            }
            FillParameterList();
        }

        private void FillParameterList()
        {
            Dictionary<int, ParamProperties> catParams = new Dictionary<int, ParamProperties>();
            if (typeCatParamSettings.ContainsKey(selectedCategory)) { catParams = typeCatParamSettings[selectedCategory]; }

            foreach (int paramId in catParams.Keys)
            {
                ParamProperties pp = catParams[paramId];
                if (pp.IsReadOnly || pp.IsLockAll) { continue; }
                if (!pp.IsVisible) { continue; }
                if (pp.ParamFormat == "String" || pp.ParamFormat == "Double") 
                {
                    ListViewItem item = new ListViewItem(pp.ParamName);
                    item.Name = paramId.ToString();
                    item.Tag = pp;

                    if (pp.IsProject) { item.ImageIndex = 1; }
                    else { item.ImageIndex = 0; }

                    listViewRevit.Items.Add(item); 
                }

                if (isEdit && pp.ParamName == linkedParameter.ControlParameter)
                {
                    textBoxParameter.Tag = pp;
                }

                if (isEdit && listViewUpdate.Items.ContainsKey(pp.ParamName))
                {
                    listViewUpdate.Items[pp.ParamName].Tag = pp;
                }
            }
        }

        private void FillTableList()
        {
            foreach (TableDef tableDef in daoDB.TableDefs)
            {
                if (tableDef.Attributes != 0) { continue; }
                comboBoxTable.Items.Add(tableDef.Name);
                fieldNames.Add(tableDef.Name, new List<string>());
                foreach (Field field in tableDef.Fields)
                {
                    fieldNames[tableDef.Name].Add(field.Name);
                }
            }
        }

        private void comboBoxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedTable = comboBoxTable.SelectedItem.ToString();
            listViewExtDatabase.Items.Clear();

            if (fieldNames.ContainsKey(selectedTable))
            {
                foreach (string fieldName in fieldNames[selectedTable])
                {
                    ListViewItem item = new ListViewItem(fieldName);
                    item.Name = fieldName;
                    item.Tag = selectedTable;

                    listViewExtDatabase.Items.Add(item);
                }
            }
        }

        private void bttnAddControl_Click(object sender, EventArgs e)
        {
            if (listViewRevit.SelectedItems.Count > 0 && listViewExtDatabase.SelectedItems.Count > 0)
            {
                string selectedParam = listViewRevit.SelectedItems[0].Text;
                ParamProperties pp = listViewRevit.SelectedItems[0].Tag as ParamProperties;
                string selectedField = listViewExtDatabase.SelectedItems[0].Text;
                string tableName = listViewExtDatabase.SelectedItems[0].Tag.ToString();

                textBoxParameter.Text = selectedParam;
                textBoxParameter.Tag = pp;
                textBoxField.Text = selectedField;
                textBoxField.Tag = tableName;

            }
            else
            {
                MessageBox.Show("Please select a Revit parameter and a database field from each list.", "Information missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnAddUpdate_Click(object sender, EventArgs e)
        {
            if (ValidateUpdateField())
            {
                string selectedParam = listViewRevit.SelectedItems[0].Text;
                ParamProperties pp = listViewRevit.SelectedItems[0].Tag as ParamProperties;
                string selectedField = listViewExtDatabase.SelectedItems[0].Text;

                ListViewItem item = new ListViewItem(selectedParam);
                item.Name = selectedParam;
                item.Tag = pp;
                item.SubItems.Add(selectedField);
                listViewUpdate.Items.Add(item);
            }
        }

        private bool ValidateUpdateField()
        {
            bool result = false;
            if (textBoxField.Text == string.Empty)
            {
                MessageBox.Show("Please select a controlling parameter and a field first.", "Information missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else if (listViewRevit.SelectedItems.Count < 1 || listViewExtDatabase.SelectedItems.Count < 1)
            {
                MessageBox.Show("Please select a Revit parameter and a database field from each list.", "Information missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else if (listViewExtDatabase.SelectedItems[0].Tag.ToString() != textBoxField.Tag.ToString())
            {
                MessageBox.Show("An updating field should exist in a same table as the selected controling field belongs to.\n Table Name: " + textBoxField.Tag.ToString(), "Table Name Mismatched", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else if (listViewUpdate.Items.Count > 4)
            {
                MessageBox.Show("Updating parameters cannot be defined more than five.", "Out of Range: Updating Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else if (listViewUpdate.Items.ContainsKey(listViewExtDatabase.SelectedItems[0].Text))
            {
                MessageBox.Show(listViewExtDatabase.SelectedItems[0].Text+" already exist in the list of updating parameters.", "Duplicate Parameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        private void bttnDelete_Click(object sender, EventArgs e)
        {
            listViewUpdate.SelectedItems[0].Remove();
        }

        private void bttnMoveUp_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listViewUpdate.Items.Count; i++)
            {
                if (listViewUpdate.SelectedIndices.Contains(i))
                {
                    if (i > 0 && !listViewUpdate.SelectedIndices.Contains(i - 1))
                    {
                        ListViewItem item = listViewUpdate.Items[i];
                        listViewUpdate.Items.RemoveAt(i);
                        listViewUpdate.Items.Insert(i - 1, item);
                        listViewUpdate.SelectedIndices.Add(i - 1);
                    }
                }
            }
        }

        private void bttnMoveDown_Click(object sender, EventArgs e)
        {
            for (int i = listViewUpdate.Items.Count - 1; i > -1; i--)
            {
                if (listViewUpdate.SelectedIndices.Contains(i))
                {
                    if (i < listViewUpdate.Items.Count - 1 && !listViewUpdate.SelectedIndices.Contains(i + 1))
                    {
                        ListViewItem item = listViewUpdate.Items[i];
                        listViewUpdate.Items.RemoveAt(i);
                        listViewUpdate.Items.Insert(i + 1, item);
                        listViewUpdate.SelectedIndices.Add(i + 1);
                    }
                }
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            if (ValidateLinkedParameter())
            {
                linkedParameter = new LinkedParameter(textBoxParameter.Text, textBoxField.Text);
                linkedParameter.CategoryName = selectedCategory;
                linkedParameter.DBPath = extDBPath;
                linkedParameter.TableName = textBoxField.Tag.ToString();

                Dictionary<string, string> updatePairs = new Dictionary<string, string>();
                foreach (ListViewItem updateParam in listViewUpdate.Items)
                {
                    string paramName = updateParam.Text;
                    string fieldName = updateParam.SubItems[1].Text;

                    updatePairs.Add(paramName, fieldName);
                }
                linkedParameter.UpdateParameterField = updatePairs;

                //ChangeUpdateParamSettings();
                UpdateDataGridView();
                this.DialogResult = DialogResult.OK;
            }
        }

        private bool ValidateLinkedParameter()
        {
            bool result = false;
            if (textBoxParameter.Text == string.Empty || textBoxField.Text == string.Empty)
            {
                MessageBox.Show("Please select a controlling parameter and a field first.", "Information missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else if (listViewUpdate.Items.Count < 1)
            {
                MessageBox.Show("Please select updating parameters and fields at least one pair.", "Information missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        private void UpdateDataGridView()
        {
            string updateParams = "";
            foreach (string updateParam in linkedParameter.UpdateParameterField.Keys)
            {
                updateParams += updateParam + ", ";
            }

            if (isEdit)
            {
                extDataGridView.Rows[rowIndex].SetValues(selectedCategory, linkedParameter.ControlParameter, updateParams, extDBPath);
            }
            else
            {
                rowIndex = extDataGridView.Rows.Add(selectedCategory, linkedParameter.ControlParameter, updateParams, extDBPath);
            }

            extDataGridView.Rows[rowIndex].Tag = linkedParameter;
            
        }

        private void ChangeUpdateParamSettings()
        {
            //Controlling parameter can be either RevitControlled or DBControlled, however, Updating parameter should be only DBControlled.
            try
            {
                ParamProperties controlProperty = textBoxParameter.Tag as ParamProperties;
                if (null != controlProperty)
                {
                    bool isEditable = controlProperty.IsEditable;

                    foreach (ListViewItem item in listViewUpdate.Items)
                    {
                        ParamProperties updateProperty = item.Tag as ParamProperties;
                        if (null != updateProperty)
                        {
                            if (typeCatParamSettings.ContainsKey(selectedCategory))
                            {
                                if (typeCatParamSettings[selectedCategory].ContainsKey(updateProperty.ParamID))
                                {
                                    ParamProperties pp = typeCatParamSettings[selectedCategory][updateProperty.ParamID];
                                    pp.IsEditable = false;
                                    typeCatParamSettings[selectedCategory].Remove(pp.ParamID);
                                    typeCatParamSettings[selectedCategory].Add(pp.ParamID, pp);
                                }
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change parameter settings of updating parameters.\n" + ex.Message, "Error: External DB", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void CloseDatabase()
        {
            if (null != oAccess.CurrentDb())
            {
                oAccess.CloseCurrentDatabase();
                oAccess.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oAccess);
                GC.Collect();

                foreach (Process pr in Process.GetProcessesByName("MSACCESS"))
                {
                    pr.Kill();
                }
            }
        }

        private void listViewRevit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (null != selectedParam)
            {
                selectedParam.BackColor = System.Drawing.Color.White;
                selectedParam.ForeColor = System.Drawing.Color.Black;
            }
            if (listViewRevit.SelectedItems.Count > 0)
            {
                selectedParam = listViewRevit.SelectedItems[0];
                selectedParam.BackColor = System.Drawing.Color.DodgerBlue;
                selectedParam.ForeColor = System.Drawing.Color.White;
            }
        }

        private void listViewExtDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (null != selectedField)
            {
                selectedField.BackColor = System.Drawing.Color.White;
                selectedField.ForeColor = System.Drawing.Color.Black;
            }
            if (listViewExtDatabase.SelectedItems.Count > 0)
            {
                selectedField = listViewExtDatabase.SelectedItems[0];
                selectedField.BackColor = System.Drawing.Color.DodgerBlue;
                selectedField.ForeColor = System.Drawing.Color.White;
            }
        }
    }
}
