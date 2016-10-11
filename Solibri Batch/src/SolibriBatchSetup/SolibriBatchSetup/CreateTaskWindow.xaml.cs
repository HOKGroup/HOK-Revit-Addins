using Microsoft.Win32;
using SolibriBatchSetup.Schema;
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
using System.Windows.Shapes;

namespace SolibriBatchSetup
{
    /// <summary>
    /// Interaction logic for CreateTaskWindow.xaml
    /// </summary>
    public partial class CreateTaskWindow : Window
    {
        private ProcessUnit task = new ProcessUnit();

        public ProcessUnit Task { get { return task; } set { task = value; } }

        public CreateTaskWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = task;
        }

        private void buttonOpenSolibri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Solibri Model Checker Files (*.smc)|*.smc";
                openDialog.Title = "Open a Solibri Model";
                openDialog.Multiselect = false;
                if (openDialog.ShowDialog() == true)
                {
                    string fileName = openDialog.FileName;
                    task.OpenSolibri = new OpenModel(fileName);
                    task.TaskName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    task.TaskDirectory = System.IO.Path.GetDirectoryName(fileName);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileFolderDialog folderDialog = new FileFolderDialog();
                folderDialog.Dialog.Title = "Select a Task Folder";
                System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderName = folderDialog.SelectedPath;
                    task.TaskDirectory = folderName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(task.TaskName) && !string.IsNullOrEmpty(task.TaskDirectory))
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Please enter a valid task name and a directory.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

      
    }
}
