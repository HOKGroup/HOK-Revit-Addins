using System.Windows;
using HOK.Core.WpfUtilities;

namespace HOK.ElementFlatter
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow
    {
        public CommandWindow()
        {
            InitializeComponent();
            Title = "Element Flatter v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            StatusBarManager.ProgressBar = progressBar;
            StatusBarManager.StatusLabel = statusLable;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
