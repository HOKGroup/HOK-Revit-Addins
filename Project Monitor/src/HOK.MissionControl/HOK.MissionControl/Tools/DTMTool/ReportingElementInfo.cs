using Autodesk.Revit.DB;
using System.ComponentModel;

namespace HOK.MissionControl.Tools.DTMTool
{
    public class ReportingElementInfo : INotifyPropertyChanged
    {
        private string configId = "";
        public string ConfigId
        {
            get => configId;
            set { configId = value; NotifyPropertyChanged("ConfigId"); }
        }

        private string updaterId = "";
        public string UpdaterId
        {
            get => updaterId;
            set { updaterId = value; NotifyPropertyChanged("UpdaterId"); }
        }

        private string centralPath = "";
        public string CentralPath
        {
            get => centralPath;
            set { centralPath = value; NotifyPropertyChanged("CentralPath"); }
        }

        private string categoryName = "";
        public string CategoryName
        {
            get => categoryName;
            set { categoryName = value; NotifyPropertyChanged("CategoryName"); }
        }

        private string description = "";
        public string Description
        {
            get => description;
            set { description = value; NotifyPropertyChanged("Description"); }
        }

        private ElementId reportingElementId = ElementId.InvalidElementId;
        public ElementId ReportingElementId
        {
            get => reportingElementId;
            set { reportingElementId = value; NotifyPropertyChanged("ReportingElementId"); }
        }

        private string reportingUniqueId = "";
        public string ReportingUniqueId
        {
            get => reportingUniqueId;
            set { reportingUniqueId = value; NotifyPropertyChanged("ReportingUniqueId"); }
        }

        public ReportingElementInfo()
        {

        }

        public ReportingElementInfo(string cId, string uId, string path, string catName, string message, ElementId eId, string uniqueId)
        {
            configId = cId;
            updaterId = uId;
            centralPath = path;
            categoryName = catName;
            description = message;
            reportingElementId = eId;
            ReportingUniqueId = uniqueId;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
