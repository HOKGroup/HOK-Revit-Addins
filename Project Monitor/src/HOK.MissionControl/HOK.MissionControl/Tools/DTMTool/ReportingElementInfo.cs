using Autodesk.Revit.DB;
using System.ComponentModel;

namespace HOK.MissionControl.Tools.DTMTool
{
    public class ReportingElementInfo : INotifyPropertyChanged
    {
        private string configId = "";
        private string updaterId = "";
        private string centralPath = "";
        private string categoryName = "";
        private string description = "";
        private ElementId reportingElementId = ElementId.InvalidElementId;
        private string reportingUniqueId = "";

        public string ConfigId { get { return configId; } set { configId = value; NotifyPropertyChanged("ConfigId"); } }
        public string UpdaterId { get { return updaterId; } set { updaterId = value; NotifyPropertyChanged("UpdaterId"); } }
        public string CentralPath { get { return centralPath; } set { centralPath = value; NotifyPropertyChanged("CentralPath"); } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; NotifyPropertyChanged("CategoryName"); } }
        public string Description { get { return description; } set { description = value; NotifyPropertyChanged("Description"); } }
        public ElementId ReportingElementId { get { return reportingElementId; } set { reportingElementId = value; NotifyPropertyChanged("ReportingElementId"); } }
        public string ReportingUniqueId { get { return reportingUniqueId; } set { reportingUniqueId = value; NotifyPropertyChanged("ReportingUniqueId"); } }

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
