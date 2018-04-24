#region References
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using GalaSoft.MvvmLight;
using HOK.Core.Utilities;
using HOK.MissionControl.Utils;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Tools.Communicator.Socket;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.HealthReport;
using HOK.MissionControl.Tools.LinkUnloadMonitor;
using HOK.MissionControl.Tools.SheetTracker;
#endregion

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
        public static Dictionary<string, DateTime> SynchTime { get; set; } = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> OpenTime { get; set; } = new Dictionary<string, DateTime>();
        //public static HealthReportData HrData { get; set; }
        public static SheetData SheetsData { get; set; }
        public static CommunicatorView CommunicatorWindow { get; set; }
        public static bool IsSynching { get; set; }
        public static bool IsSynchOverriden { get; set; }
        public static bool IsSynchNowOverriden { get; set; }
        public static CommunicatorRequestHandler CommunicatorHandler { get; set; }
        public static ExternalEvent CommunicatorEvent { get; set; }
        public static Dictionary<string, FamilyItem> FamiliesToWatch { get; set; } = new Dictionary<string, FamilyItem>();
        public PushButton CommunicatorButton { get; set; }
        public PushButton WebsiteButton { get; set; }
        public DoorUpdater DoorUpdaterInstance { get; set; }
        public DtmUpdater DtmUpdaterInstance { get; set; }
        public LinkUnloadMonitor LinkUnloadInstance { get; set; }

        private const string tabName = "  HOK - Beta";
        private static Queue<Action<UIApplication>> Tasks;

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

                // (Konrad) Create Communicator/WebsiteLink buttons and register dockable panel.
                RegisterCommunicator(application);

                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    Log.AppendLog(LogMessageType.ERROR, "Ribbon tab was not created: " + tabName);
                }

                var assembly = Assembly.GetAssembly(GetType());
                var panel = application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Mission Control")
                            ?? application.CreateRibbonPanel(tabName, "Mission Control");
                CommunicatorButton = (PushButton)panel.AddItem(new PushButtonData("Communicator_Command", "Show/Hide" + Environment.NewLine + "Communicator",
                    assembly.Location, "HOK.MissionControl.Tools.Communicator.CommunicatorCommand"));

                WebsiteButton = (PushButton)panel.AddItem(new PushButtonData("WebsiteLink_Command", "Launch" + Environment.NewLine + "MissionControl",
                    assembly.Location, "HOK.MissionControl.Tools.WebsiteLink.WebsiteLinkCommand"));
                WebsiteButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", "missionControl_32x32.png");
                WebsiteButton.ToolTip = "Launch Mission Control website.";

                // (Konrad) Since Communicator Task Assistant offers to open Families for editing,
                // it requires an External Event because new document cannot be opened from Idling Event
                CommunicatorHandler = new CommunicatorRequestHandler();
                CommunicatorEvent = ExternalEvent.Create(CommunicatorHandler);

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
            //try
            //{
            //    var pathName = args.PathName;
            //    string centralPath;
            //    if (string.IsNullOrEmpty(pathName) || args.DocumentType != DocumentType.Project) return;

            //    if (!pathName.StartsWith("BIM 360://"))
            //    {
            //        var fileInfo = BasicFileInfo.Extract(pathName);
            //        if (!fileInfo.IsWorkshared) return;

            //        centralPath = fileInfo.CentralPath;
            //        if (string.IsNullOrEmpty(centralPath)) return;
            //    }
            //    else
            //    {
            //        centralPath = pathName;
            //    }

            //    //search for config
            //    var configFound = ServerUtilities.GetByCentralPath<Configuration>(centralPath, "configurations/centralpath");
            //    if (null != configFound)
            //    {
            //        if (MissionControlSetup.Configurations.ContainsKey(centralPath))
            //        {
            //            MissionControlSetup.Configurations.Remove(centralPath);
            //        }
            //        MissionControlSetup.Configurations.Add(centralPath, configFound);

            //        var projectFound = ServerUtilities.Get<Project>("projects/configid/" + configFound.Id);
            //        if (null != projectFound)
            //        {
            //            if (MissionControlSetup.Projects.ContainsKey(centralPath))
            //            {
            //                MissionControlSetup.Projects.Remove(centralPath);
            //            }
            //            MissionControlSetup.Projects.Add(centralPath, projectFound);
            //        }

            //        OpenTime["from"] = DateTime.UtcNow;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            //}
        }

        /// <summary>
        /// Handles Communicator button image setting.
        /// </summary>
        private static void OnDocumentCreated(object sender, DocumentCreatedEventArgs e)
        {
            SetCommunicatorImage();
        }

        /// <summary>
        /// Registers IUpdaters
        /// </summary>
        private void OnDocumentOpened(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                // (Konrad) We need to set the Communicator Button image first, or it will be blank.
                SetCommunicatorImage();

                var doc = args.Document;
                if (doc == null || args.IsCancelled()) return;

                new Thread(() => new Tools.MissionControl.MissionControl().CheckIn(doc))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();

                //var doc = args.Document;
                //if (doc == null || args.IsCancelled()) return;
                //if (!doc.IsWorkshared) return;

                //var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                //if (!MissionControlSetup.Projects.ContainsKey(centralPath)) return;
                //if (!MissionControlSetup.Configurations.ContainsKey(centralPath)) return;

                //var currentConfig = MissionControlSetup.Configurations[centralPath];
                //var currentProject = MissionControlSetup.Projects[centralPath];

                //// (Konrad) Register Updaters that are in the config file.
                //ApplyConfiguration(doc, currentConfig);

                //// (Konrad) in order not to become out of synch with the database we need a way
                //// to communicate live updates from the database to task assistant/communicator
                //new Thread(() => new MissionControlSocket().Main(doc)) { Priority = ThreadPriority.BelowNormal }.Start();

                //// (Konrad) It's possible that Health Report Document doesn't exist in database yet.
                //// Create it and set the reference to it in Project if that's the case.
                //if (MonitorUtilities.IsUpdaterOn(currentProject, currentConfig,
                //    new Guid(Properties.Resources.HealthReportTrackerGuid)))
                //{
                //    if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath))
                //    {
                //        HrData = ServerUtilities.GetByCentralPath<HealthReportData>(centralPath, "healthrecords/centralpath");
                //        if (HrData == null)
                //        {
                //            HrData = ServerUtilities.Post<HealthReportData>(new HealthReportData { centralPath = centralPath.ToLower() }, "healthrecords");
                //            ServerUtilities.AddHealthRecordToProject(currentProject, HrData.Id);
                            

                //            var projectFound = ServerUtilities.Get<Project>("projects/configid/" + currentConfig.Id);
                //            if (null == projectFound) return;
                //            MissionControlSetup.Projects[centralPath] = projectFound; // this won't be null since we checked before.

                //            MissionControlSetup.HealthRecordIds.Add(centralPath, HrData.Id); // store health record
                //            MissionControlSetup.FamiliesIds.Add(centralPath, HrData.familyStats); // store families record
                //        }
                //        else
                //        {
                //            MissionControlSetup.HealthRecordIds.Add(centralPath, HrData.Id); // store health record
                //            MissionControlSetup.FamiliesIds.Add(centralPath, HrData.familyStats); // store families record
                //        }
                //    }

                //    // (Konrad) Publish info about model to Health Report. If we toss them onto a new Thread
                //    // we won't be slowing the open time and since we are not returning any of this into to the user
                //    // there is no reason to run this synchronously.
                //    var recordId = MissionControlSetup.HealthRecordIds[centralPath];
                //    new Thread(() => new StylesMonitor().PublishStylesInfo(doc, recordId)) { Priority = ThreadPriority.BelowNormal }.Start();
                //    new Thread(() => new LinkMonitor().PublishData(doc, recordId)) { Priority = ThreadPriority.BelowNormal }.Start();
                //    new Thread(() => new WorksetItemCount().PublishData(doc, recordId)) { Priority = ThreadPriority.BelowNormal }.Start();
                //    new Thread(() => new ViewMonitor().PublishData(doc, recordId)) { Priority = ThreadPriority.BelowNormal }.Start();
                //    new Thread(() => new WorksetOpenSynch().PublishData(doc, recordId, WorksetMonitorState.onopened)) { Priority = ThreadPriority.BelowNormal }.Start();
                //    new Thread(() => new ModelMonitor().PublishModelSize(doc, centralPath, recordId, doc.Application.VersionNumber)) { Priority = ThreadPriority.BelowNormal }.Start();

                //    if (OpenTime.ContainsKey("from"))
                //    {
                //        new Thread(() => new ModelMonitor().PublishOpenTime(recordId)) { Priority = ThreadPriority.BelowNormal }.Start();
                //    }
                //}

                //// (Konrad) This tool will reset Shared Parameters Location to one specified in Mission Control
                //if (currentConfig.GetType().GetProperty("sharedParamMonitor") != null && currentConfig.sharedParamMonitor.isMonitorOn)
                //{
                //    if (File.Exists(currentConfig.sharedParamMonitor.filePath))
                //    {
                //        doc.Application.SharedParametersFilename = currentConfig.sharedParamMonitor.filePath;
                //    }
                //}
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
            //try
            //{
            //    var doc = args.Document;
            //    if (!doc.IsWorkshared) return;

            //    UnregisterUpdaters(doc);
            //}
            //catch (Exception ex)
            //{
            //    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            //}
        }

        /// <summary>
        /// Handler for Document Synchronizing event.
        /// </summary>
        private static void OnDocumentSynchronizing(object source, DocumentSynchronizingWithCentralEventArgs args)
        {
            //try
            //{
            //    IsSynching = true; // disables DTM Tool
            //    FailureProcessor.IsSynchronizing = true;
            //    if (args.Document == null) return;

            //    var doc = args.Document;
            //    var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            //    if (string.IsNullOrEmpty(centralPath)) return;

            //    // (Konrad) Setup Health Report Monitor
            //    if (!MissionControlSetup.Projects.ContainsKey(centralPath) 
            //        || !MissionControlSetup.Configurations.ContainsKey(centralPath) 
            //        || !MissionControlSetup.HealthRecordIds.ContainsKey(centralPath)) return;
            //    var recordId = MissionControlSetup.HealthRecordIds[centralPath];

            //    new Thread(() => new WorksetOpenSynch().PublishData(doc, recordId, WorksetMonitorState.onsynched)) { Priority = ThreadPriority.BelowNormal }.Start();
            //    SynchTime["from"] = DateTime.UtcNow;
            //}
            //catch (Exception ex)
            //{
            //    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            //}
        }

        /// <summary>
        /// Document Synchronized handler.
        /// </summary>
        private static void OnDocumentSynchronized(object source, DocumentSynchronizedWithCentralEventArgs args)
        {
            //try
            //{
            //    IsSynching = false; // enables DTM Tool again
            //    var doc = args.Document;
            //    if (doc == null) return;

            //    var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            //    if (string.IsNullOrEmpty(centralPath)) return;

            //    FailureProcessor.IsSynchronizing = false;

            //    // (Konrad) If project is not registered with MongoDB let's skip this
            //    if (!MissionControlSetup.Projects.ContainsKey(centralPath) || !MissionControlSetup.Configurations.ContainsKey(centralPath)) return;
            //    if (MissionControlSetup.HealthRecordIds.ContainsKey(centralPath))
            //    {
            //        var recordId = MissionControlSetup.HealthRecordIds[centralPath];
            //        if (SynchTime.ContainsKey("from"))
            //        {
            //            Log.AppendLog(LogMessageType.INFO, "Finished Synching. Publishing Synch Time data.");
            //            new Thread(() => new ModelMonitor().PublishSynchTime(recordId)) { Priority = ThreadPriority.BelowNormal }.Start();
            //        }
            //    }

            //    // (Konrad) Publish Sheet data to sheet tracker
            //    if (MissionControlSetup.SheetsIds.ContainsKey(centralPath))
            //    {
            //        try
            //        {
            //            new Thread(() => new SheetTracker().SynchSheets(doc)) { Priority = ThreadPriority.BelowNormal }.Start();
            //        }
            //        catch (Exception e)
            //        {
            //            Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            //}
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
            //// (Konrad) This will disable the DTM Tool when we are reloading latest.
            //IsSynching = true;

            //// (Konrad) Reloads Latest.
            //EnqueueTask(app =>
            //{
            //    try
            //    {
            //        Log.AppendLog(LogMessageType.INFO, "Reloading Latest...");

            //        var reloadOptions = new ReloadLatestOptions();
            //        var doc = app.ActiveUIDocument.Document;
            //        doc.ReloadLatest(reloadOptions);
            //        if (!doc.HasAllChangesFromCentral())
            //        {
            //            doc.ReloadLatest(reloadOptions);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            //    }
            //});

            //// (Konrad) Turns the DTM Tool back on when reload is done.
            //EnqueueTask(app =>
            //{
            //    IsSynching = false;
            //});
        }

        /// <summary>
        /// Ovveride method for when user synchs to central.
        /// The goal here is to disable the DTM Tool and prevent pop-ups while synching.
        /// </summary>
        public static void OnSynchToCentral(object sender, ExecutedEventArgs args, SynchType synchType)
        {
            //// (Konrad) This will disable the DTM Tool when we are synching to central.
            //IsSynching = true;

            //RevitCommandId commandId;
            //switch (synchType)
            //{
            //    case SynchType.Synch:
            //        commandId = RevitCommandId.LookupCommandId("ID_FILE_SAVE_TO_MASTER");
            //        break;
            //    case SynchType.SynchNow:
            //        commandId = RevitCommandId.LookupCommandId("ID_FILE_SAVE_TO_MASTER_SHORTCUT");
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(synchType), synchType, null);
            //}
            //if (commandId == null || !commandId.CanHaveBinding) return;

            //EnqueueTask(app =>
            //{
            //    try
            //    {
            //        app.RemoveAddInCommandBinding(commandId);

            //        switch (synchType)
            //        {
            //            case SynchType.Synch:
            //                IsSynchOverriden = false;
            //                break;
            //            case SynchType.SynchNow:
            //                IsSynchNowOverriden = false;
            //                break;
            //            default:
            //                throw new ArgumentOutOfRangeException(nameof(synchType), synchType, null);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            //    }
            //});

            //EnqueueTask(app =>
            //{
            //    // (Konrad) We can now post the same Command we were overriding since the override is OFF.
            //    app.PostCommand(commandId);

            //    // (Konrad) Once the command executes this will turn the override back ON.
            //    IsSynching = false;

            //    var doc = app.ActiveUIDocument.Document;
            //    var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            //    if (string.IsNullOrEmpty(centralPath)) return;

            //    // (Konrad) Let's turn the synch command override back on.
            //    var config = MissionControlSetup.Configurations[centralPath];
            //    foreach (var updater in config.updaters)
            //    {
            //        if (!updater.isUpdaterOn) continue;

            //        if (string.Equals(updater.updaterId.ToLower(),
            //            Instance.DtmUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
            //        {
            //            Instance.DtmUpdaterInstance.CreateSynchToCentralOverride();
            //        }
            //    }
            //});
        }

        /// <summary>
        /// Removes ability to Unload a link for All Users.
        /// </summary>
        public static void OnUnloadForAllUsers(object sender, ExecutedEventArgs args)
        {
            //var ssWindow = new LinkUnloadMonitorView();
            //var o = ssWindow.ShowDialog();
            //if(o != null && ssWindow.IsActive) ssWindow.Close();
        }

        #region Utilities

        /// <summary>
        /// Unregisters all updaters as well as posts data.
        /// </summary>
        /// <param name="doc">Revit Document</param>
        private void UnregisterUpdaters(Document doc)
        {
            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            if (string.IsNullOrEmpty(centralPath)) return;

            if (MissionControlSetup.Configurations.ContainsKey(centralPath))
            {
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
                }
            }

            // (Konrad) Clean up all static classes that would be holding any relevant information. 
            // This would cause issues in case that user closes a MissionControl registered project without
            // closing Revit app. These static classes retain their values, and then would trick rest of app
            // to think that it is registered in Mission Control.
            MissionControlSetup.ClearAll();
            ClearAll();
        }

        /// <summary>
        /// Registers Communicator Dockable Panel.
        /// </summary>
        /// <param name="application">UIControlledApp</param>
        private static void RegisterCommunicator(UIControlledApplication application)
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
            try
            {
                // (Konrad) It's possible that a dockable panel with the same id already exists
                // This ensures that we don't get an exception here. 
                application.RegisterDockablePane(dpid, "Mission Control", CommunicatorWindow);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Communicator Image can only be set when we are done loading the app.
        /// </summary>
        public static void SetCommunicatorImage()
        {
            // (Konrad) This needs to run after the doc is opened, because UI elements don't get created until then.
            EnqueueTask(app =>
            {
                var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
                var dp = app.GetDockablePane(dpid);
                var assembly = Assembly.GetExecutingAssembly();
                if (dp != null)
                {
                    Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", dp.IsShown()
                        ? "communicatorOn_32x32.png"
                        : "communicatorOff_32x32.png");
                    Instance.CommunicatorButton.ItemText = dp.IsShown()
                        ? "Hide" + Environment.NewLine + "Communicator"
                        : "Show" + Environment.NewLine + "Communicator";
                }
            });
        }

        /// <summary>
        /// Due to all asynch stuff some data might not be available right away so we use this callback to instantiate the Communicator.
        /// It also get's called after synch to central is done to refresh the UI.
        /// </summary>
        public static void LunchCommunicator()
        {
            CommunicatorWindow.MainControl.Dispatcher.Invoke(() =>
            {
                // (Konrad) We have to make sure that we unregister from all Messaging before reloading UI.
                if (CommunicatorWindow.DataContext != null)
                {
                    var tabItems = CommunicatorWindow.MainControl.Items.SourceCollection;
                    foreach (var ti in tabItems)
                    {
                        var content = ((UserControl)((TabItem)ti).Content).DataContext as ViewModelBase;
                        content?.Cleanup();
                    }
                }

                // (Konrad) Now we can reset the ViewModel
                CommunicatorWindow.DataContext = new CommunicatorViewModel();
                if (CommunicatorWindow.MainControl.Items.Count > 0)
                {
                    CommunicatorWindow.MainControl.SelectedIndex = 0;
                }
            }, DispatcherPriority.Normal);
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
        /// Removes all references to old data that might linger after document is closed.
        /// </summary>
        private static void ClearAll()
        {
            //HrData = null;
            SheetsData = null;
            SynchTime = new Dictionary<string, DateTime>();
            OpenTime = new Dictionary<string, DateTime>();
            FamiliesToWatch = new Dictionary<string, FamilyItem>();
            if (CommunicatorWindow != null) CommunicatorWindow.DataContext = null;
        }

        #endregion
    }
}