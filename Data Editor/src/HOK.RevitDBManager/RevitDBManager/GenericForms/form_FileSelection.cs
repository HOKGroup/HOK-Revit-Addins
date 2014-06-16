using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RevitDBManager.Classes;
using System.IO;
using Autodesk.Revit.UI;
using RevitDBManager.GenericForms;

namespace RevitDBManager.Forms
{
    public partial class form_FileSelection : Form
    {
        private UIApplication m_app;
        private DefaultSettings settings;
        private Dictionary<string/*filePath*/, DBFileInfo> dbInfoDictionary = new Dictionary<string, DBFileInfo>();

        public DefaultSettings defaultSettings { get { return settings; } set { settings = value; } }
        public string DefualtFilePath { get { return settings.DefaultDBFile; } set { settings.DefaultDBFile = value; } }

        public form_FileSelection(UIApplication uiapp)
        {
            m_app=uiapp;
            settings = new DefaultSettings(m_app);
            dbInfoDictionary = settings.DBInfoDictionary;
            InitializeComponent();
        }

        private void form_FileSelection_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (string filePath in dbInfoDictionary.Keys)
                {
                    if (File.Exists(filePath))
                    {
                        DBFileInfo dbFileInfo = dbInfoDictionary[filePath];
                        ListViewItem item = new ListViewItem(dbFileInfo.FileName);
                        item.Name = dbFileInfo.FilePath;
                        item.Tag = dbFileInfo;

                        if (dbFileInfo.isDefault) { item.ImageIndex = 1; }
                        else { item.ImageIndex = 0; }

                        item.SubItems.Add(dbFileInfo.DateModified);
                        item.SubItems.Add(dbFileInfo.ModifiedBy);
                        item.SubItems.Add(dbFileInfo.FilePath);
                        item.SubItems.Add(dbFileInfo.Comments);

                        listViewFileItems.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the form.\n" + ex.Message, "Error: Alternative Data Source", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewFileItems.Items.Count > 0)
                {
                    foreach (string filePath in dbInfoDictionary.Keys)
                    {
                        DBFileInfo dbFileInfo = dbInfoDictionary[filePath];
                        if (dbFileInfo.isDefault)
                        {
                            settings.DefaultDBFile = dbFileInfo.FilePath;
                            break;
                        }
                    }
                    settings.DBInfoDictionary = dbInfoDictionary;
                    settings.WriteINI();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please select at least a file.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save the file information.\n"+ex.Message, "Error: Alternative Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnActivate_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewFileItems.SelectedItems.Count > 0)
                {
                    string selectedFile = listViewFileItems.SelectedItems[0].Name;
                    foreach (string filePath in dbInfoDictionary.Keys)
                    {
                        if (filePath == selectedFile)
                        {
                            dbInfoDictionary[filePath].isDefault = true;
                        }
                        else
                        {
                            dbInfoDictionary[filePath].isDefault = false;
                        }
                    }

                    foreach (ListViewItem item in listViewFileItems.Items)
                    {
                        if (item.Selected)
                        {
                            item.ImageIndex = 1;
                        }
                        else
                        {
                            item.ImageIndex = 0;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please seleca at least a file item in the list view to activate.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to activate an item: \n" + listViewFileItems.SelectedItems[0].Name + "\n" + ex.Message, "Error: Alternative Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void bttnAddComments_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewFileItems.SelectedItems.Count > 0)
                {
                    DBFileInfo selectedFile = listViewFileItems.SelectedItems[0].Tag as DBFileInfo;

                    if (null != selectedFile)
                    {
                        form_FileProperties propertyForm = new form_FileProperties(selectedFile);
                        if (DialogResult.OK == propertyForm.ShowDialog())
                        {
                            listViewFileItems.SelectedItems[0].Tag = propertyForm.DataBaseFileInfo;
                            listViewFileItems.SelectedItems[0].SubItems[4].Text = propertyForm.DataBaseFileInfo.Comments;
                            selectedFile.Comments = propertyForm.DataBaseFileInfo.Comments;
                            if (dbInfoDictionary.ContainsKey(selectedFile.FilePath)) { dbInfoDictionary.Remove(selectedFile.FilePath); }
                            dbInfoDictionary.Add(selectedFile.FilePath, selectedFile);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please seleca at least a file item in the list view to add comments.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to activate an item: \n" + listViewFileItems.SelectedItems[0].Name + "\n" + ex.Message, "Error: Alternative Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void activateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewFileItems.SelectedItems.Count > 0)
                {
                    string selectedFile = listViewFileItems.SelectedItems[0].Name;
                    foreach (string filePath in dbInfoDictionary.Keys)
                    {
                        if (filePath == selectedFile)
                        {
                            dbInfoDictionary[filePath].isDefault = true;
                        }
                        else
                        {
                            dbInfoDictionary[filePath].isDefault = false;
                        }
                    }

                    foreach (ListViewItem item in listViewFileItems.Items)
                    {
                        if (item.Selected)
                        {
                            item.ImageIndex = 1;
                        }
                        else
                        {
                            item.ImageIndex = 0;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please seleca at least a file item in the list view to activate.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to activate an item: \n" + listViewFileItems.SelectedItems[0].Name + "\n" + ex.Message, "Error: Alternative Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewFileItems.SelectedItems.Count > 0)
                {
                    DBFileInfo selectedFile = listViewFileItems.SelectedItems[0].Tag as DBFileInfo;

                    if (null != selectedFile)
                    {
                        form_FileProperties propertyForm = new form_FileProperties(selectedFile);
                        if (DialogResult.OK == propertyForm.ShowDialog())
                        {
                            listViewFileItems.SelectedItems[0].Tag = propertyForm.DataBaseFileInfo;
                            listViewFileItems.SelectedItems[0].SubItems[4].Text = propertyForm.DataBaseFileInfo.Comments;
                            selectedFile.Comments = propertyForm.DataBaseFileInfo.Comments;
                            if (dbInfoDictionary.ContainsKey(selectedFile.FilePath)) { dbInfoDictionary.Remove(selectedFile.FilePath); }
                            dbInfoDictionary.Add(selectedFile.FilePath, selectedFile);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please seleca at least a file item in the list view to add comments.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to activate an item: \n" + listViewFileItems.SelectedItems[0].Name + "\n" + ex.Message, "Error: Alternative Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void linkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string htmPath = @"V:\RVT-Data\HOK Program\Documentation\Data Editor_Instruction.pdf";
            System.Diagnostics.Process.Start(htmPath);
        }

        private void linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
    }

    
}
