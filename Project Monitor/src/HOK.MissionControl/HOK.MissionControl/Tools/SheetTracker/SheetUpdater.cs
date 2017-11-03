using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.SheetTracker
{
    public class SheetUpdater : IUpdater
    {
        private readonly UpdaterId _updaterId;
        public Guid UpdaterGuid { get; set; } = new Guid("0504A758-BF15-4C90-B996-A795D92B42DB");
        private readonly Dictionary<string, BuiltInCategory> _catDictionary = new Dictionary<string, BuiltInCategory>();
        private readonly List<ReportingElementInfo> _reportingElements = new List<ReportingElementInfo>();
        //TODO: This should be a dictionary of string,reporting element info. That way we can do the lookup way faster.
        //TODO: string key should be a combination of centralPath + element id

        public SheetUpdater(AddInId addinId)
        {
            _updaterId = new UpdaterId(addinId, UpdaterGuid);
            _catDictionary.Add("Sheets", BuiltInCategory.OST_Sheets);
            _catDictionary.Add("Revisions", BuiltInCategory.OST_Revisions);
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                if (AppCommand.IsSynching) return;
                var doc = data.GetDocument();
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);

                var configId = "";
                if (MissionControlSetup.Configurations.ContainsKey(centralPath))
                {
                    configId = MissionControlSetup.Configurations[centralPath].Id;
                }

                var addedElementIds = data.GetAddedElementIds().ToList();
                foreach (var id in addedElementIds)
                {
                    var element = doc.GetElement(id);
                    if (null == element) continue;

                    AddCategoryCache(configId, centralPath, element);
                }

                foreach (var id in data.GetModifiedElementIds())
                {
                    var infoFound = _reportingElements.FirstOrDefault(x => x.CentralPath == centralPath && x.ReportingElementId == id);
                    if (infoFound == null) continue;

                    var element = doc.GetElement(id);
                    if (element != null) RunCategoryActionItems(centralPath, data, element, infoFound);
                }

                foreach (var id in data.GetDeletedElementIds())
                {
                    var infoFound = _reportingElements.FirstOrDefault(x => x.CentralPath == centralPath && x.ReportingElementId == id);
                    if (infoFound != null)
                    {
                        // (Konrad) Trigger 
                        //ReportFailure(doc, infoFound);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.GridsLevelsReferencePlanes;
        }

        public string GetUpdaterName()
        {
            return "SheetsUpdater";
        }

        public string GetAdditionalInformation()
        {
            return "Monitor changes on Sheets and Revisions.";
        }

        /// <summary>
        /// Registers the updater in Updater Registry.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="pUpdater">Project Updater.</param>
        /// <returns>True if succeeded or False if failed.</returns>
        public bool Register(Document doc, ProjectUpdater pUpdater)
        {
            var registered = false;
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(_updaterId, doc))
                {
                    UpdaterRegistry.RegisterUpdater(this, doc);
                    RefreshTriggers(doc, pUpdater);
                    Log.AppendLog(LogMessageType.INFO, "Registered.");
                    registered = true;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return registered;
        }

        /// <summary>
        /// Unregisters the updater in Updater Registry.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        public void Unregister(Document doc)
        {
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(_updaterId, doc)) return;

                UpdaterRegistry.UnregisterUpdater(_updaterId, doc);
                Log.AppendLog(LogMessageType.INFO, "DTM Updater Removed.");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Clears current collection of reporting elements and populates it with a new set of sheets/revisions.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="pUpdater"></param>
        public void RefreshTriggers(Document doc, ProjectUpdater pUpdater)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var configId = "";
                if (MissionControlSetup.Configurations.ContainsKey(centralPath))
                {
                    configId = MissionControlSetup.Configurations[centralPath].Id;
                }

                UpdaterRegistry.RemoveDocumentTriggers(_updaterId, doc);

                _reportingElements.RemoveAll(x => x.CentralPath == centralPath);
                //var elementsToDelete = _reportingElements.Where(x => x.CentralPath == centralPath).ToList();
                //if (elementsToDelete.Any())
                //{
                //    foreach (var eInfo in elementsToDelete)
                //    {
                //        _reportingElements.Remove(eInfo);
                //    }
                //}

                foreach (var trigger in pUpdater.CategoryTriggers)
                {
                    if (!trigger.isEnabled) continue;

                    var catFilter = new ElementCategoryFilter(_catDictionary[trigger.categoryName]);
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeAny());
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeElementAddition());
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeElementDeletion());

                    switch (trigger.categoryName)
                    {
                        case "Sheets":
                        {
                            var sheets = new FilteredElementCollector(doc)
                                .WherePasses(catFilter)
                                .WhereElementIsNotElementType()
                                .Cast<View>()
                                .Where(x => x.ViewType == ViewType.DrawingSheet)
                                .ToList();

                            if (sheets.Any())
                            {
                                foreach (var s in sheets)
                                {
                                    var reportingInfo = new ReportingElementInfo(configId, UpdaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, s.Id, s.UniqueId);
                                    _reportingElements.Add(reportingInfo);
                                }
                            }
                        }
                            break;
                        default:
                        {
                            var elements = new FilteredElementCollector(doc)
                                .WherePasses(catFilter)
                                .WhereElementIsNotElementType()
                                .ToElements();
                            foreach (var element in elements)
                            {
                                var reportingInfo = new ReportingElementInfo(configId, UpdaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, element.Id, element.UniqueId);
                                _reportingElements.Add(reportingInfo);
                            }
                        }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centralPath"></param>
        /// <param name="data"></param>
        /// <param name="element"></param>
        /// <param name="reportingInfo"></param>
        private static void RunCategoryActionItems(string centralPath, UpdaterData data, Element element, ReportingElementInfo reportingInfo)
        {
            try
            {
                var doc = data.GetDocument();
                var bltCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                switch (bltCategory)
                {
                    case BuiltInCategory.OST_Sheets:
                        var sheet = (ViewSheet)element;
                        //if (GridUtils.ExtentGeometryChanged(centralPath, grid.Id, grid.GetExtents()))
                        //{
                        //    ReportFailure(doc, reportingInfo);
                        //}
                        //else if (GridUtils.gridParameters.ContainsKey(centralPath))
                        //{
                        //    //parameter changed
                        //    foreach (var paramId in GridUtils.gridParameters[centralPath])
                        //    {
                        //        if (!data.IsChangeTriggered(grid.Id, Element.GetChangeTypeParameter(paramId))) continue;
                        //        ReportFailure(doc, reportingInfo);
                        //    }
                        //}
                        break;
                    default:
                        //ReportFailure(doc, reportingInfo);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="centralPath"></param>
        /// <param name="element"></param>
        private void AddCategoryCache(string configId, string centralPath, Element element)
        {
            try
            {
                switch (element.Category.Name)
                {
                    case "Sheets":
                        AddElementToStorage(configId, centralPath, element);
                        break;
                    default:
                        AddElementToStorage(configId, centralPath, element);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Adds selected elements to current updater cache so we can monitor them.
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="centralPath"></param>
        /// <param name="element"></param>
        private void AddElementToStorage(string configId, string centralPath, Element element)
        {
            try
            {
                var categoryName = element.Category.Name;
                //var triggerMessage = GetTriggerMessage(configId, element);
                var eInfo = new ReportingElementInfo(configId, UpdaterGuid.ToString(), centralPath, categoryName, string.Empty, element.Id, element.UniqueId);
                _reportingElements.Add(eInfo);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
