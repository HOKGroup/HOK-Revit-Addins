using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using HOK.ElementWatcher.Classes;
using HOK.ElementWatcher.Updaters;
using HOK.ElementWatcher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace HOK.ElementWatcher
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class AppCommand : IExternalApplication
    {
        private static AppCommand appCommand = null;
        private UIControlledApplication m_app; 
        private string tabName = "  HOK - Beta";

        private AddInId thisAppId;
        private Guid addInGuid = Guid.Empty;
        private string addInName = "";
        
        private bool isElementChanged = false;
        private ReportingElementInfo reportingInfo = new ReportingElementInfo();

        private DTMUpdater dtmUpdater = null;
        private string[] updaterCategories = new string[] { "Grids", "Levels", "Views", "Scope Boxes", "RVT Links" };
        private Dictionary<string/*docId*/, DTMConfigurations> configurations = new Dictionary<string, DTMConfigurations>();
            
        public static AppCommand Instance { get { return appCommand; } }

        public bool IsElementChanged { get { return isElementChanged; } set { isElementChanged = value; } }
        public ReportingElementInfo ReportingInfo { get { return reportingInfo; } set { reportingInfo = value; } }
        public DTMUpdater DtmUpdater { get { return dtmUpdater; } set { dtmUpdater = value; } }
        public Dictionary<string, DTMConfigurations> Configurations { get { return configurations; } set { configurations = value; } }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened -= RegisterElementUpdaterOnOpen;
            application.ControlledApplication.FailuresProcessing -= CheckElementWarning;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                appCommand = this;
                m_app = application;
                thisAppId = application.ActiveAddInId;
                addInGuid = thisAppId.GetGUID(); //9C4D37B2-155D-4AC8-ACCF-383D86673F1C
                addInName = thisAppId.GetAddInName();

                try
                {
                    m_app.CreateRibbonTab(tabName);
                }
                catch { }

                RibbonPanel setupPanel = m_app.CreateRibbonPanel(tabName, "DTM Tool");
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                Assembly assembly = Assembly.GetExecutingAssembly();
                PushButton pushButton = setupPanel.AddItem(new PushButtonData("Admin", "Admin", currentAssembly, "HOK.ElementWatcher.AdminCommand")) as PushButton;
                pushButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.dtm32.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                dtmUpdater = new DTMUpdater(thisAppId);

                application.ControlledApplication.DocumentOpened += RegisterElementUpdaterOnOpen;
                application.ControlledApplication.FailuresProcessing += CheckElementWarning;
            }
            catch(Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to initialize DTM Tool.\n"+ex.Message, "DTM Initialization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Result.Succeeded;
        }

        private BitmapImage LoadBitmapImage(Assembly assembly, string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to load Image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return image;
        }

        private void CheckElementWarning(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                if (isElementChanged)
                {
                    FailuresAccessor fa = args.GetFailuresAccessor();

                    WarningWindow warningWindow = new WarningWindow(reportingInfo);
                    if ((bool)warningWindow.ShowDialog())
                    {
                        args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        FailureHandlingOptions option = fa.GetFailureHandlingOptions();
                        option.SetClearAfterRollback(true);
                        fa.SetFailureHandlingOptions(option);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to promt users for a warning message.\n"+ex.Message, "Check Element Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                isElementChanged = false;
            }
        }

        private void RegisterElementUpdaterOnOpen(object source, DocumentOpenedEventArgs args)
        {
            try
            {
                if (null != args.Document)
                {
                    Document doc = args.Document;
                    DTMConfigurations config = GetConfiguration(doc);
                    if (!configurations.ContainsKey(config.ProjectFileInfo._id))
                    {
                        configurations.Add(config.ProjectFileInfo._id, config);
                    }

                    ApplyConfiguration(doc, config);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to register updaters on open.\n" + ex.Message, "Register Updater on Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ApplyConfiguration(Document doc, DTMConfigurations config)
        {
            try
            {
                foreach (ProjectUpdater updater in config.ProjectUpdaters)
                {
                    if ( updater.UpdaterId == DTMUpdater.updaterGuid.ToString())
                    {
                        if (updater.IsUpdaterOn)
                        {
                            if (!UpdaterRegistry.IsUpdaterRegistered(dtmUpdater.GetUpdaterId(), doc))
                            {
                                dtmUpdater.Register(doc, updater);
                                doc.DocumentClosing += UnregisterDTMUpdaterOnClose;
                            }
                            else
                            {
                                dtmUpdater.RefreshTriggers(doc, updater);
                            }
                        }
                        else
                        {
                            if (UpdaterRegistry.IsUpdaterRegistered(dtmUpdater.GetUpdaterId(), doc))
                            {
                                dtmUpdater.Unregister(doc);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to apply configuration.\n" + ex.Message, "Apply Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private DTMConfigurations GetConfiguration(Document doc)
        {
            DTMConfigurations config = new DTMConfigurations();
            try
            {
                string projectFileId = DataStorageUtil.GetProjectFileId(doc).ToString();
                string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (projectFileId == Guid.Empty.ToString())
                {
                    //first time use
                    List<ProjectFile> items = ServerUtil.GetProjectFiles("centralpath/" + centralPath);
                    if (items.Count > 0)
                    {
                        //import file info by central path
                        ProjectFile projectFile = items.First();
                        bool found = ServerUtil.GetConfiguration(projectFile._id, out config);
                        projectFileId = projectFile._id;
                    }
                    else
                    {
                        //create file info
                        projectFileId = Guid.NewGuid().ToString();
                        
                        Project projectInfo = FileInfoUtil.GetProjectInfo(centralPath);
                        List<Project> projects = ServerUtil.GetProjects("");
                        var projectFound = from p in projects where p.ProjectNumber == projectInfo.ProjectNumber && p.ProjectName == projectInfo.ProjectName select p;
                        if (projectFound.Count() > 0)
                        {
                            projectInfo = projectFound.First();
                        }
                        else
                        {
                            projectInfo._id = Guid.NewGuid().ToString();
                        }

                        ProjectFile pFile = new ProjectFile(projectFileId, centralPath, projectInfo._id, projectInfo);
                        config.ProjectFileInfo = pFile;

                        ProjectUpdater pUpdater = new ProjectUpdater(Guid.NewGuid().ToString(), DTMUpdater.updaterGuid.ToString(), dtmUpdater.GetUpdaterName(), addInGuid.ToString(), addInName, false, pFile._id);
                        foreach (string categoryName in updaterCategories)
                        {
                            CategoryTrigger catTrigger = new CategoryTrigger(Guid.NewGuid().ToString(), categoryName, pUpdater._id, false, Environment.UserName, DateTime.Now);
                            pUpdater.CategoryTriggers.Add(catTrigger);
                        }
                        config.ProjectUpdaters.Add(pUpdater);

                        string content="";
                        string errMsg="";
                        HttpStatusCode status = ServerUtil.PostConfiguration(out content, out errMsg, config);
                    }

                    bool stored = DataStorageUtil.StoreProjectFileId(doc, new Guid(projectFileId));
                }
                else
                {
                    bool found = ServerUtil.GetConfiguration(projectFileId, out config);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to get configuration.\n" + ex.Message, "Get Configuration from Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return config;
        }

        public void UnregisterDTMUpdaterOnClose(object source, DocumentClosingEventArgs args)
        {
            try
            {
                if (null != args.Document)
                {
                    Document doc = args.Document;
                    dtmUpdater.Unregister(doc);

                    Guid projectFileId = DataStorageUtil.GetProjectFileId(doc);
                    if(configurations.ContainsKey(projectFileId.ToString()))
                    {
                        configurations.Remove(projectFileId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to unregister on close.\n" + ex.Message, "Unregister DTM Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
