using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;

namespace HOK.Feedback
{
    public class AttachmentViewModel : ObservableRecipient
    {
        public RelayCommand Delete { get; set; }
        public FeedbackViewModel ViewModel { get; set; }
        public string HtmlLink { get; set; }
        public UploadImageContent UploadImageContent { get; set; }

        public AttachmentViewModel(FeedbackViewModel vm)
        {
            ViewModel = vm;
            Delete = new RelayCommand(OnDelete);
        }

        private void OnDelete()
        {
            ViewModel.DeleteAttachment(this);
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }
    }
}
