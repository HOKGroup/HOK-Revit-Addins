using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using RevitDBManager.HelperClasses;

namespace RevitDBManager.ViewerClasses
{
    class RevitDataFilter
    {
        private DataTable m_table;
        private DataGridView m_gridView;
        private DataView m_dataView;
        private ComboBox cmb_columnName;
        private ComboBox cmb_condition;
        private ComboBox cmb_value;
        private Button bttn_filter;
        private Button bttn_clear;
        private UnitConverter unitConverter;

        private BindingSource bindingSource;
        private Dictionary<string/*condition*/, string/*operator*/> conditionsDic = new Dictionary<string, string>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, string/*DUT*/>> dutDictionary = new Dictionary<string, Dictionary<string, string>>();

        private string selectedColumn = "";
        private string selectedCondition = "";
        private string selectedValue = "";
        private string selectedColumnType = "";
        private string filterExpression = "";

        public DataTable CurDataTable { get { return m_table; } set { m_table = value; } }
        public DataGridView DataGridViewParam { get { return m_gridView; } set { m_gridView = value; } }
        public ComboBox ComboBoxColumnName { get { return cmb_columnName; } set { cmb_columnName = value; } }
        public ComboBox ComboBoxCondition { get { return cmb_condition; } set { cmb_condition = value; } }
        public ComboBox ComboBoxValue { get { return cmb_value; } set { cmb_value = value; } }
        public Dictionary<string, Dictionary<string, string>> DUTDictionary { get { return dutDictionary; } set { dutDictionary = value; } }

        public RevitDataFilter(DataGridView datagridview, ComboBox cmbColumn, ComboBox cmbCondition, ComboBox cmbValue, Button bttnFilter, Button bttnClear)
        {
            m_gridView = datagridview;
            cmb_columnName = cmbColumn;
            cmb_condition = cmbCondition;
            cmb_value = cmbValue;
            bttn_filter = bttnFilter;
            bttn_clear = bttnClear;
            unitConverter = new UnitConverter();

            cmb_columnName.SelectedIndexChanged += new EventHandler(ComboBoxColumnName_SelectedIndexChanged);
            cmb_condition.SelectedIndexChanged += new EventHandler(ComboBoxCondition_SelectedIndexChanged);
            cmb_value.SelectedIndexChanged += new EventHandler(ComboBoxValue_SelectedIndexChanged);
            bttn_filter.Click += new EventHandler(BttnFilter_Click);
            bttn_clear.Click += new EventHandler(BttnClear_Click);
        }

        private void AddComboBoxConditions()
        {
            selectedColumnType = m_table.Columns[selectedColumn].DataType.Name;

            if (dutDictionary.ContainsKey(m_table.TableName))
            {
                if (dutDictionary[m_table.TableName].ContainsKey(selectedColumn))
                {
                    selectedColumnType = "Double";
                }
            }

            cmb_condition.Items.Clear();
            if (selectedColumnType.Contains("String"))
            {
                cmb_condition.Items.Add("Equals");
                cmb_condition.Items.Add("Does Not Equals");
                cmb_condition.Items.Add("Begins With");
                cmb_condition.Items.Add("Does Not Begin With");
                cmb_condition.Items.Add("Ends With");
                cmb_condition.Items.Add("Does Not End With");
                cmb_condition.Items.Add("Contains");
                cmb_condition.Items.Add("Does Not Contain");
            }
            else if (selectedColumnType.Contains("Double") || selectedColumnType.Contains("Int"))
            {
                cmb_condition.Items.Add("Equals");
                cmb_condition.Items.Add("Does Not Equals");
                cmb_condition.Items.Add("Is Greater Than");
                cmb_condition.Items.Add("Is Greater Than Or Equal To");
                cmb_condition.Items.Add("Is Less Than");
                cmb_condition.Items.Add("Is Less Than Or Equal To");
            }
            else if (selectedColumnType.Contains("Boolean"))
            {
                cmb_condition.Items.Add("Equals");
            }
        }

