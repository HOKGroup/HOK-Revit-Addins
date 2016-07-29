using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.MissionControl.Classes;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.RevisionTracker;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.MissionControl
{
    public class SocketHandler : IExternalEventHandler
    {
        private Configuration currentConfig = null;
        private DoorUpdater doorUpdater = null;
        private DTMUpdater dtmUpdater = null;
        private RevisionUpdater revisionUpdater = null;
        private string socketMessage = "";

        public Configuration CurrentConfig { get { return currentConfig; } set { currentConfig = value; } }
        public string SocketMessage { get { return socketMessage; } set { socketMessage = value; } }
        
        public SocketHandler(DoorUpdater doorU, DTMUpdater dtmU, RevisionUpdater revisionU)
        {
            doorUpdater = doorU;
            dtmUpdater = dtmU;
            revisionUpdater = revisionU;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                if (null != currentConfig)
                {
                    DocumentSet docSet = app.Application.Documents;
                    foreach (Document doc in docSet)
                    {
                        if (doc.IsWorkshared)
                        {
                            string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                            var fileFound = from file in currentConfig.files where file.centralPath.ToLower() == centralPath.ToLower() select file;
                            if (fileFound.Count() > 0)
                            {
                                //apply config
                                MessageBoxResult msgResult = MessageBox.Show("Configuration has been changed.\n" + centralPath, socketMessage, MessageBoxButton.OKCancel, MessageBoxImage.Question);
                                if (msgResult == MessageBoxResult.OK)
                                {
                                    ApplyConfiguration(doc, centralPath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void ApplyConfiguration(Document doc, string centralPath)
        {
            try
            {
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    AppCommand.Instance.ConfigDictionary.Remove(centralPath);
                }

                switch (socketMessage)
                {
                    case "add_configuration":
                        UpdateConfiguration(doc);
                        AppCommand.Instance.ConfigDictionary.Add(centralPath, currentConfig);
                        break;
                    case "update_configuration":
                        UpdateConfiguration(doc);
                        AppCommand.Instance.ConfigDictionary.Add(centralPath, currentConfig);
                        break;
                    case "delete_configuration":
                        DeleteConfiguration(doc);
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void UpdateConfiguration(Document doc)
        {
            try
            {
                foreach (ProjectUpdater updater in currentConfig.updaters)
                {
                    if (updater.updaterId.ToLower() == doorUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        //clean up first and register
                        doorUpdater.Unregister(doc);
                        if (updater.isUpdaterOn)
                        {
                            doorUpdater.Register(doc, updater);
                        }
                    }
                    else if (updater.updaterId.ToLower() == dtmUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        dtmUpdater.Unregister(doc);
                        if (updater.isUpdaterOn)
                        {
                            dtmUpdater.Register(doc, updater);
                        }
                    }
                    else if (updater.updaterId.ToLower() == revisionUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        revisionUpdater.Unregister(doc);
                        if (updater.isUpdaterOn)
                        {
                            revisionUpdater.Register(doc, updater);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void DeleteConfiguration(Document doc)
        {
            try
            {
                foreach (ProjectUpdater updater in currentConfig.updaters)
                {
                    if (updater.updaterId.ToLower() == doorUpdater.UpdaterGuid.ToString().ToLower())
                    {
                        //clean up first and register
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
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void SetCurrentConfig(Configuration config)
        {
            try
            {
                currentConfig = config;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void SetSocketMessage(string msg)
        {
            try
            {
                socketMessage = msg;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public string GetName()
        {
            return "Socket Handler";
        }
    }
}
