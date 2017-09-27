using System.Collections.ObjectModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.HealthReport;
using HOK.MissionControl.Tools.Communicator.Tasks;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.Communicator
{
    public class CommunicatorViewModel : ViewModelBase
    {
        public ObservableCollection<TabItem> TabItems { get; set; }

        public CommunicatorViewModel()
        {
            var familyStatsId = AppCommand.HrData.familyStats;
            if (familyStatsId == null) return;

            var familyStats = ServerUtilities.FindOne<FamilyStat>("families/" + familyStatsId);
            if (familyStats == null) return;

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem{Content = new CommunicatorHealthReportView {DataContext = new CommunicatorHealthReportViewModel(familyStats)}, Header = "Health Report"},
                new TabItem{Content = new CommunicatorTasksView {DataContext = new CommunicatorTasksViewModel(familyStats)}, Header = "Tasks"}
            };
        }
    }
}
