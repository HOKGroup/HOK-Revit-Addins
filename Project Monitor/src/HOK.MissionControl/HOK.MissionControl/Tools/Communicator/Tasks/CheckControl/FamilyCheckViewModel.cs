using GalaSoft.MvvmLight;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.Communicator.Tasks.TaskAssistant;

namespace HOK.MissionControl.Tools.Communicator.Tasks.CheckControl
{
    public class FamilyCheckViewModel : ViewModelBase
    {
        public FamilyItem Family { get; set; }
        public FamilyCheckTypes CheckType { get; set; }

        public FamilyCheckViewModel(FamilyItem family)
        {
            Family = family;
        }

        private bool _isCheckVerified;
        public bool IsCheckVerified
        {
            get { return _isCheckVerified; }
            set
            {
                switch (CheckType)
                {
                    case FamilyCheckTypes.Name:
                        Family.isNameVerified = value;
                        break;
                    case FamilyCheckTypes.Size:
                        Family.isSizeVerified = value;
                        break;
                    case FamilyCheckTypes.Instances:
                        Family.isInstancesVerified = value;
                        break;
                    case FamilyCheckTypes.Voids:
                        Family.isVoidCountVerified = value;
                        break;
                    case FamilyCheckTypes.Arrays:
                        Family.isArrayCountVerified = value;
                        break;
                    case FamilyCheckTypes.NestedFamilies:
                        Family.isNestedFamilyCountVerified = value;
                        break;
                }
                _isCheckVerified = value;
                RaisePropertyChanged(() => IsCheckVerified);
            }
        }

        private bool _isCheckPassing;
        public bool IsCheckPassing
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
