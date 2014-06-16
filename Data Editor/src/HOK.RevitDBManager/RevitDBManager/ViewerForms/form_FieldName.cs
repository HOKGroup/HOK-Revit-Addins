using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RevitDBManager.ViewerForms
{
    public partial class form_FieldName : Form
    {
        private string fieldName = "";
        private DataTable dataTable;
        private CalculationType calType;
        private List<string> fieldNames = new List<string>();
        private Dictionary<string, FieldProperties> fieldDictionary = new Dictionary<string, FieldProperties>();

        public string FieldName { get { return fieldName; } set { fieldName = value; } }
        public Dictionary<string, FieldProperties> FieldDictionary { get { return fieldDictionary; } set { fieldDictionary = value; } }

        public form_FieldName(DataTable table, CalculationType calculationType)
        {
            dataTable = table;
            calType = calculationType;
            InitializeComponent();
        }

        private void form_FieldName_Load(object sender, EventArgs e)
        {
            textBoxField.Text = fieldName;

            if (fieldDictionary.Count > 0)
            {
                foreach (string field in fieldDictionary.Keys)
                {
                    FieldProperties fp = fieldDictionary[field];
                    if (fp.ReadOnly) { continue; }
                    if (fp.ParamFormat == "ElementId") { continue; }

                    if (calType == CalculationType.String)
                    {
                        comboBoxFields.Items.Add(field);
                    }
                    else if (calType == CalculationType.Math)
                    {
                        if (fp.ParamFormat == "Double")
                        {
                            comboBoxFields.Items.Add(field);
                        }
                    }
                }
            }
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            AddNewName();
        }

        private void AddNewName()
        {
            fieldName = textBoxField.Text;
            if (fieldName.Length > 0)
            {
                if (ValidateFieldName(fieldName))
                {
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("The field name you entered already existed in the table.", "Field Name Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please type a field name.\n The name cannot be an empty string.", "Field Name Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateFieldName(string name)
        {
            bool result = true;
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.ColumnName == name)
                {
                    result = false;
                }
            }
            return result;
        }

        private void textBoxField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddNewName();
            }
        }

        private void bttnApply_Click(object sender, EventArgs e)
        {
            if (null != comboBoxFields.SelectedItem)
            {
                fieldName = comboBoxFields.SelectedItem.ToString();
                if (fieldName.Length > 0)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageBox.Show("Please select a field name.", "Field Name Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
   
        }

        
    }
}
