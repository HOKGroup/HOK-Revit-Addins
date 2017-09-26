using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Tasks.FamilyChecksControl;

namespace HOK.MissionControl.Tools.Communicator.Tasks.TaskAssistant
{
    public class RegistrationInfo
    {
        public FamilyItem Family { get; set; }
    }

    public class TaskAssistantViewModel : ViewModelBase
    {
        public TaskAssistantModel Model { get; set; }
        public RelayCommand<Window> Complete { get; set; }
        public RelayCommand<Window> WindowClosing { get; set; }
        public RelayCommand EditFamily { get; set; }
        public RelayCommand RefreshChecks { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public string Title { get; set; }
        public ObservableCollection<FamilyCheckViewModel> ChecksControls { get; set; }

        public TaskAssistantViewModel(TaskAssistantModel model)
        {
            Model = model;
            Title = "Mission Control - Task Assistant v." + Assembly.GetExecutingAssembly().GetName().Version;
            FamilyName = Model.Family.name + " - " + Model.Family.elementId;
            TaskName = Model.Task.name;
            Recipient = Model.Task.assignedTo;
            Message = Model.Task.message;
            ChecksControls = Model.ProcessChecks();

            Complete = new RelayCommand<Window>(OnComplete);
            WindowClosing = new RelayCommand<Window>(OnWindowClosing);
            EditFamily = new RelayCommand(OnEditFamily);
            Close = new RelayCommand<Window>(OnClose);

            Messenger.Default.Register<RegistrationInfo>(this, HandleRegistrationInfo);
        }

        private void HandleRegistrationInfo(RegistrationInfo info)
        {
            if (info.Family.elementId != Model.Family.elementId) return;

            FamilyName = info.Family.name + " - " + info.Family.elementId;
        }

        private static void OnClose(Window win)
        {
            win.Close();
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
            // talk to the server
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
