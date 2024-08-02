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
    /// Interaction logic for TimedWarningWindow.xaml
    /// </summary>
    public partial class TimedWarningWindow : Window
    {
        private int timerCount = 0;
        private Dictionary<string, CentralFileInfo> openedCentralFiles = new Dictionary<string, CentralFileInfo>();

        public TimedWarningWindow(int count, Dictionary<string, CentralFileInfo> centralFiles)
        {
            timerCount = count;
            openedCentralFiles = centralFiles;

            InitializeComponent();
            this.Title = "Central File Notification " + timerCount + " v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            
            if (centralFiles.Count > 0)
            {
                listBoxFiles.ItemsSource = openedCentralFiles.Keys.ToList();
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonContinue_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAllow_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
