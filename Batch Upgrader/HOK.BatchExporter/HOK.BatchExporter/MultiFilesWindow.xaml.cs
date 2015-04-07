using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HOK.BatchExporter;
using WinForms = System.Windows.Forms;

namespace HOK.BatcUpgrader
{
    /// <summary>
    /// Interaction logic for MultiFilesWindow.xaml
    /// </summary>
    public partial class MultiFilesWindow : Window
    {
        private string rvtFolderName = "";
        private string outputFolder = "";
        private string[] revitFileNames;

        public string RvtFolderName { get { return rvtFolderName; } set { rvtFolderName = value; } }
        public string OutputFolder { get { return outputFolder; } set { outputFolder = value; } }
        public string[] RevitFileNames { get { return revitFileNames; } set { revitFileNames = value; } }

        public MultiFilesWindow()
        {
            InitializeComponent();
        }

        private void buttonOpenRVT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog fileFolderDialog = new FileFolderDialog();
                fileFolderDialog.Dialog.Title = "Select the directory that Revit files exist.";
                WinForms.DialogResult result = fileFolderDialog.ShowDialog();
                if (result == WinForms.DialogResult.OK)
                {
                   rvtFolderName = fileFolderDialog.SelectedPath;
                   textBoxRevitFile.Text = rvtFolderName;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open the selected folder.\n" + ex.Message, "Select Folder", MessageBoxButton.OK);
            }
        }

        private void buttonFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog fileFolderDialog = new FileFolderDialog();
                fileFolderDialog.Dialog.Title = "Select the directory that you want to store files.";
                WinForms.DialogResult result = fileFolderDialog.ShowDialog();
                if (result == WinForms.DialogResult.OK)
                {
                    outputFolder = fileFolderDialog.SelectedPath;
                    textBoxFolder.Text = outputFolder;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open the selected folder.\n" + ex.Message, "Select Folder", MessageBoxButton.OK);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(rvtFolderName) && Directory.Exists(outputFolder))
                {
                    
                    DirectoryInfo di = new DirectoryInfo(rvtFolderName);
                    if ((bool)checkBoxInclude.IsChecked)
                    {
                        FileInfo[] fileInfoArray = di.GetFiles("*.rvt", SearchOption.AllDirectories);
                        revitFileNames = new string[fileInfoArray.Length];
                        for (int i = 0; i < fileInfoArray.Length; i++)
                        {
                            FileInfo fi = fileInfoArray[i];
                            revitFileNames[i] = fi.FullName;
                        }
                       
                        
                    }
                    else
                    {
                        FileInfo[] fileInfoArray = di.GetFiles("*.rvt", SearchOption.TopDirectoryOnly);
                        revitFileNames = new string[fileInfoArray.Length];
                        for (int i = 0; i < fileInfoArray.Length; i++)
                        {
                            FileInfo fi = fileInfoArray[i];
                            revitFileNames[i] = fi.FullName;
                        }
                    }

                    NumericComparer nc = new NumericComparer();
                    Array.Sort(revitFileNames, nc);

                    if (revitFileNames.Length > 0)
                    {
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Revit Files don't exist in the selected folder.\n" + rvtFolderName, "File Not Exist", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select valid paths for input and output folder.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load information of files.\n"+ex.Message, "Add Files from Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
