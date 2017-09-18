using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerViewModel : ViewModelBase
    {
        public AddinInstallerModel Model;
        public string Title { get; set; }
        public RelayCommand<Window> CloseCommand { get; }
        public RelayCommand CheckAll { get; }
        public RelayCommand CheckNone { get; }
        public RelayCommand<Window> InstallCommand { get; }
        public RelayCommand<Window> UninstallCommand { get; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand<Window> WindowClosing { get; }

        public AddinInstallerViewModel(AddinInstallerModel model)
        {
            Model = model;
            Addins = Model.Addins;
            Title = "Beta Tools - Beta Installer v." + Assembly.GetExecutingAssembly().GetName().Version;

            CloseCommand = new RelayCommand<Window>(OnCloseCommand);
            CheckAll = new RelayCommand(OnCheckAll);
            CheckNone = new RelayCommand(OnCheckNone);
            InstallCommand = new RelayCommand<Window>(OnInstall);
            UninstallCommand = new RelayCommand<Window>(OnUninstall);
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowClosing = new RelayCommand<Window>(OnWindowClosing);
        }

        private void OnWindowClosing(Window obj)
        {
            foreach (var addin in Addins)
            {
                if (AutoUpdateStatus != null) addin.AutoUpdate = (bool) AutoUpdateStatus;
            }
        }

        private void OnWindowLoaded(Window win)
        {
            OnCheckNone();
            AutoUpdateStatus = Addins.FirstOrDefault()?.AutoUpdate;
        }

        private void OnUninstall(Window win)
        {
            Model.UninstallAddins(Addins);
            win.Close();
        }

        private void OnInstall(Window win)
        {
            Model.InstallUpdateAddins(Addins);
            MessageBox.Show("Please restart Revit for new plug-ins to work.",
                "Beta Tools - Beta Installer", MessageBoxButton.OK, MessageBoxImage.Information);
            win.Close();
        }

        private void OnCheckNone()
        {
            foreach (var addin in Addins)
            {
                addin.IsSelected = false;
            }
        }

        private void OnCheckAll()
        {
            foreach (var addin in Addins)
            {
                addin.IsSelected = true;
            }
        }

        private static void OnCloseCommand(Window win)
        {
            win.Close();
        }

        private ObservableCollection<AddinWrapper> _addins = new ObservableCollection<AddinWrapper>();
        public ObservableCollection<AddinWrapper> Addins
        {
            get { return _addins; }
            set { _addins = value; RaisePropertyChanged(() => Addins); }
        }

        private bool? _autoUpdateStatus;
        public bool? AutoUpdateStatus
        {
            get { return _autoUpdateStatus; }
            set { _autoUpdateStatus = value; RaisePropertyChanged(() => AutoUpdateStatus); }
        }
    }
}
