using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Windows.Threading;
using System.Windows;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.Core.Utilities;

namespace HOK.FileOnpeningMonitor
{
    public class AppCommand : IExternalApplication
    {
        private readonly Dictionary<string/*filePath*/, CentralFileInfo> openedCentralFiles = new Dictionary<string, CentralFileInfo>();
        private Document openedDocument;
        private UIControlledApplication m_app;
        private bool isCentral;
        private DispatcherTimer dispatcherTimer;
        private int timerCount = 1;
        private bool timerOn;

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened -= Application_DocumentOpened;
            
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                m_app = application;
                dispatcherTimer = new DispatcherTimer();
                application.ControlledApplication.DocumentOpened += Application_DocumentOpened;
                application.ControlledApplication.DocumentClosing += Application_DocumentClosing;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        private void Application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            try
            {
                openedDocument = args.Document;
                if (null == openedDocument.ActiveView) return; // to distinguish linked model 
                if (!openedDocument.IsWorkshared) return;
                if (string.IsNullOrEmpty(openedDocument.PathName)) return;
                if (openedDocument.IsDetached) return;
                var isOnNetwork = IsNetworkDrive(openedDocument.PathName);
                if (!isOnNetwork) return;
                isCentral = IsCentralFile(openedDocument);
                if (!isCentral) return;

                var fileInfo = new CentralFileInfo(openedDocument);
                var unused = new Dictionary<string, string>();

                try
                {
                    FMEServerUtil.RunFMEWorkspaceHTTP(fileInfo, "buildingSMART Notifications", "OpenCentralFileNotification.fmw", out unused);
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                if (!openedCentralFiles.ContainsKey(fileInfo.DocCentralPath))
                {
                    openedCentralFiles.Add(openedDocument.PathName, fileInfo);
                }

                var firstWindow = new CentralFileWarningWindow(fileInfo);
                if (firstWindow.ShowDialog() != true) return;

                timerCount = 1;
                var timedWindow = new TimedWarningWindow(timerCount, openedCentralFiles);
                if (timedWindow.ShowDialog() == true)
                {
                    timerOn = false;
                }
                else
                {
                    timerOn = true;
                    if (null != dispatcherTimer)
                    {
                        dispatcherTimer.IsEnabled = false;
                        dispatcherTimer.Tick -= DispatcherTimer_Tick;
                        dispatcherTimer = null;
                    }

                    dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick += DispatcherTimer_Tick;
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
                    dispatcherTimer.IsEnabled = true;
                    dispatcherTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public bool IsNetworkDrive(string pathName)
        {
            const bool onNetwork = false;
            try
            {
                if (pathName.StartsWith(@"\\")) { return true; }
                if (pathName.StartsWith("RSN:")) { return true; }

                var infoArray = DriveInfo.GetDrives();
                foreach (var info in infoArray)
                {
                    if (info.DriveType != DriveType.Network) continue;
                    if (pathName.StartsWith(info.Name)) return true;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to get information of the file location.\n" + ex.Message, "Find File Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return onNetwork;
        }

        public bool IsCentralFile(Document document)
        {
            var isCentralFile = false;
            try
            {
                var centralModelPath = document.GetWorksharingCentralModelPath();
                if (null!=centralModelPath)
                {
                    var userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                    if (centralModelPath.ServerPath)
                    {
                        const string revitServerPrefix = "RSN://";
                        const string a360Prefix = "A360://";
                        var relativePath = "";

                        if (userVisiblePath.Contains(revitServerPrefix))
                        {
                            var serverLocation = revitServerPrefix + centralModelPath.CentralServerPath;
                            relativePath = userVisiblePath.Substring(serverLocation.Length);
                        }
                        else if (userVisiblePath.Contains(a360Prefix))
                        {
                            var serverLocation = a360Prefix + centralModelPath.CentralServerPath;
                            relativePath = userVisiblePath.Substring(serverLocation.Length);
                        }

                        var serverPath = new ServerPath(centralModelPath.CentralServerPath, relativePath);
                        var centralGUID = document.Application.GetWorksharingCentralGUID(serverPath);
                        if (centralGUID != Guid.Empty)
                        {
                            var currentModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(document.PathName);
                            if (null!=currentModelPath)
                            {
                                var currentGuid = currentModelPath.GetModelGUID();
                                if (currentGuid != Guid.Empty)
                                {
                                    if (centralGUID.Equals(currentGuid))
                                    {
                                        isCentralFile = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (document.PathName.ToUpper() == userVisiblePath.ToUpper())
                        {
                            isCentralFile = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                MessageBox.Show("Failed to determine whether the file is central or local.\n"+ex.Message, "IsCentralFile", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return isCentralFile;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!timerOn) return;

                dispatcherTimer.IsEnabled = false;
                var timedWindow = new TimedWarningWindow(timerCount, openedCentralFiles);
                if (timedWindow.ShowDialog() == true)
                {
                    dispatcherTimer.Stop();
                    dispatcherTimer.IsEnabled = false;
                    dispatcherTimer.Tick -= DispatcherTimer_Tick;
                    timerOn = false;
                }
                else
                {
                    dispatcherTimer.IsEnabled = true;
                    timerCount++;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void Application_DocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            try
            {
                var closingDocument = args.Document;
                if (string.IsNullOrEmpty(closingDocument.PathName)) return;

                if (openedCentralFiles.ContainsKey(closingDocument.PathName))
                {
                    openedCentralFiles.Remove(closingDocument.PathName);
                }
                if (openedCentralFiles.Count == 0)
                {
                    timerOn = false;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
