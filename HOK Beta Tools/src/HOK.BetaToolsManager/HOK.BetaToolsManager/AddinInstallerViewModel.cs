using System.Collections.ObjectModel;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerViewModel : ViewModelBase
    {
        public AddinInstallerModel Model;
        public RelayCommand<Window> CloseCommand { get; }
        public RelayCommand CheckAll { get; }
        public RelayCommand CheckNone { get; }
        public RelayCommand InstallCommand { get; }
        public RelayCommand UninstallCommand { get; }

        public AddinInstallerViewModel(AddinInstallerModel model)
        {
            Model = model;
            Addins = Model.Addins;
            CloseCommand = new RelayCommand<Window>(OnCloseCommand);
            CheckAll = new RelayCommand(OnCheckAll);
            CheckNone = new RelayCommand(OnCheckNone);
            InstallCommand = new RelayCommand(OnInstall);
            UninstallCommand = new RelayCommand(OnUninstall);
        }

        private void OnUninstall()
        {
            Model.UninstallAddins(Addins);
        }

        private void OnInstall()
        {
            Model.InstallUpdateAddins(Addins);
        }

        private void OnCheckNone()
        {
            foreach (var addin in Addins)
            {
                addin.Install = false;
            }
        }

        private void OnCheckAll()
        {
            foreach (var addin in Addins)
            {
                addin.Install = true;
            }
        }

        private static void OnCloseCommand(Window win)
        {
            win.Close();
        }

        private ObservableCollection<AddinWrapper> _addins = new ObservableCollection<AddinWrapper>();
        public ObservableCollection<AddinWrapper> Addins
        {
            get => _addins;
            set { _addins = value; RaisePropertyChanged(() => Addins); }
        }
    }
}
