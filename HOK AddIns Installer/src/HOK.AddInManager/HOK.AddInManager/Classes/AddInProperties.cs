using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using HOK.Core.Utilities;

namespace HOK.AddInManager.Classes
{
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Addins : INotifyPropertyChanged
    {
        private ObservableCollection<AddinInfo> addinCollection = new ObservableCollection<AddinInfo>();
        public ObservableCollection<AddinInfo> AddinCollection
        {
            get { return addinCollection; }
            set { addinCollection = value; NotifyPropertyChanged("AddinCollection"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    public enum LoadType
    {
        Never = 0,
        ThisSessionOnly = 1,
        Always = 2
    }

    [Serializable]
    public class AddinInfo : INotifyPropertyChanged
    {
        private string toolName = "";
        private LoadType toolLoadType = LoadType.Never;
        private string iconName = "";
        private string iconPath = "";
        private BitmapImage toolIcon;
        private string[] addinNames;
        private string[] addInPaths;
        private string[] installPaths;
        private int index;
        private string tooltip = "";
        private string url = "";

        private LoadType[] loadTypes = {LoadType.Never, LoadType.ThisSessionOnly, LoadType.Always};
        
        [XmlAttribute]
        public string ToolName
        {
            get { return toolName; }
            set { toolName = value; NotifyPropertyChanged("ToolName"); }
        }

        [XmlAttribute]
        public LoadType ToolLoadType
        {
            get { return toolLoadType; }
            set { toolLoadType = value; NotifyPropertyChanged("ToolLoadType"); }
        }

        [XmlIgnore]
        public string IconName
        {
            get { return iconName; }
            set { iconName = value; NotifyPropertyChanged("IconName"); }
        }

        [XmlIgnore]
        public string IconPath
        {
            get { return iconPath; }
            set { iconPath = value; NotifyPropertyChanged("IconPath"); }
        }

        [XmlIgnore]
        public BitmapImage ToolIcon
        {
            get { return toolIcon; }
            set { toolIcon = value; NotifyPropertyChanged("ToolIcon"); }
        }

        [XmlIgnore]
        public string[] AddInNames
        {
            get { return addinNames; }
            set { addinNames = value; NotifyPropertyChanged("AddInNames"); }
        }

        [XmlIgnore]
        public string[] AddInPaths
        {
            get { return addInPaths; }
            set { addInPaths = value; NotifyPropertyChanged("AddInPaths"); }
        }

        [XmlIgnore]
        public string[] InstallPaths
        {
            get { return installPaths; }
            set { installPaths = value; NotifyPropertyChanged("InstallPaths"); }
        }

        [XmlIgnore]
        public int Index
        {
            get { return index; }
            set { index = value; NotifyPropertyChanged("Index"); }
        }

        [XmlIgnore]
        public string Tooltip
        {
            get { return tooltip; }
            set { tooltip = value; NotifyPropertyChanged("Tooltip"); }
        }

        [XmlIgnore]
        public string Url
        {
            get { return url; }
            set { url = value; NotifyPropertyChanged("Url"); }
        }

        [XmlIgnore]
        public LoadType[] LoadTypes
        {
            get { return loadTypes; }
            set { loadTypes = value; NotifyPropertyChanged("LoadTypes"); }
        }

        public void GetDetailInfo(string sourceDirectory, string installDirectory)
        {
            try
            {
                var iconDirectory = Path.Combine(sourceDirectory, "Icons");
                iconPath = Path.Combine(iconDirectory, iconName);

                //Tool Icon
                if (File.Exists(iconPath))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(iconPath);
                    image.EndInit();
                    toolIcon = image;
                }

                addInPaths = new string[addinNames.Length];
                installPaths = new string[addinNames.Length];
                for (var i = 0; i < addinNames.Length; i++)
                {
                    var name = addinNames[i];
                    var addinPath = Path.Combine(sourceDirectory, name);
                    addInPaths[i] = addinPath;

                    var installPath = Path.Combine(installDirectory, name);
                    installPaths[i] = installPath;
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
