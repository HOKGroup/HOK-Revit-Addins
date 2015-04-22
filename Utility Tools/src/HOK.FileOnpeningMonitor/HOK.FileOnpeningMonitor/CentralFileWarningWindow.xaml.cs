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
using Autodesk.Revit.DB;

namespace HOK.FileOnpeningMonitor
{
    /// <summary>
    /// Interaction logic for CentralFileWarningWindow.xaml
    /// </summary>
    public partial class CentralFileWarningWindow : Window
    {
        private Document openedDocument = null;

        public CentralFileWarningWindow(Document doc)
        {
            openedDocument = doc;
            
            InitializeComponent();
            if (!string.IsNullOrEmpty(doc.PathName))
            {
                textBlockFilePath.Text = "Central File Path:\n" + doc.PathName;
            }
            else
            {
                textBlockFilePath.Text = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
