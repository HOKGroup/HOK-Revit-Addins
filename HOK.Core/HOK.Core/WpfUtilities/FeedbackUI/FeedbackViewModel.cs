using System;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class FeedbackViewModel : ViewModelBase
    {
        public string ToolName { get; set; }
        public string Title { get; set; }
        public FeedbackModel Model { get; set; }
        public RelayCommand<Window> WindowLoaded { get; }
        public RelayCommand<Window> Submit { get; }
        public RelayCommand<Window> Cancel { get; }

        public FeedbackViewModel(FeedbackModel model, string toolname)
        {
            Model = model;
            ToolName = toolname;
            Title = "HOK Feedback Tool v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            Submit = new RelayCommand<Window>(OnSubmit);
            Cancel = new RelayCommand<Window>(OnCancel);
        }

        private static void OnCancel(Window win)
        {
            win.Close();
        }

        private void OnSubmit(Window win)
        {
            var result = string.Empty;

            if (string.IsNullOrEmpty(Name))
            {
                StatusBarManager.StatusLabel.Text = "Please provide your name.";
            }
            else if (!IsValidEmail(Email))
            {
                StatusBarManager.StatusLabel.Text = "Please provide valid email address.";
            }
            else
            {
                result = Model.Submit(Name, Email, Feedback, ToolName);
            }

            if (string.IsNullOrEmpty(result))
            {
                // form validation failed just display the message on status bar
            }
            else if (result == "Success")
            {
                win.Close();
            }
            else
            {
                // show the error
                StatusBarManager.StatusLabel.Text = result;
            }
        }

        private static void OnWindowLoaded(Window win)
        {
            StatusBarManager.StatusLabel = ((FeedbackView)win).statusLabel;
        }

        private string _feedback = "Please leave a message...";
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

        /// <summary>
        /// This is a basic email address validation check.
        /// </summary>
        /// <param name="email">Email to check.</param>
        /// <returns>True if email address is valid.</returns>
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
