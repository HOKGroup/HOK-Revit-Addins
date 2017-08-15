using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.ElementWrapers;
using System.Linq;

namespace HOK.MissionControl.LinksManager.StylesTab
{
    public class StylesViewModel : ViewModelBase
    {
        public StylesModel Model { get; set; }
        public ObservableCollection<CategoryWrapper> Styles { get; set; }
        public RelayCommand Delete { get; }
        public RelayCommand<UserControl> Close { get; }
        public IList SelectedRows { get; set; }

        public StylesViewModel(StylesModel model)
        {
            Model = model;
            Styles = Model.Styles;

            Delete = new RelayCommand(OnDelete);
            Close = new RelayCommand<UserControl>(OnClose);
        }

        private void OnDelete()
        {
            var wrappers = SelectedRows.Cast<CategoryWrapper>().ToList();
            var deleted = Model.Delete(wrappers);

            foreach (var i in deleted)
            {
                Styles.Remove(i);
            }
        }

        private static void OnClose(UserControl control)
        {
            var win = Window.GetWindow(control);
            win?.Close();
        }
    }
}
