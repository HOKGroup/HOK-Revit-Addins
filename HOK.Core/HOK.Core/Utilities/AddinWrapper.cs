using System.Collections.Generic;
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
        public string Version { get; set; }
        public bool ExternalCommand { get; set; }
        public List<string> ReferencedAssembliesNames { get; set; }
        public string HostDllName { get; set; }
        public string AddinName { get; set; }

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
