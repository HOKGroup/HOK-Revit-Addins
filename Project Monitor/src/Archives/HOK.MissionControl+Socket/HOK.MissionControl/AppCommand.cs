using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.MissionControl.Classes;
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
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json.Linq;
using System.Threading;

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
        //socket for a bi-directional channel
        private Socket socket = null;
        private bool socketOn = false;

        private SocketHandler socketHandler = null;
        private ExternalEvent extEvent = null;

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

                socketHandler = new SocketHandler(doorUpdater, dtmUpdater, revisionUpdater);
                extEvent = ExternalEvent.Create(socketHandler);

                LogUtil.InitializeLog();
                LogUtil.AppendLog("Mission Control AddIn Started");

                m_app.ControlledApplication.DocumentOpening += CollectConfigurationOnOpening;
                m_app.ControlledApplication.DocumentOpened += RegisterUpdatersOnOpen;
                m_app.ControlledApplication.FailuresProcessing += FailureProcessor.CheckFailure;
                m_app.ControlledApplication.DocumentClosing += UnregisterUpdaterOnClosing;
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
                        if (!socketOn) { ConnectSocket(); }

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

        private void ConnectSocket()
        {
            try
            {
                var options = new IO.Options()
                {
                    IgnoreServerCertificateValidation = true,
                    AutoConnect = true,
                    ForceNew = true
                };

                string url = ServerUtil.RestApiBaseUrl;
                socket = IO.Socket(url, options);

                socket.On(Socket.EVENT_CONNECT, () =>
                {
                    socketOn = true;
                });

                socket.On("add_configuration", (data) =>
                {
                    try
                    {
                        JObject addedConfig = JObject.FromObject(data);
                        Configuration config = (Configuration)addedConfig.ToObject(typeof(Configuration));
                        socketHandler.SetCurrentConfig(config);
                        socketHandler.SetSocketMessage("add_configuration");
                        extEvent.Raise();
                        
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                });

                socket.On("update_configuration", (data) =>
                {
                    try
                    {
                        //update configuration of opened documents
                        JObject updatedConfig = JObject.FromObject(data);
                        Configuration config = (Configuration)updatedConfig.ToObject(typeof(Configuration));
                        socketHandler.SetCurrentConfig(config);
                        socketHandler.SetSocketMessage("update_configuration");
                        extEvent.Raise();
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                });

                socket.On("delete_configuration", (data) =>
                {
                    try
                    {
                        string deletedId = data.ToString();
                        var configFound = from config in configDictionary.Values where config._id == deletedId select config;
                        if (configFound.Count() > 0)
                        {
                            Configuration config = configFound.First();
                            socketHandler.SetCurrentConfig(config);
                            socketHandler.SetSocketMessage("delete_configuration");
                            extEvent.Raise();
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                });

                while (socketOn) { Thread.Sleep(100); }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("SocketManager-ConnectSocket:" + ex.Message);
            }
        }

        private bool DisconnectSocket()
        {
            bool disconnected = false;
            try
            {
                socketOn = false;
                socket.Off("update_configuration");
                socket.Off("add_configuration");
                socket.Off("delete_configuration");
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("SocketManager-ConnectSocket:" + ex.Message);
            }
            return disconnected;
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
                            SingleSessionMonitor.OpenedDocuments.Add(centralPath, doc);

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

        public bool ApplyConfiguration(Document doc, Configuration config)
        {
            bool applied = false;
            try
            {
                foreach (ProjectUpdater updater in config.updaters)
                {
                    if (updater.updaterId.ToLower() == doorUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        if (updater.isUpdaterOn)
                        {
                            doorUpdater.Register(doc, updater);
                        }
                        else
                        {
                            doorUpdater.Unregister(doc);
                        }
                    }
                    else if (updater.updaterId.ToLower() == dtmUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        if (updater.isUpdaterOn)
                        {
                            dtmUpdater.Register(doc, updater);
                        }
                        else
                        {
                            dtmUpdater.Unregister(doc);
                        }
                    }
                    else if (updater.updaterId.ToLower() == revisionUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        if (updater.isUpdaterOn)
                        {
                            revisionUpdater.Register(doc, updater);
                        }
                        else
                        {
                            revisionUpdater.Unregister(doc);
                        }
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
                            configDictionary.Remove(centralPath);
                        }

                        if (SingleSessionMonitor.OpenedDocuments.Count == 0)
                        {
                            DisconnectSocket();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("UnregisterUpdaterOnClosing:" + ex.Message);
            }
        }
    }
}
