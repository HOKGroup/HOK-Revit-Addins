using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator
{
    public class CommunicatorViewModel : ViewModelBase
    {
        public ObservableCollection<HealthReportSummaryViewModel> HealthReports { get; set; }
        public CommunicatorModel Model { get; set; }

        public CommunicatorViewModel(HealthReportData data)
        {
            Model = new CommunicatorModel();
            HealthReports = Model.ProcessData(data);
        }
    }
}
