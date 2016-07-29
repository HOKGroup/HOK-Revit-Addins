using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Classes
{
    public class ReportingElementInfo : INotifyPropertyChanged
    {
        private Guid docId = Guid.Empty;
        private string triggerId = "";
        private string categoryName = "";
        private ElementId reportingElementId = ElementId.InvalidElementId;
        private string reportingUniqueId = "";

        public Guid DocId { get { return docId; } set { docId = value; NotifyPropertyChanged("DocId"); } }
        public string TriggerId { get { return triggerId; } set { triggerId = value; NotifyPropertyChanged("TriggerId"); } }
        public string CategoryName { get { return categoryName; } set { categoryName = value; NotifyPropertyChanged("CategoryName"); } }
        public ElementId ReportingElementId { get { return reportingElementId; } set { reportingElementId = value; NotifyPropertyChanged("ReportingElementId"); } }
        public string ReportingUniqueId { get { return reportingUniqueId; } set { reportingUniqueId = value; NotifyPropertyChanged("ReportingUniqueId"); } }

        public ReportingElementInfo()
        {

        }

        public ReportingElementInfo(Guid dId, string tId, string catName, ElementId eId, string uniqueId)
        {
            docId = dId;
            triggerId = tId;
            categoryName = catName;
            reportingElementId = eId;
            ReportingUniqueId = uniqueId;
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
