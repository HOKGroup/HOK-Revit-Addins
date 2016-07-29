using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementWatcher.Classes
{

    public class RequestQueue : INotifyPropertyChanged
    {
        private string id = "";
        private string categoryTriggerId = "";
        private string state = "";
        private string requestedBy = "";
        private DateTime requested = DateTime.Now;
        private string elementUniqueId = "";

        public string _id { get { return id; } set { id = value; NotifyPropertyChanged("_id"); } }
        public string CategoryTrigger_Id { get { return categoryTriggerId; } set { categoryTriggerId = value; NotifyPropertyChanged("CategoryTrigger_Id"); } }
        public string State { get { return state; } set { state = value; NotifyPropertyChanged("State"); } }
        public string RequestedBy { get { return requestedBy; } set { requestedBy = value; NotifyPropertyChanged("RequestedBy"); } }
        public DateTime Requested { get { return requested; } set { requested = value; NotifyPropertyChanged("Requested"); } }
        public string ElementUniqueId { get { return elementUniqueId; } set { elementUniqueId = value; NotifyPropertyChanged("ElementUniqueId"); } }

        public RequestQueue()
        {
        }

        public RequestQueue(string qId, string triggerId, string strState, string requester, string uniqueId)
        {
            id = qId;
            categoryTriggerId = triggerId;
            state = strState;
            requestedBy = requester;
            elementUniqueId = uniqueId;
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

    public enum RequestState
    {
        Unknown, Requested, Accepted, Ignored
    }
}
