#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Linq;
#endregion

namespace HOK.SheetDataManager
{
    public class App : IExternalApplication
    {
        internal static App thisApp = null;
        private string tabName = "  HOK - Beta";
        private string versionNumber = "";
        public static AddInId addinId = null;
       
        private string[] hokSheetParameters = new string[] { "Sheet Number", "Sheet Name", "Volume Number", "Sorted Discipline", "Drawing Type" };
        private string[] hokRevisionParameters = new string[] { "Revision Sequence", "Revision Number", "Revision Description", "Revision Date", "Issued to", "Issued by"};

        private Dictionary<Guid/*updaterGUid*/, DatabaseManager> registeredDB = new Dictionary<Guid, DatabaseManager>();
 
        public Result OnStartup(UIControlledApplication application)
        {
            thisApp = this;
            versionNumber = application.ControlledApplication.VersionNumber;
            addinId = application.ActiveAddInId;

            try { application.CreateRibbonTab(tabName); }
            catch { }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Sheet Data");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            BitmapSource sheetImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.sync.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            PushButton sheetButton = panel.AddItem(new PushButtonData("SheetDataManager", "Sheet Data Manager", currentAssembly, "HOK.SheetDataManager.Command")) as PushButton;
            sheetButton.LargeImage = sheetImage;
            sheetButton.AvailabilityClassName = "HOK.SheetDataManager.Availability";

            string instructionFile = @"V:\RVT-Data\HOK Program\Documentation\SheetManagerTools_Instruction.pdf";
            if (File.Exists(instructionFile))
            {
                ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, instructionFile);
                sheetButton.SetContextualHelp(contextualHelp);
            }

            application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(DocumentOpened);
            application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(DocumentClosing);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened -= new EventHandler<DocumentOpenedEventArgs>(DocumentOpened);
            application.ControlledApplication.DocumentClosing -= new EventHandler<DocumentClosingEventArgs>(DocumentClosing);
            return Result.Succeeded;
        }

