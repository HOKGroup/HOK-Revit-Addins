using System.ComponentModel;

namespace HOK.Core.Utilities
{
    public class ImageWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Instances { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
