using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.HealthReport.ObjectTrackers;
using HOK.MissionControl.Tools.RevisionTracker;
using HOK.MissionControl.Tools.SingleSession;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl
{
    public class AppCommand : IExternalApplication
    {
        public static AppCommand Instance { get; private set; }
        public Dictionary<string, Configuration> ConfigDictionary { get; set; } = new Dictionary<string, Configuration>();
        public Dictionary<string, Project> ProjectDictionary { get; set; } = new Dictionary<string, Project>();
        public DoorUpdater DoorUpdaterInstance { get; set; }
        public DtmUpdater DtmUpdaterInstance { get; set; }
        public RevisionUpdater RevisionUpdaterInstance { get; set; }
        public static bool RunExport { get; set; }

        /// <summary>
        /// Registers all event handlers during startup.
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Instance = this;
                var appId = application.ActiveAddInId;
                DoorUpdaterInstance = new DoorUpdater(appId);
                DtmUpdaterInstance = new DtmUpdater(appId);
                RevisionUpdaterInstance = new RevisionUpdater(appId);

                LogUtil.InitializeLog();
                LogUtil.AppendLog("Mission Control AddIn Started");

                application.ControlledApplication.DocumentOpening += OnDocumentOpening;
                application.ControlledApplication.DocumentOpened += OnDocumentOpened;
                application.ControlledApplication.FailuresProcessing += FailureProcessor.CheckFailure;
                application.ControlledApplication.DocumentClosing += OnDocumentClosing;
                application.ControlledApplication.DocumentSynchronizingWithCentral += OnDocumentSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentSynchronized;
                application.Idling += OnIdling;
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("OnStartup:" + ex.Message);
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Un-registers all event handlers that were registered at startup.
        /// </summary>
        public Result OnShutdown(UIControlledApplication application)
        {
            LogUtil.WriteLog();

            application.ControlledApplication.DocumentOpening -= OnDocumentOpening;
            application.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            application.ControlledApplication.FailuresProcessing -= FailureProcessor.CheckFailure;
            application.ControlledApplication.DocumentClosing -= OnDocumentClosing;
            application.ControlledApplication.DocumentSynchronizingWithCentral -= OnDocumentSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral -= OnDocumentSynchronized;
            application.Idling -= OnIdling;

            return Result.Succeeded;
        }

        /// <summary>
        /// Retrieves the configuration from Database.
        /// </summary>
        private void OnDocumentOpening(object source, DocumentOpeningEventArgs args)
        {
            try
            {
                var pathName = args.PathName;
                if (String.IsNullOrEmpty(pathName) || args.DocumentType != DocumentType.Project) return;

                var fileInfo = BasicFileInfo.Extract(pathName);
                if (!fileInfo.IsWorkshared) return;

                var centralPath = fileInfo.CentralPath;
                if (String.IsNullOrEmpty(centralPath)) return;

                //serch for config
                LogUtil.AppendLog(centralPath + " Opening.");
                var configFound = ServerUtil.GetConfigurationByCentralPath(centralPath);
                if (null != configFound)
                {
                    //check if the single session should be activated
                    if (SingleSessionMonitor.CancelOpening(centralPath, configFound))
                    {
                        if (args.Cancellable)
                        {
                            var ssWindow = new SingleSessionWindow(centralPath);
                            var o = ssWindow.ShowDialog();
                            if (o != null && (bool)o)
                            {
                                args.Cancel();
                                return;
                            }
                        }
                    }

                    if (ConfigDictionary.ContainsKey(centralPath))
                    {
                        ConfigDictionary.Remove(centralPath);
                    }
                    ConfigDictionary.Add(centralPath, configFound);

                    var projectFound = ServerUtil.GetProjectByConfigurationId(configFound.Id);
                    if (null != projectFound)
                    {
                        if (ProjectDictionary.ContainsKey(centralPath))
                        {
                            ProjectDictionary.Remove(centralPath);
                        }
                        ProjectDictionary.Add(centralPath, projectFound);
                    }

                    LogUtil.AppendLog("Configuration Found: " + configFound.Id);
                }
                else
                {
                    //not a seed file, just check if the single session is activated
                    if (!SingleSessionMonitor.SingleSessionActivated) return;

                    var ssWindow = new SingleSessionWindow(centralPath);
                    var o = ssWindow.ShowDialog();
                    if (o != null && (bool)o)
                    {
                        args.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("CollectConfigurationOnOpening:" + ex.Message);
            }
        }

        /// <summary>
        /// Registers IUpdaters
        /// </summary>
        private void OnDocumentOpened(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                if (null == args.Document || args.IsCancelled()) return;

                var doc = args.Document;
                if (!doc.IsWorkshared) return;

                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (String.IsNullOrEmpty(centralPath)) return;

                // (Konrad) Register Updaters that are in the config file.
                SingleSessionMonitor.OpenedDocuments.Add(centralPath);
                LogUtil.AppendLog(centralPath + " Opened.");
                if (ConfigDictionary.ContainsKey(centralPath))
                {
                    ApplyConfiguration(doc, ConfigDictionary[centralPath]);
                }

                // (Konrad) Setup Workset Open Monitor
                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath)) return;

                bool refreshProject;
                WorksetOpenSynch.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath], WorksetMonitorState.onOpen, centralPath, out refreshProject);

                if (!refreshProject) return;

                var projectFound = ServerUtil.GetProjectByConfigurationId(ConfigDictionary[centralPath].Id);
                if (null == projectFound) return;

                if (ProjectDictionary.ContainsKey(centralPath))
                {
                    ProjectDictionary[centralPath] = projectFound;
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("RegisterUpdatersOnOpen:" + ex.Message);
            }
        }

        /// <summary>
        /// Publishes FamilyMonitor data.
        /// </summary>
        private void OnIdling(object sender, IdlingEventArgs e)
        {
            if (!RunExport) return;

            var uiApp = sender as UIApplication;
            var doc = uiApp?.ActiveUIDocument.Document;
            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            FamilyMonitor.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);
        }

        /// <summary>
        /// Unregisters all IUpdaters that were registered onDocumentOpening
        /// </summary>
        private void OnDocumentClosing(object source, DocumentClosingEventArgs args)
        {
            try
            {
                var doc = args.Document;
                if (!doc.IsWorkshared) return;

                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var response = ServerUtil.GetFamilyStats(ProjectDictionary[centralPath].worksets.FirstOrDefault(), "familystats");
                var lastWeek = DateTime.Now.AddDays(-7);
                var alreadyPosted = response.familyStats.FirstOrDefault(x => x.createdOn > lastWeek && x.createdBy == Environment.UserName);

                if (alreadyPosted == null)
                {
                    var dialogResult = MessageBox.Show("Want to export family info?", "Mission Control", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        if (args.Cancellable) args.Cancel();
                        RunExport = !RunExport;
                    }
                    else
                    {
                        UnregisterUpdaters(doc);
                    }
                }
                else
                {
                    UnregisterUpdaters(doc);
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("UnregisterUpdaterOnClosing:" + ex.Message);
            }
        }

        /// <summary>
        /// Handler for Document Synchronizing event.
        /// </summary>
        private void OnDocumentSynchronizing(object source, DocumentSynchronizingWithCentralEventArgs args)
        {
            try
            {
                FailureProcessor.IsSynchronizing = true;

                if (args.Document == null) return;

                var doc = args.Document;
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (String.IsNullOrEmpty(centralPath)) return;

                // (Konrad) Setup Workset Open Monitor
                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath)) return;

                bool refreshProject;
                WorksetOpenSynch.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath], WorksetMonitorState.onSynch, centralPath, out refreshProject);

                if (!refreshProject) return;

                var projectFound = ServerUtil.GetProjectByConfigurationId(ConfigDictionary[centralPath].Id);
                if (null == projectFound) return;

                if (ProjectDictionary.ContainsKey(centralPath))
                {
                    ProjectDictionary[centralPath] = projectFound;
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DocumentSynchronizing:" + ex.Message);
            }
        }

        /// <summary>
        /// Document Synchronized handler.
        /// </summary>
        private static void OnDocumentSynchronized(object source, DocumentSynchronizedWithCentralEventArgs args)
        {
            try
            {
                FailureProcessor.IsSynchronizing = false;
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Registers availble configuration based on Central Model path match.
        /// </summary>
        /// <param name="doc">Revit Document</param>
        /// <param name="config">Mission Control Configuration</param>
        private void ApplyConfiguration(Document doc, Configuration config)
        {
            try
            {
                foreach (var updater in config.updaters)
                {
                    if (!updater.isUpdaterOn) continue;

                    if (String.Equals(updater.updaterId.ToLower(),
                        DoorUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        DoorUpdaterInstance.Register(doc, updater);
                    }
                    else if (String.Equals(updater.updaterId.ToLower(),
                        DtmUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        DtmUpdaterInstance.Register(doc, updater);
                    }
                    else if (String.Equals(updater.updaterId.ToLower(),
                        RevisionUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        RevisionUpdaterInstance.Register(doc, updater);
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ApplyConfiguration:" + ex.Message);
            }
        }

        /// <summary>
        /// Unregisters all updaters as well as posts data.
        /// </summary>
        /// <param name="doc">Revit Document</param>
        private void UnregisterUpdaters(Document doc)
        {
            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            if (string.IsNullOrEmpty(centralPath)) return;

            SingleSessionMonitor.CloseFile(centralPath);
            if (!ConfigDictionary.ContainsKey(centralPath)) return;

            var configFound = ConfigDictionary[centralPath];
            foreach (var updater in configFound.updaters)
            {
                if (!updater.isUpdaterOn) { continue; }
                if (string.Equals(updater.updaterId.ToLower(),
                    DoorUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                {
                    DoorUpdaterInstance.Unregister(doc);
                }
                else if (string.Equals(updater.updaterId.ToLower(),
                    DtmUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                {
                    DtmUpdaterInstance.Unregister(doc);
                }
                else if (string.Equals(updater.updaterId.ToLower(),
                    RevisionUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                {
                    RevisionUpdaterInstance.Unregister(doc);
                }
            }

            // (Konrad) Setup Workset Item count.
            if (!ProjectDictionary.ContainsKey(centralPath)) return;

            bool refreshProject;
            WorksetItemCount.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath], centralPath, out refreshProject);
            ViewMonitor.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);
            LinkMonitor.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);

            if (!refreshProject) return;

            var projectFound = ServerUtil.GetProjectByConfigurationId(ConfigDictionary[centralPath].Id);
            if (null == projectFound) return;

            if (ProjectDictionary.ContainsKey(centralPath))
            {
                ProjectDictionary[centralPath] = projectFound;
            }
        }
    }
}
