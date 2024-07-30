using HOK.RenameFamily.Util;
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

namespace HOK.RenameFamily
{

    public partial class RenameWindow : Window
    {
        private RenameViewModel viewModel = null;

        public RenameWindow()
        {
            InitializeComponent();
            ProgressManager.progressBar = progressBar;
            ProgressManager.statusLabel = statusLable;

            this.Title = "Rename Families and Types v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as RenameViewModel;
        }
    }
}
