using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class AttachmentViewModel : ViewModelBase
    {
        public GalaSoft.MvvmLight.Command.RelayCommand Delete { get; set; }
        public FeedbackViewModel ViewModel { get; set; }

        public AttachmentViewModel(FeedbackViewModel vm)
        {
            ViewModel = vm;
            Delete = new GalaSoft.MvvmLight.Command.RelayCommand(OnDelete);
        }

        private void OnDelete()
        {
            ViewModel.DeleteAttachment(this);
        }

        private string _name;
        public string Name {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(() => Name); }
        }
    }
}
