using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControlTest.Classes
{
    public class ProjectUpdater : INotifyPropertyChanged
    {
        private string updaterId = "";
        private string updaterName = "";
        private string description = "";
        private string addInId = "";
        private string addInName = "";
        private bool isUpdaterOn = false;
        private ObservableCollection<CategoryTrigger> categoryTriggers = new ObservableCollection<CategoryTrigger>();

        public string UpdaterId { get { return updaterId; } set { updaterId = value; NotifyPropertyChanged("UpdaterId"); } }
        public string UpdaterName { get { return updaterName; } set { updaterName = value; NotifyPropertyChanged("UpdaterName"); } }
        public string Description { get { return description; } set { description = value; NotifyPropertyChanged("Description"); } }
        public string AddInId { get { return addInId; } set { addInId = value; NotifyPropertyChanged("AddInId"); } }
        public string AddInName { get { return addInName; } set { addInName = value; NotifyPropertyChanged("AddInName"); } }
        public bool IsUpdaterOn { get { return isUpdaterOn; } set { isUpdaterOn = value; NotifyPropertyChanged("IsUpdaterOn"); } }
        public ObservableCollection<CategoryTrigger> CategoryTriggers { get { return categoryTriggers; } set { categoryTriggers = value; NotifyPropertyChanged("CategoryTriggers"); } }

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
