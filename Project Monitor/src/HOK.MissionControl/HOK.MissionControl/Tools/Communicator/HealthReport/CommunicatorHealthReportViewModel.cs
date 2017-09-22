using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class CommunicatorHealthReportViewModel : ViewModelBase
    {
        public ObservableCollection<HealthReportSummaryViewModel> HealthReports { get; set; }
        public CommunicatorHealthReportModel Model { get; set; }

        public CommunicatorHealthReportViewModel(HealthReportData data, FamilyStat famStat)
        {
            Model = new CommunicatorHealthReportModel();
            HealthReports = Model.ProcessData(data, famStat);
        }
    }
}
