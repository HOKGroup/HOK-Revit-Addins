using System.ComponentModel;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace HOK.Core.Utilities
{
    public sealed class AddinWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; } // used to create ribbon button
        public string Description { get; set; } // used to create ribbon button
        public string Panel { get; set; } // used to find button to be disabled when uninstalling addin
        public string ButtonText { get; set; } // used to find button to be disabled when uninstalling addin
        public string ImageName { get; set; } // used to retrieve image for datagrid
        public string ImageNamespace { get; set; } // 
        public string Version { get; set; } = ""; // version of addin in beta
        public string BetaResourcesPath { get; set; } //used by install to copy dependancies
        public string AddinFilePath { get; set; } // used by install/uninstall to copy addin file
        public string DllRelativePath { get; set; } // used to check if addin is installed and to track main DLL for each addin
        public string CommandNamespace { get; set; } // used to create ribbon button
        public bool IsInstalled { get; set; } // indicates if addin is installed on user computer by checking if dll exists in directory

        [JsonIgnore]
        public BitmapSource Image { get; set; } // used be the datagrid

        /// <summary>
        /// Assembly version number for display in DataGrid.
        /// </summary>
        private bool _versionsMatch;
        public bool VersionsMatch
        {
            get
            {
                _versionsMatch = Version == InstalledVersion;
                return _versionsMatch;
            }
            set { _versionsMatch = value; RaisePropertyChanged("VersionsMatch"); }
        }

        /// <summary>
        /// Assembly version number for display in DataGrid.
        /// </summary>
        private string _installedVersion = "";
        public string InstalledVersion
        {
            get { return _installedVersion; }
            set { _installedVersion = value; RaisePropertyChanged("InstalledVersion"); }
        }

        /// <summary>
        /// Boolean flag indicating if Addin will be installed/uninstalled.
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        private bool _autoUpdate;
        public bool AutoUpdate
        {
            get { return _autoUpdate; }
            set { _autoUpdate = value; RaisePropertyChanged("AutoUpdate"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
