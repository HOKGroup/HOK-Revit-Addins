using Autodesk.Revit.DB;
using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.AddIn.Classes
{
    public class SheetManagerConfiguration :INotifyPropertyChanged
    {
        private Guid modelId = Guid.Empty;
        private string configId = "";
        private string centralPath = "";
        private string databaseFile = "";
        private ElementId titleblockId = ElementId.InvalidElementId;
        private bool isPlaceholder = false;
        private bool autoUpdate = false;

        public Guid ModelId { get { return modelId; } set { modelId = value; NotifyPropertyChanged("ModelId"); } }
        public string ConfigId { get { return configId; } set { configId = value; NotifyPropertyChanged("ConfigId"); } }
        public string CentralPath { get { return centralPath; } set { centralPath = value; NotifyPropertyChanged("CentralPath"); } }
        public string DatabaseFile { get { return databaseFile; } set { databaseFile = value; NotifyPropertyChanged("DatabaseFile"); } }
        public ElementId TitleblockId { get { return titleblockId; } set { titleblockId = value; NotifyPropertyChanged("TitleblockId"); } }
        public bool IsPlaceholder { get { return isPlaceholder; } set { isPlaceholder = value; NotifyPropertyChanged("IsPlaceholder"); } }
        public bool AutoUpdate { get { return autoUpdate; } set { autoUpdate = value; NotifyPropertyChanged("AutoUpdate"); } }

        public SheetManagerConfiguration() { }

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
