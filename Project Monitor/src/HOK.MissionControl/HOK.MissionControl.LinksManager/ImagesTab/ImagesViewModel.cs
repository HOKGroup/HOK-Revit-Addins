using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;

namespace HOK.MissionControl.LinksManager.ImagesTab
{
    public class ImagesViewModel : ViewModelBase
    {
        public ImagesModel Model { get; set; }
        public RelayCommand CheckAll { get; }
        public RelayCommand CheckNone { get; }

        public ImagesViewModel(ImagesModel model)
        {
            Model = model;
            CheckAll = new RelayCommand(OnCheckAll);
            CheckNone = new RelayCommand(OnCheckNone);
            Images = Model.Images;
        }

        private ObservableCollection<ImageWrapper> _images = new ObservableCollection<ImageWrapper>();
        public ObservableCollection<ImageWrapper> Images
        {
            get { return _images; }
            set { _images = value; RaisePropertyChanged(() => Images); }
        }

        private void OnCheckNone()
        {
            foreach (var image in Images)
            {
                image.IsSelected = false;
            }
        }

        private void OnCheckAll()
        {
            foreach (var image in Images)
            {
                image.IsSelected = true;
            }
        }
    }
}
