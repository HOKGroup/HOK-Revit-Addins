using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

namespace HOK.Core.WpfUtilities.FeedbackUI
{
    public class FilePathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileName(System.Convert.ToString(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FeedbackViewModel : ViewModelBase
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

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

        public void DeleteAttachment(AttachmentViewModel vm)
        {
            Attachments.Remove(vm);
        }

        private void OnChooseFile()
        {
            var file = SelectImageFile();
            if (string.IsNullOrEmpty(file)) return;

            var attachment = new AttachmentViewModel(this)
            {
                Name = Path.GetFileName(file)
            };

            Attachments.Add(attachment);
        }

        public static string SelectImageFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };
            var result = dialog.ShowDialog();

            return result != true ? string.Empty : dialog.FileName;
        }

        private void OnWindowKeyDown(KeyEventArgs args)
        {
            if (args.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    var clipboardData = Clipboard.GetDataObject();
                    if (clipboardData != null)
                    {
                        if (clipboardData.GetDataPresent(DataFormats.Bitmap))
                        {
                            var bitmap = GetBitmap((BitmapSource)clipboardData.GetData(DataFormats.Bitmap));
                            if (bitmap != null)
                            {
                                var hBitmap = bitmap.GetHbitmap();
                                try
                                {
                                    var source = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                                }
                                finally
                                {
                                    DeleteObject(hBitmap);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
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
