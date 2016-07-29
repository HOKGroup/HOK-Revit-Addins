
using HOK.SheetManager.Classes;
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

namespace HOK.SheetManager.UserControls.WorkSpace
{
    /// <summary>
    /// Interaction logic for ProjectPanel.xaml
    /// </summary>
    public partial class ProjectPanel : UserControl
    {
        private ProjectViewModel viewModel = null;

        public ProjectPanel()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as ProjectViewModel;
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                FileDrop(files);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void FileDrop(string[] files)
        {
            try
            {
                foreach (string rvtFile in files)
                {
                    string fileExt = System.IO.Path.GetExtension(rvtFile);
                    if (fileExt != ".rvt") { continue; }

                    RevitProject rvtProject = new RevitProject();
                    rvtProject.FilePath = rvtFile;

                    rvtProject.FileName = System.IO.Path.GetFileName(rvtProject.FilePath);
                    string projectNumber = "00.00000.00";
                    string projectName = "Undefined";
                    RevitProject.GetProjectInfo(rvtProject.FilePath, out projectNumber, out projectName);

                    rvtProject.ProjectNumber = projectNumber;
                    rvtProject.ProjectName = projectName;

                    rvtProject.LinkedBy = Environment.UserName;
                    rvtProject.LastLinked = DateTime.Now;
                    rvtProject.LinkedDate = DateTime.Now;

                    viewModel.Projects.Add(rvtProject);

                    RevitProject selectedProject = viewModel.SelectedProject;
                    viewModel.SelectedProject = null;
                    viewModel.SelectedProject = selectedProject;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
