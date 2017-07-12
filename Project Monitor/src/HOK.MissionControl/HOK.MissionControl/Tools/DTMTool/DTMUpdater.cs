using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Tools.DTMTool.DTMUtils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.DTMTool
{
    public class DtmUpdater : IUpdater
    {
        private readonly UpdaterId _updaterId;
        private readonly Dictionary<string, BuiltInCategory> _catDictionary = new Dictionary<string, BuiltInCategory>();
        private readonly ObservableCollection<ReportingElementInfo> _reportingElements = new ObservableCollection<ReportingElementInfo>();
        public Guid UpdaterGuid { get; set; } = new Guid("A7483418-F1FF-4DBE-BB71-C6C8CEAE0FD4");

        public DtmUpdater(AddInId addinId)
        {
            _updaterId = new UpdaterId(addinId, UpdaterGuid);
            _catDictionary.Add("Grids", BuiltInCategory.OST_Grids);
            _catDictionary.Add("Levels", BuiltInCategory.OST_Levels);
            _catDictionary.Add("Views", BuiltInCategory.OST_Views);
            _catDictionary.Add("Scope Boxes", BuiltInCategory.OST_VolumeOfInterest);
            _catDictionary.Add("RVT Links", BuiltInCategory.OST_RvtLinks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="pUpdater"></param>
        /// <returns></returns>
        public bool Register(Document doc, ProjectUpdater pUpdater)
        {
            var registered = false;
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(_updaterId, doc))
                {
                    UpdaterRegistry.RegisterUpdater(this, doc);
                    RefreshTriggers(doc, pUpdater);
                    Log.AppendLog("Registered.");
                    registered = true;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return registered;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void Unregister(Document doc)
        {
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(_updaterId, doc)) return;

                UpdaterRegistry.UnregisterUpdater(_updaterId, doc);
                Log.AppendLog("DTM Updater Removed.");
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="pUpdater"></param>
        public void RefreshTriggers(Document doc, ProjectUpdater pUpdater)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var configId = "";
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    configId = AppCommand.Instance.ConfigDictionary[centralPath].Id;
                }

                UpdaterRegistry.RemoveDocumentTriggers(_updaterId, doc);
                var elementsToDelete = _reportingElements.Where(x => x.CentralPath == centralPath).ToList();
                if (elementsToDelete.Any())
                {
                    var elementsInfo = elementsToDelete.ToList();
                    foreach (var eInfo in elementsInfo)
                    {
                        _reportingElements.Remove(eInfo);
                    }
                }

                foreach (var trigger in pUpdater.CategoryTriggers)
                {
                    if (!trigger.isEnabled) continue;

                    var catFilter = new ElementCategoryFilter(_catDictionary[trigger.categoryName]);
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeAny());
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeElementAddition());
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeElementDeletion());
 
                    switch (trigger.categoryName)
                    {
                        case "Grids":
                        {
                            GridUtils.CollectGridExtents(doc, centralPath);
                            if (GridUtils.gridParameters.ContainsKey(centralPath))
                            {
                                foreach (var paramId in GridUtils.gridParameters[centralPath])
                                {
                                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeParameter(paramId));
                                }
                            }

                            var elements = new FilteredElementCollector(doc)
                                    .WherePasses(catFilter)
                                    .WhereElementIsNotElementType()
                                    .ToElements()
                                    .ToList();
                            foreach (var element in elements)
                            {
                                var reportingInfo = new ReportingElementInfo(configId, UpdaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, element.Id, element.UniqueId);
                                _reportingElements.Add(reportingInfo);
                            }
                        }
                            break;
                        case "Views":
                        {
                            var viewTemplates = new FilteredElementCollector(doc)
                                .WherePasses(catFilter)
                                .WhereElementIsNotElementType()
                                .Cast<View>()
                                .Where(x => x.IsTemplate)
                                .ToList();

                            if (viewTemplates.Any())
                            {
                                foreach (var view in viewTemplates)
                                {
                                    var reportingInfo = new ReportingElementInfo(configId, UpdaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, view.Id, view.UniqueId);
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
                                    .ToElements()
                                    .ToList();
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
                Log.AppendLog(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Execute(UpdaterData data)
        {
            try
            {
                var doc = data.GetDocument();
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var configId = "";
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    configId = AppCommand.Instance.ConfigDictionary[centralPath].Id;
                }

                var addedElementIds = data.GetAddedElementIds().ToList();
                foreach (var id in addedElementIds)
                {
                    var element = doc.GetElement(id);
                    if (null == element) continue;

                    var bltCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                    if (bltCategory == BuiltInCategory.OST_RvtLinks)
                    {
                        RunCategoryActionItems(centralPath, data, element);
                    }
                    else
                    {
                        AddCategoryCache(configId, centralPath, element);
                    }
                }

                var modifiedElementIds = data.GetModifiedElementIds().ToList();
                foreach (var id in modifiedElementIds)
                {
                    var element = doc.GetElement(id);
                    if (null != element)
                    {
                        RunCategoryActionItems(centralPath, data, element);
                    }
                }

                var deletedElementIds = data.GetDeletedElementIds().ToList();
                foreach (var id in deletedElementIds)
                {
                    //Process Failure
                    var infoFound = _reportingElements
                        .Where(x => x.CentralPath == centralPath && x.ReportingElementId == id)
                        .ToList();
                    if (infoFound.Any())
                    {
                        ReportFailure(doc, infoFound.First());
                    }
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
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
                    case "Grids":
                        var grid = (Grid)element;
                        if (!GridUtils.gridExtents.ContainsKey(centralPath))
                        {
                            var extents = new Dictionary<ElementId, Outline> {{grid.Id, grid.GetExtents()}};
                            GridUtils.gridExtents.Add(centralPath, extents);
                        }
                        else
                        {
                            GridUtils.gridExtents[centralPath].Add(grid.Id, grid.GetExtents());
                        }
                        AddElementToStorage(configId, centralPath, element);
                        break;
                    case "Views":
                        var view = (View)element;
                        if (view.IsTemplate)
                        {
                            AddElementToStorage(configId, centralPath, element);
                        }
                        break;
                    default:
                        AddElementToStorage(configId, centralPath, element);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="centralPath"></param>
        /// <param name="element"></param>
        private void AddElementToStorage(string configId, string centralPath, Element element)
        {
            try
            {
                var categoryName = element.Category.Name;
                var triggerMessage = GetTriggerMessage(configId, element);
                var eInfo = new ReportingElementInfo(configId, UpdaterGuid.ToString(), centralPath, categoryName, triggerMessage, element.Id, element.UniqueId);
                _reportingElements.Add(eInfo);
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configId"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetTriggerMessage(string configId, Element element)
        {
            var triggerMsg = "";
            try
            {
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(configId))
                {
                    var configFound = AppCommand.Instance.ConfigDictionary[configId];

                    var updaterFound = configFound.updaters
                        .Where(x => string.Equals(x.updaterId.ToLower(), UpdaterGuid.ToString().ToLower()))
                        .ToList();
                    if (updaterFound.Any())
                    {
                        var dtmUpdater = updaterFound.First();
                        var triggerFound = dtmUpdater.CategoryTriggers
                            .Where(x => x.categoryName == element.Category.Name)
                            .ToList();
                        if (triggerFound.Any())
                        {
                            triggerMsg = triggerFound.First().description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return triggerMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centralPath"></param>
        /// <param name="data"></param>
        /// <param name="element"></param>
        private void RunCategoryActionItems(string centralPath, UpdaterData data, Element element)
        {
            try
            {
                ReportingElementInfo reportingInfo = null;
                var doc = data.GetDocument();
                var infoFound = _reportingElements
                    .Where(x => x.CentralPath == centralPath && x.ReportingUniqueId == element.UniqueId)
                    .ToList();
                if (infoFound.Any())
                {
                    reportingInfo = infoFound.First();
                }

                var bltCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                switch (bltCategory)
                {
                    case BuiltInCategory.OST_Grids:
                        var grid = (Grid)element;
                        if (GridUtils.ExtentGeometryChanged(centralPath, grid.Id, grid.GetExtents()))
                        {
                            if (null != reportingInfo)
                            {
                                ReportFailure(doc, reportingInfo);
                            }
                        }
                        else if (GridUtils.gridParameters.ContainsKey(centralPath))
                        {
                            //parameter changed
                            foreach (var paramId in GridUtils.gridParameters[centralPath])
                            {
                                if (!data.IsChangeTriggered(grid.Id, Element.GetChangeTypeParameter(paramId))) continue;

                                if (null != reportingInfo)
                                {
                                    ReportFailure(doc, reportingInfo);
                                }
                            }
                        }
                        break;
                    default:
                        if (null != reportingInfo)
                        {
                            ReportFailure(doc, reportingInfo);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="eInfo"></param>
        private static void ReportFailure(Document doc, ReportingElementInfo eInfo)
        {
            try
            {
                DTMFailure.IsElementModified = true;
                DTMFailure.ElementModified = eInfo;
                DTMFailure.CurrentDoc = doc;
                FailureProcessor.IsFailureFound = true;
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        public string GetAdditionalInformation()
        {
            return "Monitor changes on elements of specific categories";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.GridsLevelsReferencePlanes;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "DTMUpdater";
        }
    }
}
