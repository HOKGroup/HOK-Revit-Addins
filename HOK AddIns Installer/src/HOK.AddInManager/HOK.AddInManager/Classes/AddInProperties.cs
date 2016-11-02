using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HOK.AddInManager.Classes
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Addins : INotifyPropertyChanged
    {
        private ObservableCollection<AddinInfo> addinCollection = new ObservableCollection<AddinInfo>();

        public ObservableCollection<AddinInfo> AddinCollection { get { return addinCollection; } set { addinCollection = value; NotifyPropertyChanged("AddinCollection"); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public Addins()
        {
        }

    }

    public enum LoadType
    {
        Never = 0, ThisSessionOnly = 1, Always = 2
    }

    [System.SerializableAttribute()]
    public class AddinInfo:INotifyPropertyChanged
    {
        private string toolName = "";
        private LoadType toolLoadType = LoadType.Never;

        private string iconName = "";
        private string iconPath = "";
        private BitmapImage toolIcon = null;
        private string[] addinNames;
        private string[] addInPaths;
        private string[] installPaths;
        private int index = 0;
        private string tooltip = "";
        private string url = "";

        private LoadType[] loadTypes = new LoadType[] {LoadType.Never, LoadType.ThisSessionOnly, LoadType.Always};
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ToolName { get { return toolName; } set { toolName = value; NotifyPropertyChanged("ToolName"); } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public LoadType ToolLoadType { get { return toolLoadType; } set { toolLoadType = value; NotifyPropertyChanged("ToolLoadType"); } }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string IconName { get { return iconName; } set { iconName = value; NotifyPropertyChanged("IconName"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string IconPath { get { return iconPath; } set { iconPath = value; NotifyPropertyChanged("IconPath"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public BitmapImage ToolIcon { get { return toolIcon; } set { toolIcon = value; NotifyPropertyChanged("ToolIcon"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string[] AddInNames { get { return addinNames; } set { addinNames = value; NotifyPropertyChanged("AddInNames"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string[] AddInPaths { get { return addInPaths; } set { addInPaths = value; NotifyPropertyChanged("AddInPaths"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string[] InstallPaths { get { return installPaths; } set { installPaths = value; NotifyPropertyChanged("InstallPaths"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int Index { get { return index; } set { index = value; NotifyPropertyChanged("Index"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Tooltip { get { return tooltip; } set { tooltip = value; NotifyPropertyChanged("Tooltip"); } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Url { get { return url; } set { url = value; NotifyPropertyChanged("Url"); } }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public LoadType[] LoadTypes { get { return loadTypes; } set { loadTypes = value; NotifyPropertyChanged("LoadTypes"); } }


        public AddinInfo()
        {
        }

        public void GetDetailInfo(string sourceDirectory, string installDirectory)
        {
            try
            {
                string iconDirectory = Path.Combine(sourceDirectory, "Icons");
                iconPath = Path.Combine(iconDirectory, iconName);

                //Tool Icon
                if (File.Exists(iconPath))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(iconPath);
                    image.EndInit();
                    toolIcon = image;
                }

                addInPaths = new string[addinNames.Length];
                installPaths = new string[addinNames.Length];
                for (int i = 0; i < addinNames.Length; i++)
                {
                    string name = addinNames[i];
                    string addinPath = Path.Combine(sourceDirectory, name);
                    addInPaths[i] = addinPath;

                    string installPath = Path.Combine(installDirectory, name);
                    installPaths[i] = installPath;
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
