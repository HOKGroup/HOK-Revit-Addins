using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControlTest.Classes
{
    public class RvtFile : INotifyPropertyChanged
    {
        private string centralPath = "";
        public string CentralPath { get { return centralPath; } set { centralPath = value; NotifyPropertyChanged("CentralPath"); } }

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
