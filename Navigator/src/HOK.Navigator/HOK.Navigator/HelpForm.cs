using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.IO;
using HOK.Core.Utilities;

namespace HOK.Navigator
{
    public partial class HelpForm : Form
    {
        private readonly string currentAssembly;
        private readonly string helpFileName = "HOK.Help.txt";
        private readonly string helpFilePath;
        private readonly string installerFileName = "HOK.Installer.txt";
        private readonly string installerFilePath;
        private readonly Dictionary<string, string> linkDictionary = new Dictionary<string, string>();
        private readonly Dictionary<string, string> installerDictionary = new Dictionary<string, string>();

        public HelpForm()
        {
            InitializeComponent();
            Text = "HOK Navigator v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            currentAssembly = System.Reflection.Assembly.GetAssembly(GetType()).Location;
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
                if (!File.Exists(filePath)) return;

                var inputTitle = "";
                var index = 0;

                using (var reader = File.OpenText(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (index % 2 == 0)
                        {
                            inputTitle = line;
                            if (line.Contains("---------")) { inputTitle = "spliter" + index; }
                        }
                        else
                        {
                            var inputUrl = line;
                            dictionary.Add(inputTitle, inputUrl);
                        }
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void DisplayListView(Dictionary<string, string> dictionary, ListView listView)
        {
            if (dictionary.Count <= 0) return;

            foreach (var title in dictionary.Keys)
            {
                var itemName = title;

                if (title.Contains("spliter")) { itemName = "-------------------------------------------------------------------------------------------"; }
                var item = new ListViewItem(itemName);
                item.Tag = dictionary[title];

                listView.Items.Add(item);
            }
        }

        private void DisplaySelection()
        {
            try
            {
                if (!InstallerTrigger.Activated) return;

                var activatedInstaller = new Dictionary<string, bool>();
                activatedInstaller = InstallerTrigger.ActivatedInstaller;
                if (activatedInstaller.Count <= 0) return;

                for (var i = 0; i < listViewInstaller.Items.Count; i++)
                {
                    var item = listViewInstaller.Items[i];
                    if (activatedInstaller.ContainsKey(item.Text))
                    {
                        item.Checked = activatedInstaller[item.Text];
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to disply selected items.\n" + ex.Message, "Display Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenEmail()
        {
            var outlookApplication = new Outlook.Application();
            var nameSpace = outlookApplication.GetNamespace("MAPI");
            var folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            var mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "Revit Problem Report";
            mailItem.Body = "**** This email will go to the Firmwide [_HOK BIM Support Request] team. ****\n" + "What office are you in? \n" + "What project are you working on? \n" + "Describe the problem:";

            mailItem.Recipients.Add("_HOK BIM Support Request");
            mailItem.Display(false);
        }

        private void linkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var outlookApplication = new Outlook.Application();
            var nameSpace = outlookApplication.GetNamespace("MAPI");
            var folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            var mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "Revit Problem Report";
            mailItem.Body = "**** This email will go to the Firmwide [_HOK BIM Support Request] team. ****\n" + "What office are you in? \n" + "What project are you working on? \n" + "Describe the problem:";

            mailItem.Recipients.Add("konrad.sobon@hok.com");
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
                var activatedInstaller = new Dictionary<string, bool>();
                foreach (ListViewItem item in listViewInstaller.Items)
                {
                    activatedInstaller.Add(item.Text, item.Checked);
                }
                InstallerTrigger.ActivatedInstaller = activatedInstaller;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to apply changes.\n" + ex.Message, "HOK Navigator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
