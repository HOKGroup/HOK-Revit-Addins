using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access.Dao;
using Access = Microsoft.Office.Interop.Access;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using RevitDBManager.Classes;
using System.Runtime.InteropServices;

namespace RevitDBManager.ViewerClasses
{
    class ExternalReference
    {
        private Database daoDB;

        private Access.Application oAccess;
        private Database extDB;
        private object mMissing = System.Reflection.Missing.Value;
        private Dictionary<string/*categoryName*/, LinkedParameter> linkedParameters = new Dictionary<string, LinkedParameter>();
        private Dictionary<string/*tableName*/, LinkedParameter> externalFields = new Dictionary<string, LinkedParameter>();
        private Dictionary<string/*tableName*/, DataTable> sourceTables = new Dictionary<string, DataTable>();//datatables from a linked external database
        private Dictionary<string/*tableName*/, List<int/*rowIndex*/>> warningRows = new Dictionary<string, List<int>>();

        public Dictionary<string, LinkedParameter> LinkedParameters { get { return linkedParameters; } set { linkedParameters = value; } }
        public Dictionary<string, LinkedParameter> ExternalFields { get { return externalFields; } set { externalFields = value; } }
        public Dictionary<string, DataTable> SourceTables { get { return sourceTables; } set { sourceTables = value; } }
        public Dictionary<string, List<int>> WarningRows { get { return warningRows; } set { warningRows = value; } }

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        
        public ExternalReference(Database database)
        {
            daoDB = database;
            CollectExternalReference();
            CreateSourceTable();
        }

