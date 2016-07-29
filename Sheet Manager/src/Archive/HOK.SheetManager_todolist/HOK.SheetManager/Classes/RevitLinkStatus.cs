using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Classes
{
    public class RevitLinkStatus : INotifyPropertyChanged
    {
        private bool isSelected = false;
        private bool isLinked = false;
        private bool modified = false;
        private string currentLinkedId = "";
        private int linkedElementId = -1;
        private string toolTip = "Not Linked.";

        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; NotifyPropertyChanged("IsLinked"); } }
        public bool Modified { get { return modified; } set { modified = value; NotifyPropertyChanged("Modified"); } }
        public string CurrentLinkedId { get { return currentLinkedId; } set { currentLinkedId = value; NotifyPropertyChanged("CurrentLinkedId"); } }
        public int LinkedElementId { get { return linkedElementId; } set { linkedElementId = value; NotifyPropertyChanged("LinkedElementId"); } }
        public string ToolTip { get { return toolTip; } set { toolTip = value; NotifyPropertyChanged("ToolTip"); } }

        public RevitLinkStatus()
        {
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
