
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
    public class CategoryTrigger : INotifyPropertyChanged
    {
        private string id = "";
        private string categoryName = "";
        private string projectUpdaterId = "";
        private bool isEnabled = false;
        private string modifiedBy = "";
        private DateTime modified = DateTime.Now;
        private ObservableCollection<RequestQueue> requests = new ObservableCollection<RequestQueue>();

        public string _id { get { return id; } set { id = value; NotifyPropertyChanged("_id"); } } //guid
        public string CategoryName { get { return categoryName; } set { categoryName = value; NotifyPropertyChanged("CategoryName"); } }
        public string ProjectUpdater_Id { get { return projectUpdaterId; } set { projectUpdaterId = value; NotifyPropertyChanged("ProjectUpdater_Id"); } } //guid
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; NotifyPropertyChanged("IsEnabled"); } }
        public string ModifiedBy { get { return modifiedBy; } set { modifiedBy = value; NotifyPropertyChanged("ModifiedBy"); } }
        public DateTime Modified { get { return modified; } set { modified = value; NotifyPropertyChanged("Modified"); } }
        [ScriptIgnore]
        public ObservableCollection<RequestQueue> Requests { get { return requests; } set { requests = value; NotifyPropertyChanged("Requests"); } }

        public CategoryTrigger()
        {

        }

        public CategoryTrigger(string idVal, string catName, string uId, bool triggerOn, string modifier, DateTime dateModified)
        {
            id = idVal;
            categoryName = catName;
            projectUpdaterId = uId;
            isEnabled = triggerOn;
            modifiedBy = modifier;
            modified = dateModified;
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
