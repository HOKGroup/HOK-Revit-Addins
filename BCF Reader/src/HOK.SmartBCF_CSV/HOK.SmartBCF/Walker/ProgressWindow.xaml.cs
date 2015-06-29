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

namespace HOK.SmartBCF.Walker
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private string statusText = "";

        public string StatusText { get { return statusText; } set { statusText = value; } }

        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

        private UpdateProgressDelegate updateProgressDelegate;

        public ProgressWindow(string labelText)
        {
            InitializeComponent();
            statusText = labelText;
            labelStatus.Content = statusText;
            this.Title = "smartBCF v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            updateProgressDelegate = new UpdateProgressDelegate(progressBarBCF.SetValue);
            AbortFlag.SetAbortFlag(false);
        }

        public void SetMaximum(double max)
        {
            try
            {
                progressBarBCF.Value = 0;
                progressBarBCF.Maximum = max;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set maximum value for the progressbar.\n"+ex.Message, "Progress Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void SetProgressValue(double progressValue)
        {
            if (progressValue < progressBarBCF.Maximum)
            {
                Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, progressValue });
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            AbortFlag.SetAbortFlag(true);
        }


    }
}
