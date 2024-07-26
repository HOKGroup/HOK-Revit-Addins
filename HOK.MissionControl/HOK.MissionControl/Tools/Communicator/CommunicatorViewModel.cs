using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm;
using HOK.MissionControl.Tools.Communicator.HealthReport;
using HOK.MissionControl.Tools.Communicator.Tasks;

namespace HOK.MissionControl.Tools.Communicator
{
    public class CommunicatorViewModel : ObservableRecipient
    {
        public ObservableCollection<TabItem> TabItems { get; set; } = new ObservableCollection<TabItem>();

        public CommunicatorViewModel()
        {
            TabItems.Add(new TabItem
            {
                Content = new CommunicatorHealthReportView { DataContext = new CommunicatorHealthReportViewModel() },
                Header = "Health Report"
            });
            TabItems.Add(new TabItem
            {
                Content = new CommunicatorTasksView { DataContext = new CommunicatorTasksViewModel(new CommunicatorTasksModel()) },
                Header = "Tasks"
            });
        }
    }
}
