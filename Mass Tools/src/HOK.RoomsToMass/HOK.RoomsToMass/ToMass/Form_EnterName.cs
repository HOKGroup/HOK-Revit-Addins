using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.RoomsToMass.ToMass
{
    public partial class Form_EnterName : Form
    {
        private string enteredName = "";
        public string EnteredName { get { return enteredName; } set { enteredName = value; } }

        public Form_EnterName(string suggestedName)
        {
            InitializeComponent();
            textBoxName.Text = suggestedName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxName.Text))
            {
                MessageBox.Show("Please enter a valid name.", "Empty Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                enteredName = textBoxName.Text;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
