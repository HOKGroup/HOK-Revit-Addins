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

namespace HOK.RoomUpdater
{
    /// <summary>
    /// Interaction logic for RoomUpdaterErrorWindow.xaml
    /// </summary>
    public partial class RoomUpdaterErrorWindow : Window
    {
        private string errorMessage = "";
        public RoomUpdaterErrorWindow(string message)
        {
            errorMessage = message;
            textBoxError.Text = errorMessage;
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
