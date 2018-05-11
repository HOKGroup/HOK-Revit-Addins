using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.Core.WpfUtilities.FeedbackUI;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerViewModel : ViewModelBase
    {
        public AddinInstallerModel Model;
        public string Title { get; set; }
        public RelayCommand<Window> CloseCommand { get; set; }
        public RelayCommand CheckAll { get; set; }
        public RelayCommand CheckNone { get; set; }
        public RelayCommand<Window> InstallCommand { get; set; }
        public RelayCommand<Window> UninstallCommand { get; set; }
        public RelayCommand<Window> WindowLoaded { get; set; }
        public RelayCommand<Window> WindowClosing { get; set; }
        public RelayCommand SubmitComment { get; set; }

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
            SubmitComment = new RelayCommand(OnSubmitComment);
        }

        private static void OnSubmitComment()
        {
            var title = "Beta Tools - Beta Installer v." + Assembly.GetExecutingAssembly().GetName().Version;
            var model = new FeedbackModel();
            var viewModel = new FeedbackViewModel(model, title);
            var view = new FeedbackView
            {
                DataContext = viewModel
            };

            var unused = new WindowInteropHelper(view)
            {
                Owner = Process.GetCurrentProcess().MainWindowHandle
            };

            view.ShowDialog();
        }

        private void OnWindowClosing(Window obj)
        {
            foreach (var addin in Addins)
            {
                addin.AutoUpdate = true;
            }
        }

        private void OnWindowLoaded(Window win)
        {
            OnCheckNone();
            StatusBarManager.StatusLabel = ((AddinInstallerWindow)win).statusLabel;
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
    }
}
