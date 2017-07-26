using System;
using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<string, Configuration> ConfigDictionary { get; set; } = new Dictionary<string, Configuration>();
        public Dictionary<string, Project> ProjectDictionary { get; set; } = new Dictionary<string, Project>();
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
                Log.AppendLog(LogMessageType.INFO, centralPath + " opening...");
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

                    if (ConfigDictionary.ContainsKey(centralPath))
                    {
                        ConfigDictionary.Remove(centralPath);
                    }
                    ConfigDictionary.Add(centralPath, configFound);

                    var projectFound = ServerUtilities.GetProjectByConfigurationId(configFound.Id);
                    if (null != projectFound)
                    {
                        if (ProjectDictionary.ContainsKey(centralPath))
                        {
                            ProjectDictionary.Remove(centralPath);
                        }
                        ProjectDictionary.Add(centralPath, projectFound);
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
                if (null == args.Document || args.IsCancelled()) return;

                var doc = args.Document;
                if (!doc.IsWorkshared) return;

                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrEmpty(centralPath)) return;

                // (Konrad) Register Updaters that are in the config file.
                SingleSessionMonitor.OpenedDocuments.Add(centralPath);
                if (ConfigDictionary.ContainsKey(centralPath))
                {
                    ApplyConfiguration(doc, ConfigDictionary[centralPath]);
                }

                // (Konrad) Setup Workset Open Monitor
                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath)) return;

                bool refreshProject;
                WorksetOpenSynch.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath], WorksetMonitorState.onopened, centralPath, out refreshProject);
                ModelMonitor.PublishModelSize(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath], centralPath);
                ModelMonitor.PublishSessionInfo(ProjectDictionary[centralPath].worksets.FirstOrDefault(), SessionEvent.documentOpened);
                if (OpenTime.ContainsKey("from"))
                {
                    ModelMonitor.PublishOpenTime(ProjectDictionary[centralPath].worksets.FirstOrDefault());
                }
                
                if (!refreshProject) return;

                var projectFound = ServerUtilities.GetProjectByConfigurationId(ConfigDictionary[centralPath].Id);
                if (null == projectFound) return;
                ProjectDictionary[centralPath] = projectFound; // this won't be null since we checked before.
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

                //// (Konrad) Only prompt user once a week if they are willing to post Family stats to MongoDB.
                //// This usually takes some time so it's best to minimize the annoyance factor. Getting data
                //// from every user on the project once a week should be more than enough.
                //var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                //var response = ServerUtilities.GetFamilyStats(ProjectDictionary[centralPath].worksets.FirstOrDefault(), "familystats");
                //var lastWeek = DateTime.Now.AddDays(-7);
                //var alreadyPosted = response.familyStats.FirstOrDefault(x => x.createdOn > lastWeek && x.createdBy == Environment.UserName);

                //if (alreadyPosted == null)
                //{
                //    var famViewModel = new FamilyMonitorViewModel();
                //    var famWindow = new FamilyMonitorView { DataContext = famViewModel };
                //    var showDialog = famWindow.ShowDialog();
                //    if (showDialog != null && (bool)showDialog)
                //    {
                //        if (args.Cancellable) args.Cancel();
                //        RunExport = !RunExport;
                //    }
                //    else
                //    {
                //        UnregisterUpdaters(doc);
                //    }
                //}
                //else
                //{
                //    UnregisterUpdaters(doc);
                //}
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
                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath)) return;

                bool refreshProject;
                WorksetOpenSynch.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath], WorksetMonitorState.onsynched, centralPath, out refreshProject);
                SynchTime["from"] = DateTime.Now;

                if (!refreshProject) return;

                var projectFound = ServerUtilities.GetProjectByConfigurationId(ConfigDictionary[centralPath].Id);
                if (null == projectFound) return;

                if (ProjectDictionary.ContainsKey(centralPath))
                {
                    ProjectDictionary[centralPath] = projectFound;
                }
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
                if (SynchTime.ContainsKey("from"))
                {
                    ModelMonitor.PublishSynchTime(ProjectDictionary[centralPath].worksets.FirstOrDefault());
                }
                ModelMonitor.PublishSessionInfo(ProjectDictionary[centralPath].worksets.First(), SessionEvent.documentSynched);
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
            ModelMonitor.PublishSessionInfo(ProjectDictionary[centralPath].worksets.First(), SessionEvent.documentClosed);

            // TODO: Do we need this?
            if (!refreshProject) return;

            var projectFound = ServerUtilities.GetProjectByConfigurationId(ConfigDictionary[centralPath].Id);
            if (null == projectFound) return;

            if (ProjectDictionary.ContainsKey(centralPath))
            {
                ProjectDictionary[centralPath] = projectFound;
            }
        }
    }
}