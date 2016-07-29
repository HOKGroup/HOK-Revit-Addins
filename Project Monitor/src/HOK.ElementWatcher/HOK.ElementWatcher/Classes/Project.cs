using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Classes
{
    public class Project : INotifyPropertyChanged
    {
        private string id = "";
        private string projectNumber = "";
        private string projectName = "";
        private string office = "";

        public string _id { get { return id; } set { id = value; NotifyPropertyChanged("_id"); } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; NotifyPropertyChanged("ProjectNumber"); } }
        public string ProjectName { get { return projectName; } set { projectName = value; NotifyPropertyChanged("ProjectName"); } }
        public string Office { get { return office; } set { office = value; NotifyPropertyChanged("Office"); } }

        public Project()
        {

        }

        public Project(string objId, string pNumber, string pName, string officeName)
        {
            id = objId;
            projectNumber = pNumber;
            projectName = pName;
            office = officeName;
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
