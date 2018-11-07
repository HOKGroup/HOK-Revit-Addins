using System.Diagnostics;
using System.Reflection;
using System.Windows.Interop;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.Core.WpfUtilities.FeedbackUI;

namespace HOK.CameraDuplicator
{
    public class CameraViewModel : ViewModelBase
    {
        public RelayCommand SubmitComment { get; set; }

        public CameraViewModel()
        {
            SubmitComment = new RelayCommand(OnSubmitComment);
        }

        private static void OnSubmitComment()
        {
            var title = "HOK Feedback Tool v." + Assembly.GetCallingAssembly().GetName().Version;
            var model = new FeedbackModel();
            var viewModel = new FeedbackViewModel(model, title);
            var view = new FeedbackView
            {
                DataContext = viewModel
            };

            var unused = new WindowInteropHelper(view)
            {
                Owner = Process.GetCurrentProcess().MainWindowHandle
            };

            view.ShowDialog();
        }
    }
}
