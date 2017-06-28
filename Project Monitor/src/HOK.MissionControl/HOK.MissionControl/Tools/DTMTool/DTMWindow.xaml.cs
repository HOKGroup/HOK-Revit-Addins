using System.Windows;
using System.Windows.Input;

namespace HOK.MissionControl.Tools.DTMTool
{
    /// <summary>
    /// Interaction logic for DTMWindow.xaml
    /// </summary>
    public partial class DTMWindow
    {
        public DTMWindow()
        {
            InitializeComponent();
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
