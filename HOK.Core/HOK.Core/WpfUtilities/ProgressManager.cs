using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace HOK.Core.WpfUtilities
{
    public static class ProgressManager
    {
        public static ProgressBar progressBar = null;
        public static TextBlock statusLabel = null;
        public static double progressValue;
        public static string databaseFilePath = "";

        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
        private delegate void UpdateStatusLabelDelegate(DependencyProperty dp, object value);

        private static UpdateStatusLabelDelegate updateLabelDelegate;
        private static UpdateProgressBarDelegate updatePbDelegate;

        public static void InitializeProgress(string statusText, int maximum)
        {
            if (null == progressBar || null == statusLabel) return;

            progressBar.Visibility = Visibility.Visible;
            progressValue = 0;

            updateLabelDelegate = statusLabel.SetValue;
            Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, DispatcherPriority.Background, TextBlock.TextProperty, statusText);

            updatePbDelegate = progressBar.SetValue;
            progressBar.Value = progressValue;
            progressBar.Maximum = maximum;
        }

        public static void StepForward()
        {
            if (null == progressBar || null == statusLabel) return;

            progressValue++;
            Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, RangeBase.ValueProperty, progressValue);
        }

        public static void FinalizeProgress()
        {
            if (null == progressBar || null == statusLabel) return;

            progressValue = 0;
            progressBar.Visibility = Visibility.Hidden;

            statusLabel.Text = !string.IsNullOrEmpty(databaseFilePath) ? databaseFilePath : "Ready";
        }
    }
}