        public void DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            try
            {
                Document doc = args.Document;
                if (null != doc)
                {
                    //read data storage and add trigger
                    SheetManagerConfiguration config = DataStorageUtil.GetConfiguration(doc);
                    if (config.AutoUpdate && !string.IsNullOrEmpty(config.DatabaseFile))
                    {
                        if (File.Exists(config.DatabaseFile))
                        {
                            bool registered = RegisterSheetUpdater(doc, config);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to trigger the document opened event.\n"+ex.Message, "Document Opened Event", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void DocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            try
            {
                Document doc = args.Document;
                bool unregistered = UnregisterSheetUpdater(doc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unregister updater.\n"+ex.Message, "Sheet Manager : Unregister Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool RegisterSheetUpdater(Document doc, SheetManagerConfiguration config)
        {
            bool registered = false;
            try
            {
                IList<UpdaterInfo> registeredUpdaters = UpdaterRegistry.GetRegisteredUpdaterInfos(doc);
                DatabaseManager dbManager = new DatabaseManager(config.DatabaseFile);
                var sheetUpdaterInfo = from updater in registeredUpdaters where updater.UpdaterName ==  "SheetDBUpdater" select updater;
                if(sheetUpdaterInfo.Count() == 0)
                {
                    Guid updaterGuid = Guid.NewGuid();
                    Dictionary<ElementId, string> idMaps = GetIdMaps(doc, BuiltInCategory.OST_Sheets); // elementId vs uniqueId
                    SheetUpdater sheetUpdater = new SheetUpdater(addinId, updaterGuid, config.ModelId, idMaps);
                    sheetUpdater.sheetParameters = FindParameters(doc, BuiltInCategory.OST_Sheets, hokSheetParameters);
                    sheetUpdater.dbManager = dbManager;
                    if (!registeredDB.ContainsKey(updaterGuid))
                    {
                        registeredDB.Add(updaterGuid, dbManager);
                    }

                    UpdaterRegistry.RegisterUpdater(sheetUpdater, doc);
                    
                    ElementClassFilter sheetFilter = new ElementClassFilter(typeof(ViewSheet));
                    UpdaterRegistry.AddTrigger(sheetUpdater.GetUpdaterId(), sheetFilter, Element.GetChangeTypeElementAddition());
                    UpdaterRegistry.AddTrigger(sheetUpdater.GetUpdaterId(), sheetFilter, Element.GetChangeTypeElementDeletion());
                    foreach (ElementId paramId in sheetUpdater.sheetParameters.Keys)
                    {
                        UpdaterRegistry.AddTrigger(sheetUpdater.GetUpdaterId(), sheetFilter, Element.GetChangeTypeParameter(paramId));
                    }
                    registered = true;
                }

                var revisionUpdaterInfo = from updater in registeredUpdaters where updater.UpdaterName == "RevisionDBUpdater" select updater;
                if (revisionUpdaterInfo.Count() == 0)
                {
                    Guid updaterGuid = Guid.NewGuid();
                    Dictionary<ElementId, string> idMaps = GetIdMaps(doc, BuiltInCategory.OST_Revisions);  //elementId vs uniqueId
                    RevisionUpdater revisionUpdater = new RevisionUpdater(addinId, updaterGuid, config.ModelId, idMaps);
                    revisionUpdater.revisionParameters = FindParameters(doc, BuiltInCategory.OST_Revisions, hokRevisionParameters);
                    revisionUpdater.dbManager = dbManager;
                    if (!registeredDB.ContainsKey(updaterGuid))
                    {
                        registeredDB.Add(updaterGuid, dbManager);
                    }

                    UpdaterRegistry.RegisterUpdater(revisionUpdater, doc);

                    ElementCategoryFilter revisionFilter = new ElementCategoryFilter(BuiltInCategory.OST_Revisions);
                    UpdaterRegistry.AddTrigger(revisionUpdater.GetUpdaterId(), revisionFilter, Element.GetChangeTypeElementAddition());
                    UpdaterRegistry.AddTrigger(revisionUpdater.GetUpdaterId(), revisionFilter, Element.GetChangeTypeElementDeletion());
                    foreach (ElementId paramId in revisionUpdater.revisionParameters.Keys)
                    {
                        UpdaterRegistry.AddTrigger(revisionUpdater.GetUpdaterId(), revisionFilter, Element.GetChangeTypeParameter(paramId));
                    }
                    registered = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to register updater.\n" + ex.Message, "Sheet Manager : Register Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return registered;
        }

        public bool UnregisterSheetUpdater(Document doc)
        {
            bool unregistered = false;
            try
            {
                IList<UpdaterInfo> registeredUpdaters = UpdaterRegistry.GetRegisteredUpdaterInfos(doc);
                var sheetUpdaterInfo = from updater in registeredUpdaters where updater.UpdaterName ==  "SheetDBUpdater" select updater;
                if (sheetUpdaterInfo.Count() > 0)
                {
                    foreach (UpdaterInfo info in sheetUpdaterInfo)
                    {
                        string addInfo = info.AdditionalInformation;
                        Guid guid = new Guid(addInfo);
                        UpdaterId updaterId = new UpdaterId(addinId, guid);
                        if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                        {
                            UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                            if (registeredDB.ContainsKey(guid))
                            {
                                registeredDB[guid].CloseDatabse();
                                registeredDB.Remove(guid);
                            }
                            unregistered = true;
                        }
                    }
                }

                var revisionUpdaterInfo = from updater in registeredUpdaters where updater.UpdaterName == "RevisionDBUpdater" select updater;
                if (revisionUpdaterInfo.Count() == 0)
                {
                    foreach (UpdaterInfo info in revisionUpdaterInfo)
                    {
                        string addInfo = info.AdditionalInformation;
                        Guid guid = new Guid(addInfo);
                        UpdaterId updaterId = new UpdaterId(addinId, guid);
                        if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                        {
                            UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                            if (registeredDB.ContainsKey(guid))
                            {
                                registeredDB[guid].CloseDatabse();
                                registeredDB.Remove(guid);
                            }
                            unregistered = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unregister updater.\n" + ex.Message, "Sheet Manager : Unregister Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return unregistered;
        }

        private Dictionary<ElementId, string/*uniqueId*/> GetIdMaps(Document doc, BuiltInCategory category)
        {
            Dictionary<ElementId, string> idMaps = new Dictionary<ElementId, string>();
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> elements = collector.OfCategory(category).ToElements().ToList();
                foreach (Element element in elements)
                {
                    if (!idMaps.ContainsKey(element.Id))
                    {
                        idMaps.Add(element.Id, element.UniqueId);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return idMaps;
        }

        private Dictionary<ElementId, string> FindParameters(Document doc, BuiltInCategory category, string[] parameterNames)
        {
            Dictionary<ElementId, string> parameters = new Dictionary<ElementId, string>();
            try
            {
                //Revit2016: SharedParameterElement
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> elements = collector.OfCategory(category).ToElements().ToList();
                if (elements.Count > 0)
                {
                    Element element = elements.First();
                    
                    foreach (string paramName in parameterNames)
                    {
                        Parameter param = element.LookupParameter(paramName);
                        if (null != param)
                        {
                            if (!parameters.ContainsKey(param.Id))
                            {
                                parameters.Add(param.Id, paramName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find parameters.\n" + ex.Message, "Find Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return parameters;
        }

    }

    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }

    public class SheetUpdater : IUpdater
    {
        public AddInId m_appId;
        public UpdaterId m_updaterId;
        public Dictionary<ElementId, string> idMaps = new Dictionary<ElementId, string>();
        public Dictionary<ElementId/*paramId*/, string/*paramName*/> sheetParameters = new Dictionary<ElementId, string>();
        public DatabaseManager dbManager = null;
        public Guid linkedProjectId = Guid.Empty;

        public static bool isSheetManagerOn = false;

        public SheetUpdater(AddInId id, Guid guid, Guid projectId, Dictionary<ElementId, string> ids)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, guid);
            linkedProjectId = projectId;
            idMaps = ids;
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                if (!isSheetManagerOn && null != dbManager)
                {
                    Document doc = data.GetDocument();
                    
                    foreach (ElementId sheetId in data.GetAddedElementIds())
                    {
                        ViewSheet viewSheet = doc.GetElement(sheetId) as ViewSheet;
                        if (null != viewSheet)
                        {
                            // insert into the data base connected
                            SheetInfo sheetInfo = new SheetInfo(viewSheet);
                            LinkedSheet linkedSheet = new LinkedSheet(Guid.NewGuid(), Guid.NewGuid(), linkedProjectId, sheetInfo.SheetUniqueId, true);
                            sheetInfo.LinkedSheetItem = linkedSheet;

                            bool inserted = dbManager.InsertSheetInfo(sheetInfo);
                            if (!idMaps.ContainsKey(viewSheet.Id))
                            {
                                idMaps.Add(viewSheet.Id, viewSheet.UniqueId);
                            }
                        }
                    }

                    foreach (ElementId sheetId in data.GetModifiedElementIds())
                    {
                         ViewSheet viewSheet = doc.GetElement(sheetId) as ViewSheet;
                         if (null != viewSheet)
                         {
                             LinkedSheet linkedSheet = dbManager.GetLinkedSheet(viewSheet.UniqueId, linkedProjectId);
                             if (null != linkedSheet)
                             {
                                 foreach (ElementId paramId in sheetParameters.Keys)
                                 {
                                     if (data.IsChangeTriggered(sheetId, Element.GetChangeTypeParameter(paramId)))
                                     {
                                         string paramName = sheetParameters[paramId];
                                         Parameter param = viewSheet.LookupParameter(paramName);
                                         if (null != param)
                                         {
                                             string paramValue = param.AsString();
                                             bool updated = dbManager.UpdateSheetInfo(linkedSheet.SheetId, paramName, paramValue);
                                         }
                                         break;
                                     }
                                 }
                             }
                         }
                    }

                    foreach (ElementId sheetId in data.GetDeletedElementIds())
                    {
                        if (idMaps.ContainsKey(sheetId))
                        {
                            string uniqueId = idMaps[sheetId];
                            LinkedSheet linkedSheet = dbManager.GetLinkedSheet(uniqueId, linkedProjectId);
                            if (null != linkedSheet)
                            {
                                if (linkedSheet.IsSource)
                                {
                                    MessageBoxResult msgResult = MessageBox.Show("Would you like to delete the sheet item in the linked database?", "Delete Sheet Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (msgResult == MessageBoxResult.Yes)
                                    {
                                        bool deleted = dbManager.DeleteSheetInfo(linkedSheet.SheetId, linkedProjectId, true);
                                    }
                                    else
                                    {
                                        bool deleted = dbManager.DeleteSheetInfo(linkedSheet.SheetId, linkedProjectId, false); //only delete linked info
                                    }
                                }
                                else
                                {
                                    bool deleted = dbManager.DeleteSheetInfo(linkedSheet.SheetId, linkedProjectId, false); //only delete linked info
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to excute Sheet Updater.\n" + ex.Message, "Sheet Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetAdditionalInformation()
        {
            return m_updaterId.GetGUID().ToString();
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Views;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "SheetDBUpdater";
        }
    }

    public class RevisionUpdater : IUpdater
    {
        public AddInId m_appId;
        public UpdaterId m_updaterId;
        public Dictionary<ElementId, string> idMaps = new Dictionary<ElementId, string>();
        public Dictionary<ElementId/*paramId*/, string/*paramName*/> revisionParameters = new Dictionary<ElementId, string>();
        public DatabaseManager dbManager = null;
        public Guid linkedProjectId = Guid.Empty;

        public static bool isSheetManagerOn = false;

        public RevisionUpdater(AddInId id, Guid guid, Guid projectId, Dictionary<ElementId, string> ids)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, guid);
            linkedProjectId = projectId;
            idMaps = ids;
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                if (!isSheetManagerOn && null != dbManager)
                {
                    Document doc = data.GetDocument();
                    foreach (ElementId revisionId in data.GetAddedElementIds())
                    {
                        Revision revision = doc.GetElement(revisionId) as Revision;
                        if (null != revision)
                        {
                            // insert into the data base connected
                            RevisionInfo revisionInfo = new RevisionInfo(revision);
                            LinkedRevision linkedRevision = new LinkedRevision(Guid.NewGuid(), Guid.NewGuid(), revisionInfo.RevisionSequence, revisionInfo.RevisionNumber, revisionInfo.NumberType
                                , linkedProjectId, revisionInfo.RevisionUniqueId, true);
                            revisionInfo.LinkedRevisionItem = linkedRevision;

                            bool inserted = dbManager.InsertRevisionInfo(revisionInfo);
                            if (!idMaps.ContainsKey(revision.Id))
                            {
                                idMaps.Add(revision.Id, revision.UniqueId);
                            }
                        }
                    }

                    foreach (ElementId revisionId in data.GetModifiedElementIds())
                    {
                        Revision revision = doc.GetElement(revisionId) as Revision;
                        if (null != revision)
                        {
                            LinkedRevision linkedRevision = dbManager.GetLinkedRevision(revision.UniqueId, linkedProjectId);
                            if (null != linkedRevision)
                            {
                                foreach (ElementId paramId in revisionParameters.Keys)
                                {
                                    if (data.IsChangeTriggered(revisionId, Element.GetChangeTypeParameter(paramId)))
                                    {
                                        string paramName = revisionParameters[paramId];
                                        Parameter param = revision.LookupParameter(paramName);
                                        if (null != param)
                                        {
                                            object paramValue = null;
                                            if (paramName == "Revision Sequence")
                                            {
                                                paramValue = param.AsInteger();
                                            }
                                            else
                                            {
                                                paramValue = param.AsString();
                                            }

                                            bool updated = dbManager.UpdateRevisionInfo(linkedRevision.RevisionId, revision.UniqueId, paramName, paramValue);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    foreach (ElementId revisionId in data.GetDeletedElementIds())
                    {
                        if (idMaps.ContainsKey(revisionId))
                        {
                            string uniqueId = idMaps[revisionId];
                            LinkedRevision linkedRevision = dbManager.GetLinkedRevision(uniqueId, linkedProjectId);
                            if (null != linkedRevision)
                            {
                                if (linkedRevision.IsSource)
                                {
                                    MessageBoxResult msgResult = MessageBox.Show("Would you like to delete the revision item in the linked database?", "Delete Revision Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (msgResult == MessageBoxResult.Yes)
                                    {
                                        bool deleted = dbManager.DeleteRevisionInfo(linkedRevision.RevisionId, linkedProjectId, true);
                                    }
                                    else
                                    {
                                        bool deleted = dbManager.DeleteRevisionInfo(linkedRevision.RevisionId, linkedProjectId, false);
                                    }
                                }
                                else
                                {
                                    bool deleted = dbManager.DeleteRevisionInfo(linkedRevision.RevisionId, linkedProjectId, false);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute Revision updater.\n" + ex.Message, "Execute Revision Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetAdditionalInformation()
        {
            return m_updaterId.GetGUID().ToString();
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents; ;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "RevisionDBUpdater";
        }
    }
}
