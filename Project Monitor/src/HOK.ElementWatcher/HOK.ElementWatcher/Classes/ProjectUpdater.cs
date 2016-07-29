using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace HOK.ElementWatcher.Classes
{
    public class ProjectUpdater : INotifyPropertyChanged
    {
        private string id = "";
        private string updaterId = "";
        private string updaterName = "";
        private string addInId = "";
        private string addInName = "";
        private bool isUpdaterOn = false;
        private string projectFileId = "";
        private ObservableCollection<CategoryTrigger> categoryTriggers = new ObservableCollection<CategoryTrigger>();

        public string _id { get { return id; } set { id = value; NotifyPropertyChanged("_id"); } }
        public string UpdaterId { get { return updaterId; } set { updaterId = value; NotifyPropertyChanged("UpdaterId"); } }
        public string UpdaterName { get { return updaterName; } set { updaterName = value; NotifyPropertyChanged("UpdaterName"); } }
        public string AddInId { get { return addInId; } set { addInId = value; NotifyPropertyChanged("AddInId"); } }
        public string AddInName { get { return addInName; } set { addInName = value; NotifyPropertyChanged("AddInName"); } }
        public bool IsUpdaterOn { get { return isUpdaterOn; } set { isUpdaterOn = value; NotifyPropertyChanged("IsUpdaterOn"); } }
        public string ProjectFile_Id { get { return projectFileId; } set { projectFileId = value; NotifyPropertyChanged("ProjectFile_Id"); } }
        [ScriptIgnore]
        public ObservableCollection<CategoryTrigger> CategoryTriggers { get { return categoryTriggers; } set { categoryTriggers = value; NotifyPropertyChanged("CategoryTriggers"); } }

        public ProjectUpdater()
        {
        }

        public ProjectUpdater(string objId, string uId, string uName, string aId, string aName, bool updaterOn, string fileId)
        {
            id = objId;
            updaterId = uId;
            updaterName = uName;
            addInId = aId;
            addInName = aName;
            isUpdaterOn = updaterOn;
            projectFileId = fileId;
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
