using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace HOK.BCFReader_SerialMaker
{
    public partial class MainForm : Form
    {
        private Guid bcfGuid = new Guid("{A81E6B45-A8FF-49B7-8AF3-1EF5FC8F3027}");
        private string productID = "";
        private string selectedGuid = "";
        private string licenseKey = "";
        private string productName = "";
        private string companyName = "";
        private string identifier = "";
        private bool keyGenerated = false;
        private RecordForm recordForm;
        private Dictionary<string, LicenseKey> keyDictionary = new Dictionary<string, LicenseKey>();

        public MainForm()
        {
            InitializeComponent();
            radioBttnBCF.Tag = bcfGuid.ToString();
            buttonEmail.Enabled = false;
            radioBttnBCF.Checked = true;
            recordForm = new RecordForm();
            keyDictionary = recordForm.KeyDictionary;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                keyGenerated = false;
                if (radioBttnBCF.Checked && null != radioBttnBCF.Tag)
                {
                    selectedGuid = radioBttnBCF.Tag.ToString();
                    productName = radioBttnBCF.Text;
                }

                if (ValidateInput())
                {
                    companyName = textBoxCompany.Text;
                    identifier = textBoxId.Text;

                    productID = GetProductId(companyName, selectedGuid);
                    productID = Encryption.Boring(Encryption.InverseByBase(productID, 10));

                    licenseKey = Encryption.MakePassword(productID, identifier);
                    if (keyDictionary.ContainsKey(licenseKey))
                    {
                        MessageBox.Show("This activation code with the identifier has been already generated before.","Duplicate Codes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    labelLicense.Text = BreakFiveDigits(licenseKey);
                    keyGenerated = true;
                    buttonEmail.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate the activation code.\n" + ex.Message, "mainForm:btnGenerate_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateInput()
        {
            try
            {
                if (null == textBoxCompany.Text)
                {
                    MessageBox.Show("Please enter a valid company name.", "Company Name Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (textBoxCompany.TextLength < 1)
                {
                    MessageBox.Show("Please enter a valid company name.", "Company Name Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (null == textBoxId.Text)
                {
                    MessageBox.Show("Please enter a valid identifier.", "Identifier Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else if (textBoxId.TextLength != 3)
                {
                    MessageBox.Show("Please enter a valid 3-digits identifier.", "Identifier Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate the user's input.\n"+ex.Message,"mainForm:ValidateInput",MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private string GetProductId(string companyName, string productGuid)
        {
            string baseString = "";
            try
            {
                baseString += companyName;
                baseString += productGuid;
                baseString = RemoveUseLess(baseString);

                return baseString.Substring(0, 25).ToUpper();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get a product ID.\n" + ex.Message, "TrialMaker:GetProductId", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return baseString;
            }
        }

        private string RemoveUseLess(string baseString)
        {
            char ch;
            for (int i = baseString.Length - 1; i >= 0; i--)
            {
                ch = char.ToUpper(baseString[i]);
                if ((ch < 'A' || ch > 'Z') && (ch < '0' || ch > '9'))
                {
                    baseString = baseString.Remove(i, 1);
                }
            }
            return baseString;
        }

        private string BreakFiveDigits(string keyString)
        {
            string stringResult = "";
            try
            {
                string[] fiveChars=new string[5];

                for (int i = 0; i < 5; i++)
                {
                    int stratIndex = i * 5;
                    string subString = keyString.Substring(stratIndex, 5);
                    fiveChars[i] = subString;
                }

                stringResult = fiveChars[0] + "-" + fiveChars[1] + "-" + fiveChars[2] + "-" + fiveChars[3] + "-" + fiveChars[4];

                return stringResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate the activation code.\n" + ex.Message, "mainForm:btnGenerate_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return stringResult;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(labelLicense.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy the activation code.\n" + ex.Message, "mainForm:copyToolStripMenuItem_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonEmail_Click(object sender, EventArgs e)
        {
            try
            {
                if (keyGenerated)
                {
                    Outlook.Application outlookApplication = new Outlook.Application();
                    Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
                    Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
                    Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

                    mailItem.Subject ="Activation Information: "+productName+" for Revit 2013";

                    StringBuilder strBuilder = new StringBuilder();
                    strBuilder.AppendLine("Please activate "+productName+" with the following information.");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("Company Name: "+companyName);
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("Identifier: "+identifier);
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("Activation Code: " + labelLicense.Text);
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("**** All characters are case-sensitive. *****");

                    mailItem.Body = strBuilder.ToString();

                    mailItem.Display(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send an email with the activation code.\n" + ex.Message, "mainForm:buttonEmail_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonrecord_Click(object sender, EventArgs e)
        {
            try
            {
                if (keyGenerated)
                {
                    if (keyDictionary.ContainsKey(licenseKey))
                    {
                        keyDictionary.Remove(licenseKey);
                    }

                    LicenseKey license = new LicenseKey();
                    license.CompanyName = companyName;
                    license.Identifier = identifier;
                    license.KeyString = licenseKey;
                    license.DateGenerated = DateTime.Now.ToString();
                    license.GeneratedBy = Environment.UserName;

                    keyDictionary.Add(licenseKey, license);
                    recordForm.KeyDictionary = keyDictionary;
                    recordForm.WriteRecords();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot save the information of the activation code.\n" + ex.Message, "MainForm:buttonrecord_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonReview_Click(object sender, EventArgs e)
        {
            try
            {
                recordForm = new RecordForm();
                recordForm.Show();
                recordForm.DisplayRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot review the information of the activation code.\n" + ex.Message, "MainForm:buttonReview_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
