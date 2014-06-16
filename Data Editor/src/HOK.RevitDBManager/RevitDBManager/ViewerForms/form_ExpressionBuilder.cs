using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Access.Dao;

namespace RevitDBManager.ViewerForms
{
    public enum CalculationType { String, Math };

    public partial class form_ExpressionBuilder : Form
    {
        private string selField = "";
        private DataTable selDataTable;
        private CalculationType calType;
        private string tableName = "";
        private string paramTable;
        private Database daoDB;
        private DataGridView mainGridView;
        private List<string> expressionList = new List<string>();
        private List<string> internalExpression = new List<string>();
        private string previewVal = "";
        private string strDataType = "System.String";
        //private string dblDataType = "System.Double";
        private bool setDecimal = false;
        private bool roundUp = false;
        private bool roundDown = false;
        private int decimalPlace = 0;
        private object mMissing = System.Reflection.Missing.Value;

        private string[] internalField = new string[] { "FacingFlipped", "FromRoom", "HandFlipped", "Host", "IsSlantedColumn", "Mirrored", "Room", "ToRoom" };
        private string[] systemField = new string[] { "InstanceID", "TypeID", "FamilyID", "FamilyName", "FamilyType", "ID"};
        private Dictionary<string/*paramName*/, FieldProperties> fieldDictionary = new Dictionary<string, FieldProperties>();
        private Dictionary<string/*paramId*/, string/*paramName*/> doubleFields = new Dictionary<string/*paramId*/, string/*paramName*/>();

        public string SelectedField { get { return selField; } set { selField = value; } }
        public DataTable DataTable { get { return selDataTable; } set { selDataTable = value; } }
        public Database DaoDB { get { return daoDB; } set { daoDB = value; } }
        public Dictionary<string, FieldProperties> FieldDictionary { get { return fieldDictionary; } set { fieldDictionary = value; } }
        public bool SetDecimal { get { return setDecimal; } set { setDecimal = value; } }
        public int DecimalPlace { get { return decimalPlace; } set { decimalPlace = value; } }
        public Dictionary<string, string> DoubleFields { get { return doubleFields; } set { doubleFields = value; } }

        public form_ExpressionBuilder(string newFieldName, DataTable dataTable, CalculationType calculation, Database database, DataGridView datagridView)
        {
            selField = newFieldName;
            selDataTable = dataTable;
            calType = calculation;
            daoDB = database;
            tableName = dataTable.TableName;
            mainGridView = datagridView;

            if (tableName.Contains("Inst_")) { paramTable = "Rvt_Inst_Parameters"; }
            if (tableName.Contains("Type_")) { paramTable = "Rvt_Type_Parameters"; }
            if (tableName.Contains("Rvt_")) { paramTable = "Rvt_Type_Parameters"; } //Rooms and Spaces
            CollectFieldData();
            InitializeComponent();
        }

        private void form_ExpressionBuilder_Load(object sender, EventArgs e)
        {
            linkCalCol.Text = selField;
            lblCalField.Text = selField;

            if (calType == CalculationType.String)
            {
                bttnAddString.Enabled = true;
                bttnSpace.Enabled = true;
                radioButtonDecimal.Visible = false;
                textBoxDecimal.Visible = false;
                radioButtonRoundUp.Visible = false;
                radioButtonRoundDown.Visible = false;
            }
            if (calType == CalculationType.Math)
            {
                bttnAdd.Enabled = true;
                bttnSubtract.Enabled = true;
                bttnMultiply.Enabled = true;
                bttnDivide.Enabled = true;
                bttnPower.Enabled = true;
                bttnLeft.Enabled = true;
                bttnRight.Enabled = true;
            }

            DisplayFieldListBox();

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(this.bttnAddString, "Concatenation of string");
            toolTip.SetToolTip(this.bttnSpace, "Add space between strings");
            toolTip.SetToolTip(this.bttnAdd, "Sum two numbers");
            toolTip.SetToolTip(this.bttnSubtract, "Subtraction or indication of the negative value");
            toolTip.SetToolTip(this.bttnMultiply, "Multiply two numbers");
            toolTip.SetToolTip(this.bttnPower, "Raise a number to the power of an exponent.");
            toolTip.SetToolTip(this.bttnLeft, "Left Parenthesis");
            toolTip.SetToolTip(this.bttnRight, "Right Parenthesis");
        }

