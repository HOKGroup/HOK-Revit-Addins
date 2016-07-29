using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.UserControls.WorkSpace
{
    public class ProjectViewModel : INotifyPropertyChanged
    {
  
        private ObservableCollection<RevitProject> projects = new ObservableCollection<RevitProject>();
        private RevitProject selectedProject = null;

        public ObservableCollection<RevitProject> Projects { get { return projects; } set { projects = value; NotifyPropertyChanged("Projects"); } }
        public RevitProject SelectedProject { get { return selectedProject; } set { selectedProject = value; NotifyPropertyChanged("SelectedProject"); } }

        public ProjectViewModel()
        {
            projects.Add(new RevitProject() { ProjectNumber = "00.00000.00", ProjectName = "Undefined", FilePath=@"B:\Revit Projects\SheetManager\2016\TestModel.rvt", FileName = "TestModel.rvt", LastLinked = DateTime.Now, LinkedBy = "Jinsol" });
            projects.Add(new RevitProject() { ProjectNumber = "00.00000.00", ProjectName = "Undefined", FilePath = @"B:\Revit Projects\SheetManager\2016\TestModel2.rvt", FileName = "TestModel2.rvt", LastLinked = DateTime.Now, LinkedBy = "Jinsol" });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
