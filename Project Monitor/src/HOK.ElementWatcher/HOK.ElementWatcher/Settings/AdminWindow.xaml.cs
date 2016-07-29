using System;
using System.Collections.Generic;
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

namespace HOK.ElementWatcher.Settings
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private AdminViewModel viewModel = null;

        public AdminViewModel ViewModel { get { return viewModel; } set { viewModel = value; } }

        public AdminWindow()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel = this.DataContext as AdminViewModel;
                if (viewModel.Configuration.ProjectUpdaters.Count > 0)
                {
                    viewModel.SelectedUpdaterIndex = 0;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
