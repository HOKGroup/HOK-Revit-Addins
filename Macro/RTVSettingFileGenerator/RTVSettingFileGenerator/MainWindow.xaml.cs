using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RTVSettingFileGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rvtDirectory = "";
        private string xmlDirectory = "";


        public MainWindow()
        {
            InitializeComponent();
        }


        private void buttonBrowseRvt_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select a Directory for Revit Files";
            folderDialog.ShowNewFolderButton = true;
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rvtDirectory = folderDialog.SelectedPath;
                textboxRvt.Text = rvtDirectory;
            }
        }

        private void buttonBrowseXml_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select a Directory for xml Files";
            folderDialog.ShowNewFolderButton = true;
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xmlDirectory = folderDialog.SelectedPath;
                textboxXml.Text = xmlDirectory;
            }
        }

        private void buttonRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rvtDirectory = textboxRvt.Text;
                xmlDirectory = textboxXml.Text;

                if (!string.IsNullOrEmpty(rvtDirectory) && !string.IsNullOrEmpty(xmlDirectory))
                {
                    if (Directory.Exists(rvtDirectory) && Directory.Exists(xmlDirectory))
                    {
                        SettingFileGenerator generator = new SettingFileGenerator(rvtDirectory, xmlDirectory);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Please enter valid directories.", "Directory Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please enter valid directories.", "Directory Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot create xml files.\n" + ex.Message, "Create XML Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
    }
}
