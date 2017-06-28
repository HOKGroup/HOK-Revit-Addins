using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.RevisionTracker;
using HOK.MissionControl.Tools.SingleSession;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.MissionControl
{
    public class AppCommand : IExternalApplication
    {
        private static AppCommand appCommand = null;
        private UIControlledApplication m_app;
        private Guid addInGuid = Guid.Empty;
        private string addInName = "";
        
        private Dictionary<string/*centralPath*/, Configuration> configDictionary = new Dictionary<string, Configuration>();
        private Dictionary<string/*centralPath*/, Project> projectDictionary = new Dictionary<string, Project>();
        //updaters
        private DoorUpdater doorUpdater = null;
        private DTMUpdater dtmUpdater = null;
        private RevisionUpdater revisionUpdater = null;

        public static AppCommand Instance { get { return appCommand; } }
        public Dictionary<string/*centralPath*/, Configuration> ConfigDictionary { get { return configDictionary; } set { configDictionary = value; } }
        public Dictionary<string/*centralPath*/, Project> ProjectDictionary { get { return projectDictionary; } set { projectDictionary = value; } }
        public DoorUpdater DoorUpdaterInstance { get { return doorUpdater; } set { doorUpdater = value; } }
        public DTMUpdater DTMUpdaterInstance { get { return dtmUpdater; } set { dtmUpdater = value; } }
        public RevisionUpdater RevisionUpdaterInstance { get { return revisionUpdater; } set { revisionUpdater = value; } }

        public Result OnShutdown(UIControlledApplication application)
        {
            LogUtil.WriteLog();

            application.ControlledApplication.DocumentOpening -= CollectConfigurationOnOpening;
            application.ControlledApplication.DocumentOpened -= RegisterUpdatersOnOpen;
            application.ControlledApplication.FailuresProcessing -= FailureProcessor.CheckFailure;
            application.ControlledApplication.DocumentClosing -= UnregisterUpdaterOnClosing;
            application.ControlledApplication.DocumentSynchronizingWithCentral -= DocumentSynchronizing;
            application.ControlledApplication.DocumentSynchronizedWithCentral -= DocumentSynchronized;

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                appCommand = this;
                m_app = application;
               
                AddInId appId = m_app.ActiveAddInId;
                addInGuid = appId.GetGUID();
                addInName = appId.GetAddInName();

                doorUpdater = new DoorUpdater(appId);
                dtmUpdater = new DTMUpdater(appId);
                revisionUpdater = new RevisionUpdater(appId);

                LogUtil.InitializeLog();
                LogUtil.AppendLog("Mission Control AddIn Started");

                application.ControlledApplication.DocumentOpening += CollectConfigurationOnOpening;
                application.ControlledApplication.DocumentOpened += RegisterUpdatersOnOpen;
                application.ControlledApplication.FailuresProcessing += FailureProcessor.CheckFailure;
                application.ControlledApplication.DocumentClosing += UnregisterUpdaterOnClosing;
                application.ControlledApplication.DocumentSynchronizingWithCentral += DocumentSynchronizing;
                application.ControlledApplication.DocumentSynchronizedWithCentral += DocumentSynchronized;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("OnStartup:" + ex.Message);
            }
            return Result.Succeeded;
        }

        private void CollectConfigurationOnOpening(object source, DocumentOpeningEventArgs args)
        {
            try
            {
                string pathName = args.PathName;
                if (!string.IsNullOrEmpty(pathName) && args.DocumentType == DocumentType.Project)
                {
                    BasicFileInfo fileInfo = BasicFileInfo.Extract(pathName);
                    if (fileInfo.IsWorkshared)
                    {
                        string centralPath = fileInfo.CentralPath;
                        if (!string.IsNullOrEmpty(centralPath))
                        {
                            //serch for config
                            LogUtil.AppendLog(centralPath + " Opening.");
                            Configuration configFound = ServerUtil.GetConfigurationByCentralPath(centralPath);
                            if (null != configFound)
                            {
                                //check if the single session should be activated
                                if (SingleSessionMonitor.CancelOpening(centralPath, configFound))
                                {
                                    if (args.Cancellable)
                                    {
                                        SingleSessionWindow ssWindow = new SingleSessionWindow(centralPath);
                                        if ((bool)ssWindow.ShowDialog())
                                        {
                                            args.Cancel();
                                            return;
                                        }
                                    }
                                }

                                if (configDictionary.ContainsKey(centralPath))
                                {
                                    configDictionary.Remove(centralPath);
                                }
                                configDictionary.Add(centralPath, configFound);

                                Project projectFound = ServerUtil.GetProjectByConfigurationId(configFound._id);
                                if (null != projectFound)
                                {
                                    if (projectDictionary.ContainsKey(centralPath))
                                    {
                                        projectDictionary.Remove(centralPath);
                                    }
                                    projectDictionary.Add(centralPath, projectFound);
                                }

                                LogUtil.AppendLog("Configuration Found: " + configFound._id);
                            }
                            else
                            {
                                //not a seed file, just check if the single session is activated
                                if (SingleSessionMonitor.SingleSessionActivated)
                                {
                                    SingleSessionWindow ssWindow = new SingleSessionWindow(centralPath);
                                    if ((bool)ssWindow.ShowDialog())
                                    {
                                        args.Cancel();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("CollectConfigurationOnOpening:" + ex.Message);
            }
        }

        private void RegisterUpdatersOnOpen(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                if (null != args.Document && !args.IsCancelled())
                {
                    Document doc = args.Document;
                    if (doc.IsWorkshared)
                    {
                        string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                        if (!string.IsNullOrEmpty(centralPath))
                        {
                            //serch for config
                            SingleSessionMonitor.OpenedDocuments.Add(centralPath);

                            LogUtil.AppendLog(centralPath + " Opned.");
                            if (configDictionary.ContainsKey(centralPath))
                            {
                                bool applied = ApplyConfiguration(doc, configDictionary[centralPath]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("RegisterUpdatersOnOpen:" + ex.Message);
            }
        }

        private bool ApplyConfiguration(Document doc, Configuration config)
        {
            bool applied = false;
            try
            {
                foreach (ProjectUpdater updater in config.updaters)
                {
                    if (!updater.isUpdaterOn) { continue; }
                    if (updater.updaterId.ToLower() == doorUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        doorUpdater.Register(doc, updater);
                    }
                    else if (updater.updaterId.ToLower() == dtmUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        dtmUpdater.Register(doc, updater);
                    }
                    else if (updater.updaterId.ToLower() == revisionUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        revisionUpdater.Register(doc, updater);
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("ApplyConfiguration:" + ex.Message);
            }
            return applied;
        }

        private void UnregisterUpdaterOnClosing(object source, DocumentClosingEventArgs args)
        {
            try
            {
                Document doc = args.Document;
                if (doc.IsWorkshared)
                {
                    string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        SingleSessionMonitor.CloseFile(centralPath);

                        if (configDictionary.ContainsKey(centralPath))
                        {
                            Configuration configFound = configDictionary[centralPath];
                            foreach (ProjectUpdater updater in configFound.updaters)
                            {
                                if (!updater.isUpdaterOn) { continue; }
                                if (updater.updaterId.ToLower() == doorUpdater.UpdaterGuid.ToString().ToLower())
                                {
                                    doorUpdater.Unregister(doc);
                                }
                                else if (updater.updaterId.ToLower() == dtmUpdater.UpdaterGuid.ToString().ToLower())
                                {
                                    dtmUpdater.Unregister(doc);
                                }
                                else if (updater.updaterId.ToLower() == revisionUpdater.UpdaterGuid.ToString().ToLower())
                                {
                                    revisionUpdater.Unregister(doc);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("UnregisterUpdaterOnClosing:" + ex.Message);
            }
        }

        private void DocumentSynchronizing(object source, DocumentSynchronizingWithCentralEventArgs args)
        {
            try
            {
                FailureProcessor.IsSynchronizing = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void DocumentSynchronized(object source, DocumentSynchronizedWithCentralEventArgs args)
        {
            try
            {
                FailureProcessor.IsSynchronizing = false;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
