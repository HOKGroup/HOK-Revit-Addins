using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using RevitDBManager.Classes;
using Microsoft.Office.Interop.Access.Dao;
using RevitDBManager.Helper;
using RevitDBManager.ViewerForms;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using RevitDBManager.ViewerClasses;
using RevitDBManager.HelperClasses;

namespace RevitDBManager.Forms
{
    public partial class form_Viewer : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private string dbPath = "";
        private DataSet dataSet=new DataSet();
        private Database database;
        private RevitDBViewer dbViewer;
        private SynchronizeToRevit syncRevit;
        private RevitDataFilter revitFilter;
        private CellValueControl cellControl = new CellValueControl();
        private UnitConverter unitConverter = new UnitConverter();
        private RevisionManager revisionManager;
        private Dictionary<string/*table name for display*/, DataTable> tableDictionary = new Dictionary<string, DataTable>();
        private string selectedTable = "";
        private int selTableIndex = 0;
        private bool isVisibleInternal = true;
        private bool isRoomSelected = false;
        private bool isSpaceSelected = false;
        private bool isAreaSelected = false;
        private bool userTyped = true;
        private int controlIndex = 0; //index of combobox list of controlling field 
        private bool editExt = false;
        private bool warningMode = true;
        private bool filterMode = false;

        private Dictionary<string/*tableName*/, List<string>/*fieldNames*/> selectedFields = new Dictionary<string, List<string>>();
        private string[] internalFields = new string[] { "FromRoom", "HandFlipped", "Host", "IsSlantedColumn", "Mirrored", "Room", "ToRoom" };
        private string[] systemFamilies = new string[] { "Ceilings", "Floors", "Railings", "Ramps", "Roofs", "Stairs", "Structural Foundations", "Walls" };

        private Dictionary<string/*tableName*/, Dictionary<string/*keyId*/, List<string>/*colNames*/>> changedValues = new Dictionary<string, Dictionary<string, List<string>>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, string/*expression*/>> calculatedExpression = new Dictionary<string, Dictionary<string,string>>();
        private List<string> warningTables = new List<string>();
        private Dictionary<string/*tableName*/, List<int>/*rowIndex*/> warningRows = new Dictionary<string, List<int>>();
        private Dictionary<string/*paramId*/, string/*paramName*/> doubleFields = new Dictionary<string/*paramId*/, string/*paramName*/>();
        private TabPage sheetPage;

        public bool WarningMode { get { return warningMode; } set { warningMode = value; } }
        public List<string> WarningTables { get { return warningTables; } set { warningTables = value; } }
        public Dictionary<string, List<int>> WarningRows { get { return warningRows; } set { warningRows = value; } }

