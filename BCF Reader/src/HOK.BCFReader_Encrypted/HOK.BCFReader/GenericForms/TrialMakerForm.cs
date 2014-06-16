using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HOK.BCFReader.GenericClasses;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace HOK.BCFReader.GenericForms
{
    public partial class TrialMakerForm : Form
    {
        private TrialMaker trialMaker;
        private RunTypes runType;

        public TrialMakerForm(TrialMaker trial)
        {
            trialMaker = trial;
            runType = trialMaker.RunType;
            InitializeComponent();

            DisplaySettings();
        }

        private void DisplaySettings()
        {
            switch (runType)
            {
                case RunTypes.Trial:
                    labelTopic.Visible = false;
                    labelExpried.Text = "The activation will be expired in " + trialMaker.TrialDays + " [Days]";
                    bttnRun.Enabled = true;
                    break;
                case RunTypes.Expired:
                    labelTopic.Visible = true;
                    labelExpried.Text = "The activation has been expired.";
                    bttnRun.Enabled = false;
                    break;
                case RunTypes.Unknown:
                    labelExpried.Visible = false;
                    labelTopic.Visible = true;
                    bttnRun.Enabled = false;
                    break;
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void bttnActivate_Click(object sender, EventArgs e)
        {
            if (ValidateUserInput())
            {
                trialMaker.ProductName = "HOKBCFReader";
                trialMaker.CompanyName = textBoxCompany.Text;
                trialMaker.Identifier = textBoxId1.Text + textBoxId2.Text + textBoxId3.Text;
                trialMaker.UserPassword = textBoxLicense.Text.Replace("-","");

                if (runType == RunTypes.Expired || runType == RunTypes.Trial)
                {
                    if (trialMaker.Reactivate())
                    {
                        bttnRun.Enabled = true;
                        bttnActivate.Enabled = false;
                        MakeReadOnly();
                        labelTopic.Text = "Successfully Activated !!";
                        labelExpried.Text = "The activation will be expired in " + trialMaker.TrialDays + " [Days]";
                        labelExpried.Visible = true;
                    }
                }
                else if (runType == RunTypes.Unknown)
                {
                    if (trialMaker.FirstActivate())
                    {
                        bttnRun.Enabled = true;
                        bttnActivate.Enabled = false;
                        MakeReadOnly();
                        labelTopic.Text = "Successfully Activated !!";
                        labelExpried.Text = "The activation will be expired in " + trialMaker.TrialDays + " [Days]";
                        labelExpried.Visible = true;
                    }
                }
            }
        }

        private void MakeReadOnly()
        {
            textBoxCompany.ReadOnly = true;
            textBoxId1.ReadOnly = true;
            textBoxId2.ReadOnly = true;
            textBoxId3.ReadOnly = true;
            textBoxLicense.ReadOnly = true;
        }

        private bool ValidateUserInput()
        {
            bool result = false;
            try
            {
                if (null == textBoxCompany.Text)
                {
                    MessageBox.Show("Please enter a valid company name.", "Company Name Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (textBoxCompany.Text.Length < 1)
                {
                    MessageBox.Show("Please enter a valid company name.", "Company Name Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (null == textBoxId1.Text || null == textBoxId2.Text || null == textBoxId3.Text)
                {
                    MessageBox.Show("Please enter a valid 3-digits identifier.", "Identifier Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (textBoxId1.Text.Length<1 || textBoxId2.Text.Length<1 || textBoxId3.Text.Length<1)
                {
                    MessageBox.Show("Please enter a valid 3-digits identifier.", "Identifier Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (null == textBoxLicense.Text)
                {
                    MessageBox.Show("Please enter a valid 25-digits activation code.", "Activation Code Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (textBoxLicense.Text.Length<25)
                {
                    MessageBox.Show("Please enter a valid 25-digits activation code.", "Activation Code Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate the user's input.\n"+ex.Message, "TrialMakerForm:ValidateUserInput", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private void bttnRun_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void linkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Outlook.Application outlookApplication = new Outlook.Application();
                Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
                Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
                Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

                mailItem.Subject = "Activation Request: " + trialMaker.ProductName + " for Revit 2013";

                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("I'm sending a request for the " + trialMaker.ProductName + " with the following information.");
                strBuilder.AppendLine("");
                strBuilder.AppendLine("Company Name: " + trialMaker.CompanyName);
                strBuilder.AppendLine("");
                strBuilder.AppendLine("**** All characters are case-sensitive. *****");
                strBuilder.AppendLine("**** Our HOK team will contact you shortly. *****");

                mailItem.Body = strBuilder.ToString();

                mailItem.Display(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to request an activation code.\n" + ex.Message, "TrialMakerForm:linkEmail_LinkClicked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
