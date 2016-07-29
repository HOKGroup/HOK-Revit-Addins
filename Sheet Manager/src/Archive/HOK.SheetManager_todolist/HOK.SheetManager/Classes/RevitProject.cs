using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class RevitProject : INotifyPropertyChanged
    {
        private Guid id = Guid.Empty;
        private string projectNumber = "";
        private string projectName = "";
        private string filePath = "";
        private string fileName = "";
        private DateTime linkedDate = DateTime.MinValue;
        private DateTime lastLinked = DateTime.MinValue;
        private string linkedBy = "";

        public Guid Id { get { return id; } set { id = value; NotifyPropertyChanged("Id"); } }
        public string ProjectNumber { get { return projectNumber; } set { projectNumber = value; NotifyPropertyChanged("ProjectNumber"); } }
        public string ProjectName { get { return projectName; } set { projectName = value; NotifyPropertyChanged("ProjectName"); } }
        public string FilePath { get { return filePath; } set { filePath = value; NotifyPropertyChanged("FilePath"); } }
        public string FileName { get { return fileName; } set { fileName = value; NotifyPropertyChanged("FileName"); } }
        public DateTime LinkedDate { get { return linkedDate; } set { linkedDate = value; NotifyPropertyChanged("LinkedDate"); } }
        public DateTime LastLinked { get { return lastLinked; } set { lastLinked = value; NotifyPropertyChanged("LastLinked"); } }
        public string LinkedBy { get { return linkedBy; } set { linkedBy = value; NotifyPropertyChanged("LinkedBy"); } }

        public RevitProject()
        {
        }

        public RevitProject(Guid projectId)
        {
            id = projectId;
        }

        public static void GetProjectInfo(string centralPath, out string projectNumber, out string projectName)
        {
            projectNumber = "00.00000.00";
            projectName = "Undefined";
            string[] prefixes = new string[] { "E-CAD", "E-BIM", "REVIT" };
            try
            {
                string regPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                Regex regex = new Regex(regPattern, RegexOptions.IgnoreCase);
                Match match = regex.Match(centralPath);
                if (match.Success)
                {
                    projectNumber = match.Groups[1].Value;
                    projectName = match.Groups[2].Value;
                }

                if (string.IsNullOrEmpty(projectNumber) || projectNumber == "00.00000.00")
                {
                    string[] paths = centralPath.Split('\\');
                    for (int i = 0; i < paths.Length; i++)
                    {
                        if (prefixes.Contains(paths[i]))
                        {
                            projectName = paths[i - 1];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
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
