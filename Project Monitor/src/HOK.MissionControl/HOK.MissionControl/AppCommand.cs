using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using HOK.Core.Utilities;
using HOK.MissionControl.Utils;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Tools.Communicator.HealthReport;
using HOK.MissionControl.Tools.Communicator.Socket;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.HealthReport;
using HOK.MissionControl.Tools.LinkUnloadMonitor;
using HOK.MissionControl.Tools.SheetTracker;

namespace HOK.MissionControl
{
    [Name(nameof(Properties.Resources.MissionControl_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.MissionControl_Desc), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.MissionControl_ImageName), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.MissionControl_Namespace), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.MissionControl_PanelName), typeof(Properties.Resources))]
    [AdditionalButtonNames(nameof(Properties.Resources.MissionControl_AdditionalButtons), typeof(Properties.Resources))]
    public class AppCommand : IExternalApplication
    {
        public static AppCommand Instance { get; private set; }
        public static SessionInfo SessionInfo { get; set; }
        public static Dictionary<string, DateTime> SynchTime { get; set; } = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> OpenTime { get; set; } = new Dictionary<string, DateTime>();
        private static Queue<Action<UIApplication>> Tasks;
        public static HealthReportData HrData { get; set; }
        public static SheetData SheetsData { get; set; }
        public static CommunicatorView CommunicatorWindow { get; set; }
        public PushButton CommunicatorButton { get; set; }
        public DoorUpdater DoorUpdaterInstance { get; set; }
        public DtmUpdater DtmUpdaterInstance { get; set; }
        public SheetUpdater SheetUpdaterInstance { get; set; }
        public LinkUnloadMonitor LinkUnloadInstance { get; set; }
        private const string tabName = "  HOK - Beta";
        public static bool IsSynching { get; set; }

        public static CommunicatorRequestHandler CommunicatorHandler { get; set; }
        public static ExternalEvent CommunicatorEvent { get; set; }
        public static Dictionary<string, FamilyItem> FamiliesToWatch { get; set; } = new Dictionary<string, FamilyItem>();

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
                SheetUpdaterInstance = new SheetUpdater(appId);
                LinkUnloadInstance = new LinkUnloadMonitor();
                Tasks = new Queue<Action<UIApplication>>();

                application.Idling += OnIdling;
                application.ControlledApplication.DocumentOpening += OnDocumentOpening;
                application.ControlledApplication.DocumentOpened += OnDocumentOpened;
                application.ControlledApplication.FailuresProcessing += FailureProcessor.CheckFailure;
                application.ControlledApplication.DocumentClosing += OnDocumentClosing;
                application.ControlledApplication.DocumentSynchronizingWithCentral += OnDocumentSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentSynchronized;
                application.ControlledApplication.DocumentCreated += OnDocumentCreated;

                // (Konrad) Create Communicator button and register dockable panel.
                RegisterCommunicator(application);
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    Log.AppendLog(LogMessageType.ERROR, "Ribbon tab was not created: " + tabName);
                }

                var currentAssembly = Assembly.GetAssembly(GetType()).Location;
                var panel = application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Mission Control")
                            ?? application.CreateRibbonPanel(tabName, "Mission Control");
                CommunicatorButton = (PushButton)panel.AddItem(new PushButtonData("Communicator_Command", "Show/Hide" + Environment.NewLine + "Communicator",
                    currentAssembly, "HOK.MissionControl.Tools.Communicator.CommunicatorCommand"));

                // (Konrad) Since Communicator Task Assistant offers to open Families for editing,
                // it requires an External Event because new document cannot be opened from Idling Event
                CommunicatorHandler = new CommunicatorRequestHandler();
                CommunicatorEvent = ExternalEvent.Create(CommunicatorHandler);

                // (Konrad) in order not to become out of synch with the database we need a way
                // to communicate live updates from the database to task assistant/communicator
                new Thread( new MissionControlSocket().Main){ Priority = ThreadPriority.BelowNormal }.Start();

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
            application.Idling -= OnIdling;
            application.ControlledApplication.DocumentOpening -= OnDocumentOpening;
            application.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            application.ControlledApplication.FailuresProcessing -= FailureProcessor.CheckFailure;
            application.ControlledApplication.DocumentClosing -= OnDocumentClosing;
            application.ControlledApplication.DocumentSynchronizingWithCentral -= OnDocumentSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral -= OnDocumentSynchronized;
            application.ControlledApplication.DocumentCreated -= OnDocumentCreated;

            return Result.Succeeded;
        }

        /// <summary>
        /// Retrieves the configuration from Database.
        /// </summary>
        private static void OnDocumentOpening(object source, DocumentOpeningEventArgs args)
        {
            try
            {
                var pathName = args.PathName;
                if (string.IsNullOrEmpty(pathName) || args.DocumentType != DocumentType.Project) return;

                var fileInfo = BasicFileInfo.Extract(pathName);
                if (!fileInfo.IsWorkshared) return;

                var centralPath = fileInfo.CentralPath;
                if (string.IsNullOrEmpty(centralPath)) return;

                //search for config
                var configFound = ServerUtilities.GetConfigurationByCentralPath(centralPath);
                if (null != configFound)
                {
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
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Handles Communicator button image setting.
        /// </summary>
        private static void OnDocumentCreated(object sender, DocumentCreatedEventArgs e)
        {
            new CommunicatorHealthReportModel().SetCommunicatorImage();
        }

        /// <summary>
        /// Registers IUpdaters
        /// </summary>
        private void OnDocumentOpened(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                // (Konrad) We need to set the Communicator Button image first, or it will be blank.
                new CommunicatorHealthReportModel().SetCommunicatorImage();

                var doc = args.Document;
                if (doc == null || args.IsCancelled()) return;
                if (!doc.IsWorkshared) return;

                var centralPath = BasicFileInfo.Extract(doc.PathName).CentralPath;
                if (!MissionControlSetup.Projects.ContainsKey(centralPath)) return;
                if (!MissionControlSetup.Configurations.ContainsKey(centralPath)) return;

                var currentConfig = MissionControlSetup.Configurations[centralPath];
                var currentProject = MissionControlSetup.Projects[centralPath];

                // (Konrad) Register Updaters that are in the config file.
                ApplyConfiguration(doc, currentConfig);

                // (Konrad) It's possible that Health Report Document doesn't exist in database yet.
                // Create it and set the reference to it in Project if that's the case.
                if (MonitorUtilities.IsUpdaterOn(currentProject, currentConfig,
                    new Guid(Properties.Resources.HealthReportTrackerGuid)))
                {
                    var refreshProject = false;
                    if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath))
                    {
                        HrData = ServerUtilities.GetHealthRecordByCentralPath(centralPath);
                        if (HrData == null)
                        {
                            HrData = ServerUtilities.Post<HealthReportData>(new HealthReportData { centralPath = centralPath }, "healthrecords");
                            ServerUtilities.AddHealthRecordToProject(currentProject, HrData.Id);
                            refreshProject = true;
                        }
                        else
                        {
                            MissionControlSetup.HealthRecordIds.Add(centralPath, HrData.Id); // store health record
                            //CommunicatorWindow.DataContext = new CommunicatorViewModel(); // create new communicator VM
                        }
                    }

                    var recordId = MissionControlSetup.HealthRecordIds[centralPath];

                    WorksetOpenSynch.PublishData(doc, recordId, currentConfig, currentProject, WorksetMonitorState.onopened);
                    ModelMonitor.PublishModelSize(centralPath, recordId, currentConfig, currentProject, doc.Application.VersionNumber);
                    ModelMonitor.PublishSessionInfo(recordId, SessionEvent.documentOpened);
                    if (OpenTime.ContainsKey("from"))
                    {
                        ModelMonitor.PublishOpenTime(recordId);
                    }

                    if (refreshProject)
                    {
                        var projectFound = ServerUtilities.GetProjectByConfigurationId(currentConfig.Id);
                        if (null == projectFound) return;
                        MissionControlSetup.Projects[centralPath] = projectFound; // this won't be null since we checked before.
                    }
                }

                // (Konrad) This tool will reset Shared Parameters Location to one specified in Mission Control
                if (currentConfig.GetType().GetProperty("sharedParamMonitor") != null && currentConfig.sharedParamMonitor.isMonitorOn)
                {
                    if (File.Exists(currentConfig.sharedParamMonitor.filePath))
                    {
                        doc.Application.SharedParametersFilename = currentConfig.sharedParamMonitor.filePath;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void LunchCommunicator()
        {
            var control = CommunicatorWindow.MainControl;
            control.Dispatcher.Invoke(() =>
            {
                CommunicatorWindow.DataContext = new CommunicatorViewModel(); // create new communicator VM
            }, DispatcherPriority.Normal);
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
        private static void OnDocumentSynchronizing(object source, DocumentSynchronizingWithCentralEventArgs args)
        {
            try
            {
                IsSynching = true; // disables DTM Tool
                FailureProcessor.IsSynchronizing = true;
                if (args.Document == null) return;

                var doc = args.Document;
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrEmpty(centralPath)) return;

                // (Konrad) Setup Health Report Monitor
                if (!MissionControlSetup.Projects.ContainsKey(centralPath) || !MissionControlSetup.Configurations.ContainsKey(centralPath)) return;
                if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath)) return;
                var recordId = MissionControlSetup.HealthRecordIds[centralPath];

                WorksetOpenSynch.PublishData(doc, recordId, MissionControlSetup.Configurations[centralPath], MissionControlSetup.Projects[centralPath], WorksetMonitorState.onsynched);
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
        private static void OnDocumentSynchronized(object source, DocumentSynchronizedWithCentralEventArgs args)
        {
            try
            {
                IsSynching = false; // enables DTM Tool again
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
        /// Handled Idling events. Currently Communicator uses it to interact with Revit.
        /// It checks a queue for any outstanding tasks and executes them.
        /// </summary>
        private static void OnIdling(object sender, IdlingEventArgs e)
        {
            var app = (UIApplication)sender;
            lock (Tasks)
            {
                if (Tasks.Count <= 0) return;

                var task = Tasks.Dequeue();
                task(app);
            }
        }

        /// <summary>
        /// Replacement method for reloading latest which disables the DTM Tool.
        /// </summary>
        public static void OnReloadLatest(object sender, ExecutedEventArgs args)
        {
            // (Konrad) This will disable the DTM Tool when we are reloading latest.
            IsSynching = true;

            // (Konrad) Reloads Latest.
            EnqueueTask(app =>
            {
                try
                {
                    var reloadOptions = new ReloadLatestOptions();
                    var doc = app.ActiveUIDocument.Document;
                    doc.ReloadLatest(reloadOptions);
                    if (!doc.HasAllChangesFromCentral())
                    {
                        doc.ReloadLatest(reloadOptions);
                    }
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                }
            });

            // (Konrad) Turns the DTM Tool back on when reload is done.
            EnqueueTask(app =>
            {
                IsSynching = false;
            });
        }
        
        /// <summary>
        /// Removes ability to Unload a link for All Users.
        /// </summary>
        public static void OnUnloadForAllUsers(object sender, ExecutedEventArgs args)
        {
            var ssWindow = new LinkUnloadMonitorView();
            var o = ssWindow.ShowDialog();
            if(o != null && ssWindow.IsActive) ssWindow.Close();
        }

        /// <summary>
        /// Registers Communicator Dockable Panel.
        /// </summary>
        /// <param name="application">UIControlledApp</param>
        private void RegisterCommunicator(UIControlledApplication application)
        {
            var view = new CommunicatorView();
            CommunicatorWindow = view;

            var unused = new DockablePaneProviderData
            {
                FrameworkElement = CommunicatorWindow,
                InitialState = new DockablePaneState
                {
                    DockPosition = DockPosition.Tabbed,
                    TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
                }
            };

            var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
            application.RegisterDockablePane(dpid, "Mission Control", CommunicatorWindow);
        }

        /// <summary>
        /// Adds action to task list.
        /// </summary>
        /// <param name="task">Task to be executed.</param>
        public static void EnqueueTask(Action<UIApplication> task)
        {
            lock (Tasks)
            {
                Tasks.Enqueue(task);
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
                        DtmUpdaterInstance.CreateReloadLatestOverride();
                    }
                    else if (string.Equals(updater.updaterId.ToLower(),
                        LinkUnloadInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        LinkUnloadInstance.CreateLinkUnloadOverride();
                    }
                    else if (string.Equals(updater.updaterId.ToLower(),
                        SheetUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        SheetUpdaterInstance.Register(doc, updater);

                        new Thread(() => new SheetTracker().SynchSheets(doc)) { Priority = ThreadPriority.BelowNormal }.Start();
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
                    SheetUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                {
                    SheetUpdaterInstance.Unregister(doc);
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