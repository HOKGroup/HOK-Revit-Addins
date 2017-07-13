//using System;
//using System.Windows.Controls;

//namespace HOK.ElementFlatter.Util
//{
//    public static class ProgressManager
//    {
//        public static ProgressBar progressBar = null;
//        public static TextBlock statusLabel = null;
//        public static double progressValue = 0;
//        public static string databaseFilePath = "";

//        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
//        private delegate void UpdateStatusLabelDelegate(System.Windows.DependencyProperty dp, Object value);

//        private static UpdateStatusLabelDelegate updateLabelDelegate = null;
//        private static UpdateProgressBarDelegate updatePbDelegate = null;

//        public static void InitializeProgress(string statusText, int maximum)
//        {
//            if (null != progressBar && null != statusLabel)
//            {
//                progressBar.Visibility = System.Windows.Visibility.Visible;
//                progressValue = 0;

//                updateLabelDelegate = new UpdateStatusLabelDelegate(statusLabel.SetValue);
//                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { TextBlock.TextProperty, statusText });

//                updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
//                progressBar.Value = progressValue;
//                progressBar.Maximum = maximum;
//            }
//        }

//        public static void StepForward()
//        {
//            if (null != progressBar && null != statusLabel)
//            {
//                progressValue++;
//                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });
//            }
//        }

//        public static void FinalizeProgress()
//        {
//            if (null != progressBar && null != statusLabel)
//            {

//                progressValue = 0;
//                progressBar.Visibility = System.Windows.Visibility.Hidden;

//                if (!string.IsNullOrEmpty(databaseFilePath))
//                {
//                    statusLabel.Text = databaseFilePath;
//                }
//                else
//                {
//                    statusLabel.Text = "Ready";
//                }
//            }
//        }

//    }
//}