        private void CollectFieldData()
        {
            try
            {
                string categoryName = tableName.Substring(5); if (tableName.Contains("Rvt_")) { categoryName = tableName.Substring(4); } //rooms and space
                string strSql = "SELECT * FROM " + paramTable + " WHERE CategoryName ='" + categoryName + "' AND IsVisible = 'True'";

                Recordset recordset = daoDB.OpenRecordset(strSql);
                while (!recordset.EOF)
                {
                    string paramName = recordset.Fields["ParamName"].Value;
                    string paramId = recordset.Fields["ParamID"].Value;
                    string paramFormat = recordset.Fields["ParamFormat"].Value;
                    FieldProperties fp = new FieldProperties(paramName, paramId, paramFormat);
                    //to set values for preview
                    fp.StrValue = selDataTable.Rows[0][paramName].ToString();
                    fp.ReadOnly = selDataTable.Columns[paramName].ReadOnly;

                    if (!fieldDictionary.ContainsKey(paramName))
                    {
                        fieldDictionary.Add(paramName, fp);
                    }

                    recordset.MoveNext();
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect field data.\n" + ex.Message, "Expression Builder Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkCalCol_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            form_FieldName fieldNameForm = new form_FieldName(selDataTable,calType);
            fieldNameForm.Text = "Rename Field Name";
            fieldNameForm.FieldName = linkCalCol.Text;
            fieldNameForm.FieldDictionary = fieldDictionary;
            if (DialogResult.OK == fieldNameForm.ShowDialog())
            {
                selField = fieldNameForm.FieldName;
                linkCalCol.Text = selField;
                lblCalField.Text = selField;
                DisplayFieldListBox();
                fieldNameForm.Close();
            }
        }

        private void DisplayFieldListBox()
        {
            listBoxFields.Items.Clear();
            if (fieldDictionary.Count > 0)
            {
                foreach (string key in fieldDictionary.Keys)
                {
                    if (key == selField) { continue; }
                    FieldProperties fp = fieldDictionary[key];
                    if (calType == CalculationType.String)
                    {
                        if (fp.ParamFormat == "ElementId") { continue; }
                        listBoxFields.Items.Add(fp.ParamName);
                    }
                    if (calType == CalculationType.Math && fp.ParamFormat == "Double")
                    {
                        listBoxFields.Items.Add(fp.ParamName);
                    }
                }
                listBoxFields.Sorted = true;
            }
        }

        private void DisplayExpression()
        {
            string expression = "";
            for (int i = 0; i < expressionList.Count; i++)
            {
                expression += expressionList[i];
            }
            richTexExpression.Text = expression; //expression for display with param names
        }

        private void DisplayPreview()
        {
            previewVal = "";
            if (calType == CalculationType.Math)
            {
                for (int i = 0; i < expressionList.Count; i++)
                {
                    string expression = expressionList[i];
                    if (expression.Contains('['))
                    {
                        expression = expression.Replace("[", "");
                        expression = expression.Replace("]", "");
                        if (fieldDictionary.ContainsKey(expression))
                        {
                            previewVal += fieldDictionary[expression].StrValue + " ";
                        }
                    }
                    else
                    {
                        previewVal += expression + " ";
                    }
                }
            }
            if(calType==CalculationType.String)
            {
                for (int i = 0; i < expressionList.Count; i++)
                {
                    string expression = expressionList[i];
                    if (expression.Contains('['))
                    {
                        expression=expression.Replace("[", "");
                        expression=expression.Replace("]", "");
                        if (fieldDictionary.ContainsKey(expression))
                        {
                            previewVal += fieldDictionary[expression].StrValue;
                        }
                    }
                    else if(expression.Contains('\"'))
                    {
                        expression = expression.Replace("\"", "");
                        expression = expression.Replace("\"", "");
                        previewVal += expression;
                    }
                }
            }
            lblPreview.Text=previewVal;
        }

        private bool AddFields()
        {
            bool result = false;
            try
            {
                string strExpression = "";
               
                foreach (string expression in internalExpression)
                {
                    strExpression += expression;
                }
                TableDef tableDef = null;
                if (strExpression.Length > 0)
                {
                    foreach (TableDef table in daoDB.TableDefs)
                    {
                        if (table.Name == tableName)
                        {
                            tableDef = table;
                            break;
                        }
                    }
                    if (null != tableDef)
                    {
                        bool exist = false;
                        if (fieldDictionary.ContainsKey(selField)) { exist = true; }
                        if (calType == CalculationType.Math)
                        {
                            //create double fields: "Dbl_[paraimId]"
                            CreateDoubleFields(tableDef);
                            strExpression = GetDoubleFieldExpression(strExpression);
                        }
                        if (ValidateExpression(tableDef, strExpression))
                        {
                            CreateField(exist, tableDef, strExpression);
                            daoDB.TableDefs.Refresh();
                            result = true;
                        }
                    }
                    
                }
                else
                {
                    MessageBox.Show("Please enter any expression elements.", "Expression Builder Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("The expression you entered contains invalid syntax.\n"+ex.Message, "Expression Builder Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private void CreateDoubleFields(TableDef tableDef)
        {
            try
            {
                List<string> dblFields = new List<string>();
                foreach (Field field in tableDef.Fields)
                {
                    if (field.Name.Contains("Dbl_"))
                    {
                        dblFields.Add(field.Name); //existing double fields
                    }
                }

                foreach (string paramId in doubleFields.Keys)
                {
                    string fieldName = "Dbl_" + paramId;
                    if (!dblFields.Contains(fieldName))
                    {
                        Field field = tableDef.CreateField(fieldName, DataTypeEnum.dbDouble);
                        tableDef.Fields.Append(field);
                    }
                }

                string keyField = mainGridView.Columns[0].Name;
                foreach (DataGridViewRow row in mainGridView.Rows)
                {
                    string keyValue = row.Cells[keyField].Value.ToString();
                    foreach (string paramId in doubleFields.Keys)
                    {
                        string paramName = doubleFields[paramId];
                        string paramValue = row.Cells[paramName].Value.ToString();
                        if (null != row.Cells[paramName].Tag)
                        {
                            paramValue = row.Cells[paramName].Tag.ToString();
                        }

                        string fieldName = "Dbl_" + paramId;
                        string queryStr = "UPDATE [" + tableName + "] SET [" + fieldName + "] = '" + paramValue + "' WHERE [" + keyField + "] = '" + keyValue + "'";
                        try { daoDB.Execute(queryStr, RecordsetOptionEnum.dbFailOnError); }
                        catch { continue; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create double fields.\n"+ex.Message, "Expression Builder Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string GetDoubleFieldExpression(string expression)
        {
            string strExpression = expression;
            foreach (string paramId in doubleFields.Keys)
            {
                strExpression = strExpression.Replace(paramId, "Dbl_" + paramId);
            }
            return strExpression;
        }

        private void CreateField(bool exist, TableDef tableDef, string strExpression)
        {
            try
            {
                if (exist)
                {
                    string paramId = fieldDictionary[selField].ParamId;
                    tableDef.Fields.Delete(paramId);
                    
                    if (calType == CalculationType.String)
                    {
                        Field2 field2 = tableDef.CreateField(paramId, DataTypeEnum.dbText) as Field2;
                        field2.Expression = strExpression;
                        field2.AllowZeroLength = true;
                        tableDef.Fields.Append(field2);
                    }
                    else if (calType == CalculationType.Math)
                    {
                        Field2 field2 = tableDef.CreateField(paramId, DataTypeEnum.dbDouble) as Field2;
                        field2.Expression = strExpression;
                        tableDef.Fields.Append(field2);

                        if (setDecimal)
                        {
                            Property property = field2.CreateProperty("DecimalPlaces", DataTypeEnum.dbByte, decimalPlace);
                            field2.Properties.Append(property);

                            property = field2.CreateProperty("Format", DataTypeEnum.dbText, "Fixed");
                            field2.Properties.Append(property);

                            field2.Properties.Refresh();
                        }
                    }
                }
                else// not exist in the database
                {
                    if (calType == CalculationType.String)
                    {
                        Field2 field2 = tableDef.CreateField(selField, DataTypeEnum.dbText) as Field2;
                        field2.Expression = strExpression;
                        field2.AllowZeroLength = true;
                        tableDef.Fields.Append(field2);
                    }
                    else if (calType == CalculationType.Math)
                    {
                        Field2 field2 = tableDef.CreateField(selField, DataTypeEnum.dbDouble) as Field2;
                        if (roundUp)
                        {
                            string oldExpression = strExpression;
                            strExpression = string.Format("IIf({0}>Int({0}),Int({0})+1,Int({0}))", oldExpression);
                        }
                        else if (roundDown)
                        {
                            strExpression = "Int(" + strExpression + ")";
                        }
                        field2.Expression = strExpression;
                        tableDef.Fields.Append(field2);

                        if (setDecimal)
                        {
                            Property property = field2.CreateProperty("DecimalPlaces", DataTypeEnum.dbByte, decimalPlace);
                            field2.Properties.Append(property);

                            property = field2.CreateProperty("Format", DataTypeEnum.dbText, "Fixed");
                            field2.Properties.Append(property);

                            field2.Properties.Refresh();
                        }
                    }

                    DataColumn column = new DataColumn();
                    column.DataType = System.Type.GetType(strDataType);
                    column.ColumnName = selField;
                    selDataTable.Columns.Add(column);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a calculated field.\n"+ex.Message, "Expression Builder Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateExpression(TableDef tableDef, string expression)
        {
            try
            {
                Field2 field2 = tableDef.CreateField("temp", DataTypeEnum.dbText) as Field2;
                field2.Expression = expression;
                field2.AllowZeroLength = true;
                tableDef.Fields.Append(field2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The expression you entered contains invalid syntax.\n" + ex.Message, "Expression Builder Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            tableDef.Fields.Delete("temp");
            return true;
        }

        private void bttnAddField_Click(object sender, EventArgs e)
        {
            string selectedField = listBoxFields.SelectedItem.ToString();
            expressionList.Add("[" + selectedField + "]");
            internalExpression.Add("[" + fieldDictionary[selectedField].ParamId + "]");
            if (calType == CalculationType.Math)
            {
                doubleFields.Add(fieldDictionary[selectedField].ParamId, selectedField);
            }
            DisplayExpression();
        }

        private void bttnAddConst_Click(object sender, EventArgs e)
        {
            string constant = textBoxConst.Text;
            if (calType == CalculationType.String)
            {
                if (constant.Length > 0)
                {
                    expressionList.Add("\""+constant+"\"");
                    internalExpression.Add("\"" + constant + "\"");
                }
            }
            if (calType == CalculationType.Math)
            {
                double dblConst;
                if (constant.Length > 0 && double.TryParse(constant, out dblConst))
                {
                    expressionList.Add(constant);
                    internalExpression.Add(constant);
                }
                else
                {
                    MessageBox.Show("Please enter an appropriate number.", "Not matched data type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            DisplayExpression();
            textBoxConst.Text = "";
        }

        private void bttnUndo_Click(object sender, EventArgs e)
        {
            expressionList.RemoveAt(expressionList.Count - 1);
            internalExpression.RemoveAt(internalExpression.Count - 1);
            if (calType == CalculationType.Math)
            {
                string lastField = internalExpression[internalExpression.Count - 1];
                lastField = lastField.Replace("[", "");
                lastField = lastField.Replace("]", "");
                if (doubleFields.ContainsKey(lastField))
                {
                    doubleFields.Remove(lastField);
                }
            }
            DisplayExpression();
        }

        #region Operator Buttons
        private void bttnAddString_Click(object sender, EventArgs e)
        {
            expressionList.Add("+");
            internalExpression.Add("+");
            DisplayExpression();
        }

        private void bttnSpace_Click(object sender, EventArgs e)
        {
            expressionList.Add("\" \"");
            internalExpression.Add("\" \"");
            DisplayExpression();
        }

        private void bttnAdd_Click(object sender, EventArgs e)
        {
            expressionList.Add("+");
            internalExpression.Add("+");
            DisplayExpression();
        }

        private void bttnSubtract_Click(object sender, EventArgs e)
        {
            expressionList.Add("-");
            internalExpression.Add("-");
            DisplayExpression();
        }

        private void bttnMultiply_Click(object sender, EventArgs e)
        {
            expressionList.Add("*");
            internalExpression.Add("*");
            DisplayExpression();
        }

        private void bttnDivide_Click(object sender, EventArgs e)
        {
            expressionList.Add("/");
            internalExpression.Add("/");
            DisplayExpression();
        }

        private void bttnPower_Click(object sender, EventArgs e)
        {
            expressionList.Add("^");
            internalExpression.Add("^");
            DisplayExpression();
        }

        private void bttnLeft_Click(object sender, EventArgs e)
        {
            expressionList.Add("(");
            internalExpression.Add("(");
            DisplayExpression();
        }

        private void bttnRight_Click(object sender, EventArgs e)
        {
            expressionList.Add(")");
            internalExpression.Add(")");
            DisplayExpression();
        }
        #endregion

        private void bttnOK_Click(object sender, EventArgs e)
        {
            if (AddFields()) 
            { 
                this.DialogResult = DialogResult.OK; 
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void richTexExpression_TextChanged(object sender, EventArgs e)
        {
            DisplayPreview();
        }

        private void textBoxDecimal_TextChanged(object sender, EventArgs e)
        {
            int place = 0;
            if (int.TryParse(textBoxDecimal.Text, out place))
            {
                if ((place > -1) && (place < 6))
                {
                    decimalPlace = place;
                }
                else
                {
                    MessageBox.Show("Please enter with numbers from 0 to 5. ", "Out of Range");
                }
            }
            else
            {
                MessageBox.Show("Please enter with numbers from 0 to 5.", "Invalid Format");
            }
        }

        private void radioButtonDecimal_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDecimal.Checked)
            {
                textBoxDecimal.Enabled = true;
                setDecimal = true;
                roundUp = false;
                roundDown = false;
            }
        }

        private void radioButtonRoundUp_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRoundUp.Checked)
            {
                roundUp = true;
                textBoxDecimal.Enabled = false;
                setDecimal = false;
                roundDown = false;
            }
        }

        private void radioButtonRoundDown_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRoundDown.Checked)
            {
                roundDown = true;
                roundUp = false;
                textBoxDecimal.Enabled = false;
                setDecimal = false;
            }
        }

        

    }

    public class FieldProperties
    {
        private string paramName = "";
        private string paramId = "";
        private string paramFormat = "";
        private string strVal = "";

        public FieldProperties(string parameterName, string parameterId, string parameterFormat)
        {
            paramName = parameterName;
            paramId = parameterId;
            paramFormat = parameterFormat;
        }

        public string ParamName { get { return paramName; } set { paramName = value; } }
        public string ParamId { get { return paramId; } set { paramId = value; } }
        public string ParamFormat { get { return paramFormat; } set { paramFormat = value; } }
        public string ParamType { get; set; }
        public string StrValue { get { return strVal; } set { if (value.Length > 0) { strVal = value; } else { strVal = "null"; } } }
        public string DisplayUnitType { get; set; }
        public bool ReadOnly { get; set; }
    }
}
