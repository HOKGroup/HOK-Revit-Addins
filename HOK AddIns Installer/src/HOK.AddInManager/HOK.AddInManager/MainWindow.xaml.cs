using System.Windows;
using HOK.AddInManager.UserControls;

namespace HOK.AddInManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AddInViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as AddInViewModel;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hyperlinkHelp_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
