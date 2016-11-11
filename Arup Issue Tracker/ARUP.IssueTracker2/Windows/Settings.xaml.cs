using System.Windows;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        public Settings()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        


    }
}
