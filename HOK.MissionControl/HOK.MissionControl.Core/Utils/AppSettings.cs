using System.Collections.Generic;
using System.ComponentModel;

namespace HOK.MissionControl.Core.Utils
{
    public sealed class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Instance { get; } = new AppSettings();

        public List<string> LocalPathRgx { get; set; } = new List<string>();

        static AppSettings()
        {
        }

        private AppSettings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
