using System.ComponentModel;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace HOK.Core.Utilities
{
    public sealed class AddinWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Panel { get; set; }
        public string ButtonText { get; set; }
        public string FullName { get; set; }
        public string ImageName { get; set; }

        [JsonIgnore]
        public BitmapSource Image { get; set; }

        private bool _install;
        public bool Install
        {
            get => _install;
            set { _install = value; RaisePropertyChanged("Install"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
