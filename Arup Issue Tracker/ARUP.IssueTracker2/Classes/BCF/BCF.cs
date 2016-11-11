using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ARUP.IssueTracker.Classes.BCF1
{
    public class BCF : INotifyPropertyChanged
    {
        public string path { get; set; }
        private string filename;
        private bool hasBeenSaved;
        private ObservableCollection<BCF1.IssueBCF> issues;

        public BCF()
        {
            hasBeenSaved = true;
            Filename = "New BCF Report";
            Issues = new ObservableCollection<BCF1.IssueBCF>();
            path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BCFtemp", System.IO.Path.GetRandomFileName());
        }
        public bool HasBeenSaved
        {
            get
            {
                return hasBeenSaved;
            }

            set
            {
                hasBeenSaved = value;
                NotifyPropertyChanged("HasBeenSaved");
            }
        }

        public string Filename
        {
            get
            {
                return filename;
            }

            set
            {
                filename = value;
                NotifyPropertyChanged("Filename");
            }
        }



        public ObservableCollection<BCF1.IssueBCF> Issues
        {
            get
            {
                return issues;
            }

            set
            {
                issues = value;
                NotifyPropertyChanged("Issues");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }
}
