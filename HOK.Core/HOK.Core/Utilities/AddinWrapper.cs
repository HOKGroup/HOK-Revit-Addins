using System.ComponentModel;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace HOK.Core.Utilities
{
    public sealed class AddinWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; } // used for display in datagrid
        public string Description { get; set; } // used for display in datagrid
        public string Panel { get; set; } // used to find button to be disabled when uninstalling addin
        public string ButtonText { get; set; } // used to find button to be disabled when uninstalling addin
        public string ImageName { get; set; } // used to retrieve image for datagrid
        public string Version { get; set; } // used for display in datagrid
        public string BetaResourcesPath { get; set; } //used by install to copy dependancies
        public string AddinFilePath { get; set; } // used by install/uninstall to copy addin file
        public string DllRelativePath { get; set; }
        public string CommandNamespace { get; set; }
        public bool IsInstalled { get; set; }

        [JsonIgnore]
        public BitmapSource Image { get; set; } // used be the datagrid

        /// <summary>
        /// Assembly version number for display in DataGrid.
        /// </summary>
        private string _installedVersion;
        public string InstalledVersion
        {
            get => _installedVersion;
            set { _installedVersion = value; RaisePropertyChanged("InstalledVersion"); }
        }

        /// <summary>
        /// Boolean flag indicating if Addin will be installed/uninstalled.
        /// </summary>
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