        public form_Viewer(UIApplication application, string filePath, bool fixingMode)
        {
            try
            {
                m_app = application;
                dbPath = filePath;
                warningMode = fixingMode;

                InitializeComponent();
                toolStripStatusLabel1.Text = dbPath;
                toolStripProgressBar.Visible = false;

                dbViewer = new RevitDBViewer(m_app, dbPath, true, warningMode);
                database = dbViewer.DaoDB;
                dataSet = dbViewer.DataSet;
                selectedFields = dbViewer.InternalFields;
                isRoomSelected = dbViewer.IsRoomSelected;
                isSpaceSelected = dbViewer.IsSpaceSelected;
                isAreaSelected = dbViewer.IsAreaSelected;
                warningMode = dbViewer.WarningMode;
                warningTables = dbViewer.WarningTables;

                syncRevit = new SynchronizeToRevit(m_app, database);
                syncRevit.LockTypeFields = dbViewer.LockTypeFields;
                syncRevit.ParamIdMap = dbViewer.ParamIdMap;
                syncRevit.ProgressBar = toolStripProgressBar;

                if (warningMode)
                {
                    textBoxWarnings.Visible = true;
                    groupBoxWarning.Visible = true;
                }

                revitFilter = new RevitDataFilter(dataGridParam, cmbColumnName, cmbConditions, cmbValue, bttnFilter, bttnCancel);
                revitFilter.DUTDictionary = dbViewer.DUTDictionary;

                sheetPage = tabPageSheet;
                revisionManager = new RevisionManager(m_app);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize Data Editor.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void form_DBViewer_Load(object sender, EventArgs e)
        {
            panelHome.Dock = DockStyle.Fill;
            panelSheet.Dock = DockStyle.Fill;
            panelSheet.Visible = false;

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(this.bttnDatabase, "Open an alternative database in the viewer.");
            toolTip.SetToolTip(this.bttnSync, "Export data to the Revit project.");
            toolTip.SetToolTip(this.bttnSettings, "Select hidden properties of FamilyInstace.");
            toolTip.SetToolTip(this.bttnView, "Display hidden properties.");
            toolTip.SetToolTip(this.bttnHiden, "Hide hidden properties.");
            toolTip.SetToolTip(this.bttnExtView, "Display linked reference fields.");
            toolTip.SetToolTip(this.bttnExtHide, "Hide linked reference fields. ");
            toolTip.SetToolTip(this.bttnString, "Build expression for string concatenation.");
            toolTip.SetToolTip(this.bttnMath, "Build expression for math calculation. ");
            toolTip.SetToolTip(this.bttnRemoveCal, "Remove calculated fields. ");
            toolTip.SetToolTip(this.bttnRefresh, "Refresh data in calculated fields.");
            toolTip.SetToolTip(this.bttnFilterView, "Move to Filter view.");
            toolTip.SetToolTip(this.bttnFilter, "Filter");
            toolTip.SetToolTip(this.bttnCancel, "Clear Filter.");
            toolTip.SetToolTip(this.bttnHome, "Clear Filter and move to Home view.");

            LoadForm();
        }

        private void LoadForm()
        {
            try
            {
                tableDictionary = new Dictionary<string, DataTable>();
                listBoxDataTable.Items.Clear();

                foreach (DataTable table in dataSet.Tables)
                {
                    string tableName = table.TableName;
                    string newName = "";
                    if (tableName.Contains("Inst_") && !tableName.Contains("Parameters"))
                    {
                        newName = tableName.Replace("Inst_", "") + " Instances";  //e.g) Inst_Columns ==> Columns Instances
                        listBoxDataTable.Items.Add(newName);
                        tableDictionary.Add(newName, table);
                    }

                    if (tableName.Contains("Type_") && !tableName.Contains("Parameters"))
                    {
                        newName = tableName.Replace("Type_", "") + " Types";  //e.g) Type_Columns ==> Columns Types
                        listBoxDataTable.Items.Add(newName);
                        tableDictionary.Add(newName, table);
                    }

                    if (tableName.Contains("Rooms") && isRoomSelected)
                    {
                        listBoxDataTable.Items.Add("Rooms");
                        tableDictionary.Add("Rooms", table);
                    }
                    if (tableName.Contains("Spaces") && isSpaceSelected)
                    {
                        listBoxDataTable.Items.Add("Spaces");
                        tableDictionary.Add("Spaces", table);
                    }
                    if (tableName.Contains("Areas") && isAreaSelected)
                    {
                        listBoxDataTable.Items.Add("Areas");
                        tableDictionary.Add("Areas", table);
                    }
                }

                selTableIndex = 0;
                listBoxDataTable.SetSelected(selTableIndex, true);
                selectedTable = listBoxDataTable.SelectedItem.ToString();
                if (isVisibleInternal) { dbViewer.UnhideInternalData(dataGridParam); }

                foreach (DataGridViewColumn col in dataGridParam.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                if (tableDictionary.ContainsKey("Views Instances") || tableDictionary.ContainsKey("Views Types"))
                {
                    if (!tabControlMenu.TabPages.ContainsKey("tabPageSheet")) { tabControlMenu.TabPages.Add(sheetPage); }
                }
                else
                {
                    tabControlMenu.TabPages.RemoveByKey("tabPageSheet");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Data Editor form.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindDataSource(DataTable dataTable, DataGridView dataGridView, bool isHome)
        {
            try
            {
                splitContainerParam.Panel2Collapsed = true;
                dataGridView.AutoGenerateColumns = true;
                dataGridView.AllowUserToOrderColumns = true;

                BindingSource bindingSource = new BindingSource();
                bindingSource.DataSource = dataTable;
                dataGridView.DataSource = bindingSource;

                foreach (DataGridViewColumn col in dataGridView.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                if (isHome)
                {
                    dataGridViewExt.Columns.Clear();
                    bool exist = dbViewer.ConnectExternalReference(dataTable, dataGridViewExt);
                    if (exist)
                    {
                        editExt = false;
                        dbViewer.DisplayExternalData(dataTable, dataGridViewExt);
                        if (warningMode) 
                        {
                            dbViewer.WarningRows = warningRows;
                            dbViewer.ColorWarningCells(dataTable, dataGridParam, dataGridViewExt);
                            warningRows = dbViewer.WarningRows;
                        }
                        splitContainerParam.Panel2Collapsed = false;
                        dbViewer.HideExtColumns(dataTable, dataGridParam);
                        editExt = true;
                    }
                }

                dbViewer.AddDoubleTag(dataTable.TableName, dataGridView);
                dbViewer.CreateCheckBoxColumns(dataTable, dataGridView);
                dbViewer.GrayTextReadOnly(dataTable, dataGridView);
                dbViewer.HideInternalObject(dataGridView);
                dbViewer.OrderColumns(dataTable, dataGridView);
                dbViewer.AddToolTipExpression(dataTable.TableName, dataGridView);

                if (isVisibleInternal) { dbViewer.UnhideInternalData(dataGridParam); }
                else { dbViewer.HideInternalData(dataGridParam); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to bind data source.\n" + ex.Message, "Error:Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void form_DBViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            dbViewer.CloseDatabase();
        }

        private void listBoxDataTable_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                //saving data before changing the index of selected table.
                dataGridParam.SelectionMode = DataGridViewSelectionMode.CellSelect;
                if (tableDictionary.ContainsKey(selectedTable) && null!=dataGridParam.DataSource)
                {
                    BindingSource bs = (BindingSource)dataGridParam.DataSource;
                    DataTable datatable = (DataTable)bs.DataSource;
                    dataSet.Tables.Remove(tableDictionary[selectedTable]);
                    dataSet.Tables.Add(datatable);

                    tableDictionary.Remove(selectedTable);
                    tableDictionary.Add(selectedTable, datatable);
                }
                selTableIndex = listBoxDataTable.SelectedIndex;
                selectedTable = listBoxDataTable.SelectedItem.ToString();
                BindDataSource(tableDictionary[selectedTable], dataGridParam, true);
                ChangeTextColor(tableDictionary[selectedTable].TableName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot bind data into the DataGridView.\n"+ex.Message,"Error: Data Editor",MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ChangeTextColor(string tableName)
        {
            try
            {
                if (changedValues.ContainsKey(tableName))
                {
                    foreach (DataGridViewRow row in dataGridParam.Rows)
                    {
                        string keyID = row.Cells[0].Value.ToString();
                        if (changedValues[tableName].ContainsKey(keyID))
                        {
                            foreach (string colName in changedValues[tableName][keyID])
                            {
                                row.Cells[colName].Style.ForeColor = System.Drawing.Color.RoyalBlue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change text color.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //change text color of changed cells by Fill Copy, Fill Seriese, and Paste
        private void ChangeTextColor(DataGridViewSelectedCellCollection selectedCells)
        {
            foreach (DataGridViewCell selCell in selectedCells)
            {
                selCell.Style.ForeColor = System.Drawing.Color.RoyalBlue;
            }
        }

        #region dataGridParam Tab
        private void dataGridParam_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                double dblValue = 0.00;
                DataGridViewCell selectedCell = dataGridParam.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string userValue = selectedCell.Value.ToString();
                string suffix = "";
                
                if (userTyped && !filterMode)
                {
                    selectedCell.Style.ForeColor = System.Drawing.Color.RoyalBlue;
                    
                    if (null != dataGridParam.Columns[e.ColumnIndex].Tag)
                    {
                        suffix = dataGridParam.Columns[e.ColumnIndex].Tag.ToString();
                        string trueSuffix = suffix.Replace(" ", string.Empty);

                        if(suffix.Contains("FFI") || suffix.Contains("FI")||suffix.Contains("NONE"))
                        {
                            if (double.TryParse(userValue, out dblValue))
                            {
                                selectedCell.Value = unitConverter.ConvertDecimalToFractional(userValue, suffix);
                            }
                            else if (unitConverter.ValidateFormat(userValue, suffix))
                            {
                                selectedCell.Tag = unitConverter.GetDoubleValue(userValue, suffix);
                            }
                            else
                            {
                                selectedCell.Value = unitConverter.FixToValidFormat(userValue, suffix); //e.g. 10'3"
                            }
                            
                        }
                        else if (userValue.Contains(suffix)) //10.00 SF
                        {
                            selectedCell.Tag = unitConverter.GetDoubleValue(userValue, suffix);
                        }
                        else if (userValue.Contains(trueSuffix)) //10.00SF
                        {
                            selectedCell.Value = userValue.Replace(trueSuffix, " " + trueSuffix);//when users type suffix without space
                        }
                        else if (double.TryParse(userValue, out dblValue)) //10.00
                        {
                            selectedCell.Value = Math.Round(dblValue, 2) + suffix;
                        }
                        else // something invalid format
                        {
                            selectedCell.Value = dblValue + suffix;
                        }
                    }
                }

                if (filterMode)
                {
                    if (double.TryParse(userValue, out dblValue)) { selectedCell.Tag = dblValue; }
                    else { selectedCell.Value = 0; }
                }

                string tableName = tableDictionary[selectedTable].TableName;
                string fieldName = dataGridParam.Columns[e.ColumnIndex].Name;
                string value = selectedCell.Value.ToString();
                string keyField = dataGridParam.Columns[0].Name;
                string keyValue = dataGridParam.Rows[e.RowIndex].Cells[0].Value.ToString();

                if (dataGridParam.Columns[e.ColumnIndex].CellTemplate.GetType()==typeof(DataGridViewCheckBoxCell))
                {
                    if (value == "True") { value = "1"; } else { value = "0"; }
                }

                if (null != selectedCell.Tag)
                {
                    double.TryParse(selectedCell.Tag.ToString(), out dblValue);
                }

                if (filterMode) { value = value + suffix; }

                if (dbViewer.UpdateCell(tableName, fieldName, value, keyField, keyValue, dblValue))
                {
                    if (changedValues.ContainsKey(tableName))
                    {
                        if (changedValues[tableName].ContainsKey(keyValue))
                        {
                            if (!changedValues[tableName][keyValue].Contains(fieldName))
                            {
                                changedValues[tableName][keyValue].Add(fieldName);
                            }
                        }
                        else
                        {
                            changedValues[tableName].Add(keyValue, new List<string>());
                            changedValues[tableName][keyValue].Add(fieldName);
                        }
                    }
                    else
                    {
                        changedValues.Add(tableName, new Dictionary<string, List<string>>());
                        changedValues[tableName].Add(keyValue, new List<string>());
                        changedValues[tableName][keyValue].Add(fieldName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change the cell value as you entered.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dataGridParam_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!dataGridParam.Columns[e.ColumnIndex].Name.Contains("Ext:"))
            {
                dataGridParam.SelectionMode = DataGridViewSelectionMode.CellSelect;
            }
        }

        private void dataGridParam_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                string tableName = tableDictionary[selectedTable].TableName;
                dbViewer.DrawHeaderCell(e, tableName, dataGridParam, imageListLock);
            }
        }

        private void dataGridParam_Sorted(object sender, EventArgs e)
        {
            dbViewer.GrayTextReadOnly(tableDictionary[selectedTable],dataGridParam);
        }

        private void dataGridParam_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.ColumnIndex > -1 && e.RowIndex > -1 && e.Button == MouseButtons.Right)
                {
                    DataGridViewCell cell = dataGridParam[e.ColumnIndex, e.RowIndex];

                    if (!selectedTable.Contains("Type") && e.ColumnIndex == 0 && cell.Selected) { menuSelect.Enabled = true; }
                    else { menuSelect.Enabled = false; }

                    if (cell.ReadOnly)
                    {
                        menuCut.Enabled = false; menuPaste.Enabled = false; menuFillCopy.Enabled = false; menuFillSeries.Enabled = false;
                    }
                    else if (dataGridParam.SelectedCells.Count > 1)
                    {
                        menuCut.Enabled = true; menuPaste.Enabled = true; menuFillCopy.Enabled = true; menuFillSeries.Enabled = true;
                    }
                    else
                    {
                        menuCut.Enabled = true; menuPaste.Enabled = true; menuFillCopy.Enabled = false; menuFillSeries.Enabled = false;
                    }
                    System.Drawing.Rectangle rec = dataGridParam.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    menuCellControl.Show((System.Windows.Forms.Control)sender, rec.Left + e.X, rec.Top + e.Y);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to invoke dataGridParam_CellMouseClick.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dataGridParam_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridParam.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
            dataGridParam.Columns[e.ColumnIndex].Selected = true;

            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Rectangle rec = dataGridParam.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                menuColumnControl.Show((System.Windows.Forms.Control)sender, rec.Left + e.X, rec.Top + e.Y);
            }
        }

        private void dataGridParam_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                string columnName = dataGridParam.Columns[e.ColumnIndex].Name;
                MessageBox.Show(e.Exception.Message + "\n ColumnName: " + columnName + ",  RowIndex: " + e.RowIndex, "DataGridParam DataError");
                dataGridParam.CancelEdit();
                dataGridParam.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the cell value.\n" + ex.Message, "DataGridParam DataError", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dataGridParam_Scroll(object sender, ScrollEventArgs e)
        {
            if (dataGridViewExt.RowCount == dataGridParam.RowCount)
            {
                dataGridViewExt.FirstDisplayedScrollingRowIndex = dataGridParam.FirstDisplayedScrollingRowIndex;
            }
        }

        #endregion

        #region dataGridExt

        private void dataGridViewExt_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell selectedCell = dataGridViewExt[e.ColumnIndex, e.RowIndex];
            try
            {
                if (editExt) //to prevent invoke this event when copy from main table to ext table
                {
                    string value = selectedCell.Value.ToString();
                    if (e.ColumnIndex == 0) //Controling Parameter
                    {
                        if (controlIndex >= 0)
                        {
                            dbViewer.UpdateExternalData(tableDictionary[selectedTable], dataGridViewExt, controlIndex, e.RowIndex);
                            CopyValueToDataGridParam(selectedCell, e.RowIndex);

                            if (warningMode)
                            {
                                dataGridViewExt.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.White;
                                string revitControl = selectedCell.Tag as string;
                                if (null != revitControl)
                                {
                                    //dataGridParam.Rows[e.RowIndex].Cells[revitControl].Style.BackColor = System.Drawing.Color.White;
                                    dataGridParam.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.White;
                                }

                                string tableName = tableDictionary[selectedTable].TableName;
                                if (warningRows.ContainsKey(tableName))
                                {
                                    if (warningRows[tableName].Contains(e.RowIndex))
                                    {
                                        warningRows[tableName].Remove(e.RowIndex);
                                    }
                                    if (warningRows[tableName].Count == 0 && warningTables.Contains(tableName))
                                    {
                                        warningTables.Remove(tableName);
                                        if (warningTables.Count == 0)
                                        {
                                            textBoxWarnings.Visible = false;
                                            groupBoxWarning.Visible = false;
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                    else
                    {
                        CopyValueToDataGridParam(selectedCell, e.RowIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update cell value: " + selectedCell.Value + "\n" + ex.Message, "DataGridViewExt SelectedValueChanged", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CopyValueToDataGridParam(DataGridViewCell selectedCell, int rowIndex)
        {
            if (null != selectedCell.Tag)
            {
                string paramName = selectedCell.Tag.ToString();
                if (dataGridParam.Columns.Contains(paramName))
                {
                    if (dataGridParam.Columns[paramName].ReadOnly)
                    {
                        MessageBox.Show(paramName + " field is ReadOnly. \n The field cannot be replaced by the new input.", "Update Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        dataGridParam[paramName, rowIndex].Value = selectedCell.Value;
                    }
                }
            }
        }

        private void dataGridViewExt_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dataGridViewExt.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void dataGridViewExt_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridViewExt.CurrentCell.ColumnIndex == 0)
            {
                System.Windows.Forms.ComboBox comboBox = e.Control as System.Windows.Forms.ComboBox;
                comboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
            }
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            controlIndex = ((System.Windows.Forms.ComboBox)sender).SelectedIndex;
        }

        private void dataGridViewExt_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                string tableName = tableDictionary[selectedTable].TableName;
                dbViewer.DrawHeaderCell(e, tableName, dataGridViewExt, imageListLock);
            }
        }

        private void dataGridViewExt_Scroll(object sender, ScrollEventArgs e)
        {
            if (dataGridViewExt.RowCount == dataGridParam.RowCount)
            {
                dataGridParam.FirstDisplayedScrollingRowIndex = dataGridViewExt.FirstDisplayedScrollingRowIndex;
            }
        }

        #endregion

        #region ColumnControl

        private void hideColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn col in dataGridParam.SelectedColumns)
            {
                string colName = col.HeaderText;
                dataGridParam.Columns[colName].Visible = false;
            }
            dataGridParam.SelectionMode = DataGridViewSelectionMode.CellSelect;
        }

        private void unhideColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn col in dataGridParam.Columns)
            {
                col.Visible = true;
            }
            dataGridParam.SelectionMode = DataGridViewSelectionMode.CellSelect;
        }

        private void sortAZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn col in dataGridParam.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            dataGridParam.Sort(dataGridParam.SelectedColumns[0], ListSortDirection.Ascending);
            foreach (DataGridViewColumn col in dataGridParam.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void sortZAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewColumn col in dataGridParam.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            dataGridParam.Sort(dataGridParam.SelectedColumns[0], ListSortDirection.Descending);
            foreach (DataGridViewColumn col in dataGridParam.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selColumn = dataGridParam.SelectedColumns[0].Name;
            tabControlMenu.SelectedIndex = 1;
            revitFilter.SetSelectedColumn(selColumn);
        }

        #endregion

        #region menuCellControl
        private void menuFillCopy_Click(object sender, EventArgs e)
        {
            userTyped = false;
            if (dataGridParam.SelectedCells.Count > 1)
            {
                cellControl.FillCopy(dataGridParam, dataGridParam.SelectedCells);
                ChangeTextColor(dataGridParam.SelectedCells);
            }
            userTyped = true;
        }

        private void menuFillSeries_Click(object sender, EventArgs e)
        {
            userTyped = false;
            if (dataGridParam.SelectedCells.Count > 1)
            {
                cellControl.AutoFillSeriese(dataGridParam, dataGridParam.SelectedCells);
                ChangeTextColor(dataGridParam.SelectedCells);
            }
            userTyped = true;
        }

        private void menuCopy_Click(object sender, EventArgs e)
        {
            if (dataGridParam.SelectedCells.Count > 0)
            {
                try
                {
                    Clipboard.SetDataObject(dataGridParam.GetClipboardContent(),true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The Clipboard could not be accessed. Please try again.\n" + ex.Message);
                }
            }
        }

        private void menuCut_Click(object sender, EventArgs e)
        {
            if (dataGridParam.SelectedCells.Count > 0)
            {
                try
                {
                    Clipboard.SetDataObject(dataGridParam.GetClipboardContent(), true);
                    for (int i = 0; i < dataGridParam.SelectedCells.Count; i++)
                    {
                        if (!dataGridParam.SelectedCells[i].ReadOnly)
                        {
                            dataGridParam.SelectedCells[i].Value = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The Clipboard could not be accessed. Please try again.\n" + ex.Message);
                }
            }
        }

        private void menuPaste_Click(object sender, EventArgs e)
        {
            try
            {
                userTyped = false;

                //char[] rowSplitter = { '\r', '\n' };
                string[] rowSplitter = { "\r\n" };
                char[] columnSplitter = { '\t' };

                IDataObject dataInClipboard = Clipboard.GetDataObject();
                string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);

                string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.None);

                List<int> rowIndex = new List<int>(); //selected rows
                List<int> colIndex = new List<int>(); //selected columns
                foreach (DataGridViewCell cell in dataGridParam.SelectedCells)
                {
                    if (!rowIndex.Contains(cell.RowIndex)) { rowIndex.Add(cell.RowIndex); }
                    if (!colIndex.Contains(cell.ColumnIndex)) { colIndex.Add(cell.ColumnIndex); }
                }
                rowIndex.Sort();
                colIndex.Sort();


                for (int iRow = 0; iRow < rowIndex.Count; iRow++)
                {
                    if (rowIndex[iRow] < dataGridParam.RowCount)
                    {
                        int rowClip = iRow % rowsInClipboard.Length;
                        string[] valuesInRow = rowsInClipboard[rowClip].Split(columnSplitter);

                        for (int iCol = 0; iCol < colIndex.Count; iCol++)
                        {
                            if (colIndex[iCol] < dataGridParam.ColumnCount)
                            {
                                int colClip = iCol % valuesInRow.Length;
                                string value = valuesInRow[colClip];
                                DataGridViewCell cell = dataGridParam.Rows[rowIndex[iRow]].Cells[colIndex[iCol]];
                                if (null != cell.OwningColumn.Tag)
                                {
                                    string suffix = cell.OwningColumn.Tag.ToString();
                                    cell.Tag = unitConverter.GetDoubleValue(value, suffix);
                                }
                                if (!cell.ReadOnly)
                                {
                                    cell.Value = value;
                                }
                            }
                        }
                    }
                }
                ChangeTextColor(dataGridParam.SelectedCells);
                userTyped = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to paste text.\n"+ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void menuDelete_Click(object sender, EventArgs e)
        {
            userTyped = false;
            for (int i = 0; i < dataGridParam.SelectedCells.Count; i++)
            {
                if (!dataGridParam.SelectedCells[i].ReadOnly)
                {
                    if (null != dataGridParam.SelectedCells[i].OwningColumn.Tag)
                    {
                        string suffix = dataGridParam.SelectedCells[i].OwningColumn.Tag.ToString();
                        dataGridParam.SelectedCells[i].Tag = 0;
                        dataGridParam.SelectedCells[i].Value = 0 + suffix;
                    }
                    else
                    {
                        dataGridParam.SelectedCells[i].Value = "";
                    }
                }
            }
            userTyped = true;
        }

        private void menuSelect_Click(object sender, EventArgs e)
        {
            try
            {
                UIDocument uidoc = m_app.ActiveUIDocument;
                Autodesk.Revit.DB.Document doc = uidoc.Document;
                List<ElementId> elementIds=new List<ElementId>();
#if RELEASE2013||RELEASE2014
                SelElementSet newSelection = SelElementSet.Create();

                foreach (DataGridViewCell cell in dataGridParam.SelectedCells)
                {
                    if (cell.ColumnIndex == 0)
                    {
                        int instId;
                        bool isId = int.TryParse(cell.Value.ToString(), out instId);
                        if (isId)
                        {
                            ElementId elementId = new ElementId(instId);
                            Element element = doc.GetElement(elementId);
                            if (null != element)
                            {
                                newSelection.Add(element);
                                elementIds.Add(elementId);
                            }
                        }
                    }
                }
                uidoc.ShowElements(elementIds);
                uidoc.Selection.Elements = newSelection;
#elif RELEASE2015 || RELEASE2016
                Selection selection = uidoc.Selection;
                foreach (DataGridViewCell cell in dataGridParam.SelectedCells)
                {
                    if (cell.ColumnIndex == 0)
                    {
                        int instId;
                        bool isId = int.TryParse(cell.Value.ToString(), out instId);
                        if (isId)
                        {
                            ElementId elementId = new ElementId(instId);
                            if (elementId != ElementId.InvalidElementId)
                            {
                                elementIds.Add(elementId);
                            }
                        }
                    }
                }
                uidoc.ShowElements(elementIds);
                selection.SetElementIds(elementIds);
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select elements with values from selected cells.\n"+ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Home Tab Buttons
        //change database source
        private void bttnDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                form_FileSelection fileSelectionForm = new form_FileSelection(m_app);
                if (fileSelectionForm.ShowDialog() == DialogResult.OK)
                {
                    if (fileSelectionForm.DefualtFilePath != dbPath)
                    {
                        dbPath = fileSelectionForm.DefualtFilePath;
                        toolStripStatusLabel1.Text = dbPath;
                        toolStripProgressBar.Visible = false;

                        dbViewer = new RevitDBViewer(m_app, dbPath, true,false);
                        database = dbViewer.DaoDB;
                        dataSet = new DataSet();
                        dataSet = dbViewer.DataSet;
                        selectedFields = dbViewer.InternalFields;
                        isRoomSelected = dbViewer.IsRoomSelected;
                        isSpaceSelected = dbViewer.IsSpaceSelected;
                        isAreaSelected = dbViewer.IsAreaSelected;
                        warningMode = dbViewer.WarningMode;
                        warningTables = dbViewer.WarningTables;

                        syncRevit = new SynchronizeToRevit(m_app, database);
                        syncRevit.LockTypeFields = dbViewer.LockTypeFields;
                        syncRevit.ParamIdMap = dbViewer.ParamIdMap;
                        syncRevit.ProgressBar = toolStripProgressBar;

                        dataGridParam.DataSource = null;
                        LoadForm();
                        if (warningMode) { textBoxWarnings.Visible = true; groupBoxWarning.Visible = true; }
                        else { textBoxWarnings.Visible = false; groupBoxWarning.Visible = false; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change data source.\n" + ex.Message, "Error:Data Source", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnSettings_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> categoryNames = new List<string>();
                foreach (DataTable table in dataSet.Tables)
                {
                    string tableName = table.TableName;
                    string newName = "";
                    if (tableName.Contains("Inst_") && !tableName.Contains("Views"))
                    {
                        newName = tableName.Replace("Inst_", "");  //e.g) Inst_Columns ==> Columns
                        if (systemFamilies.Contains(newName)) { continue; }
                        categoryNames.Add(newName);
                    }
                }
                //Inst_Columns ==> Columns Instances
                string categoryName = "";
                foreach (string category in categoryNames)
                {
                    if (selectedTable.Contains(category))
                    {
                        categoryName = category;
                    }
                }

                form_InternalDB internalDBForm = new form_InternalDB(categoryName);
                internalDBForm.CategoryList = categoryNames;
                internalDBForm.SelectedFields = selectedFields;

                DialogResult dr = internalDBForm.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    selectedFields = internalDBForm.SelectedFields;
                    dbViewer.InternalFields = selectedFields;
                    dbViewer.UpdateDataTable();
                    dataSet = dbViewer.DataSet;

                    foreach (DataTable table in dataSet.Tables)
                    {
                        string tableName = table.TableName;
                        string newName = "";
                        if (tableName.Contains("Inst_"))
                        {
                            newName = tableName.Replace("Inst_", "") + " Instances";  //e.g) Inst_Columns ==> Columns Instances
                            tableDictionary.Remove(newName);
                            tableDictionary.Add(newName, table);
                        }
                    }
                    listBoxDataTable.SetSelected(selTableIndex, true);
                    internalDBForm.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set Revit hidden properties.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnView_Click(object sender, EventArgs e)
        {
            isVisibleInternal = true;
            dbViewer.UnhideInternalData(dataGridParam);
        }

        private void bttnHiden_Click(object sender, EventArgs e)
        {
            isVisibleInternal = false;
            dbViewer.HideInternalData(dataGridParam);
        }

        private void bttnString_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable selDataTable = tableDictionary[selectedTable];
                string newFieldName = "";

                form_ExpressionBuilder expBuilder = new form_ExpressionBuilder(newFieldName, selDataTable, CalculationType.String, database, dataGridParam);
                expBuilder.Text += " : String and Text concatenation";

                form_FieldName fieldNameForm = new form_FieldName(selDataTable, CalculationType.String);
                fieldNameForm.Text += " : Text";
                fieldNameForm.FieldDictionary = expBuilder.FieldDictionary;

                if (DialogResult.OK == fieldNameForm.ShowDialog())
                {
                    expBuilder.SelectedField = fieldNameForm.FieldName;
                    fieldNameForm.Close();
                }

                if (expBuilder.SelectedField != string.Empty)
                {
                    if (DialogResult.OK == expBuilder.ShowDialog())
                    {
                        expBuilder.Close();
                        RefreshDataTable(selDataTable);
                    }
                }
                else { expBuilder.Dispose(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create calculated fields using string concatenation.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnMath_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable selDataTable = tableDictionary[selectedTable];
                string newFieldName = "";

                form_ExpressionBuilder expBuilder = new form_ExpressionBuilder(newFieldName, selDataTable, CalculationType.Math, database,dataGridParam);
                expBuilder.DaoDB = database;
                expBuilder.Text += " : Math";

                form_FieldName fieldNameForm = new form_FieldName(selDataTable, CalculationType.Math);
                fieldNameForm.Text += " : Math";
                fieldNameForm.FieldDictionary = expBuilder.FieldDictionary;

                if (DialogResult.OK == fieldNameForm.ShowDialog())
                {
                    expBuilder.SelectedField = fieldNameForm.FieldName;
                    fieldNameForm.Close();
                }

                if (expBuilder.SelectedField != string.Empty)
                {
                    if (DialogResult.OK == expBuilder.ShowDialog())
                    {
                        doubleFields = expBuilder.DoubleFields;
                        expBuilder.Close();
                        database = expBuilder.DaoDB;
                        dbViewer.DaoDB = expBuilder.DaoDB;

                        foreach (string paramId in doubleFields.Keys)
                        {
                            if (dbViewer.DoubleFields.ContainsKey(selDataTable.TableName))
                            {
                                if (!dbViewer.DoubleFields[selDataTable.TableName].ContainsKey(paramId))
                                {
                                    dbViewer.DoubleFields[selDataTable.TableName].Add(paramId, "Dbl_"+paramId);
                                }
                            }
                        }
                        RefreshDataTable(selDataTable);
                    }
                }
                else { expBuilder.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create calculated fields using Math calculation.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnRemoveCal_Click(object sender, EventArgs e)
        {
            try
            {
                userTyped = false;
                DataTable selDataTable = tableDictionary[selectedTable];
                form_DeleteCalculated delCaculateForm = new form_DeleteCalculated(selDataTable, database);
                delCaculateForm.ParamIdMap = dbViewer.ParamIdMap;
                delCaculateForm.AddListViewItems();

                if (DialogResult.OK == delCaculateForm.ShowDialog())
                {
                    List<string> fieldsToRecover = new List<string>();
                    Dictionary<string/*filedName*/, LockType> lockTypes = new Dictionary<string, LockType>();
                    fieldsToRecover = delCaculateForm.FieldsToRecover;
                    foreach (string rField in fieldsToRecover)
                    {
                        selDataTable.Columns[rField].ReadOnly = false;
                        LockType oldLockType = RestoreLockType(selDataTable.TableName, rField);
                        lockTypes.Add(rField, oldLockType);
                        dbViewer.UpdateFomulatedLockType(selDataTable.TableName, rField, oldLockType);
                    }

                    if (fieldsToRecover.Count > 0)
                    {
                        foreach (DataGridViewRow row in dataGridParam.Rows)
                        {
                            foreach (string rField in fieldsToRecover)
                            {
                                if (null != dataGridParam.Columns[rField].Tag)
                                {
                                    string suffix = dataGridParam.Columns[rField].Tag.ToString();
                                    string value = row.Cells[rField].Value.ToString();
                                    row.Cells[rField].Value = value + suffix;
                                }
                            }
                        }
                    }
                    
                    delCaculateForm.Close();
                    RefreshDataTable(selDataTable);
                }
                userTyped = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to remove calculated fields.\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        private LockType RestoreLockType(string tableName, string paramName)
        {
            LockType lockType = LockType.None;
            string paramTable = "Rvt_Type_Parameters";
            string categoryName = "";
            if (tableName.Contains("Inst_")) { paramTable = "Rvt_Inst_Parameters"; categoryName = tableName.Substring(5); }
            else if (tableName.Contains("Type_")) { paramTable = "Rvt_Type_Parameters"; categoryName = tableName.Substring(5); }
            else if (tableName.Contains("Rvt_")) { paramTable = "Rvt_Type_Parameters"; categoryName = tableName.Substring(4); }

            string strSql = "SELECT IsEditable FROM " + paramTable + " WHERE CategoryName ='" + categoryName + "'  AND ParamName ='" + paramName + "'";
            Recordset recordset;
            recordset = database.OpenRecordset(strSql);

            if (recordset.RecordCount > 0)
            {
                string isEditable = recordset.Fields["IsEditable"].Value;

                if (isEditable == "True")
                {
                    lockType = LockType.Editable;
                }
            }
            recordset.Close();
            return lockType;
        }

        private void bttnExtView_Click(object sender, EventArgs e)
        {
            splitContainerParam.Panel2Collapsed = false;
            dbViewer.HideExtColumns(tableDictionary[selectedTable], dataGridParam);
        }

        private void bttnExtHide_Click(object sender, EventArgs e)
        {
            splitContainerParam.Panel2Collapsed = true;
            dbViewer.ShowExtColumns(tableDictionary[selectedTable], dataGridParam);
        }

        private void bttnSync_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = tableDictionary[selectedTable];

                if (changedValues.ContainsKey(table.TableName))
                {
                    toolStripStatusLabel1.Visible = false;
                    toolStripProgressBar.Visible = true;

                    syncRevit.ChangedValues = changedValues;
                    syncRevit.LockTypeFields = dbViewer.LockTypeFields;

                    if (syncRevit.SyncTableToRevit(table.TableName))
                    {
                        dataSet.Tables.Remove(table.TableName);
                        dataSet.Tables.Add(table);
                        changedValues.Remove(table.TableName);
                        BindDataSource(tableDictionary[selectedTable], dataGridParam, true);
                    }
                    toolStripStatusLabel1.Visible = true;
                    toolStripProgressBar.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to synchronize the selected table,"+selectedTable+"\n" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void  RefreshDataTable(DataTable selTable)
        {
            DataTable newTable = dbViewer.ReloadDataTable(selTable);
            tableDictionary.Remove(selectedTable);
            tableDictionary.Add(selectedTable, newTable);
            dataSet = dbViewer.DataSet;
            BindDataSource(tableDictionary[selectedTable], dataGridParam,true);
            ChangeTextColor(newTable.TableName);
        }

        private void bttnRefresh_Click(object sender, EventArgs e)
        {
            DataTable selDataTable = tableDictionary[selectedTable];
            RefreshDataTable(selDataTable);
        }

        private void bttnWarning_Click(object sender, EventArgs e)
        {
            if (warningTables.Count > 0)
            {
                string tableName = warningTables[0];
                if (warningRows.ContainsKey(tableName))
                {
                    if (warningRows[tableName].Count > 0)
                    {
                        for (int i = 0; i < listBoxDataTable.Items.Count; i++)
                        {
                            string itemName = listBoxDataTable.Items[i].ToString();
                            if (tableDictionary[itemName].TableName == tableName)
                            {
                                listBoxDataTable.SetSelected(i, true);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < listBoxDataTable.Items.Count; i++)
                    {
                        string itemName = listBoxDataTable.Items[i].ToString();
                        if (tableDictionary[itemName].TableName == tableName)
                        {
                            listBoxDataTable.SetSelected(i, true);
                            break;
                        }
                    }
                }

            }
        }

        #endregion

        #region Filter Tab
        
        private void tabControlMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                switch (tabControlMenu.SelectedTab.Name)
                {
                    case "tabPageHome":
                        panelSheet.Visible = false;
                        if (filterMode)
                        {
                            revitFilter.RemoveFilter();
                            revitFilter.AddSuffix();
                            DataTable curTable = revitFilter.CurDataTable;
                            dataSet.Tables.Remove(tableDictionary[selectedTable]);
                            dataSet.Tables.Add(curTable);

                            tableDictionary.Remove(selectedTable);
                            tableDictionary.Add(selectedTable, curTable);

                            splitContainer2.Panel1Collapsed = false;
                            BindDataSource(tableDictionary[selectedTable], dataGridParam, true);
                            ChangeTextColor(tableDictionary[selectedTable].TableName);
                            filterMode = false;
                        }
                        break;

                    case "tabPageFilter":
                        panelSheet.Visible = false;
                        splitContainerParam.Panel2Collapsed = true;
                        dbViewer.ShowExtColumns(tableDictionary[selectedTable], dataGridParam);
                        splitContainer2.Panel1Collapsed = true;
                        revitFilter.CurDataTable = tableDictionary[selectedTable];
                        revitFilter.RemoveSuffix();
                        revitFilter.BindDataTable();
                        dataGridParam.SelectionMode = DataGridViewSelectionMode.CellSelect;
                        filterMode = true;
                        break;

                    case "tabPageSheet":
                        splitContainer2.Panel1Collapsed = true;
                        panelSheet.Visible = true;
                        
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to invoke tabControlMenu_SelectedIndexChanged" + ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnHome_Click(object sender, EventArgs e)
        {
            tabControlMenu.SelectedIndex = 0;
        }

        private void bttnFilterView_Click(object sender, EventArgs e)
        {
            tabControlMenu.SelectedIndex = 1;
        }

        private void bttnFind_Click(object sender, EventArgs e)
        {
            string keywords = textBoxKeywords.Text;
            foreach (DataGridViewRow row in dataGridParam.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value.ToString().Contains(keywords))
                    {
                        cell.Selected = true;
                    }
                    else
                    {
                        cell.Selected = false;
                    }
                }
            }
        }
        #endregion

        private void form_Viewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (warningMode)
                {
                    DialogResult dr=DialogResult.None;
                    List<string> keyLists = new List<string>();
                    keyLists = tableDictionary.Keys.ToList();//name for display in the list box

                    foreach (string itemName in keyLists)
                    {
                        DataTable dataTable = tableDictionary[itemName];
                        if (changedValues.ContainsKey(dataTable.TableName))
                        {
                            if (changedValues[dataTable.TableName].Count > 0)
                            {
                                if (dr==DialogResult.None)
                                {
                                    dr = MessageBox.Show("Would you like to update changed values before closing?", "Synchronization", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                }
                                if (dr == DialogResult.OK)
                                {
                                    toolStripStatusLabel1.Visible = false;
                                    toolStripProgressBar.Visible = true;

                                    syncRevit.ChangedValues = changedValues;
                                    syncRevit.LockTypeFields = dbViewer.LockTypeFields;

                                    if (syncRevit.SyncTableToRevit(dataTable.TableName))
                                    {
                                        dataSet.Tables.Remove(dataTable.TableName);
                                        dataSet.Tables.Add(dataTable);
                                        changedValues.Remove(dataTable.TableName);
                                        BindDataSource(tableDictionary[itemName], dataGridParam, true);
                                    }
                                    toolStripStatusLabel1.Visible = true;
                                    toolStripProgressBar.Visible = false;
                                }
                            }
                        }
                    }
                    if (dr == DialogResult.OK)
                    {
                        MessageBox.Show("Successfully Synchronized. All Revit elements are up to date.", "Synchronization", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to synchronize before closing the Data Editor.\n"+ex.Message, "Error: Data Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
