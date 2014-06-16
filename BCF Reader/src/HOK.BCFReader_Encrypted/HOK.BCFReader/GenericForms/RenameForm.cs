using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.BCFReader.GenericForms
{
    public partial class RenameForm : Form
    {
        private string issueNumber = "";

        public string IssueNumber { get { return issueNumber; } set { issueNumber = value; } }

        public RenameForm(string oldName)
        {
            issueNumber = oldName;
            InitializeComponent();
            labelInfo.Text = "Type a new name for the issue - " + issueNumber;

        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            issueNumber = textBoxIssue.Text;
            if (null != issueNumber)
            {
                if (issueNumber.Length > 0)
                {
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Please type an issue number.", "Invalid Issue Number", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please type an issue number.", "Invalid Issue Number", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
