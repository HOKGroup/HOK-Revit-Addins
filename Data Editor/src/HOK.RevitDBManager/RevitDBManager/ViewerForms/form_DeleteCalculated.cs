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

namespace RevitDBManager.ViewerForms
{
    public partial class form_DeleteCalculated : Form
    {
        private Database daoDB;
        private DataTable selDataTable;
        private TableDef selTable = null;
        private List<string> calculatedFields = new List<string>();
        private bool removed = false;
        private List<string> fieldsToRecover = new List<string>(); //only exisitng fields
        private Dictionary<string/*paramId*/, string/*paramName*/> paramIdMap = new Dictionary<string, string>();

        public List<string> FieldsToRecover { get { return fieldsToRecover; } set { fieldsToRecover = value; } }
        public Dictionary<string, string> ParamIdMap { get { return paramIdMap; } set { paramIdMap = value; } }

        public form_DeleteCalculated(DataTable dataTable, Database database)
        {
            selDataTable = dataTable;
            daoDB = database;
            InitializeComponent();
        }

        public void AddListViewItems()
        {
            try
            {
                Recordset recordset = daoDB.OpenRecordset(selDataTable.TableName);
                if (recordset.RecordCount > 0)
                {
                    foreach (Field field in recordset.Fields)
                    {
                        Field2 field2 = field as Field2;
                        if (field2.Expression != string.Empty && !calculatedFields.Contains(field.Name))
                        {
                            if (paramIdMap.ContainsKey(field.Name))
                            {
                                string paramName = paramIdMap[field.Name];
                                ListViewItem item = new ListViewItem(paramName);
                                item.Name = field.Name;
                                listViewFields.Items.Add(item);
                            }
                            else
                            {
                                ListViewItem item = new ListViewItem(field.Name);
                                item.Name = field.Name;
                                listViewFields.Items.Add(item);
                            }
                        }
                    }
                }
                recordset.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect calculated fields.\n" + ex.Message, "DeleteCalculated Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteCalculatedField(ListViewItem listViewItem)
        {
            try
            {
                int paramId = 0;
                if (int.TryParse(listViewItem.Name, out paramId))
                {
                    selTable.Fields.Delete(listViewItem.Name);
                    Field field = selTable.CreateField(listViewItem.Name, DataTypeEnum.dbText);
                    field.AllowZeroLength = true;
                    selTable.Fields.Append(field);
                    fieldsToRecover.Add(listViewItem.Text); //paramName
                }
                else
                {
                    selTable.Fields.Delete(listViewItem.Name);
                    selDataTable.Columns.Remove(listViewItem.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete calculated fields from database.\n" + ex.Message, "DeleteCalculated Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (TableDef tblDef in daoDB.TableDefs)
                {
                    if (tblDef.Name == selDataTable.TableName)
                    {
                        selTable = tblDef;
                        break;
                    }
                }
                
                List<string> fieldNames = new List<string>();
                foreach (ListViewItem item in listViewFields.SelectedItems)
                {
                    DeleteCalculatedField(item);
                    fieldNames.Add(item.Name);
                }

                foreach (string name in fieldNames)
                {
                    listViewFields.Items.RemoveByKey(name);
                }
                removed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete calculated fields.\n" + ex.Message, "DeleteCalculated Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void form_DeleteCalculated_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (removed)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        
    }
}
