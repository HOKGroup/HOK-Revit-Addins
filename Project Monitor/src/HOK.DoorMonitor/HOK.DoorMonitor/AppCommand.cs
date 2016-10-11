using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

namespace HOK.DoorMonitor
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class AppCommand:IExternalApplication
    {
        internal static AppCommand appCommand = null;
        private UIControlledApplication m_app;
        private string tabName = "  HOK - Beta";
        private static DoorUpdater doorUpdater = null;
        private AddInId thisAppId;
   
        private bool isDoorFail = false;
        private ElementId failingDoorId=ElementId.InvalidElementId;

        public static AppCommand Instance
        {
            get { return appCommand; }
        }

        public bool IsDoorFail { get { return isDoorFail; } set { isDoorFail = value; } }
        public ElementId FailingDoorId { get { return failingDoorId; } set { failingDoorId = value; } }
        

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            appCommand = this;
            m_app = application;
            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch { }

            RibbonPanel setupPanel = m_app.CreateRibbonPanel(tabName, "Project Setup");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            Assembly assembly = Assembly.GetExecutingAssembly();
            PushButton pushButton = setupPanel.AddItem(new PushButtonData("Settings", "Settings", currentAssembly, "HOK.DoorMonitor.SettingCommand")) as PushButton;
            pushButton.LargeImage = LoadBitmapImage(assembly, "settings.png");

            //doorFail = new DoorFailureProcessor();
            //Autodesk.Revit.ApplicationServices.Application.RegisterFailuresProcessor(doorFail);
            //Autodesk.Revit.ApplicationServices.ControlledApplication.RegisterFailuresProcessor(doorFail);
            
            thisAppId = application.ActiveAddInId;
            doorUpdater = new DoorUpdater(thisAppId);

            application.ControlledApplication.DocumentOpened += RegisterDoorUpdaterOnOpen;
            application.ControlledApplication.FailuresProcessing += CheckDoorWarning;
            
            return Result.Succeeded;
        }

        private void CheckDoorWarning(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                if (isDoorFail)
                {
                    FailuresAccessor fa = args.GetFailuresAccessor();
                    IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
                    failList = fa.GetFailureMessages();
                    bool foundFailingElement = false;
                    foreach (FailureMessageAccessor failure in failList)
                    {
                        foreach (ElementId id in failure.GetFailingElementIds())
                        {
                            if (failingDoorId.IntegerValue == id.IntegerValue) { foundFailingElement = true; }
                        }
                    }
                    if (foundFailingElement)
                    {
                        args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        FailureHandlingOptions option = fa.GetFailureHandlingOptions();
                        option.SetClearAfterRollback(true);
                        fa.SetFailureHandlingOptions(option);
                    }
                    
                    isDoorFail = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

      

        private void RegisterDoorUpdaterOnOpen(object source, DocumentOpenedEventArgs args)
        {
            if (null != args.Document)
            {
                Document doc = args.Document;
                
                MonitorProjectSetup projectSetup = ProjectSetupDataStroageUtil.GetProjectSetup(doc);
                if (projectSetup.IsMonitorOn)
                {
#if RELEASE2013
                    if (!UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId()))
                    {
                        doorUpdater.Register(doc, projectSetup);
                    }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 || RELEASE2017
                     if (!UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId(), doc))
                    {
                        doorUpdater.Register(doc, projectSetup);
                    }
#endif
                    args.Document.DocumentClosing += UnregisterDoorUpdaterOnClose;
                }
            }
        }

        private void UnregisterDoorUpdaterOnClose(object source, DocumentClosingEventArgs args)
        {
            if (null != args.Document && null!=doorUpdater)
            {
                Document doc = args.Document;
#if RELEASE2013
                if (UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId()))
                {
                    UpdaterRegistry.UnregisterUpdater(doorUpdater.GetUpdaterId());
                }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 ||RELEASE2017
                if (UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId(), doc))
                {
                    UpdaterRegistry.UnregisterUpdater(doorUpdater.GetUpdaterId(), doc);
                }
#endif
            }
        }

        public void RefreshMonitorSetup(Document doc, MonitorProjectSetup projectSetup)
        {
            try
            {
                if (projectSetup.IsMonitorOn)
                {
#if RELEASE2013
                    if (!UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId()))
                    {
                        doorUpdater.Register(doc, projectSetup);
                        doc.DocumentClosing += UnregisterDoorUpdaterOnClose;
                    }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 ||RELEASE2017
                    if (!UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId(), doc))
                    {
                        doorUpdater.Register(doc, projectSetup);
                        doc.DocumentClosing += UnregisterDoorUpdaterOnClose;
                    }
#endif
                }
                else
                {
#if RELEASE2013
                    if (UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId()))
                    {
                        UpdaterRegistry.UnregisterUpdater(doorUpdater.GetUpdaterId());
                    }
#elif RELEASE2014||RELEASE2015 || RELEASE2016 ||RELEASE2017
                    if (UpdaterRegistry.IsUpdaterRegistered(doorUpdater.GetUpdaterId(), doc))
                    {
                        UpdaterRegistry.UnregisterUpdater(doorUpdater.GetUpdaterId(), doc);
                    }
#endif
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
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
            }
            return image;
        }
        
    }

    public class DoorFailureProcessor : IFailuresProcessor
    {
        private bool isDoorFail = false;

        public bool IsDoorFail { get { return isDoorFail; } set { isDoorFail = value; } }

        public void Dismiss(Document document)
        {
            
        }

        public FailureProcessingResult ProcessFailures(FailuresAccessor data)
        {
            if (isDoorFail)
            {
                isDoorFail = false;
                return FailureProcessingResult.ProceedWithRollBack;
            }
            else
            {
                IList<FailureResolutionType> resolutionTypeList = new List<FailureResolutionType>();
                IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
                // Inside event handler, get all warnings
                failList = data.GetFailureMessages();

               
                foreach (FailureMessageAccessor failure in failList)
                {
                    
                    FailureDefinitionId failId = failure.GetFailureDefinitionId();
                    string failureMessage = failure.GetDescriptionText();
                    FailureSeverity severity = failure.GetSeverity();
                    resolutionTypeList = data.GetAttemptedResolutionTypes(failure);

                    foreach (ElementId id in failure.GetFailingElementIds())
                    {
                        int elementId = id.IntegerValue;
                       
                    }
                }

                return FailureProcessingResult.Continue;
            }
            
            
        }
    }
}
