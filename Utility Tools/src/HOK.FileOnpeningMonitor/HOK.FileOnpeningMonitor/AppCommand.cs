using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using System.Windows.Threading;



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
                if (openedDocument.IsWorkshared)
                {
                    if (string.IsNullOrEmpty(openedDocument.PathName)) { return; }
#if RELEASE2015 || RELEASE2016
                    if (openedDocument.IsDetached) { return; }
#endif

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

        public bool IsCentralFile(Document document)
        {
            bool isCentralFile = false;
            try
            {
                ModelPath modelPath = document.GetWorksharingCentralModelPath();
                string centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                if (!string.IsNullOrEmpty(centralPath))
                {
                    if (document.PathName == centralPath)
                    {
                        isCentralFile = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
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
