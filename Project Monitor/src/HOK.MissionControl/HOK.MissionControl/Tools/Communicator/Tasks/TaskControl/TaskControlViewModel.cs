using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Tasks.TaskControl
{
    public class TaskUpdatedMessage
    {
        public FamilyTask Task { get; set; }
    }

    public class TaskControlViewModel : ViewModelBase
    {
        public RelayCommand<Button> LaunchTaskAssistant { get; set; }
        public TaskControlModel Model { get; set; }

        public TaskControlViewModel()
        {
            LaunchTaskAssistant = new RelayCommand<Button>(OnLaunchTaskAssistant);

            Messenger.Default.Register<TaskUpdatedMessage>(this, OnTaskUpdatedReceived);
        }

        private void OnTaskUpdatedReceived(TaskUpdatedMessage msg)
        {
            if (msg.Task.Id != Model.Task.Id) return;

            TaskComplete = !string.IsNullOrEmpty(msg.Task.completedBy);
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
