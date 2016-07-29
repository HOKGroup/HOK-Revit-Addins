using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Classes
{
    public class DTMConfigurations : INotifyPropertyChanged
    {
        private ProjectFile projectFileInfo = new ProjectFile();
        private ObservableCollection<ProjectUpdater> projectUpdaters = new ObservableCollection<ProjectUpdater>();

        public ProjectFile ProjectFileInfo { get { return projectFileInfo; } set { projectFileInfo = value; NotifyPropertyChanged("ProjectFileInfo"); } }
        public ObservableCollection<ProjectUpdater> ProjectUpdaters { get { return projectUpdaters; } set { projectUpdaters = value; NotifyPropertyChanged("ProjectUpdaters"); } }

        public DTMConfigurations()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(info));
            }
        }
    }
}
