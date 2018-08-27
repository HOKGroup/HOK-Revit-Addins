#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Utils;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Warnings;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Tools.Communicator.Socket;
using HOK.MissionControl.Tools.MissionControl;

#endregion

namespace HOK.MissionControl
{
    public class AppCommand : IExternalApplication
    {
        #region Properties

        public static AppCommand Instance { get; private set; }
        public static Dictionary<string, DateTime> SynchTime { get; set; } = new Dictionary<string, DateTime>();
        public static Dictionary<string, DateTime> OpenTime { get; set; } = new Dictionary<string, DateTime>();
        public static Dictionary<string, WarningItem> Warnings { get; set; } = new Dictionary<string, WarningItem>();
        public static CommunicatorView CommunicatorWindow { get; set; }
        public static MissionControlSocket Socket { get; set; }
        public static bool IsSynching { get; set; }
        public static bool IsSynchOverriden { get; set; }
        public static bool IsSynchNowOverriden { get; set; }
        public static CommunicatorRequestHandler CommunicatorHandler { get; set; }
        public static ExternalEvent CommunicatorEvent { get; set; }
        public static Dictionary<string, FamilyItem> FamiliesToWatch { get; set; } = new Dictionary<string, FamilyItem>();
        public PushButton CommunicatorButton { get; set; }
        public PushButton WebsiteButton { get; set; }
        public PushButton FamilyPublishButton { get; set; }
        public DoorUpdater DoorUpdaterInstance { get; set; }
        public DtmUpdater DtmUpdaterInstance { get; set; }
        private const string tabName = "   HOK   ";
        private static Queue<Action<UIApplication>> Tasks;

        #endregion

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Instance = this;
                var appId = application.ActiveAddInId;
                DoorUpdaterInstance = new DoorUpdater(appId);
                DtmUpdaterInstance = new DtmUpdater(appId);
                Tasks = new Queue<Action<UIApplication>>();

                application.Idling += OnIdling;
                application.ControlledApplication.DocumentOpening += OnDocumentOpening;
                application.ControlledApplication.DocumentOpened += OnDocumentOpened;
                application.ControlledApplication.FailuresProcessing += FailureProcessor.CheckFailure;
                application.ControlledApplication.DocumentClosing += OnDocumentClosing;
                application.ControlledApplication.DocumentSynchronizingWithCentral += OnDocumentSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral += OnDocumentSynchronized;
                application.ControlledApplication.DocumentCreated += OnDocumentCreated;
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                // (Konrad) We are not going to process warnings here.
#else
                application.ControlledApplication.FailuresProcessing += OnFailureProcessing;
#endif
                // (Konrad) Create buttons and register dockable panel.
                CommunicatorUtilities.RegisterCommunicator(application);
                CreateButtons(application);

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
#if RELEASE2015 || RELEASE2016 || RELEASE2017
        // (Konrad) We are not going to process warnings here.
#else
        private static void OnFailureProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var fa = e.GetFailuresAccessor();
            var doc = fa.GetDocument();

