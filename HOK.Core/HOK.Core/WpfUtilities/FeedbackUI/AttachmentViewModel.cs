using GalaSoft.MvvmLight;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class AttachmentViewModel : ViewModelBase
    {
        public GalaSoft.MvvmLight.Command.RelayCommand Delete { get; set; }
        public FeedbackViewModel ViewModel { get; set; }
        public string HtmlLink { get; set; }
        public UploadImageContent UploadImageContent { get; set; }

        public AttachmentViewModel(FeedbackViewModel vm)
        {
            ViewModel = vm;
            Delete = new GalaSoft.MvvmLight.Command.RelayCommand(OnDelete);
        }

        private void OnDelete()
        {
            ViewModel.DeleteAttachment(this);
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; RaisePropertyChanged(() => FilePath); }
        }
    }
}
