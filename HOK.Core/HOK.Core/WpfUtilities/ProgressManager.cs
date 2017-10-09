using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace HOK.Core.WpfUtilities
{
    /// <summary>
    /// Used to manage the progress bar. Plugins using it:
    ///  - HOK.Utilities.Arrowhead
    ///  - HOK.MissionControl.PublishFamily
    ///  - HOK.ElementFlatter
    /// </summary>
    public static class StatusBarManager
    {
        public static ProgressBar ProgressBar = null;
        public static TextBlock StatusLabel = null;
        public static double ProgressValue;
        public static bool Cancel;

        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
        private delegate void UpdateStatusLabelDelegate(DependencyProperty dp, object value);

        private static UpdateStatusLabelDelegate _updateLabelDelegate;
        private static UpdateProgressBarDelegate _updatePbDelegate;

        /// <summary>
        /// Initialize Progress Bar and set Status Label.
        /// </summary>
        /// <param name="statusText">Status Label initial value.</param>
        /// <param name="maximum">Max count for the Progress Bar.</param>
        public static void InitializeProgress(string statusText, int maximum)
        {
            if (null == ProgressBar || null == StatusLabel) return;

            ProgressBar.Visibility = Visibility.Visible;
            ProgressValue = 0;

            _updateLabelDelegate = StatusLabel.SetValue;
            Dispatcher.CurrentDispatcher.Invoke(_updateLabelDelegate, DispatcherPriority.Background, TextBlock.TextProperty, statusText);

            _updatePbDelegate = ProgressBar.SetValue;
            ProgressBar.Value = ProgressValue;
            ProgressBar.Maximum = maximum;
        }

        /// <summary>
        /// Iterate Progress Bar by one.
        /// </summary>
        public static void StepForward()
        {
            if (null == ProgressBar || null == StatusLabel) return;

            ProgressValue++;
            Dispatcher.CurrentDispatcher.Invoke(_updatePbDelegate, DispatcherPriority.Background, RangeBase.ValueProperty, ProgressValue);
        }

        /// <summary>
        /// Cancels the progress bar and resets the status.
        /// </summary>
        public static void CancelProgress()
        {
            Cancel = false;
            FinalizeProgress();
        }

        /// <summary>
        /// Clean up Progress bar by resetting Status Label.
        /// </summary>
        public static void FinalizeProgress()
        {
            if (null == ProgressBar || null == StatusLabel) return;

            ProgressValue = 0;
            ProgressBar.Visibility = Visibility.Hidden;
            StatusLabel.Text = "Ready";
        }
    }
}
