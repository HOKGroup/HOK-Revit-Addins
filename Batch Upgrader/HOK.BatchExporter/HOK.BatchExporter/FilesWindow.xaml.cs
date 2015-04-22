using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using System.IO;
using WinForms=System.Windows.Forms;

namespace HOK.BatchExporter
{
    /// <summary>
    /// Interaction logic for FilesWindow.xaml
    /// </summary>
    public partial class FilesWindow : Window
    {
        private string rvtFileName = "";

        public string RvtFileName { get { return rvtFileName; } set { rvtFileName = value; } }

        public FilesWindow()
        {
            InitializeComponent();
        }

        public void DisplayPredefined()
        {
            try
            {
                textBoxRevitFile.Text = rvtFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot display the predefined settiong.\n" + ex.Message, "Display Pre-defined Settings", MessageBoxButton.OK);
            }
        }

        private void buttonOpenRVT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Title = "Open a Revit Project File";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.DefaultExt = ".rvt";
                openFileDialog.Filter = "Revit Project File (.rvt)|*.rvt";

                Nullable<bool> result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    rvtFileName = openFileDialog.FileName;
                    textBoxRevitFile.Text = rvtFileName;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot open the selected Revit file.\n"+ex.Message, "Open Revit File", MessageBoxButton.OK);
            }
        }

        private string FindDefaultIFCPath(string revitFileName)
        {
            string ifcFolder = "";
            try
            {
                string curDirectory = System.IO.Path.GetDirectoryName(revitFileName);
                DirectoryInfo softwareDirectory = Directory.GetParent(curDirectory);
                if (!string.IsNullOrEmpty(curDirectory) && null != softwareDirectory)
                {
                    if (softwareDirectory.Name == "Software")
                    {
                        string ifcDirectory = softwareDirectory.FullName + "\\IFC";
                        if (Directory.Exists(ifcDirectory))
                        {
                            ifcFolder = ifcDirectory;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find the default IFC directory.\n"+ex.Message, "Find a Default Directory for IFC", MessageBoxButton.OK);
            }
            return ifcFolder;
        }

        private string FindDefaultSoftWarePath(string revitFileName)
        {
            string softwareDirectoryPath = "";
            try
            {
                string curDirectory = System.IO.Path.GetDirectoryName(revitFileName);
                if (curDirectory.Contains("Software") && curDirectory.Contains("Revit"))
                {
                    DirectoryInfo softwareDirectory = Directory.GetParent(curDirectory);
                    while (softwareDirectory.Name != "Software")
                    {
                        softwareDirectory = Directory.GetParent(softwareDirectory.FullName);
                    }
                    softwareDirectoryPath = softwareDirectory.FullName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find the default IFC directory.\n" + ex.Message, "Find a Default Directory for IFC", MessageBoxButton.OK);
            }
            return softwareDirectoryPath;
        }

       

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(rvtFileName))
                {
                    this.DialogResult = true;
                }
                else if(!File.Exists(rvtFileName))
                {
                    MessageBox.Show("The selected file doesn't exist.\n" + rvtFileName, "File Not Exist", MessageBoxButton.OK);
                }
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Cannot load files information.\n" + ex.Message, "Add Revit File/ Ouput Folder", MessageBoxButton.OK);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
