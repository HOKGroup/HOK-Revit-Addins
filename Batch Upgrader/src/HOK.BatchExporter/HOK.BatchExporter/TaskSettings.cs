using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32.TaskScheduler;
using System.IO;
using System.Diagnostics;
using System.Management;

namespace HOK.BatchExporter
{
    public class TaskSettings
    {
        private string username = "";
        private string password = "";
        private string exePath2014 = "";
        private string exePath2015 = "";

        public string UserName { get { return username; } set { username = value; } }
        public string PassWord { get { return password; } set { password = value; } }

        public TaskSettings()
        {
            exePath2014 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2014\HOK-BatchProcessor.bundle\Contents\HOK.BatchUpgradTrigger.exe";
            exePath2015 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2015\HOK-BatchProcessor.bundle\Contents\HOK.BatchUpgradTrigger.exe";
            CreateTaskFolder();
        }

        public bool InitializeTaskScheduler()
        {
            bool result = false;
            try
            {
                TaskSchedulerLogOn taskLogOn = new TaskSchedulerLogOn();
                taskLogOn.Owner = Application.Current.MainWindow;

                Nullable<bool> results = taskLogOn.ShowDialog();
                if (results == true)
                {
                    username = taskLogOn.UserName;
                    password = taskLogOn.Password;

                    taskLogOn.Close();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot initialize the default settings for the task scheduler.\n" + ex.Message, "InitializeTaskScheduler", MessageBoxButton.OK);
                result = false;
            }
            return result;
        }

        public bool ClearScheduledTask()
        {
            bool result = false;
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskFolder tf = ts.GetFolder("BatchUpgrader");
                    foreach (Task task in tf.Tasks)
                    {
                        tf.DeleteTask(task.Name);
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to clear all scheduled tasks.\n"+ex.Message, "Clear Task Scheduled Task", MessageBoxButton.OK);
            }
            return result;
        }

        public bool CreateScheduluedTask(ProjectSettings ps)
        {
            bool result = false;
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskFolder tf = ts.GetFolder("BatchUpgrader");

                    TaskDefinition td = ts.NewTask();
                    td.Principal.UserId = username;

                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    td.Settings.Enabled = true;
                    td.Settings.StartWhenAvailable = true;
                    td.Settings.Priority = System.Diagnostics.ProcessPriorityClass.High;
                    td.Settings.WakeToRun = true;

                    //Upgrade trigger
                    td.Actions.Clear();
                    string taskName = ps.UpgradeOptions.TaskScheduleSettings.TaskName;
                    td.RegistrationInfo.Description = GetDescription(ps, "Upgrade Version");
                    string exePath = (ps.UpgradeOptions.UpgradeVersion == "2014") ? exePath2014 : exePath2015;
                    ExecAction execAction = new ExecAction(exePath, taskName, null);
                    td.Actions.Add(execAction);
                    td.Triggers.Clear();
                    DateTime startTime = ps.UpgradeOptions.TaskScheduleSettings.StartTime;
                    TimeTrigger timeTrigger = new TimeTrigger { StartBoundary = startTime };
                    td.Triggers.Add(timeTrigger);
                    Task task = tf.RegisterTaskDefinition(ps.UpgradeOptions.TaskScheduleSettings.TaskName, td, TaskCreation.CreateOrUpdate, username, password, TaskLogonType.InteractiveTokenOrPassword);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Project Name:"+ps.ProjectName+"\nCannot create a scheduled task.\n" + ex.Message, "CreateScheduluedTask", MessageBoxButton.OK);
            }
            return result;
        }

        public string GetDescription(ProjectSettings ps, string fileType)
        {
            string description = "";
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine("This will automatically run HOK's Batch Processor on schedule.");
            strBuilder.AppendLine("Project Name: " + ps.ProjectName);
            strBuilder.AppendLine("Office: " + ps.Office);
            strBuilder.AppendLine("Task Type: " + fileType);
            strBuilder.AppendLine("Configuration File: " + ps.ConfigFileName);

            description = strBuilder.ToString();
            return description;
        }

        private void CreateTaskFolder()
        {
            try
            {
                string taskFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\Tasks\\BatchUpgrader";
                if (!Directory.Exists(taskFolderPath))
                {
                    Directory.CreateDirectory(taskFolderPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create task folder.\n" + ex.Message, "CreateTaskFolder", MessageBoxButton.OK);
            }
        }

        public bool DeleteExistingTask(string taskName)
        {
            bool result = false;
            try
            {
                if (!string.IsNullOrEmpty(taskName))
                {
                    using (TaskService ts = new TaskService())
                    {
                        TaskFolder tf = ts.GetFolder("BatchUpgrader");
                        tf.DeleteTask(taskName);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                //MessageBox.Show("Cannot delete the task named "+taskName+"\n"+ex.Message, "DeleteExisitngTask", MessageBoxButton.OK);
            }
            return result;
        }

        public bool RunTask(string taskname)
        {
            bool result = false;
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskFolder tf = ts.GetFolder("BatchUpgrader");
                    foreach (Task task in tf.Tasks)
                    {
                        if (task.Name == taskname)
                        {
                            if (task.Enabled) { task.Run(); }
                            else
                            {
                                task.Enabled = true;
                                task.Run();
                                task.Enabled = false;
                            }
                            
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Task Name:"+taskname+"\nCannot run the task."+ex.Message, "Run Tasks", MessageBoxButton.OK);
            }
            return result;
        }

        public void RevitKill()
        {
            IList<int> intRevitProcIDs = new List<int>();

            // Get all revit.exe process ids
            intRevitProcIDs = ProcessGet("REVIT");

            // Make sure at least one valid id is found
            if (intRevitProcIDs.Count == 0)
                return;

            // Attempt to end all revit.exe precesses
            foreach (int intRevitProcID in intRevitProcIDs)
            {
                // End the process
                Process prs = System.Diagnostics.Process.GetProcessById(intRevitProcID);
                prs.Kill();
            }
        }

        private IList<int> ProcessGet(string strProcess)
        {
            IList<int> intRevitProcIDs = new List<int>();

            // Get the processes
            Process[] processlist = Process.GetProcesses();

            // Iterate through each process
            foreach (Process theprocess in processlist)
            {
                // Look for the wanted process name
                if (theprocess.ProcessName.ToUpper() == strProcess.ToUpper())
                {
                    // Get only processes by the logged in user
                    if (ProcessOwnerGet(theprocess.Id).ToUpper() == Environment.GetEnvironmentVariable("USERNAME").ToUpper())
                    {
                        intRevitProcIDs.Add(theprocess.Id);
                    }
                }
            }
            return intRevitProcIDs;
        }

        private static string ProcessOwnerGet(int processId)
        {
            // Get the processes
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            // Iterate through each process
            foreach (ManagementObject obj in processList)
            {
                // Get the user nam or owner
                string[] argList = new string[] { string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                    return argList[0];
            }
            return "NO OWNER";
        }
    }
}
