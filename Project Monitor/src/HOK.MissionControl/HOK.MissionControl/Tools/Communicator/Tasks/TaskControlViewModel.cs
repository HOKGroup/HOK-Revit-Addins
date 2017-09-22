using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class TaskControlViewModel : ViewModelBase
    {
        public RelayCommand<Button> LaunchTaskAssistant { get; set; }
        public TaskControlModel Model { get; set; }

        public TaskControlViewModel()
        {
            LaunchTaskAssistant = new RelayCommand<Button>(OnLaunchTaskAssistant);
        }

        private void OnLaunchTaskAssistant(Button button)
        {
            Model.LaunchTaskAssistant();
        }

        private string _taskName;
        public string TaskName {
            get { return _taskName; }
            set { _taskName = value; RaisePropertyChanged(() => TaskName); }
        }

        private bool _taskComplete;
        public bool TaskComplete {
            get { return _taskComplete; }
            set { _taskComplete = value; RaisePropertyChanged(() => TaskComplete); }
        }
    }
}
