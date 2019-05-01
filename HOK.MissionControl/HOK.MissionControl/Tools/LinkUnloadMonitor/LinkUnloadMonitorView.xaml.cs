using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace HOK.MissionControl.Tools.LinkUnloadMonitor
{
    /// <summary>
    /// Interaction logic for LinkUnloadMonitorView.xaml
    /// </summary>
    public partial class LinkUnloadMonitorView
    {
        public LinkUnloadMonitorView()
        {
            InitializeComponent();
            Title = "Link Unload Monitor v." + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        //private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        //{
        //    WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        //}

        //private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        //{
        //    WindowState = WindowState.Minimized;
        //}

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
