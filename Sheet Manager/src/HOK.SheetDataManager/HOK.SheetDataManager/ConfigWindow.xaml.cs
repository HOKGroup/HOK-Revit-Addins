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

namespace HOK.SheetDataManager
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private bool autoUpdate = true;

        public bool AutoUpdate { get { return autoUpdate; } set { autoUpdate = value; } }

        public ConfigWindow()
        {
            //by configuration
            InitializeComponent();
            DisplayConfiguration();
        }

        private void DisplayConfiguration()
        {
            try
            {
                sliderAutoUpdate.Value = autoUpdate ? 1 : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display configuration.\n"+ex.Message, "Display Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            autoUpdate = (sliderAutoUpdate.Value == 1) ? true : false;
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
