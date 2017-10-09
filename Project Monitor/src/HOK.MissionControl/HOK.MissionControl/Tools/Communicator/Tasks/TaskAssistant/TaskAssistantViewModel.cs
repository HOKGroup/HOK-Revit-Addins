using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.Tasks.TaskAssistant
{
    public class TaskAssistantViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public TaskAssistantModel Model { get; set; }
        public RelayCommand<Window> Complete { get; set; }
        public RelayCommand EditFamily { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public RelayCommand<Window> WindowClosed { get; set; }

        public TaskAssistantViewModel(FamilyTaskWrapper wrapper)
        {
            Model = new TaskAssistantModel();
            Wrapper = wrapper;
            Title = "Mission Control - Task Assistant v." + Assembly.GetExecutingAssembly().GetName().Version;
            Checks = Model.CollectChecks(wrapper.Family);

            Complete = new RelayCommand<Window>(OnComplete);
            EditFamily = new RelayCommand(OnEditFamily);
            Close = new RelayCommand<Window>(OnClose);
            WindowClosed = new RelayCommand<Window>(OnWindowClosed);
        }

        private static void OnWindowClosed(Window win)
        {
            Messenger.Default.Send(new TaskAssistantClosedMessage { IsClosed = true });
        }

        private static void OnClose(Window win)
        {
            win.Close();
        }

        private void OnEditFamily()
        {
            Model.EditFamily(Wrapper.Family);
        }

        private void OnComplete(Window win)
        {
            Model.Submit(Wrapper);
            win.Close();
        }

        private ObservableCollection<CheckWrapper> _checks;
        public ObservableCollection<CheckWrapper> Checks
        {
            get { return _checks; }
            set { _checks = value; RaisePropertyChanged(() => Checks); }
        }

        private FamilyTaskWrapper _wrapper;
        public FamilyTaskWrapper Wrapper
        {
            get { return _wrapper; }
            set { _wrapper = value; RaisePropertyChanged(() => Wrapper); }
        }
    }
}
