using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.WpfUtilities;

namespace HOK.MissionControl.FamilyPublish
{
    public class FamilyMonitorViewModel : ViewModelBase
    {
        public RelayCommand<Window> Close { get; }
        public RelayCommand<Window> Cancel { get; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand<Window> WindowShown { get; }
        public string Title { get; set; }
        public bool ExecuteFamilyPublish { get; set; } = false;
        private FamilyMonitorModel Model { get; }

        public FamilyMonitorViewModel(FamilyMonitorModel model, string message)
        {
            Message = message;
            Model = model;
            Close = new RelayCommand<Window>(OnClose);
            Cancel = new RelayCommand<Window>(OnCancel);
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowShown = new RelayCommand<Window>(OnWindowShown);
            Title = "Mission Control - Family Publish v." + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static void OnCancel(Window win)
        {
            StatusBarManager.Cancel = true;
            win.Close();
        }

        private void OnWindowShown(Window win)
        {
            if (!ExecuteFamilyPublish) return;

            Model.PublishData();
            win.Close();
        }

        /// <summary>
        /// Binds Progress/Status Bar Manager to controls.
        /// </summary>
        /// <param name="win">Window reference.</param>
        private static void OnWindowLoaded(Window win)
        {
            StatusBarManager.ProgressBar = ((FamilyMonitorView)win).progressBar;
            StatusBarManager.StatusLabel = ((FamilyMonitorView)win).statusLabel;
        }

        private static void OnClose(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(() => Message); }
        }
    }
}
