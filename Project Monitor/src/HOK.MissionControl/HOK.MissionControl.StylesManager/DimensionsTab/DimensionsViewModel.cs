using System.Collections;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace HOK.MissionControl.StylesManager.DimensionsTab
{
    public class DimensionsViewModel : ViewModelBase
    {
        public DimensionsModel Model { get; set; }
        public ObservableCollection<DimensionTypeWrapper> Dimensions { get; set; }
        public IList SelectedRows { get; set; }
        public RelayCommand SelectAll { get; set; }
        public RelayCommand SelectNone { get; set; }

        public DimensionsViewModel(DimensionsModel model)
        {
            Model = model;
            SelectAll = new RelayCommand(OnSelectAll);
            SelectNone = new RelayCommand(OnSelectNone);
        }

        private void OnSelectNone()
        {
            foreach (var wrapper in Dimensions)
            {
                wrapper.IsSelected = false;
            }
        }

        private void OnSelectAll()
        {
            foreach (var wrapper in Dimensions)
            {
                wrapper.IsSelected = true;
            }
        }
    }
}
