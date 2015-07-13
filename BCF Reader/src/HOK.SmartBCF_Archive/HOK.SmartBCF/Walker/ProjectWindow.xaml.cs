using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Apis.Drive.v2.Data;
using HOK.SmartBCF.GoogleUtils;

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for ProjectStartUpWindow.xaml
    /// </summary>
    public partial class ProjectWindow : Window
    {
        private string projectId = "";

        public string ProjectId { get { return projectId; } set { projectId = value; } }

        public ProjectWindow()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxProjectId.Text))
            {
                File rootFolder = null;
                projectId = textBoxProjectId.Text;
                if (FileManager.RootFolderExist(projectId, out rootFolder))
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Shared Google folders don't exist with the entered file Id.\nPlease enter a valid Id.", "Google Folder Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid Google Folder Id.", "Empty Input", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
