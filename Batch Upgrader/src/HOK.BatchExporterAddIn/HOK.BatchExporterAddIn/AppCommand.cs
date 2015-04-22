using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Management;
using Autodesk.Revit.DB.Events;


namespace HOK.BatchExporterAddIn
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

    public class AppCommand :IExternalApplication
    {
        public UIControlledApplication uiControlApp;
        private bool addinActivated = false;
        private string currentTaskName = "";
        private TaskType currentTaskType;
        private List<Guid> FailureDefinitionIds = new List<Guid>();

#if RELEASE2014
        private string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\BatchUpgrader";
#elif RELEASE2015
        private string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2015\\BatchUpgrader";
#endif

        private Dictionary<string/*config file*/, ProjectSettings> projectSettings = new Dictionary<string, ProjectSettings>();
        private DataReader dataReader;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            uiControlApp = application;

            addinActivated = IsAddInActivated();
            currentTaskName = GetCurrentTaskName();

            if (addinActivated && !string.IsNullOrEmpty(currentTaskName))
            {
                dataReader = new DataReader();
                projectSettings = dataReader.ProjectSettingsDictionary;

                uiControlApp.DialogBoxShowing += new EventHandler<DialogBoxShowingEventArgs>(HandleDialogBoxShowing);
                uiControlApp.Idling += new EventHandler<IdlingEventArgs>(IdlingUpdate);
                uiControlApp.ControlledApplication.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessing);
            }
            return Result.Succeeded;
        }

        private bool IsAddInActivated()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null == registryKey)
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(registryKey.GetValue("ActivateAddIn").ToString());
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return false;
        }

        private string GetCurrentTaskName()
        {
            string taskName = "";
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null != registryKey)
                {
                    taskName = registryKey.GetValue("CurrentTaskName").ToString();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return taskName;
        }

        //run by task scheduler or instant export to IFC
        public void IdlingUpdate(object sender, IdlingEventArgs arg) 
        {
            UIApplication uiApp = sender as UIApplication;

            try
            {
                if (projectSettings.Count > 0)
                {

                    ProjectSettings ps = FindProjectOfTask();
                    if (null != ps)
                    {
                        UtilityMethods utilityMethods = new UtilityMethods(uiApp, ps);
                        utilityMethods.Upgrade();
                    }
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogFileManager.AppendLog("[Error] Idling Updates", message);
            }
            finally
            {
                SetRegistryKey("ActivateAddIn", false);
                uiControlApp.DialogBoxShowing -= new EventHandler<DialogBoxShowingEventArgs>(HandleDialogBoxShowing);
                uiControlApp.Idling -= new EventHandler<IdlingEventArgs>(IdlingUpdate);
                uiControlApp.ControlledApplication.FailuresProcessing -= new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessing);
                RevitKill();
            }
        }

        private ProjectSettings FindProjectOfTask()
        {
            ProjectSettings selectedProject = null;
            try
            {
                foreach (string configFile in projectSettings.Keys)
                {
                    ProjectSettings ps = projectSettings[configFile];
                    if (ps.UpgradeOptions.TaskScheduleSettings.TaskName == currentTaskName)
                    {
                        currentTaskType = TaskType.Upgrade;
                        return ps;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return selectedProject;
        }

        private void RevitKill()
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

        public IList<int> ProcessGet(string strProcess)
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

        public static string ProcessOwnerGet(int processId)
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

        public void SetRegistryKey(string keyName, object value)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null == registryKey)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(keyAddress);
                }
                registryKey.SetValue(keyName, value);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void HandleDialogBoxShowing(object sender, Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs e)
        {
            TaskDialogShowingEventArgs taskEvent= e as TaskDialogShowingEventArgs;
            MessageBoxShowingEventArgs msgEvent=e as MessageBoxShowingEventArgs;
            try
            {
                if (taskEvent != null)
                {
                    //  Click OK
                    string dialogId = taskEvent.DialogId;
                    int helpId = taskEvent.HelpId;
                    string message = taskEvent.Message;
                    LogFileManager.AppendLog("TaskDialog Message", message);
                    taskEvent.OverrideResult((int)WinForms.DialogResult.OK);
                }
                else if (msgEvent != null)
                {
                    int okid = (int)WinForms.DialogResult.OK;
                    int dialogType=msgEvent.DialogType;
                    int helpId = msgEvent.HelpId;
                    string message = msgEvent.Message;
                    LogFileManager.AppendLog("MessageBox Message", message);
                    msgEvent.OverrideResult(okid);
                }
                else
                {
                    LogFileManager.AppendLog("Windows MessageBox Id", e.HelpId.ToString());
                    e.OverrideResult(1);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor= e.GetFailuresAccessor();
            //string transactionName = failuresAccessor.GetTransactionName();

            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count>0)
            {
                List<FailureMessageInfo> failureMessageInfoList = new List<FailureMessageInfo>();
                foreach (FailureMessageAccessor fma in fmas)
                {
                    FailureMessageInfo messageInfo = new FailureMessageInfo();
                    try
                    {
                        messageInfo.ErrorMessage = fma.GetDescriptionText();
                        messageInfo.ErrorSeverity = fma.GetSeverity().ToString();
                        messageInfo.FailingElementIds = fma.GetFailingElementIds().ToList();
                    }
                    catch { messageInfo.ErrorMessage = "Unknown Error"; }
                    failureMessageInfoList.Add(messageInfo);

                    //add log message
                    FailureDefinitionId definitionId = fma.GetFailureDefinitionId();
                    Guid defGuid = definitionId.Guid;
                    if (!FailureDefinitionIds.Contains(defGuid))
                    {
                        LogFileManager.AppendLog(messageInfo);
                        FailureDefinitionIds.Add(defGuid);
                    }

                    if (FailureSeverity.Warning == fma.GetSeverity())
                    {
                        failuresAccessor.DeleteWarning(fma);
                    }
                    else if (FailureSeverity.Error == fma.GetSeverity())
                    {
                        e.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        return;
                    }
                }

                if (failuresAccessor.IsFailureResolutionPermitted())
                {
                    failuresAccessor.ResolveFailures(fmas);
                }
                
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
                return;
            }
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }
    }

    public enum TaskType
    {
        IFCExport=0, DWGExport, PDFPrint, Utility, Upgrade
    }
}
