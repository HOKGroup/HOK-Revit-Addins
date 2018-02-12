using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.Utilities;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class FeedbackViewModel : ViewModelBase
    {
        public string ToolName { get; set; }
        public string Title { get; set; }
        public FeedbackModel Model { get; set; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand<KeyEventArgs> WindowKeyDown { get; }
        public RelayCommand<Window> Submit { get; }
        public RelayCommand<Window> Cancel { get; }
        public GalaSoft.MvvmLight.Command.RelayCommand ChooseFile { get; }
        public ObservableCollection<AttachmentViewModel> Attachments { get; set; } = new ObservableCollection<AttachmentViewModel>();

        public FeedbackViewModel(FeedbackModel model, string toolname)
        {
            Model = model;
            ToolName = toolname;
            Title = "HOK Feedback Tool v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowKeyDown = new RelayCommand<KeyEventArgs>(OnWindowKeyDown);
            Submit = new RelayCommand<Window>(OnSubmit);
            Cancel = new RelayCommand<Window>(OnCancel);
            ChooseFile = new GalaSoft.MvvmLight.Command.RelayCommand(OnChooseFile);
        }

        private static void OnWindowLoaded(Window win)
        {
            StatusBarManager.StatusLabel = ((FeedbackView)win).statusLabel;
        }

        public async void DeleteAttachment(AttachmentViewModel vm)
        {
            Attachments.Remove(vm);
            var currentState = string.Copy(Feedback);
            Feedback = Feedback.Replace(vm.HtmlLink.Replace(Environment.NewLine, ""), "");

            var response = await Model.RemoveImage<Response>(vm);
            if (response.commit == null)
            {
                Attachments.Add(vm);
                Feedback = currentState;
                StatusBarManager.StatusLabel.Text = "Failed to remove image. Please try again.";
                return;
            }
            
            StatusBarManager.StatusLabel.Text = "Successfully removed: " + vm.UploadImageContent.name;
        }

        private async void OnChooseFile()
        {
            var file = Dialogs.SelectImageFile();
            if (string.IsNullOrEmpty(file)) return;

            var attachment = new AttachmentViewModel(this)
            {
                FilePath = file
            };

            Attachments.Add(attachment); // updates UI

            var response = await Model.PostImage<Response>(attachment, true);
            if (response.content == null)
            {
                StatusBarManager.StatusLabel.Text = "Failed to upload. Please try again.";
                Attachments.Remove(attachment);
                return;
            }

            StatusBarManager.StatusLabel.Text = "Successfully uploaded: " + response.content.name;
            attachment.HtmlLink = "\n![image](" + response.content.html_url + "?raw=true)\n";
            attachment.UploadImageContent = response.content;

            Feedback += attachment.HtmlLink;
        }

        private async void OnWindowKeyDown(KeyEventArgs args)
        {
            // (Konrad) Only handle CTRL + V
            if (args.Key != Key.V || Keyboard.Modifiers != ModifierKeys.Control) return;
            if (!Clipboard.ContainsImage()) return;

            var image = Clipboard.GetImage();
            if (image == null) return;

            var tempFile = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddTHHmmss") + "_ClipboardFile.jpg");
            using (var fileStream = new FileStream(tempFile, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }

            if (!File.Exists(tempFile)) return;

            var attachment = new AttachmentViewModel(this)
            {
                FilePath = tempFile
            };

            Attachments.Add(attachment); // updates UI

            var response = await Model.PostImage<Response>(attachment, false);
            if (response.content == null)
            {
                StatusBarManager.StatusLabel.Text = "Failed to upload. Please try again.";
                Attachments.Remove(attachment);
                return;
            }

            StatusBarManager.StatusLabel.Text = "Successfully uploaded: " + response.content.name;
            attachment.HtmlLink = "\n![image](" + response.content.html_url + "?raw=true)\n";
            attachment.UploadImageContent = response.content;

            Feedback += attachment.HtmlLink;
        }

        private static void OnCancel(Window win)
        {
            win.Close();
        }

        private async void OnSubmit(Window win)
        {
            Issue response = null;

            if (string.IsNullOrEmpty(Name))
            {
                StatusBarManager.StatusLabel.Text = "Please provide your name.";
            }
            else if (!Validations.IsValidEmail(Email))
            {
                StatusBarManager.StatusLabel.Text = "Please provide valid email address.";
            }
            else
            {
                response = await Model.Submit<Issue>(Name, Email, Feedback, ToolName);
            }

            if (response != null && response.body == null)
            {
                StatusBarManager.StatusLabel.Text = "Failed to post an issue. Please try again.";
                return;
            }

            win.Close();
        }

        private string _feedback;
        public string Feedback
        {
            get { return _feedback; }
            set { _feedback = value; RaisePropertyChanged(() => Feedback); }
        }

        private string _email = Environment.UserName.ToLower() + "@hok.com";
        public string Email
        {
            get { return _email; }
            set { _email = value; RaisePropertyChanged(() => Email); }
        }

        private string _name = Environment.UserName.ToLower().Replace('.', ' ');
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(() => Name); }
        }
    }
}
