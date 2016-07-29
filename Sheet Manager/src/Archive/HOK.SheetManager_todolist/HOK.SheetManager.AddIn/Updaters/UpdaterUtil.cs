using Autodesk.Revit.DB;
using HOK.SheetManager.AddIn.Classes;
using HOK.SheetManager.AddIn.Utils;
using HOK.SheetManager.AddIn.Windows;
using HOK.SheetManager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SheetManager.AddIn.Updaters
{
    public static class UpdaterUtil
    {
        public static AddInId addinId = null;
        public static Dictionary<Guid/*projectId*/, SheetUpdater> sheetUpdaters = new Dictionary<Guid, SheetUpdater>();
        public static Dictionary<Guid/*projectId*/, RevisionUpdater> revisionUpdaters = new Dictionary<Guid, RevisionUpdater>();

        public static bool RegisterUpdaters(Document doc, SheetManagerConfiguration config)
        {
            bool registered = false;
            try
            {
                bool sheetUpdaterRegistered = false;
                bool revisionUpdaterRegistered = false;

                if (File.Exists(config.DatabaseFile))
                {
                    IList<UpdaterInfo> registeredUpdaters = UpdaterRegistry.GetRegisteredUpdaterInfos(doc);
                    var sheetUpdaterFound = from updater in registeredUpdaters where updater.UpdaterName == "SheetDBUpdater" select updater;
                    if (sheetUpdaterFound.Count() == 0)
                    {
                        sheetUpdaterRegistered = RegisterSheetUpdater(doc, config);
                    }

                    var revisionUpdaterFound = from updater in registeredUpdaters where updater.UpdaterName == "RevisionDBUpdater" select updater;
                    if (revisionUpdaterFound.Count() == 0)
                    {
                        revisionUpdaterRegistered = RegisterRevisionUpdater(doc, config);
                    }
                }
                if (sheetUpdaterRegistered && revisionUpdaterRegistered) { registered = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to register updaters.\n" + ex.Message, "Register Updaters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return registered;
        }

        private static bool RegisterSheetUpdater(Document doc, SheetManagerConfiguration config)
        {
            bool registered = false;
            try
            {
                Guid updaterGuid = Guid.NewGuid();
                SheetUpdater sheetUpdater = new SheetUpdater(addinId, updaterGuid, config);
                sheetUpdater.CollectCustomSheetParamIds(doc);
                sheetUpdater.CollectIdMaps(doc);

                UpdaterRegistry.RegisterUpdater(sheetUpdater, doc);

                ElementClassFilter sheetFilter = new ElementClassFilter(typeof(ViewSheet));
                UpdaterRegistry.AddTrigger(sheetUpdater.GetUpdaterId(), sheetFilter, Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(sheetUpdater.GetUpdaterId(), sheetFilter, Element.GetChangeTypeElementDeletion());
                foreach (ElementId paramId in sheetUpdater.SheetParameters.Keys)
                {
                    UpdaterRegistry.AddTrigger(sheetUpdater.GetUpdaterId(), sheetFilter, Element.GetChangeTypeParameter(paramId));
                }

                if (!sheetUpdaters.ContainsKey(config.ModelId))
                {
                    sheetUpdaters.Add(config.ModelId, sheetUpdater);
                }

                registered = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to register sheet updater.\n" + ex.Message, "Register Sheet Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return registered;
        }

        private static bool RegisterRevisionUpdater(Document doc, SheetManagerConfiguration config)
        {
            bool registered = false;
            try
            {
                Guid updaterGuid = Guid.NewGuid();
                RevisionUpdater revisionUpdater = new RevisionUpdater(addinId, updaterGuid, config);
                revisionUpdater.CollectIdMaps(doc);

                UpdaterRegistry.RegisterUpdater(revisionUpdater, doc);

                ElementCategoryFilter revisionFilter = new ElementCategoryFilter(BuiltInCategory.OST_Revisions);
                UpdaterRegistry.AddTrigger(revisionUpdater.GetUpdaterId(), revisionFilter, Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(revisionUpdater.GetUpdaterId(), revisionFilter, Element.GetChangeTypeElementDeletion());
                foreach (ElementId paramId in revisionUpdater.RevisionParameters.Keys)
                {
                    UpdaterRegistry.AddTrigger(revisionUpdater.GetUpdaterId(), revisionFilter, Element.GetChangeTypeParameter(paramId));
                }

                if (!revisionUpdaters.ContainsKey(config.ModelId))
                {
                    revisionUpdaters.Add(config.ModelId, revisionUpdater);
                }
                registered = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to register revision updater.\n" + ex.Message, "Register Revision Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return registered;
        }

        public static bool UnregisterUpdaters(Document doc, SheetManagerConfiguration config)
        {
            bool unregistered = false;
            try
            {
                IList<UpdaterInfo> registeredUpdaters = UpdaterRegistry.GetRegisteredUpdaterInfos(doc);
                var sheetUpdaterInfo = from updater in registeredUpdaters where updater.UpdaterName == "SheetDBUpdater" select updater;
                if (sheetUpdaterInfo.Count() > 0)
                {
                    foreach (UpdaterInfo info in sheetUpdaterInfo)
                    {
                        string addInfo = info.AdditionalInformation;
                        Guid guid = new Guid(addInfo);
                        UpdaterId updaterId = new UpdaterId(addinId, guid);
                        if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                        {
                            if (sheetUpdaters.ContainsKey(config.ModelId))
                            {
                                sheetUpdaters[config.ModelId].CloseDataManager();
                                sheetUpdaters.Remove(config.ModelId);
                            }
                            UpdaterRegistry.UnregisterUpdater(updaterId, doc);
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
                            if (revisionUpdaters.ContainsKey(config.ModelId))
                            {
                                revisionUpdaters[config.ModelId].CloseDataManager();
                                revisionUpdaters.Remove(config.ModelId);
                            }
                            UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                           
                            unregistered = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to unregister updaters.\n" + ex.Message, "Unregister Updaters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return unregistered;
        }

    }
}
