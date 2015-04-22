using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB;

namespace HOK.ModelReporting
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class AppCommand:IExternalApplication
    {
        private Dictionary<string/*docPath*/, EventSettings> syncSettingsDictionary = new Dictionary<string, EventSettings>();
        private Dictionary<string/*docPath*/, EventSettings> openSettingsDictionary = new Dictionary<string, EventSettings>();
        private Dictionary<string/*docPath*/, EventSettings> purgeSettingsDictionary = new Dictionary<string, EventSettings>();

        private EventSettings syncSettings;
        private EventSettings openSettings;
        private EventSettings purgeSettings;
        static private RevitCommandId commandId;
        private AddInCommandBinding binding = null;
        private bool purgeStarted = false;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(EventDocOpen);
                application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(EventDocClose);
                application.ControlledApplication.DocumentSynchronizingWithCentral += new EventHandler<DocumentSynchronizingWithCentralEventArgs>(EventSwcStart);
                application.ControlledApplication.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(EventSwcStop);

#if RELEASE2014 || RELEASE2015
                application.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(EventCommandFinished);

                if (binding == null)
                {
                    commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.PurgeUnused);
                    if (commandId.CanHaveBinding)
                    {
                        binding = application.CreateAddInCommandBinding(commandId);
                        binding.BeforeExecuted +=EventCommandStart;
                    }
                }
#endif

                return Result.Succeeded;
            }
            catch
            {
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                application.ControlledApplication.DocumentOpened -= new EventHandler<DocumentOpenedEventArgs>(EventDocOpen);
                application.ControlledApplication.DocumentClosing -= new EventHandler<DocumentClosingEventArgs>(EventDocClose);
                application.ControlledApplication.DocumentSynchronizingWithCentral -= new EventHandler<DocumentSynchronizingWithCentralEventArgs>(EventSwcStart);
                application.ControlledApplication.DocumentSynchronizedWithCentral -= new EventHandler<DocumentSynchronizedWithCentralEventArgs>(EventSwcStop);

#if RELEASE2014 || RELEASE2015
                application.ControlledApplication.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(EventCommandFinished);
               
                if (commandId.HasBinding)
                {
                    application.RemoveAddInCommandBinding(commandId);
                }
#endif
                return Result.Succeeded;
            }
            catch
            {
                return Result.Failed;
            }
        }

        private void EventSwcStart(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            try
            {
                if (!e.Document.IsFamilyDocument)
                {
                    syncSettings = new EventSettings(e.Document);
                    syncSettings.SizeStart = syncSettings.GetFileSize();
                    syncSettings.StartTime = DateTime.Now;

                    if (!string.IsNullOrEmpty(syncSettings.DocCentralPath))
                    {
                        if (syncSettingsDictionary.ContainsKey(syncSettings.DocCentralPath))
                        {
                            syncSettingsDictionary.Remove(syncSettings.DocCentralPath);
                        }
                        syncSettingsDictionary.Add(syncSettings.DocCentralPath, syncSettings);
                    }
                }
                
            }
            catch { }
        }

        private void EventSwcStop(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            try
            {
                if (!e.Document.IsFamilyDocument)
                {
                    string docPath = GetCentralPath(e.Document);
                    if (syncSettingsDictionary.ContainsKey(docPath))
                    {
                        EventSettings eventSettings = syncSettingsDictionary[docPath];
                        eventSettings.SizeEnd = eventSettings.GetFileSize();
                        eventSettings.EndTime = DateTime.Now;
                        WriteRecord(eventSettings, "SYNC-FILE");
                    }
                }
            }
            catch { }
        }

        private void EventDocOpen(object sender, DocumentOpenedEventArgs e)
        {
            try
            {
                if (!e.Document.IsFamilyDocument)
                {
                    openSettings = new EventSettings(e.Document);
                    openSettings.SizeStart = openSettings.GetFileSize();
                    openSettings.StartTime = DateTime.Now;

                    if (openSettings.OpenDetached)
                    {
                        openSettings.SizeEnd = openSettings.SizeStart;
                        openSettings.EndTime = DateTime.Now;
                        WriteRecord(openSettings, "OPEN-DETACHED");
                    }
                    else if(!string.IsNullOrEmpty(openSettings.DocCentralPath))
                    {
                        if (openSettingsDictionary.ContainsKey(openSettings.DocCentralPath))
                        {
                            openSettingsDictionary.Remove(openSettings.DocCentralPath);
                        }
                        openSettingsDictionary.Add(openSettings.DocCentralPath, openSettings);
                    }
                }
            }
            catch { }
        }

        private void EventDocClose(object sender, DocumentClosingEventArgs e)
        {
            try
            {
                if (!e.Document.IsFamilyDocument)
                {
                    string docPath = GetCentralPath(e.Document);
                    if (openSettingsDictionary.ContainsKey(docPath))
                    {
                        EventSettings eventSettings = openSettingsDictionary[docPath];
                        eventSettings.SizeEnd = eventSettings.GetFileSize();
                        eventSettings.EndTime = DateTime.Now;

                        WriteRecord(eventSettings, "OPEN-FILE");
                    }
                }
            }
            catch { }
        }

