using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.HealthReport;
using HOK.MissionControl.Tools.HealthReport.ObjectTrackers;
using HOK.MissionControl.Tools.SheetTracker;
using HOK.MissionControl.Tools.SingleSession;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl
{
    [Name(nameof(Properties.Resources.MissionControl_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.MissionControl_Desc), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.MissionControl_ImageName), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.MissionControl_Namespace), typeof(Properties.Resources))]
    public class AppCommand : IExternalApplication
    {
        public static AppCommand Instance { get; private set; }
        public static SessionInfo SessionInfo { get; set; }
        public static Dictionary<string, DateTime> SynchTime { get; set; } = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> OpenTime { get; set; } = new Dictionary<string, DateTime>();

        public DoorUpdater DoorUpdaterInstance { get; set; }
        public DtmUpdater DtmUpdaterInstance { get; set; }
        public RevisionUpdater RevisionUpdaterInstance { get; set; }

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

                application.ControlledApplication.DocumentOpening += OnDocumentOpening;
                application.ControlledApplication.DocumentOpened += OnDocumentOpened;
                application.ControlledApplication.FailuresProcessing += FailureProcessor.CheckFailure;
                application.ControlledApplication.DocumentClosing += OnDocumentClosing;
                application.ControlledApplication.DocumentSynchronizingWithCentral += OnDocumentSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentSynchronized;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Un-registers all event handlers that were registered at startup.
        /// </summary>
        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpening -= OnDocumentOpening;
            application.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            application.ControlledApplication.FailuresProcessing -= FailureProcessor.CheckFailure;
            application.ControlledApplication.DocumentClosing -= OnDocumentClosing;
            application.ControlledApplication.DocumentSynchronizingWithCentral -= OnDocumentSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral -= OnDocumentSynchronized;

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
                if (string.IsNullOrEmpty(pathName) || args.DocumentType != DocumentType.Project) return;

                var fileInfo = BasicFileInfo.Extract(pathName);
                if (!fileInfo.IsWorkshared) return;

                var centralPath = fileInfo.CentralPath;
                if (string.IsNullOrEmpty(centralPath)) return;

                //serch for config
                var configFound = ServerUtilities.GetConfigurationByCentralPath(centralPath);
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

                    if (MissionControlSetup.Configurations.ContainsKey(centralPath))
                    {
                        MissionControlSetup.Configurations.Remove(centralPath);
                    }
                    MissionControlSetup.Configurations.Add(centralPath, configFound);

                    var projectFound = ServerUtilities.GetProjectByConfigurationId(configFound.Id);
                    if (null != projectFound)
                    {
                        if (MissionControlSetup.Projects.ContainsKey(centralPath))
                        {
                            MissionControlSetup.Projects.Remove(centralPath);
                        }
                        MissionControlSetup.Projects.Add(centralPath, projectFound);
                    }

                    OpenTime["from"] = DateTime.Now;
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Registers IUpdaters
        /// </summary>
        private void OnDocumentOpened(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                var doc = args.Document;
                if (doc == null || args.IsCancelled()) return;
                if (!doc.IsWorkshared) return;

                // (Konrad) HashCode will be the same for the document in this session.
                // If the same document will be open in a different session new HashCode is created.
                var centralPath = BasicFileInfo.Extract(doc.PathName).CentralPath;
                if (!MissionControlSetup.Projects.ContainsKey(centralPath)) return;
                if (!MissionControlSetup.Configurations.ContainsKey(centralPath)) return;

                var currentConfig = MissionControlSetup.Configurations[centralPath];
                var currentProject = MissionControlSetup.Projects[centralPath];

                // (Konrad) Register Updaters that are in the config file.
                SingleSessionMonitor.OpenedDocuments.Add(centralPath);
                ApplyConfiguration(doc, currentConfig);

                // (Konrad) It's possible that Health Report Document doesn't exist in database yet.
                // Create it and set the reference to it in Project if that's the case.
                if (!MonitorUtilities.IsUpdaterOn(currentProject, currentConfig, new Guid(Properties.Resources.HealthReportTrackerGuid))) return;

                bool refreshProject = false;
                if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath))
                {
                    var id = ServerUtilities.GetHealthRecordByCentralPath(centralPath);
                    if (string.IsNullOrEmpty(id))
                    {
                        id = ServerUtilities.PostDataScheme(new HealthReportData() { centralPath = centralPath }, "healthrecords").Id;
                        ServerUtilities.AddHealthRecordToProject(currentProject, id);
                        refreshProject = true;
                    }
                    MissionControlSetup.HealthRecordIds.Add(centralPath, id);
                }

                var recordId = MissionControlSetup.HealthRecordIds[centralPath];

                WorksetOpenSynch.PublishData(doc, recordId, currentConfig, currentProject, WorksetMonitorState.onopened);
                ModelMonitor.PublishModelSize(centralPath, recordId, currentConfig, currentProject);
                ModelMonitor.PublishSessionInfo(recordId, SessionEvent.documentOpened);
                if (OpenTime.ContainsKey("from"))
                {
                    ModelMonitor.PublishOpenTime(recordId);
                }
                
                if (!refreshProject) return;

                var projectFound = ServerUtilities.GetProjectByConfigurationId(currentConfig.Id);
                if (null == projectFound) return;
                MissionControlSetup.Projects[centralPath] = projectFound; // this won't be null since we checked before.
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
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

                UnregisterUpdaters(doc);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
                if (string.IsNullOrEmpty(centralPath)) return;

                // (Konrad) Setup Workset Open Monitor
                if (!MissionControlSetup.Projects.ContainsKey(centralPath) || !MissionControlSetup.Configurations.ContainsKey(centralPath)) return;
                if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath)) return;

                WorksetOpenSynch.PublishData(doc, centralPath, MissionControlSetup.Configurations[centralPath], MissionControlSetup.Projects[centralPath], WorksetMonitorState.onsynched);
                SynchTime["from"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Document Synchronized handler.
        /// </summary>
        private void OnDocumentSynchronized(object source, DocumentSynchronizedWithCentralEventArgs args)
        {
            try
            {
                if (args.Document == null) return;

                var centralPath = FileInfoUtil.GetCentralFilePath(args.Document);
                if (string.IsNullOrEmpty(centralPath)) return;

                FailureProcessor.IsSynchronizing = false;

                // (Konrad) If project is not registered with MongoDB let's skip this
                if(!MissionControlSetup.Projects.ContainsKey(centralPath) || !MissionControlSetup.Configurations.ContainsKey(centralPath)) return;
                if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath)) return;
                var recordId = MissionControlSetup.HealthRecordIds[centralPath];

                if (SynchTime.ContainsKey("from"))
                {
                    ModelMonitor.PublishSynchTime(recordId);
                }
                ModelMonitor.PublishSessionInfo(recordId, SessionEvent.documentSynched);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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

                    if (string.Equals(updater.updaterId.ToLower(),
                        DoorUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        DoorUpdaterInstance.Register(doc, updater);
                    }
                    else if (string.Equals(updater.updaterId.ToLower(),
                        DtmUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        DtmUpdaterInstance.Register(doc, updater);
                    }
                    else if (string.Equals(updater.updaterId.ToLower(),
                        RevisionUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        RevisionUpdaterInstance.Register(doc, updater);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
            if (!MissionControlSetup.Configurations.ContainsKey(centralPath)) return;

            var currentConfig = MissionControlSetup.Configurations[centralPath];
            foreach (var updater in currentConfig.updaters)
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

            // (Konrad) Make sure that Project and HealthRecords are specified.
            if (!MissionControlSetup.Projects.ContainsKey(centralPath)) return;
            if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath)) return;

            var recordId = MissionControlSetup.HealthRecordIds[centralPath];
            var currentProject = MissionControlSetup.Projects[centralPath];

            WorksetItemCount.PublishData(doc, recordId, currentConfig, currentProject);
            ViewMonitor.PublishData(doc, recordId, currentConfig, currentProject);
            LinkMonitor.PublishData(doc, recordId, currentConfig, currentProject);
            ModelMonitor.PublishSessionInfo(recordId, SessionEvent.documentClosed);
        }
    }
}