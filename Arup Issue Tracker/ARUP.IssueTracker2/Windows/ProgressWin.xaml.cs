using System;
using System.Windows;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for ProgressWin.xaml
    /// </summary>
    public partial class ProgressWin : Window
    {
        public event EventHandler killWorker;

        public ProgressWin()
        {
            InitializeComponent();
        }
        public void SetProgress(int i, string s) {
            progress.Value = i;
            taskProgress.Content = s;

        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (killWorker != null)
            {
                killWorker(this, e);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (killWorker != null)
            {
                killWorker(this, e);
            }
        }
    }
}
