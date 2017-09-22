using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class TaskAssistantViewModel : ViewModelBase
    {
        public TaskAssistantModel Model { get; set; }
        public RelayCommand<Window> Complete { get; set; }
        public RelayCommand<Window> WindowClosing { get; set; }
        public RelayCommand EditFamily { get; set; }

        public TaskAssistantViewModel(TaskAssistantModel model)
        {
            Model = model;
            FamilyName = Model.Family.name + " - " + Model.Family.elementId;
            TaskName = Model.Task.name;
            Recipient = Model.Task.assignedTo;
            Message = Model.Task.message;

            Complete = new RelayCommand<Window>(OnComplete);
            WindowClosing = new RelayCommand<Window>(OnWindowClosing);
            EditFamily = new RelayCommand(OnEditFamily);
        }

        private void OnEditFamily()
        {
            Model.EditFamily();
        }

        private void OnWindowClosing(Window win)
        {
            Model.SubmitEdits();
        }

        private void OnComplete(Window win)
        {
            win.Close();
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(() => Message); }
        }

        private string _recipient;
        public string Recipient
        {
            get { return _recipient; }
            set { _recipient = value; RaisePropertyChanged(() => Recipient); }
        }

        private string _taskName;
        public string TaskName {
            get { return _taskName; }
            set { _taskName = value; RaisePropertyChanged(() => TaskName); }
        }

        private string _familyName;
        public string FamilyName {
            get { return _familyName; }
            set { _familyName = value; RaisePropertyChanged(() => FamilyName); }
        }
    }
}
