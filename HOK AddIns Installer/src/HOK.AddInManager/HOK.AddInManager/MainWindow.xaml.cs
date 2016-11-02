using HOK.AddInManager.UserControls;
using HOK.AddInManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.AddInManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AddInViewModel viewModel = null;

        public AddInViewModel ViewModel { get { return viewModel; } set { viewModel = value; } }

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "HOK Addin Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as AddInViewModel;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {

            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void hyperlinkHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToolTipProperties ttt = AppCommand.thisApp.addinManagerToolTip;
                if (!string.IsNullOrEmpty(ttt.HelpUrl))
                {
                    System.Diagnostics.Process.Start(ttt.HelpUrl);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
