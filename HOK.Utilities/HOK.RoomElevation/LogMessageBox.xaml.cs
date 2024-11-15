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

namespace HOK.RoomElevation
{
    /// <summary>
    /// Interaction logic for LogMessageBox.xaml
    /// </summary>
    public partial class LogMessageBox : Window
    {
        public LogMessageBox()
        {
            InitializeComponent();
            textBoxLog.Text = LogMessageBuilder.GetLogMessages();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public static class LogMessageBuilder
    {
        private static StringBuilder logMessages = new StringBuilder();

        public static void AddLogMessage(string str)
        {
            logMessages.AppendLine(str);
        }

        public static string GetLogMessages()
        {
            return logMessages.ToString();
        }

        public static void RefreshMessages()
        {
            logMessages = new StringBuilder();
        }
    }
}
