using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class CommunicatorHealthReportViewModel : ViewModelBase
    {
        public CommunicatorHealthReportModel Model { get; set; } = new CommunicatorHealthReportModel();
        public RelayCommand<UserControl> WindowClosed { get; set; }
        private readonly object _lock = new object();

        public CommunicatorHealthReportViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_healthReports, _lock);
            WindowClosed = new RelayCommand<UserControl>(OnWindowClosed);

            Messenger.Default.Register<HealthReportSummaryAdded>(this, OnHealthReportSummaryAdded);
        }

        private void OnHealthReportSummaryAdded(HealthReportSummaryAdded obj)
        {
            var vm = Model.CreateSummary(obj);
            if (vm == null) return;

            lock (_lock)
            {
                HealthReports.Add(vm);
            }
        }

        private void OnWindowClosed(UserControl win)
        {
            // (Konrad) We need to unregister the event handler when window is closed, otherwise it will add another one next time.
            Cleanup();
        }

        private ObservableCollection<HealthReportSummaryViewModel> _healthReports =
            new ObservableCollection<HealthReportSummaryViewModel>();
        public ObservableCollection<HealthReportSummaryViewModel> HealthReports
        {
            get { return _healthReports; }
            set { _healthReports = value; RaisePropertyChanged(() => HealthReports); }
        }
    }
}
