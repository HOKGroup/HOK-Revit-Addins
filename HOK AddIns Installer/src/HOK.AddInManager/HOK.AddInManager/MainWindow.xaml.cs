using System;
using System.Windows;
using HOK.AddInManager.UserControls;
using HOK.Core.Utilities;

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
            Title = "HOK Addin Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
            try
            {
                var ttt = AppCommand.thisApp.addinManagerToolTip;
                if (!string.IsNullOrEmpty(ttt.HelpUrl))
                {
                    System.Diagnostics.Process.Start(ttt.HelpUrl);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }
    }
}
