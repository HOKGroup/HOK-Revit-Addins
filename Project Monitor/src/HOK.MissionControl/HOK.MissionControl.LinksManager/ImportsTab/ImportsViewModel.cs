using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.ElementWrapers;

namespace HOK.MissionControl.LinksManager.ImportsTab
{
    public class ImportsViewModel : ViewModelBase
    {
        public ImportsModel Model { get; set; }
        public ObservableCollection<CadLinkTypeWrapper> Imports { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }

        public ImportsViewModel(ImportsModel model)
        {
            Model = model;
            Imports = Model.Imports;

            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
        }

        private void OnDelete()
        {
            var deleted = Model.Delete(Imports);

            foreach (var i in deleted)
            {
                Imports.Remove(i);
            }
        }

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }
    }
}
