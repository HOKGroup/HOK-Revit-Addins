using System.Windows;
using System.Windows.Input;

namespace HOK.MissionControl.Tools.SingleSession
{
    /// <summary>
    /// Interaction logic for SingleSessionWindow.xaml
    /// </summary>
    public partial class SingleSessionWindow
    {
        public SingleSessionWindow(string filePath)
        {
            InitializeComponent();
            textBlockFile.Text = filePath + " will be closed.";
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
