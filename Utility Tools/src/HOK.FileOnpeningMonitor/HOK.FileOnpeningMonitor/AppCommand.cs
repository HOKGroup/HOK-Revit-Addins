using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Windows.Threading;
using System.Windows;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;



namespace HOK.FileOnpeningMonitor
{
    public class AppCommand:IExternalApplication
    {
        private Dictionary<string/*filePath*/, CentralFileInfo> openedCentralFiles = new Dictionary<string, CentralFileInfo>();
        private Document openedDocument = null;
        private UIControlledApplication m_app = null;
        private bool isCentral = false;
        private DispatcherTimer dispatcherTimer = null;
        private int timerCount = 1;
        private bool timerOn = false;

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
                application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(Application_DocumentOpened);
                application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(Application_DocumentClosing);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        private void Application_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            try
            {
                openedDocument = args.Document;
                if (null == openedDocument.ActiveView) { return; } // to distinguish linked model 
                if (openedDocument.IsWorkshared)
                {
                    if (string.IsNullOrEmpty(openedDocument.PathName)) { return; }
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                    if (openedDocument.IsDetached) { return; }
#endif
                    bool isOnNetwork = IsNetworkDrive(openedDocument.PathName);
                    if (!isOnNetwork) { return; }

                    isCentral = IsCentralFile(openedDocument);
                    if (isCentral)
                    {
                        CentralFileInfo fileInfo = new CentralFileInfo(openedDocument);
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        bool submittedJob = FMEServerUtil.RunFMEWorkspace(fileInfo, "buildingSMART Notifications", "OpenCentralFileNotification.fmw", out properties);

                        if (!openedCentralFiles.ContainsKey(fileInfo.DocCentralPath))
                        {
                            openedCentralFiles.Add(openedDocument.PathName, fileInfo);
                        }

                        CentralFileWarningWindow firstWindow = new CentralFileWarningWindow(fileInfo);
                        if (firstWindow.ShowDialog() == true)
                        {
                            timerCount = 1;
                            TimedWarningWindow timedWindow = new TimedWarningWindow(timerCount, openedCentralFiles);
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
                                dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                                dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
                                dispatcherTimer.IsEnabled = true;
                                dispatcherTimer.Start();
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

        public bool IsNetworkDrive(string pathName)
        {
            bool onNetwork = false;
            try
            {
                if (pathName.StartsWith(@"\\")) { return true; }
                if (pathName.StartsWith("RSN:")) { return true; }

                DriveInfo[] infoArray = DriveInfo.GetDrives();
                foreach (DriveInfo info in infoArray)
                {
                    if (info.DriveType == DriveType.Network)
                    {
                        if (pathName.StartsWith(info.Name)) { return true; }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get information of the file location.\n" + ex.Message, "Find File Location", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return onNetwork;
        }

        public bool IsCentralFile(Document document)
        {
            bool isCentralFile = false;
            try
            {
                ModelPath centralModelPath = document.GetWorksharingCentralModelPath();
                if (null!=centralModelPath)
                {
                    string userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath);
                    if (centralModelPath.ServerPath)
                    {
                        string revitServerPrefix = "RSN://";
                        string a360Prefix = "A360://";
                        string relativePath = "";

                        if (userVisiblePath.Contains(revitServerPrefix))
                        {
                            string serverLocation = revitServerPrefix + centralModelPath.CentralServerPath;
                            relativePath = userVisiblePath.Substring(serverLocation.Length);
                        }
                        else if (userVisiblePath.Contains(a360Prefix))
                        {
                            string serverLocation = a360Prefix + centralModelPath.CentralServerPath;
                            relativePath = userVisiblePath.Substring(serverLocation.Length);
                        }

                        ServerPath serverPath = new ServerPath(centralModelPath.CentralServerPath, relativePath);
                        Guid centralGUID = document.Application.GetWorksharingCentralGUID(serverPath);
                        if (centralGUID != Guid.Empty)
                        {
                            ModelPath currentModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(document.PathName);
                            if (null!=currentModelPath)
                            {
#if RELEASE2015|| RELEASE2016 || RELEASE2017
                                Guid currentGuid = currentModelPath.GetModelGUID();
                                if (currentGuid != Guid.Empty)
                                {
                                    if (centralGUID.Equals(currentGuid))
                                    {
                                        isCentralFile = true;
                                    }
                                }
#endif
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
                MessageBox.Show("Failed to determine whether the file is central or local.\n"+ex.Message, "IsCentralFile", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return isCentralFile;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (timerOn)
                {
                    dispatcherTimer.IsEnabled = false;
                    TimedWarningWindow timedWindow = new TimedWarningWindow(timerCount, openedCentralFiles);
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
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void Application_DocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            try
            {
                Document closingDocument = args.Document;
                if (!string.IsNullOrEmpty(closingDocument.PathName))
                {
                    if (openedCentralFiles.ContainsKey(closingDocument.PathName))
                    {
                        openedCentralFiles.Remove(closingDocument.PathName);
                    }
                    if (openedCentralFiles.Count == 0)
                    {
                        timerOn = false;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }
}
