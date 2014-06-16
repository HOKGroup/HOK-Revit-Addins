using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.IO;

namespace HOK.Navigator
{
    public partial class HelpForm : Form
    {
        private string currentAssembly = "";
        private string helpFileName = "HOK.Help.txt";
        private string helpFilePath = "";
        private string installerFileName = "HOK.Installer.txt";
        private string installerFilePath = "";
        private Dictionary<string, string> linkDictionary = new Dictionary<string, string>();
        private Dictionary<string, string> installerDictionary = new Dictionary<string, string>();

        public HelpForm()
        {
            InitializeComponent();
            this.Text = "HOK Navigator v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            helpFilePath = Path.GetDirectoryName(currentAssembly) + "/Resources/" + helpFileName;
            installerFilePath = Path.GetDirectoryName(currentAssembly) + "/Resources/" + installerFileName;
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            ReadFileToArray(helpFilePath, linkDictionary);
            DisplayListView(linkDictionary, listViewWebPages);
            ReadFileToArray(installerFilePath, installerDictionary);
            DisplayListView(installerDictionary, listViewInstaller);
            DisplaySelection();
        }

        private void ReadFileToArray(string filePath, Dictionary<string, string> dictionary)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string inputTitle = "";
                    string inputUrl = "";
                    int index = 0;

                    using (StreamReader reader = File.OpenText(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (index % 2 == 0)
                            {
                                inputTitle = line;
                                if (line.Contains("---------")) { inputTitle = "spliter" + index.ToString(); }
                            }
                            else
                            {
                                inputUrl = line;
                                dictionary.Add(inputTitle, inputUrl);
                            }
                            index++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors: cannot read a text file.\n" + filePath + "\n" + ex.Message);
            }
        }

        private void DisplayListView(Dictionary<string, string> dictionary, ListView listView)
        {
            if (dictionary.Count > 0)
            {
                foreach (string title in dictionary.Keys)
                {
                    string itemName = title;

                    if (title.Contains("spliter")) { itemName = "-------------------------------------------------------------------------------------------"; }
                    ListViewItem item = new ListViewItem(itemName);
                    item.Tag = dictionary[title];

                    listView.Items.Add(item);
                }
            }
        }

        private void DisplaySelection()
        {
            try
            {
                if (InstallerTrigger.Activated)
                {
                    Dictionary<string, bool> activatedInstaller = new Dictionary<string, bool>();
                    activatedInstaller = InstallerTrigger.ActivatedInstaller;
                    if (activatedInstaller.Count > 0)
                    {
                        for (int i = 0; i < listViewInstaller.Items.Count; i++)
                        {
                            ListViewItem item = listViewInstaller.Items[i];
                            if (activatedInstaller.ContainsKey(item.Text))
                            {
                                item.Checked = activatedInstaller[item.Text];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to disply selected items.\n" + ex.Message, "Display Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenEmail()
        {
            Outlook.Application outlookApplication = new Outlook.Application();
            Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
            Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "Revit 2013 Problem Report";
            mailItem.Body = "**** This email will go to the Firmwide [_HOK BIM Support Request] team. ****\n" + "What office are you in? \n" + "What project are you working on? \n" + "Describe the problem:";

            mailItem.Recipients.Add("_HOK BIM Support Request");
            mailItem.Display(false);
        }

        private void linkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Outlook.Application outlookApplication = new Outlook.Application();
            Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
            Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "Revit 2013 Problem Report";
            mailItem.Body = "**** This email will go to the Firmwide [_HOK BIM Support Request] team. ****\n" + "What office are you in? \n" + "What project are you working on? \n" + "Describe the problem:";

            mailItem.Recipients.Add("jinsol.kim@hok.com");
            mailItem.Display(false);
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewInstaller.CheckedItems.Count > 0)
                {
                    InstallerTrigger.Activated = true;
                }
                else
                {
                    InstallerTrigger.Activated = false;
                }

                InstallerTrigger.InstallerUrl = installerDictionary;
                Dictionary<string, bool> activatedInstaller = new Dictionary<string, bool>();
                foreach (ListViewItem item in listViewInstaller.Items)
                {
                    activatedInstaller.Add(item.Text, item.Checked);
                }
                InstallerTrigger.ActivatedInstaller = activatedInstaller;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply changes.\n" + ex.Message, "HOK Navigator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listViewWebPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem item in listViewWebPages.SelectedItems)
                {
                    if (item.Text.Contains("Email"))
                    {
                        OpenEmail();
                    }
                    else if (null != item.Tag && !item.Text.Contains("---------") && !item.Text.Contains("Version"))
                    {
                        System.Diagnostics.Process.Start(item.Tag.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open the selected web page: \n" + ex.Message);
            }
        }

    }
}
