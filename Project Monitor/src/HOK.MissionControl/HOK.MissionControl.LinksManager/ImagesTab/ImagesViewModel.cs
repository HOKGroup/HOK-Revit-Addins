using System.Collections;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.ElementWrapers;

namespace HOK.MissionControl.LinksManager.ImagesTab
{
    public class ImagesViewModel : ViewModelBase
    {
        public ImagesModel Model { get; set; }
        public ObservableCollection<ImageTypeWrapper> Images { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }
        public IList SelectedRows { get; set; }

        public ImagesViewModel(ImagesModel model)
        {
            Model = model;
            Images = Model.Images;

            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
        }

        private void OnDelete()
        {
            var wrappers = SelectedRows.Cast<ImageTypeWrapper>().ToList();
            var deleted = Model.Delete(wrappers);

            foreach (var i in deleted)
            {
                Images.Remove(i);
            }
        }

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }
    }
}
