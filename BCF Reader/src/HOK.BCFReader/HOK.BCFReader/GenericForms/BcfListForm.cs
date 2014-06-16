using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HOK.BCFReader.GenericClasses;
using System.IO;

namespace HOK.BCFReader.GenericForms
{
    public partial class BcfListForm : Form
    {
        private string bcfPath = "";
        private Dictionary<string/*filePath*/, Dictionary<string, BCF>> refDictionary = new Dictionary<string, Dictionary<string, BCF>>();

        public string BCFPath { get { return bcfPath; } set { bcfPath = value; } }
        public Dictionary<string, Dictionary<string, BCF>> RefDictionary { get { return refDictionary; } set { refDictionary = value; } }

        public BcfListForm()
        {
            InitializeComponent();
        }

        public void DisplayBcfFiles()
        {
            if (null != refDictionary)
            {
                foreach (string path in refDictionary.Keys)
                {
                    Dictionary<string, BCF> bcfDictionary = new Dictionary<string, BCF>();
                    bcfDictionary = refDictionary[path];
                    FileInfo fileInfo = new FileInfo(path);
                    
                    if (File.Exists(path))
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = fileInfo.Name;
                        item.SubItems.Add(fileInfo.FullName);
                        item.SubItems.Add(bcfDictionary.Count.ToString());
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.Tag = path;

                        listViewBcf.Items.Add(item);
                    }
                }
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnSelect_Click(object sender, EventArgs e)
        {
            if (listViewBcf.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewBcf.SelectedItems[0];
                if (null != selectedItem.Tag)
                {
                    bcfPath = selectedItem.Tag.ToString();
                    this.DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageBox.Show("Please select a bcf file in the lists.", "Select a bcf file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void listViewBcf_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewBcf.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewBcf.SelectedItems[0];
                if (null != selectedItem.Tag)
                {
                    bcfPath = selectedItem.Tag.ToString();
                    this.DialogResult = DialogResult.OK;
                }
            }
        }
    }
}
