using System.Collections.Generic;
using System.ComponentModel;
using HOK.MissionControl.Core.Schemas.Settings;
// ReSharper disable UnusedMember.Local

namespace HOK.MissionControl.Core.Utils
{
    public sealed class AppSettings : INotifyPropertyChanged
    {
        public static AppSettings Instance { get; } = new AppSettings();

        public List<string> LocalPathRgx { get; set; } = new List<string>();
        public UserLocation UserLocation { get; set; }
        public TempLocation TempLocation { get; set; }
        public HOK.MissionControl.Core.Schemas.Settings.ProjectInfo ProjectInfo { get; set; }

        static AppSettings()
        {
        }

        private AppSettings()
        {
        }

        /// <summary>
        /// Utility for passing Settings properties into the Static class.
        /// </summary>
        /// <param name="settings"></param>
        public void SetSettings(HOK.MissionControl.Core.Schemas.Settings.Settings settings)
        {
            LocalPathRgx = settings.LocalPathRgx;
            UserLocation = settings.UserLocation;
            ProjectInfo = settings.ProjectInfo;
            TempLocation = settings.TempLocation;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
