using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControlTest.Classes
{
    public class Configuration : INotifyPropertyChanged
    {
        private string name = "";
        private ObservableCollection<RvtFile> files = new ObservableCollection<RvtFile>();
        private ObservableCollection<ProjectUpdater> updaters = new ObservableCollection<ProjectUpdater>();

        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public ObservableCollection<RvtFile> Files { get { return files; } set { files = value; NotifyPropertyChanged("Files"); } }
        public ObservableCollection<ProjectUpdater> Updaters { get { return updaters; } set { updaters = value; NotifyPropertyChanged("Updaters"); } }

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
