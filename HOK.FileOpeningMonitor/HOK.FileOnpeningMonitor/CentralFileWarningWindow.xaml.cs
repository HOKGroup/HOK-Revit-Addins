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

namespace HOK.FileOpeningMonitor
{
    /// <summary>
    /// Interaction logic for CentralFileWarningWindow.xaml
    /// </summary>
    public partial class CentralFileWarningWindow : Window
    {
        private CentralFileInfo fileInfo = null;

        public CentralFileWarningWindow(CentralFileInfo info)
        {
            fileInfo = info;
            
            InitializeComponent();
            this.Title = "Central File Opened!  v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!string.IsNullOrEmpty(fileInfo.DocCentralPath))
            {
                textBlockFilePath.Text = "Central File Path:\n" + fileInfo.DocCentralPath;
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
