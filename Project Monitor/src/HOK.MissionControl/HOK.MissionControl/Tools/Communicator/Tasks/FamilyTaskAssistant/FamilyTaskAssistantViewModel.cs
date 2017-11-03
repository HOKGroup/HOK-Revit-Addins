using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.Tasks.FamilyTaskAssistant
{
    public class FamilyTaskAssistantViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public FamilyTaskAssistantModel Model { get; set; }
        public RelayCommand<Window> Complete { get; set; }
        public RelayCommand EditFamily { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public RelayCommand<Window> WindowClosed { get; set; }

        public FamilyTaskAssistantViewModel(FamilyTaskWrapper wrapper)
        {
            Model = new FamilyTaskAssistantModel();
            Wrapper = wrapper;
            Title = "Mission Control - Family Task Assistant v." + Assembly.GetExecutingAssembly().GetName().Version;
            Checks = Model.CollectChecks((FamilyItem)wrapper.Element);

            Complete = new RelayCommand<Window>(OnComplete);
            EditFamily = new RelayCommand(OnEditFamily);
            Close = new RelayCommand<Window>(OnClose);
            WindowClosed = new RelayCommand<Window>(OnWindowClosed);
        }

        private static void OnWindowClosed(Window win)
        {
            Messenger.Default.Send(new FamilyTaskAssistantClosedMessage { IsClosed = true });
        }

        private static void OnClose(Window win)
        {
            win.Close();
        }

        private void OnEditFamily()
        {
            Model.EditFamily((FamilyItem)Wrapper.Element);
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
