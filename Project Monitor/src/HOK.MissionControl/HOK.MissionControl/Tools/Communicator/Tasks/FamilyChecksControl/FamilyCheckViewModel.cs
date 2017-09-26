using GalaSoft.MvvmLight;

namespace HOK.MissionControl.Tools.Communicator.Tasks.FamilyChecksControl
{
    public class FamilyCheckViewModel : ViewModelBase
    {
        public FamilyCheckViewModel()
        {
        }

        private bool? _isCheckPassing;
        public bool? IsCheckPassing
        {
            get { return _isCheckPassing; }
            set { _isCheckPassing = value; RaisePropertyChanged(() => IsCheckPassing); }
        }

        private string _checkName;
        public string CheckName
        {
            get { return _checkName; }
            set { _checkName = value; RaisePropertyChanged(() => CheckName); }
        }
    }
}
