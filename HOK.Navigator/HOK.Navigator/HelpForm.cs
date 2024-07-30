using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using HOK.Core.Utilities;

namespace HOK.Navigator
{
    public partial class HelpForm : Form
    {
        private readonly string currentAssembly;
        private readonly string helpFileName = "HOK.Help.txt";
        private readonly string helpFilePath;
        private readonly Dictionary<string, string> linkDictionary = new Dictionary<string, string>();

        public HelpForm()
        {
            InitializeComponent();
            Text = "HOK Navigator v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            currentAssembly = System.Reflection.Assembly.GetAssembly(GetType()).Location;
            helpFilePath = Path.GetDirectoryName(currentAssembly) + "/Resources/" + helpFileName;
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            ReadFileToArray(helpFilePath, linkDictionary);
            DisplayListView(linkDictionary, listViewWebPages);
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

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void listViewWebPages_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem item in listViewWebPages.SelectedItems)
                {
                    if (null != item.Tag && !item.Text.Contains("---------") && !item.Text.Contains("Version"))
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
