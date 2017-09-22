using GalaSoft.MvvmLight;
using HOK.MissionControl.Core.Schemas;
using System.Collections.ObjectModel;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class CommunicatorTasksViewModel : ViewModelBase
    {
        public CommunicatorTasksModel Model { get; set; }
        public ObservableCollection<TaskControlViewModel> TaskControls { get; set; }

        public CommunicatorTasksViewModel(FamilyStat famData)
        {
            Model = new CommunicatorTasksModel();
            TaskControls = Model.ProcessData(famData);
        }
    }
}
