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
using System.Windows.Shapes;

namespace HOK.AddInsInstaller
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {

        private delegate void UpdateLableDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateProgressDelegate(System.Windows.DependencyProperty dp, Object value);

        private UpdateLableDelegate updateLabelDelegate;
        private UpdateProgressDelegate updateProgressDelegate;

        public ProgressWindow()
        {
            InitializeComponent();
            updateLabelDelegate = new UpdateLableDelegate(statusLabel.SetValue);
            updateProgressDelegate = new UpdateProgressDelegate(statusProgressBar.SetValue);
        }

        public void SetStatusLabel(string text)
        {
            Dispatcher.Invoke(updateLabelDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { Label.ContentProperty, text });
        }

        public void SetProgressBar(double value)
        {
            Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
        }

        public void RefreshProgressBar(double maximum)
        {
            double initialValue = 0;
            statusProgressBar.Maximum = maximum;

            Dispatcher.Invoke(updateProgressDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, initialValue });
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
