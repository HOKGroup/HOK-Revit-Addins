using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace HOK.ElementWatcher.Classes
{
    public class ProjectFile : INotifyPropertyChanged
    {
        private string id = "";
        private string centralPath = "";
        private string projectId = "";
        private Project projectInfo = new Project();
        
        public string _id { get { return id; } set { id = value; NotifyPropertyChanged("_id"); } } // document GUID
        public string CentralPath { get { return centralPath; } set { centralPath = value; NotifyPropertyChanged("CentralPath"); } }
        public string Project_Id { get { return projectId; } set { projectId = value; NotifyPropertyChanged("Project_Id"); } }
        [ScriptIgnore]
        public Project ProjectInfo { get { return projectInfo; } set { projectInfo = value; NotifyPropertyChanged("ProjectInfo"); } }

        public ProjectFile()
        {
        }

        public ProjectFile(string guid, string path, string proId, Project project)
        {
            id = guid;
            centralPath = path;
            projectId = proId;
            projectInfo = project;
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
