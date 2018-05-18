using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using HOK.Core.Utilities;

namespace HOK.AddInManager.Classes
{
    [Serializable]
    public class AddinInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="installDirectory"></param>
        public void GetDetailInfo(string sourceDirectory, string installDirectory)
        {
            try
            {
                var iconDirectory = Path.Combine(sourceDirectory, "Icons");
                _iconPath = Path.Combine(iconDirectory, _iconName);

                //Tool Icon
                if (File.Exists(_iconPath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(_iconPath);
                    image.EndInit();
                    _toolIcon = image;
                }

                _addInPaths = new string[_addinNames.Length];
                _installPaths = new string[_addinNames.Length];
                for (var i = 0; i < _addinNames.Length; i++)
                {
                    var name = _addinNames[i];
                    var addinPath = Path.Combine(sourceDirectory, name);
                    _addInPaths[i] = addinPath;

                    var installPath = Path.Combine(installDirectory, name);
                    _installPaths[i] = installPath;
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private string _toolName = string.Empty;
        [XmlAttribute]
        public string ToolName
        {
            get { return _toolName; }
            set { _toolName = value; NotifyPropertyChanged("ToolName"); }
        }

        private LoadType _toolLoadType = LoadType.Never;
        [XmlAttribute]
        public LoadType ToolLoadType
        {
            get { return _toolLoadType; }
            set { _toolLoadType = value; NotifyPropertyChanged("ToolLoadType"); }
        }

        private string _iconName = string.Empty;
        [XmlIgnore]
        public string IconName
        {
            get { return _iconName; }
            set { _iconName = value; NotifyPropertyChanged("IconName"); }
        }

        private string _iconPath = string.Empty;
        [XmlIgnore]
        public string IconPath
        {
            get { return _iconPath; }
            set { _iconPath = value; NotifyPropertyChanged("IconPath"); }
        }

        private BitmapImage _toolIcon;
        [XmlIgnore]
        public BitmapImage ToolIcon
        {
            get { return _toolIcon; }
            set { _toolIcon = value; NotifyPropertyChanged("ToolIcon"); }
        }


        private string[] _addinNames;
        [XmlIgnore]
        public string[] AddInNames
        {
            get { return _addinNames; }
            set { _addinNames = value; NotifyPropertyChanged("AddInNames"); }
        }

        private string[] _addInPaths;
        [XmlIgnore]
        public string[] AddInPaths
        {
            get { return _addInPaths; }
            set { _addInPaths = value; NotifyPropertyChanged("AddInPaths"); }
        }

        private string[] _installPaths;
        [XmlIgnore]
        public string[] InstallPaths
        {
            get { return _installPaths; }
            set { _installPaths = value; NotifyPropertyChanged("InstallPaths"); }
        }

        private int _index;
        [XmlIgnore]
        public int Index
        {
            get { return _index; }
            set { _index = value; NotifyPropertyChanged("Index"); }
        }

        private string _tooltip = string.Empty;
        [XmlIgnore]
        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; NotifyPropertyChanged("Tooltip"); }
        }

        private string _url = string.Empty;
        [XmlIgnore]
        public string Url
        {
            get { return _url; }
            set { _url = value; NotifyPropertyChanged("Url"); }
        }

        private LoadType[] _loadTypes =
        {
            LoadType.Never,
            LoadType.ThisSessionOnly,
            LoadType.Always
        };
        [XmlIgnore]
        public LoadType[] LoadTypes
        {
            get { return _loadTypes; }
            set { _loadTypes = value; NotifyPropertyChanged("LoadTypes"); }
        }

        private bool _requiresRestart;
        [XmlIgnore]
        public bool RequiresRestart
        {
            get { return _requiresRestart; }
            set { _requiresRestart = value; NotifyPropertyChanged("RequiresRestart"); }
        }

        #region Utilities

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #endregion
    }

    public enum LoadType
    {
        Never = 0,
        ThisSessionOnly = 1,
        Always = 2
    }
}
