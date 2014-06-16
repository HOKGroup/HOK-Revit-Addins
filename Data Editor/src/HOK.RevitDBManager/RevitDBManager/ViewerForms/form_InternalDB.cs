using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RevitDBManager.Forms
{
    public partial class form_InternalDB : Form
    {
        private Dictionary<string/*object name*/, string[]/*param name*/> fieldsDictionary = new Dictionary<string, string[]>();
        private Dictionary<string/*tableName*/, List<string>/*fieldNames*/> selectedFields = new Dictionary<string, List<string>>();
        private List<string> categoryList=new List<string>();
        private string selTable = "";
        private string defCategory = "";

        public Dictionary<string, List<string>> SelectedFields { get { return selectedFields; } set { selectedFields = value; } }
        public List<string> CategoryList { get { return categoryList; } set { categoryList = value; } }

        public form_InternalDB(string categoryName)
        {
            defCategory = categoryName;
            InitializeComponent();
        }

        private void form_InternalDB_Load(object sender, EventArgs e)
        {
            AddFieldsDictionary();

            foreach (string objectName in fieldsDictionary.Keys)
            {
                comboBoxField.Items.Add(objectName);
            }
            comboBoxField.SelectedIndex = 0;

            foreach (string categoryName in categoryList)
            {
                comboBoxCategory.Items.Add(categoryName);
            }

            if (defCategory != string.Empty)
            {
                int index = comboBoxCategory.Items.IndexOf(defCategory);
                comboBoxCategory.SelectedIndex = index;
            }
            else
            {
                comboBoxCategory.SelectedIndex = 0;
            }
            selTable = "Inst_" + comboBoxCategory.SelectedItem.ToString();
        }

        private void AddFieldsDictionary()
        {
            string[] familyInstance = new string[] { "FacingFlipped", "HandFlipped", "IsSlantedColumn", "Mirrored" };
            string[] host = new string[] { "Id", "Category", "Name" };
            string[] room = new string[]{"Base Finish", "Base Offset", "Ceiling Finish", "Comments", "Department", "Floor Finish","Level", "Limit Offset", "Name", "Number", 
                "Occupancy", "Occupant", "Unbounded Height", "Upper Limit", "Volume","Wall Finish" };

            fieldsDictionary.Add("Family Instance", familyInstance);
            fieldsDictionary.Add("Host", host);
            fieldsDictionary.Add("Room", room);
            fieldsDictionary.Add("FromRoom", room);
            fieldsDictionary.Add("ToRoom", room);
        }

        private void comboBoxField_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshListBox();
        }

        private void bttnAdd_Click(object sender, EventArgs e)
        {
            if (listBoxAvailable.SelectedIndex > -1)
            {
                for (int i = listBoxAvailable.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    object item = listBoxAvailable.Items[listBoxAvailable.SelectedIndices[i]];
                    listBoxAvailable.Items.RemoveAt(listBoxAvailable.SelectedIndices[i]);
                    listBoxSelected.Items.Add(item);
                }
            }
        }

        private void bttnRemove_Click(object sender, EventArgs e)
        {
            if (listBoxSelected.SelectedIndex > -1)
            {
                for (int i = listBoxSelected.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    object item = listBoxSelected.Items[listBoxSelected.SelectedIndices[i]];
                    listBoxSelected.Items.RemoveAt(listBoxSelected.SelectedIndices[i]);
                }
            }
            RefreshListBox();
        }

        private void RefreshListBox()
        {
            int index = comboBoxField.SelectedIndex;
            string objectName = comboBoxField.Items[index].ToString();

            string[] fields = fieldsDictionary[objectName];

            listBoxAvailable.Items.Clear();
            foreach (string field in fields)
            {
                string itemName = objectName + ":" + field;
                if (!listBoxSelected.Items.Contains(itemName))
                {
                    listBoxAvailable.Items.Add(objectName + ":" + field);
                }
            }
        }

        private void bttnUp_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listBoxSelected.Items.Count; i++)
            {
                if (listBoxSelected.SelectedIndices.Contains(i))
                {
                    if (i > 0 && !listBoxSelected.SelectedIndices.Contains(i - 1))
                    {
                        object item = listBoxSelected.Items[i];
                        listBoxSelected.Items.RemoveAt(i);
                        listBoxSelected.Items.Insert(i - 1, item);
                        listBoxSelected.SelectedIndices.Add(i - 1);
                    }
                }
            }
        }

        private void bttnDown_Click(object sender, EventArgs e)
        {
            for (int i = listBoxSelected.Items.Count - 1; i > -1; i--)
            {
                if (listBoxSelected.SelectedIndices.Contains(i))
                {
                    if (i < listBoxSelected.Items.Count - 1 && !listBoxSelected.SelectedIndices.Contains(i + 1))
                    {
                        object item = listBoxSelected.Items[i];
                        listBoxSelected.Items.RemoveAt(i);
                        listBoxSelected.Items.Insert(i + 1, item);
                        listBoxSelected.SelectedIndices.Add(i + 1);
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
            SaveSetting();
        }

        private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSetting();

            selTable = "Inst_" + comboBoxCategory.SelectedItem.ToString();
            
            listBoxSelected.Items.Clear();
            if (selectedFields.ContainsKey(selTable))
            {
                List<string> fields = new List<string>();
                fields = selectedFields[selTable];
                foreach (string field in fields)
                {
                    listBoxSelected.Items.Add(field);
                }
            }
            RefreshListBox();
        }

        private void SaveSetting()
        {
            List<string> selectedItems = new List<string>();
            foreach (object item in listBoxSelected.Items)
            {
                selectedItems.Add(item.ToString());
            }

            if (selectedFields.ContainsKey(selTable))
            {
                selectedFields.Remove(selTable);
                selectedFields.Add(selTable, selectedItems);
            }
            else
            {
                selectedFields.Add(selTable, selectedItems);
            }
            
        }
        
    }
}
