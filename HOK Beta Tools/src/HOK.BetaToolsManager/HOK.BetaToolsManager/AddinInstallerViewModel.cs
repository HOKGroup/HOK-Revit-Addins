using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using HOK.Core.Utilities;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerViewModel : ViewModelBase
    {
        public AddinInstallerModel Model;
        public ObservableCollection<AddinWrapper> Addins { get; set; }

        public AddinInstallerViewModel(AddinInstallerModel model)
        {
            Model = model;
            Addins = Model.LoadAddins();
        }
    }
}