        private void CollectExternalReference()
        {
            try
            {
                string tableName = "UI_ExternalReference";
                Recordset recordset;
                recordset = daoDB.OpenTable(tableName);

                while (!recordset.EOF)
                {
                    string controlParam = recordset.Fields["ControlParam"].Value;
                    string controlField = recordset.Fields["ControlField"].Value;
                    string categoryName = recordset.Fields["CategoryName"].Value;

                    if (!linkedParameters.ContainsKey(categoryName))
                    {
                        LinkedParameter lp = new LinkedParameter(controlParam, controlField);
                        lp.CategoryName = categoryName;
                        lp.TableName = recordset.Fields["TableName"].Value;
                        lp.DBPath = recordset.Fields["DBPath"].Value;

                        Dictionary<string, string> updateParams = new Dictionary<string, string>();
                        for (int i = 1; i < 6; i++)
                        {
                            var updateParam = recordset.Fields["UpdateParam" + i].Value;
                            var updateField = recordset.Fields["UpdateField" + i].Value;
                            if (!Convert.IsDBNull(updateParam) && updateParam!="NULL" && !updateParams.ContainsKey(updateParam)) { updateParams.Add(updateParam, updateField); }
                        }
                        lp.UpdateParameterField = updateParams;

                        if (lp.ControlField.Contains("NULL") || lp.ControlParameter.Contains("NULL") || lp.TableName.Contains("NULL") || lp.DBPath.Contains("NULL")) { continue; }
                        linkedParameters.Add(lp.CategoryName, lp);

                        string viewerTable = "";
                        if (lp.CategoryName.Contains("Rooms") || lp.CategoryName.Contains("Spaces") || lp.CategoryName.Contains("Areas")) { viewerTable = "Rvt_" + lp.CategoryName; }
                        else { viewerTable = "Type_" + lp.CategoryName; }

                        if (!externalFields.ContainsKey(viewerTable))
                        {
                            externalFields.Add(viewerTable, lp);
                        }
                    }
                    recordset.MoveNext();
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Read Data from UI_ExternalReference table: \n" + ex.Message);
            }
        }
        
        //DataTable from the linked external database with coulumns including a controlling field and updating fields.
        private void CreateSourceTable()
        {
            try
            {
                oAccess = new Access.Application();

                foreach (string tableName in externalFields.Keys)
                {
                    LinkedParameter linkedParam = externalFields[tableName];
                    DataTable sourceTable = new DataTable(linkedParam.TableName);
                    sourceTable.Columns.Add(linkedParam.ControlField);

                    foreach (string updateParam in linkedParam.UpdateParameterField.Keys)
                    {
                        string updateField = linkedParam.UpdateParameterField[updateParam];
                        sourceTable.Columns.Add(updateField);
                    }

                    oAccess.OpenCurrentDatabase(linkedParam.DBPath);
                    oAccess.Visible = false;
                    extDB = oAccess.CurrentDb();

                    string strFields = "";
                    foreach (DataColumn col in sourceTable.Columns)
                    {
                        strFields += " [" + col.ColumnName + "],";
                    }

                    strFields=strFields.Substring(0, strFields.Length - 1);//to remove the last colone in the string

                    string sqlQuery = "SELECT" + strFields + " FROM [" + linkedParam.TableName + "]";

                    Recordset recordset;
                    recordset = extDB.OpenRecordset(sqlQuery);

                    DataRow row;
                    while (!recordset.EOF)
                    {
                        row = sourceTable.NewRow();

                        foreach (DataColumn column in sourceTable.Columns)
                        {
                            row[column] = recordset.Fields[column.ColumnName].Value;
                        }
                        sourceTable.Rows.Add(row);
                        recordset.MoveNext();
                    }
                    recordset.Close();
                    if (null != oAccess.CurrentDb()) { oAccess.CloseCurrentDatabase(); }
                    sourceTables.Add(tableName, sourceTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create source tables. \n" + ex.Message, "ExternalReference Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
      
        public bool CreateComboBoxList(DataTable dataTable, DataGridView dataGridViewExt)
        {
            bool exist = false;
            try
            {
                string tableName = dataTable.TableName;

                if (sourceTables.ContainsKey(tableName) && externalFields.ContainsKey(tableName) )
                {
                    DataTable sourceTable = sourceTables[tableName];
                    LinkedParameter lp = externalFields[tableName];

                    BindingSource bindingSource = new BindingSource();
                    bindingSource.DataSource = sourceTable;

                    DataGridViewComboBoxColumn combocolumn = new DataGridViewComboBoxColumn();
                    combocolumn.Name = lp.ControlField;
                    combocolumn.DataPropertyName = lp.ControlField;
                    combocolumn.HeaderText = "Ext: " + lp.ControlParameter; ;
                    combocolumn.MaxDropDownItems = 15;
                    combocolumn.DataSource = bindingSource;
                    combocolumn.DisplayMember = lp.ControlField;
                    combocolumn.ValueMember = lp.ControlField;
                    combocolumn.Tag = lp.ControlParameter;
                    combocolumn.FlatStyle = FlatStyle.Flat;
                    combocolumn.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                    combocolumn.ReadOnly = false;
                    dataGridViewExt.Columns.Add(combocolumn);

                    foreach (string updateParam in lp.UpdateParameterField.Keys)
                    {
                        string updateField = lp.UpdateParameterField[updateParam];
                        DataGridViewTextBoxColumn txtcolumn = new DataGridViewTextBoxColumn();
                        txtcolumn.Name = updateField;
                        txtcolumn.HeaderText = "Ext: " + updateParam;
                        txtcolumn.Tag = updateParam;
                        dataGridViewExt.Columns.Add(txtcolumn);
                    }

                    if (dataGridViewExt.Columns.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            int index = dataGridViewExt.Rows.Add();
                            DataGridViewRow gridRow = dataGridViewExt.Rows[index];
                            foreach (DataGridViewColumn column in dataGridViewExt.Columns)
                            {
                                gridRow.Cells[column.Name].Tag = column.Tag as string; //datagridview cell tag>> Revit parameter name 
                            }
                        }
                        exist = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create a comboboxList. \n"+ex.Message, "ExternalReference Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                exist = false;
            }
            return exist;
        }
        
        public void DisplayExternalData(DataTable mainTable, DataGridView dataGridViewExt)
        {
            try
            {
                string tableName = mainTable.TableName;
                if (externalFields.ContainsKey(tableName) && sourceTables.ContainsKey(tableName))
                {
                    LinkedParameter lp = externalFields[tableName];

                    for (int i = 0; i < mainTable.Rows.Count; i++)
                    {
                        string value = mainTable.Rows[i][lp.ControlParameter].ToString();
                        if (value != string.Empty && value != "NULL")
                        {
                            dataGridViewExt[lp.ControlField, i].Value = value;
                        }

                        foreach (string updateParam in lp.UpdateParameterField.Keys)
                        {
                            string updateField = lp.UpdateParameterField[updateParam];
                            string mainVal = mainTable.Rows[i][updateParam].ToString();
                            if (mainVal != string.Empty && mainVal != "NULL")
                            {
                                dataGridViewExt[updateField, i].Value = mainVal;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display external data. \n" + ex.Message, "ExternalReference Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void UpdateExternalData(DataTable mainTable, DataGridView dataGridViewExt, int id, int rowIndex)
        {
            try
            {
                if (sourceTables.ContainsKey(mainTable.TableName))
                {
                    DataTable sourceTable = sourceTables[mainTable.TableName];

                    for (int i = 1; i < sourceTable.Columns.Count; i++) //exclude the control field
                    {
                        dataGridViewExt[i, rowIndex].Value = sourceTable.Rows[id][i].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update external data. \n" + ex.Message, "ExternalReference Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void HideParameters(DataTable mainTable, DataGridView dataGridViewMain)
        {
            if (externalFields.ContainsKey(mainTable.TableName))
            {
                LinkedParameter linkedParam = externalFields[mainTable.TableName];
                
                List<string> columsToHide=new List<string>();
                columsToHide.Add(linkedParam.ControlParameter);
                columsToHide.AddRange(linkedParam.UpdateParameterField.Keys.ToList());

                foreach (DataGridViewColumn column in dataGridViewMain.Columns)
                {
                    if (columsToHide.Contains(column.Name))
                    {
                        column.Visible = false;
                    }
                    else
                    {
                        column.Visible = true;
                    }
                }
            }
        }

        public void ShowParameters(DataTable mainTable, DataGridView dataGridViewMain)
        {
            if (externalFields.ContainsKey(mainTable.TableName))
            {
                LinkedParameter linkedParam = externalFields[mainTable.TableName];

                List<string> columsToReveal = new List<string>();
                columsToReveal.Add(linkedParam.ControlParameter);
                columsToReveal.AddRange(linkedParam.UpdateParameterField.Keys.ToList());

                foreach (DataGridViewColumn column in dataGridViewMain.Columns)
                {
                    if (columsToReveal.Contains(column.Name))
                    {
                        column.Visible = true;
                    }
                }
            }
        }

        public void ColorWarningCells(DataTable dataTable, DataGridView dataGridViewMain, DataGridView dataGridViewExt)
        {
            try
            {
                string tableName = dataTable.TableName;

                if (sourceTables.ContainsKey(tableName) && externalFields.ContainsKey(tableName))
                {
                    DataTable sourceTable = sourceTables[tableName];
                    LinkedParameter lp = externalFields[tableName];

                    for (int rowIndex = 0; rowIndex < dataGridViewExt.Rows.Count; rowIndex++)
                    {
                        if (null!=dataGridViewExt.Rows[rowIndex].Cells[0].Value && dataGridViewExt.Rows[rowIndex].Cells[0].Value.ToString()==string.Empty)
                        {
                            //dataGridViewExt.Rows[rowIndex].Cells[0].Style.BackColor = System.Drawing.Color.AntiqueWhite;
                            dataGridViewExt.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.AntiqueWhite;
                            string revitControl = dataGridViewExt.Rows[rowIndex].Cells[0].Tag as string;
                            if (null != revitControl)
                            {
                                //dataGridViewMain.Rows[rowIndex].Cells[revitControl].Style.BackColor = System.Drawing.Color.AntiqueWhite;
                                dataGridViewMain.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.AntiqueWhite;
                            }

                            if (warningRows.ContainsKey(tableName))
                            {
                                if (!warningRows[tableName].Contains(rowIndex))
                                {
                                    warningRows[tableName].Add(rowIndex);
                                }
                            }
                            else
                            {
                                warningRows.Add(tableName, new List<int>());
                                warningRows[tableName].Add(rowIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to mark mismatched cells in the data grid view. \n" + ex.Message, "ExternalReference Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void CloseDatabase()
        {
            if (null != oAccess.CurrentDb()) { oAccess.CloseCurrentDatabase(); }
            oAccess.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oAccess);
            GC.Collect();

            foreach (Process pr in Process.GetProcessesByName("MSACCESS"))
            {
                pr.Kill();
            }

        }

        private void MinimizeAccessApp()
        {
            foreach (Process process in Process.GetProcessesByName("MSACCESS"))
            {
                IntPtr intPtr = (IntPtr)process.MainWindowHandle;
                ShowWindowAsync(intPtr, 2);//hide=0, shwoNormal=1, showMinimized=2; showMaximized=3;
            }
        }
    }
}
