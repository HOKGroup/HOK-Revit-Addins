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

namespace HOK.SmartBCF.Windows
{
    /// <summary>
    /// Interaction logic for OptionWindow.xaml
    /// </summary>
    public partial class OptionWindow : Window
    {
        private bool createNewDB = false;

        public bool CreateNewDB { get { return createNewDB; } set { createNewDB = value; } }

        public OptionWindow()
        {
            InitializeComponent();
        }

        private void buttonNewDB_Click(object sender, RoutedEventArgs e)
        {
            createNewDB = true;
            this.DialogResult = true;
        }

        private void buttonAddBCF_Click(object sender, RoutedEventArgs e)
        {
            createNewDB = false;
            this.DialogResult = true;
        }
    }
}
