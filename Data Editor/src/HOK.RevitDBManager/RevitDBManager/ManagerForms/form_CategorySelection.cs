using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RevitDBManager.ManagerForms
{
    public partial class form_CategorySelection : Form
    {
        private string selectedCategory = "";
        private List<string> categoryList = new List<string>();

        public string SelectedCategory { get { return selectedCategory; } set { selectedCategory = value; } }
        public List<string> CategoryList { get { return categoryList; } set { categoryList = value; } }

        public form_CategorySelection()
        {
            InitializeComponent();
        }
        
        private void form_CategorySelection_Load(object sender, EventArgs e)
        {
            foreach (string categoryName in categoryList)
            {
                comboBoxCategory.Items.Add(categoryName);
            }
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            if (comboBoxCategory.SelectedItem != null)
            {
                string selectedItem = comboBoxCategory.SelectedItem.ToString();
                if (categoryList.Contains(selectedItem))
                {
                    selectedCategory = selectedItem;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Please select a valid category item in the combobox list.", "Invalid Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a valid category item in the combobox list.", "Invalid Item", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        
    }
}
