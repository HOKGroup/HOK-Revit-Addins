using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HOK.AddInManager.Classes
{
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Addins : INotifyPropertyChanged
    {
        private ObservableCollection<AddinInfo> _addinCollection = new ObservableCollection<AddinInfo>();
        public ObservableCollection<AddinInfo> AddinCollection
        {
            get { return _addinCollection; }
            set { _addinCollection = value; NotifyPropertyChanged("AddinCollection"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
