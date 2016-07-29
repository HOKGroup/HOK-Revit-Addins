using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class LinkedProject : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string projectNumber = "";
        private string projectName = "";
        private string filePath = "";
        private DateTime linkedDate = DateTime.MinValue;
        private string linkedBy = "";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; NotifyPropertyChanged("ProjectNumber"); } }
        public string ProjectName { get { return projectName; } set { projectName = value; NotifyPropertyChanged("ProjectName"); } }
        public string FilePath { get { return filePath; } set { filePath = value; NotifyPropertyChanged("FilePath"); } }
        public DateTime LinkedDate { get { return linkedDate; } set { linkedDate = value; NotifyPropertyChanged("LinkedDate"); } }
        public string LinkedBy { get { return linkedBy; } set { linkedBy = value; NotifyPropertyChanged("LinkedBy"); } }

        public LinkedProject()
        {
        }

        public LinkedProject(Guid projectId)
        {
            id = projectId;
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