        public void AddSuffix()
        {
            foreach (DataGridViewColumn column in m_gridView.Columns)
            {
                if (null != column.Tag)
                {
                    string suffix = column.Tag.ToString();
                    DataColumn dtcolumn=m_table.Columns[column.Name];
                    bool isReadOnly=dtcolumn.ReadOnly;
                    dtcolumn.ReadOnly = false;
                    if (suffix == "FFI" || suffix == "FI")
                    {
                        foreach (DataRow row in m_table.Rows)
                        {
                            string value = unitConverter.ConvertDecimalToFractional(row[dtcolumn].ToString(), suffix);
                            row[dtcolumn] = value;
                        }
                    }
                    else if (suffix == "NONE")
                    {
                        continue;
                    }
                    else
                    {
                        foreach (DataRow row in m_table.Rows)
                        {
                            string value = row[dtcolumn].ToString() + suffix;
                            row[dtcolumn] = value;
                        }
                    }

                    dtcolumn.ReadOnly = isReadOnly;
                    dtcolumn.DataType = System.Type.GetType("System.String");
                }
            }
        }

        public void RemoveSuffix()
        {
            Dictionary<string/*paramName*/, string/*suffix*/> suffixDictionary = new Dictionary<string, string>();
            foreach (DataGridViewColumn column in m_gridView.Columns)
            {
                if (null != column.Tag)
                {
                    string suffix = column.Tag.ToString();
                    if (!suffixDictionary.ContainsKey(column.Name))
                    {
                        suffixDictionary.Add(column.Name, suffix);
                    }
                }
            }

            for (int i = 0; i < m_table.Columns.Count; i++)
            {
                DataColumn column = m_table.Columns[i];
                if (suffixDictionary.ContainsKey(column.ColumnName))
                {
                    bool isReadOnly = column.ReadOnly;
                    column.ReadOnly = false;
                    string suffix = suffixDictionary[column.ColumnName];
   
                    foreach (DataRow row in m_table.Rows)
                    {
                        string value = row[column].ToString();
                        row[column] = unitConverter.GetDoubleValue(value, suffix).ToString();
                    }
                    column.ReadOnly = isReadOnly;
                }
            }
        }

        public void BindDataTable()
        {
            m_dataView = new DataView(m_table);
            bindingSource = new BindingSource();
            bindingSource.DataSource = m_dataView;
            m_gridView.DataSource = bindingSource;

            cmb_columnName.Items.Clear();
            foreach (DataColumn colName in m_table.Columns)
            {
                string datatype = colName.DataType.Name;
                cmb_columnName.Items.Add(colName.ColumnName);
            }
        }

        public void RemoveFilter()
        {
            bindingSource.RemoveFilter();

            cmb_columnName.ResetText();
            cmb_condition.ResetText();
            cmb_value.ResetText();

            cmb_columnName.Items.Clear();
            cmb_condition.Items.Clear();
            cmb_value.Items.Clear();

            BindingSource bs = (BindingSource)m_gridView.DataSource;
            DataView dataView = (DataView)bs.DataSource;
            m_table = dataView.Table;
        }

        private void ComboBoxColumnName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = cmb_columnName.SelectedIndex;
            selectedColumn = cmb_columnName.Items[index].ToString();

            cmb_condition.ResetText();
            cmb_value.ResetText();
            AddComboBoxConditions();

            cmb_value.Items.Clear();
            foreach (DataRow row in m_table.Rows)
            {
                string tempVal = row[selectedColumn].ToString();
                if (!cmb_value.Items.Contains(tempVal))
                {
                    cmb_value.Items.Add(tempVal);
                }
            }
        }