#if RELEASE2014 || RELEASE2015
        private void EventCommandStart(object sender, BeforeExecutedEventArgs e)
        {
            try
            {
                UIApplication uiapp = (UIApplication)sender;
                Document activeDoc = uiapp.ActiveUIDocument.Document;

                if (!activeDoc.IsFamilyDocument)
                {
                    purgeSettings = new EventSettings(activeDoc);
                    purgeSettings.SizeStart = purgeSettings.GetFileSize();
                    purgeSettings.StartTime = DateTime.Now;
                    purgeStarted = true;

                    if (!string.IsNullOrEmpty(purgeSettings.DocCentralPath))
                    {
                        if (purgeSettingsDictionary.ContainsKey(purgeSettings.DocCentralPath))
                        {
                            purgeSettingsDictionary.Remove(purgeSettings.DocCentralPath);
                        }
                        purgeSettingsDictionary.Add(purgeSettings.DocCentralPath, purgeSettings);
                    }
                }

            }
            catch { }
        }

        private void EventCommandFinished(object sender, DocumentChangedEventArgs e)
        {
            try
            {
                if (purgeStarted)
                {
                    List<ElementId> deletedIds = e.GetDeletedElementIds().ToList();
                    if (deletedIds.Count > 0)
                    {
                        Document doc = e.GetDocument();
                        if (!doc.IsFamilyDocument)
                        {
                            string docPath = GetCentralPath(doc);
                            if (purgeSettingsDictionary.ContainsKey(docPath))
                            {
                                EventSettings eventSettings = purgeSettingsDictionary[docPath];
                                eventSettings.SizeEnd = eventSettings.GetFileSize();
                                eventSettings.EndTime = DateTime.Now;
                                WriteRecord(eventSettings, "PURGE-UNUSED");
                            } 
                        }
                    }
                    purgeStarted = false;
                }
            }
            catch { }
        }
#endif

        private bool WriteRecord(EventSettings settings, string eventTypeName)
        {
            bool result = false;
            try
            {
                var entities = new KNetBIMDataServiceReference.KNetBIMDataCollectionEntities(new Uri("http://bimservices.hok.com/BIM/HOK.BIM.Services/KNetBIMDataService.svc/"));
                var dataPointEntity = new KNetBIMDataServiceReference.RevitEvent();
                {
                    dataPointEntity.ID = Guid.NewGuid();
                    dataPointEntity.EventType = eventTypeName;
                    dataPointEntity.ProjectNumber = settings.ProjectNumber;
                    dataPointEntity.ProjectName = settings.ProjectName;
                    dataPointEntity.ProjectLatitude = settings.ProjectLatitude;
                    dataPointEntity.ProjectLongitude = settings.ProjectLongitude;
                    dataPointEntity.FileName = settings.DocCentralPath;
                    dataPointEntity.LocalFileName = settings.DocLocalPath;
                    dataPointEntity.FileLocation = settings.FileLocation;
                    dataPointEntity.LocalFileLocation = settings.LocalFileLocation;
                    dataPointEntity.PreEventFileSize = (int)settings.SizeStart;
                    dataPointEntity.PostEventFileSize = (int)settings.SizeEnd;
                    dataPointEntity.EventStart = settings.StartTime;
                    dataPointEntity.EventFinish = settings.EndTime;
                    dataPointEntity.UserName = Environment.UserName;
                    dataPointEntity.UserLocation = settings.UserLocation;
                    dataPointEntity.UserIPAddress = settings.IPAddress;
                    dataPointEntity.ComputerName = Environment.MachineName;
                    dataPointEntity.SoftwareVersion = settings.VersionNumber;
                }

                entities.AddToRevitEvents(dataPointEntity);
                entities.SaveChanges();

                result = true;
            }
            catch { result = false; }
            return result;
        }

        private string GetCentralPath(Document doc)
        {
            string docCentralPath = "";
            try
            {
                if (doc.IsWorkshared)
                {
                    ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                    string centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        docCentralPath = centralPath;
                    }
                    else
                    {
                        //detached
                        docCentralPath = doc.PathName;
                    }
                }
                else
                {
                    docCentralPath = doc.PathName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                docCentralPath = doc.PathName;
            }
            return docCentralPath;
        }
    }
}