            foreach (var fma in fa.GetFailureMessages(FailureSeverity.Warning))
            {
                var w = new WarningItem(fma, doc);
                if(Warnings.ContainsKey(w.UniqueId)) continue;

                Warnings.Add(w.UniqueId, w);
            }
        }
#endif

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
#if RELEASE2015 || RELEASE2016 || RELEASE2017
            // (Konrad) We are not going to process warnings here.
#else
            application.ControlledApplication.FailuresProcessing -= OnFailureProcessing;
#endif
            return Result.Succeeded;
        }

        private static void OnDocumentOpening(object source, DocumentOpeningEventArgs args)
        {
            if (args.DocumentType != DocumentType.Project) return;

            OpenTime["from"] = DateTime.UtcNow;
        }

        private static void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
        {
            try
            {
                var doc = args.Document;
                if (doc == null || args.IsCancelled() || doc.IsFamilyDocument) return;

                CheckIn(doc);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static void OnDocumentOpened(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                var doc = args.Document;
                if (doc == null || args.IsCancelled() || doc.IsFamilyDocument) return;

                CheckIn(doc);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static void OnDocumentClosing(object source, DocumentClosingEventArgs args)
        {
            try
            {
                var doc = args.Document;
                if (doc == null || args.IsCancelled() || doc.IsFamilyDocument) return;

                // (Konrad) Cleanup updaters.
                Tools.MissionControl.MissionControl.UnregisterUpdaters(doc);

                // (Konrad) Disconnect from Sockets.
                Socket?.Kill();

                // (Konrad) If any task windows are still open, let's shut them down.
                Messenger.Default.Send(new DocumentClosed { CloseWindow = true });
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

                SynchTime["from"] = DateTime.UtcNow;

                // (Konrad) We need to make sure that we were able to check into Mission Control 
                // If all of these values are present that means we can publish data.
                if (MissionControlSetup.Projects.ContainsKey(centralPath) &&
                    MissionControlSetup.Configurations.ContainsKey(centralPath))
                {
                    Tools.MissionControl.MissionControl.ProcessWorksets(ActionType.Synch, doc, centralPath);
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
        private static void OnDocumentSynchronized(object source, DocumentSynchronizedWithCentralEventArgs args)
        {
            try
            {
                IsSynching = false; // enables DTM Tool again
                var doc = args.Document;
                if (doc == null) return;

                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrEmpty(centralPath)) return;

                FailureProcessor.IsSynchronizing = false;

                // (Konrad) If project is not registered with MongoDB let's skip this
                if (MissionControlSetup.Projects.ContainsKey(centralPath) &&
                    MissionControlSetup.Configurations.ContainsKey(centralPath))
                {
                    Tools.MissionControl.MissionControl.ProcessModels(ActionType.Synch, doc, centralPath);
                    Tools.MissionControl.MissionControl.ProcessSheets(ActionType.Synch, doc, centralPath);
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                    // (Konrad) We are not going to process warnings here.
#else
                    Tools.MissionControl.MissionControl.ProcessWarnings(ActionType.Synch, doc, centralPath);
#endif
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

#region Utilities

        /// <summary>
        /// Utility method for creating all MissionControl buttons that don't have their own AppCommand.
        /// </summary>
        /// <param name="application">UI Controlled Application.</param>
        private void CreateButtons(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch
            {
                Log.AppendLog(LogMessageType.INFO, "Ribbon tab was not created because it already exists: " + tabName);
            }

            var assembly = Assembly.GetAssembly(GetType());
            var panel = application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Mission Control")
                        ?? application.CreateRibbonPanel(tabName, "Mission Control");
            CommunicatorButton = (PushButton)panel.AddItem(new PushButtonData("Communicator_Command",
                "Show/Hide" + Environment.NewLine + "Communicator",
                assembly.Location, "HOK.MissionControl.Tools.Communicator.CommunicatorCommand"));

            WebsiteButton = (PushButton)panel.AddItem(new PushButtonData("WebsiteLink_Command",
                "Launch" + Environment.NewLine + "MissionControl",
                assembly.Location, "HOK.MissionControl.Tools.WebsiteLink.WebsiteLinkCommand"));
            WebsiteButton.LargeImage =
                ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", "missionControl_32x32.png");
            WebsiteButton.ToolTip = "Launch Mission Control website.";

            var currentAssembly = Assembly.GetAssembly(GetType()).Location;
            var currentDirectory = Path.GetDirectoryName(currentAssembly);

            // (Konrad) Publish Family Data
            var fpAssembly = Assembly.LoadFrom(currentDirectory + "/HOK.MissionControl.FamilyPublish.dll");
            FamilyPublishButton = (PushButton)panel.AddItem(new PushButtonData("FamilyPublish_Command",
                "Publish Family" + Environment.NewLine + " Data",
                currentDirectory + "/HOK.MissionControl.FamilyPublish.dll",
                "HOK.MissionControl.FamilyPublish.FamilyPublishCommand"));
            FamilyPublishButton.LargeImage = ButtonUtil.LoadBitmapImage(fpAssembly, "HOK.MissionControl.FamilyPublish",
                "publishFamily_32x32.png");
            FamilyPublishButton.ToolTip =
                "This plug-in publishes information about Families currently loaded in the project to Mission Control.";

            // (Konrad) Links Manager
            var lmAssembly = Assembly.LoadFrom(currentDirectory + "/HOK.MissionControl.LinksManager.dll");
            var linksButton = (PushButton)panel.AddItem(new PushButtonData("LinksManager_Command",
                "Links" + Environment.NewLine + " Manager ",
                currentDirectory + "/HOK.MissionControl.LinksManager.dll",
                "HOK.MissionControl.LinksManager.LinksManagerCommand"));
            linksButton.LargeImage = ButtonUtil.LoadBitmapImage(lmAssembly, "HOK.MissionControl.LinksManager",
                "linksManager_32x32.png");
            linksButton.ToolTip =
                "Utility tool that allows users to quickly identify Imported Images, DWGs, and Styles.";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        private static void CheckIn(Document doc)
        {
            CommunicatorUtilities.SetCommunicatorImage();

            new Thread(() => new Tools.MissionControl.MissionControl().CheckIn(doc))
            {
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            }.Start();
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
        public static void ClearAll()
        {
            SynchTime = new Dictionary<string, DateTime>();
            OpenTime = new Dictionary<string, DateTime>();
            FamiliesToWatch = new Dictionary<string, FamilyItem>();
            if (CommunicatorWindow != null) CommunicatorWindow.DataContext = null;
        }

#endregion
    }
}