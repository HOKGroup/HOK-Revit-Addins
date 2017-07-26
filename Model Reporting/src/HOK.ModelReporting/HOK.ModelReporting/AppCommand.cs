using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.ModelReporting
{
    [Transaction(TransactionMode.Manual)]
    public class AppCommand : IExternalApplication
    {
        private readonly Dictionary<string/*docPath*/, EventSettings> _syncSettingsDictionary = new Dictionary<string, EventSettings>();
        private readonly Dictionary<string/*docPath*/, EventSettings> _openSettingsDictionary = new Dictionary<string, EventSettings>();
        private readonly Dictionary<string/*docPath*/, EventSettings> _purgeSettingsDictionary = new Dictionary<string, EventSettings>();

        private EventSettings _syncSettings;
        private EventSettings _openSettings;
        private EventSettings _purgeSettings;
        private static RevitCommandId _commandId;
        private AddInCommandBinding _binding;
        private bool _purgeStarted;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                application.ControlledApplication.DocumentOpened += EventDocOpen;
                application.ControlledApplication.DocumentClosing += EventDocClose;
                application.ControlledApplication.DocumentSynchronizingWithCentral += EventSwcStart;
                application.ControlledApplication.DocumentSynchronizedWithCentral += EventSwcStop;

                application.ControlledApplication.DocumentChanged += EventCommandFinished;

                if (_binding != null) return Result.Succeeded;
                _commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.PurgeUnused);

                if (!_commandId.CanHaveBinding) return Result.Succeeded;
                _binding = application.CreateAddInCommandBinding(_commandId);
                _binding.BeforeExecuted +=EventCommandStart;
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                application.ControlledApplication.DocumentOpened -= EventDocOpen;
                application.ControlledApplication.DocumentClosing -= EventDocClose;
                application.ControlledApplication.DocumentSynchronizingWithCentral -= EventSwcStart;
                application.ControlledApplication.DocumentSynchronizedWithCentral -= EventSwcStop;

                application.ControlledApplication.DocumentChanged -= EventCommandFinished;
               
                if (_commandId.HasBinding)
                {
                    application.RemoveAddInCommandBinding(_commandId);
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return Result.Failed;
            }
        }

        private void EventSwcStart(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            try
            {
                if (e.Document.IsFamilyDocument) return;

                _syncSettings = new EventSettings(e.Document);
                _syncSettings.SizeStart = _syncSettings.GetFileSize();
                _syncSettings.StartTime = DateTime.Now;

                if (string.IsNullOrEmpty(_syncSettings.DocCentralPath)) return;

                if (_syncSettingsDictionary.ContainsKey(_syncSettings.DocCentralPath))
                {
                    _syncSettingsDictionary.Remove(_syncSettings.DocCentralPath);
                }
                _syncSettingsDictionary.Add(_syncSettings.DocCentralPath, _syncSettings);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void EventSwcStop(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            try
            {
                if (e.Document.IsFamilyDocument) return;
                var docPath = GetCentralPath(e.Document);

                if (!_syncSettingsDictionary.ContainsKey(docPath)) return;
                var eventSettings = _syncSettingsDictionary[docPath];
                eventSettings.SizeEnd = eventSettings.GetFileSize();
                eventSettings.EndTime = DateTime.Now;
                WriteRecord(eventSettings, "SYNC-FILE");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void EventDocOpen(object sender, DocumentOpenedEventArgs e)
        {
            try
            {
                if (e.Document.IsFamilyDocument) return;
                _openSettings = new EventSettings(e.Document);
                _openSettings.SizeStart = _openSettings.GetFileSize();
                _openSettings.StartTime = DateTime.Now;

                if (_openSettings.OpenDetached)
                {
                    _openSettings.SizeEnd = _openSettings.SizeStart;
                    _openSettings.EndTime = DateTime.Now;
                    WriteRecord(_openSettings, "OPEN-DETACHED");
                }
                else if (!string.IsNullOrEmpty(_openSettings.DocCentralPath))
                {
                    if (_openSettingsDictionary.ContainsKey(_openSettings.DocCentralPath))
                    {
                        _openSettingsDictionary.Remove(_openSettings.DocCentralPath);
                    }
                    _openSettingsDictionary.Add(_openSettings.DocCentralPath, _openSettings);
                }

                if (!_openSettings.IsRecordable)
                {
                    //warning message
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void EventDocClose(object sender, DocumentClosingEventArgs e)
        {
            try
            {
                if (e.Document.IsFamilyDocument) return;
                var docPath = GetCentralPath(e.Document);

                if (!_openSettingsDictionary.ContainsKey(docPath)) return;
                var eventSettings = _openSettingsDictionary[docPath];
                eventSettings.SizeEnd = eventSettings.GetFileSize();
                eventSettings.EndTime = DateTime.Now;
                WriteRecord(eventSettings, eventSettings.OpenCentral ? "OPEN-CENTRAL" : "OPEN-FILE");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void EventCommandStart(object sender, BeforeExecutedEventArgs e)
        {
            try
            {
                var uiapp = (UIApplication) sender;
                var activeDoc = uiapp.ActiveUIDocument.Document;

                if (activeDoc.IsFamilyDocument) return;
                _purgeSettings = new EventSettings(activeDoc);
                _purgeSettings.SizeStart = _purgeSettings.GetFileSize();
                _purgeSettings.StartTime = DateTime.Now;
                _purgeStarted = true;

                if (string.IsNullOrEmpty(_purgeSettings.DocCentralPath)) return;
                if (_purgeSettingsDictionary.ContainsKey(_purgeSettings.DocCentralPath))
                {
                    _purgeSettingsDictionary.Remove(_purgeSettings.DocCentralPath);
                }
                _purgeSettingsDictionary.Add(_purgeSettings.DocCentralPath, _purgeSettings);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void EventCommandFinished(object sender, DocumentChangedEventArgs e)
        {
            try
            {
                if (!_purgeStarted) return;
                var deletedIds = e.GetDeletedElementIds().ToList();
                if (deletedIds.Count > 0)
                {
                    var doc = e.GetDocument();
                    if (!doc.IsFamilyDocument)
                    {
                        var docPath = GetCentralPath(doc);
                        if (_purgeSettingsDictionary.ContainsKey(docPath))
                        {
                            var eventSettings = _purgeSettingsDictionary[docPath];
                            eventSettings.SizeEnd = eventSettings.GetFileSize();
                            eventSettings.EndTime = DateTime.Now;
                            WriteRecord(eventSettings, "PURGE-UNUSED");
                        }
                    }
                }
                _purgeStarted = false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void WriteRecord(EventSettings settings, string eventTypeName)
        {
            try
            {
                var entities = new KNetBIMDataServiceReference.KNetBIMDataCollectionEntities(
                    new Uri("http://bimservices.hok.com/BIM/HOK.BIM.Services/KNetBIMDataService.svc/"));
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
                    dataPointEntity.PreEventFileSize = (int) settings.SizeStart;
                    dataPointEntity.PostEventFileSize = (int) settings.SizeEnd;
                    dataPointEntity.EventStart = settings.StartTime;
                    dataPointEntity.EventFinish = settings.EndTime;
                    dataPointEntity.UserName = Environment.UserName;
                    dataPointEntity.UserLocation = settings.UserLocation;
                    dataPointEntity.UserIPAddress = settings.IpAddress;
                    dataPointEntity.ComputerName = Environment.MachineName;
                    dataPointEntity.SoftwareVersion = settings.VersionNumber;
                }

                entities.AddToRevitEvents(dataPointEntity);
                entities.SaveChanges();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        private static string GetCentralPath(Document doc)
        {
            string docCentralPath;
            try
            {
                if (doc.IsWorkshared)
                {
                    var modelPath = doc.GetWorksharingCentralModelPath();
                    var centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    docCentralPath = !string.IsNullOrEmpty(centralPath) ? centralPath : doc.PathName;
                }
                else
                {
                    docCentralPath = doc.PathName;
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                docCentralPath = doc.PathName;
            }
            return docCentralPath;
        }
    }
}