        private void ComboBoxCondition_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCondition = cmb_condition.SelectedItem.ToString();
        }

        private void ComboBoxValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (null != cmb_value.SelectedItem)
            {
                selectedValue = cmb_value.SelectedItem.ToString();
            }
        }

        private void BttnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                if (null == cmb_value.SelectedItem && cmb_value.Text != string.Empty)
                {
                    selectedValue = cmb_value.Text;
                }

                if (null != cmb_columnName.SelectedItem && null != cmb_condition.SelectedItem && selectedValue != string.Empty)
                {
                    switch (selectedCondition)
                    {
                        case "Equals":
                            filterExpression = "[" + selectedColumn + "] = " + ConvertValueString("Equals");
                            break;
                        case "Does Not Equals":
                            filterExpression = "[" + selectedColumn + "] <> " + ConvertValueString("Does Not Equals");
                            break;
                        case "Is Greater Than":
                            filterExpression = "[" + selectedColumn + "] > " + ConvertValueString("Is Greater Than");
                            break;
                        case "Is Greater Than Or Equal To":
                            filterExpression = "[" + selectedColumn + "] >= " + ConvertValueString("Is Greater Than Or Equal To");
                            break;
                        case "Is Less Than":
                            filterExpression = "[" + selectedColumn + "] < " + ConvertValueString("Is Less Than");
                            break;
                        case "Is Less Than Or Equal To":
                            filterExpression = "[" + selectedColumn + "] <= " + ConvertValueString("Is Less Than Or Equal To");
                            break;
                        case "Begins With":
                            filterExpression = "[" + selectedColumn + "] LIKE " + ConvertValueString("Begins With");
                            break;
                        case "Does Not Begin With":
                            filterExpression = "[" + selectedColumn + "] NOT LIKE " + ConvertValueString("Does Not Begin With");
                            break;
                        case "Ends With":
                            filterExpression = "[" + selectedColumn + "] LIKE " + ConvertValueString("Ends With");
                            break;
                        case "Does Not End With":
                            filterExpression = "[" + selectedColumn + "] NOT LIKE " + ConvertValueString("Does Not End With");
                            break;
                        case "Contains":
                            filterExpression = "[" + selectedColumn + "] LIKE " + ConvertValueString("Contains");
                            break;
                        case "Does Not Contain":
                            filterExpression = "[" + selectedColumn + "] NOT LIKE " + ConvertValueString("Does Not Contain");
                            break;
                    }
                    bindingSource.Filter = filterExpression;
                }
                else
                {
                    MessageBox.Show("Please select a column name, a condition and a value to view a subset of the data."); 
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to filter data with the following filter expression\n"+filterExpression+"\n"+ex.Message, 
                    "RevitDataFilter Error:", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }
        }

        private string ConvertValueString(string conditionType)
        {
            string convertedVal = "";

            convertedVal = selectedValue;

            if (selectedColumnType.Contains("String"))
            {
                if (conditionType.Contains("Begin"))
                {
                    convertedVal = "'" + selectedValue + "*'";
                }
                else if (conditionType.Contains("End"))
                {
                    convertedVal = "'*" + selectedValue + "'";
                }
                else if (conditionType.Contains("Contain"))
                {
                    convertedVal = "'*" + selectedValue + "*'";
                }
                else if (conditionType.Contains("Equal"))
                {
                    convertedVal = "'" + selectedValue + "'";
                }
            }
            else if(selectedColumnType.Contains("Double"))
            {
                if (!selectedValue.Contains("."))
                {
                    convertedVal = selectedValue + ".00";//to be regarded as double type.
                }
            }

            return convertedVal;
        }

        private void BttnClear_Click(object sender, EventArgs e)
        {
            bindingSource.RemoveFilter();
            cmb_columnName.ResetText();
            cmb_condition.ResetText();
            cmb_value.ResetText();
            cmb_condition.Items.Clear();
            cmb_value.Items.Clear();
        }

        public void SetSelectedColumn(string selColumn)
        {
            int index = 0;
            foreach (string item in cmb_columnName.Items)
            {
                if (item.ToString() == selColumn)
                {
                    index = cmb_columnName.Items.IndexOf(item);
                    break;
                }
            }
            cmb_columnName.SelectedIndex = index;
        }
    }
}
