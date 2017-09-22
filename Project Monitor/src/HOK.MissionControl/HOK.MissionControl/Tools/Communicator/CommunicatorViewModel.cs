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

        public CommunicatorViewModel(HealthReportData hrData)
        {
            var familiesCollectionId = hrData.familyStats;
            var familyStats = ServerUtilities.FindOne<FamilyStat>("families/" + familiesCollectionId);

            TabItems = new ObservableCollection<TabItem>
            {
                new TabItem{Content = new CommunicatorHealthReportView {DataContext = new CommunicatorHealthReportViewModel(hrData, familyStats)}, Header = "Health Report"},
                new TabItem{Content = new CommunicatorTasksView {DataContext = new CommunicatorTasksViewModel(familyStats)}, Header = "Tasks"}
            };
        }
    }
}
