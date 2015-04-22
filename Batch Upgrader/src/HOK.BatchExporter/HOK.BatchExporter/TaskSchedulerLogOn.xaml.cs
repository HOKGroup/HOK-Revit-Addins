using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32.TaskScheduler;

namespace HOK.BatchExporter
{
    /// <summary>
    /// Interaction logic for TaskSchedulerLogOn.xaml
    /// </summary>
    public partial class TaskSchedulerLogOn : Window
    {
        private string userName = "";
        private string password = "";

        public string UserName { get { return userName; } set { userName = value; } }
        public string Password { get { return password; } set { password = value; } }

        public TaskSchedulerLogOn()
        {
            userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                       
            InitializeComponent();
            labelMessage.Visibility = Visibility.Hidden;
            textBoxUser.Text = userName;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            password = passwordBox.Password; 
            if (TestLogon(userName, password))
            {
                this.DialogResult = true;
            }
        }

        private bool TestLogon(string user, string pw)
        {
            bool result = false;
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    string exePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2014\HOK-BatchProcessor.bundle\Contents\HOK.BatchExportTrigger.exe";
                    ExecAction execAction = new ExecAction(exePath, null, null);
                    td.Actions.Add(execAction);

                    Task task = ts.RootFolder.RegisterTaskDefinition(@"Test", td, TaskCreation.CreateOrUpdate, user, pw, TaskLogonType.InteractiveTokenOrPassword);
                    if (null != task)
                    {
                        ts.RootFolder.DeleteTask("Test");
                        return true;
                    }
                }
            }
            catch
            {
                passwordBox.Clear();
                labelMessage.Visibility = Visibility.Visible;
                return false;
            }
            return result;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                password = passwordBox.Password;
                if (TestLogon(userName, password))
                {
                    this.DialogResult = true;
                }
            }
        }
    }
}
