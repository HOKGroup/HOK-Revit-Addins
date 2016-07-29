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

namespace HOK.SheetManager.UserControls
{
    /// <summary>
    /// Interaction logic for EditorMainPanel.xaml
    /// </summary>
    public partial class EditorMainPanel : UserControl
    {
        private EditorMainViewModel viewModel = null;

        public EditorMainPanel()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as EditorMainViewModel;
        }
    }
}
