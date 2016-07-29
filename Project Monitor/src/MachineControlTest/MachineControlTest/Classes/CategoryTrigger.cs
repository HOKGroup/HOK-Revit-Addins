using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineControlTest.Classes
{
    public class CategoryTrigger : INotifyPropertyChanged
    {
        private string categoryName = "";
        private bool isEnabled = false;
        private bool locked = false;
        private string modifiedBy = "";
        private DateTime modified = DateTime.Now;

        public string CategoryName { get { return categoryName; } set { categoryName = value; NotifyPropertyChanged("CategoryName"); } }
        public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; NotifyPropertyChanged("IsEnabled"); } }
        public bool Locked { get { return locked; } set { locked = value; NotifyPropertyChanged("Locked"); } }
        public string ModifiedBy { get { return modifiedBy; } set { modifiedBy = value; NotifyPropertyChanged("ModifiedBy"); } }
        public DateTime Modified { get { return modified; } set { modified = value; NotifyPropertyChanged("Modified"); } }

        public CategoryTrigger()
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
