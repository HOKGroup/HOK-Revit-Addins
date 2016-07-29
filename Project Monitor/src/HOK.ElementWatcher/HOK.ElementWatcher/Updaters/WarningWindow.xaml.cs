using HOK.ElementWatcher.Classes;
using HOK.ElementWatcher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace HOK.ElementWatcher.Updaters
{
    /// <summary>
    /// Interaction logic for WarningWindow.xaml
    /// </summary>
    public partial class WarningWindow : Window
    {
        private ReportingElementInfo reportingInfo = new ReportingElementInfo();
        private string descriptionText = "Grid elements are locked.";

        public string DescriptionText { get { return descriptionText; } set { descriptionText = value; } }

        public WarningWindow(ReportingElementInfo info)
        {
            reportingInfo = info;
            descriptionText = reportingInfo.CategoryName + " are locked.";
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to load warning window.\n" + ex.Message,"Warning Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://youtu.be/qyUnSuYYs18");
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to play video.\n" + ex.Message, "Warning Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonIgnore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to ignore warning window.\n" + ex.Message, "Warning Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //update configuration
                if (AppCommand.Instance.Configurations.ContainsKey(reportingInfo.DocId.ToString()))
                {
                    var updaterFound = from updater in AppCommand.Instance.Configurations[reportingInfo.DocId.ToString()].ProjectUpdaters where updater.UpdaterId == DTMUpdater.updaterGuid.ToString() select updater;
                    if (updaterFound.Count() > 0)
                    {
                        ProjectUpdater updater = updaterFound.First();
                        int updaterIndex = AppCommand.Instance.Configurations[reportingInfo.DocId.ToString()].ProjectUpdaters.IndexOf(updater);

                        var triggerFound = from trigger in updater.CategoryTriggers where trigger._id == reportingInfo.TriggerId select trigger;
                        if (triggerFound.Count() > 0)
                        {
                            CategoryTrigger trigger = triggerFound.First();
                            int triggerIndex = AppCommand.Instance.Configurations[reportingInfo.DocId.ToString()].ProjectUpdaters[updaterIndex].CategoryTriggers.IndexOf(trigger);

                            RequestQueue queue = new RequestQueue(Guid.NewGuid().ToString(), trigger._id, RequestState.Requested.ToString(), Environment.UserName, reportingInfo.ReportingUniqueId);
                            AppCommand.Instance.Configurations[reportingInfo.DocId.ToString()].ProjectUpdaters[updaterIndex].CategoryTriggers[triggerIndex].Requests.Add(queue);

                            string content = "";
                            string errMsg = "";
                            HttpStatusCode status = ServerUtil.PostRequestQueues(out content, out errMsg, queue);
                        }
                    }
                }
               
                this.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to ignore warning window.\n" + ex.Message, "Warning Window", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }
      
    }
}
