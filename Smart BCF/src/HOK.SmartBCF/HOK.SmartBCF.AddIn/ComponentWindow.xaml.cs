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

namespace HOK.SmartBCF.AddIn
{
    /// <summary>
    /// Interaction logic for ComponentWindow.xaml
    /// </summary>
    public partial class ComponentWindow : Window
    {
        private ComponentViewModel componentView = null;

        public ComponentViewModel ComponentView { get { return componentView; } set { componentView = value; } }

        public ComponentWindow(ComponentViewModel viewModel)
        {
            componentView = viewModel;
            DataContext = componentView;
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilterWindow filterWindow = new FilterWindow(componentView);
                filterWindow.Owner = this;
                if (filterWindow.ShowDialog() == true)
                {
                    ComponentCategoryFilter.Categories = filterWindow.Categories;
                    componentView.ApplyCategoryFilter();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
