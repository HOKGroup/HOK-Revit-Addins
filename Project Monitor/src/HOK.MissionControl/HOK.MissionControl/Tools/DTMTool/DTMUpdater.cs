using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Configurations;
using HOK.MissionControl.Tools.DTMTool.DTMUtils;
using HOK.MissionControl.Utils;
using HOK.MissionControl.Core.Utils;

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
        /// Registers the DTM Updater with the Revit Updater Registry.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="pUpdater">Updater instance.</param>
        /// <returns>True if registered successfully, otherwise false.</returns>
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
        /// Removes DTM Updater from the Revit Updater Registry.
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
                if (MissionControlSetup.Configurations.ContainsKey(centralPath))
                {
                    configId = MissionControlSetup.Configurations[centralPath].Id;
                }

                UpdaterRegistry.RemoveDocumentTriggers(_updaterId, doc);

                var elementsToDelete = _reportingElements.Where(x => x.CentralPath == centralPath).ToList();
                if (elementsToDelete.Any())
                {
                    foreach (var eInfo in elementsToDelete)
                    {
                        _reportingElements.Remove(eInfo);
                    }
                }

                foreach (var trigger in pUpdater.CategoryTriggers)
                {
                    if (!trigger.IsEnabled) continue;

                    var catFilter = new ElementCategoryFilter(_catDictionary[trigger.CategoryName]);
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeAny());
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeElementAddition());
                    UpdaterRegistry.AddTrigger(_updaterId, catFilter, Element.GetChangeTypeElementDeletion());
 
                    switch (trigger.CategoryName)
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
                                .ToElements();
                            foreach (var element in elements)
                            {
                                var reportingInfo = new ReportingElementInfo(
                                    configId, 
                                    UpdaterGuid.ToString(), 
                                    centralPath, 
                                    trigger.CategoryName, 
                                    trigger.Description, 
                                    element.Id, 
                                    element.UniqueId);
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
                                    var reportingInfo = new ReportingElementInfo(
                                        configId, 
                                        UpdaterGuid.ToString(), 
                                        centralPath, 
                                        trigger.CategoryName, 
                                        trigger.Description, 
                                        view.Id, 
                                        view.UniqueId);
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
                                var reportingInfo = new ReportingElementInfo(
                                    configId, 
                                    UpdaterGuid.ToString(), 
                                    centralPath, 
                                    trigger.CategoryName, 
                                    trigger.Description, 
                                    element.Id, 
                                    element.UniqueId);
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
        /// <param name="data"></param>
        public void Execute(UpdaterData data)
        {
            try
            {
                if (AppCommand.IsSynching) return;
                var doc = data.GetDocument();
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);

                foreach (var id in data.GetModifiedElementIds())
                {
                    var infoFound = _reportingElements.FirstOrDefault(x => x.CentralPath == centralPath && x.ReportingElementId == id);
                    if (infoFound == null) continue;

                    var element = doc.GetElement(id);
                    if (element != null) RunCategoryActionItems(centralPath, data, element, infoFound);
                }

                foreach (var id in data.GetDeletedElementIds())
                {
                    //Process Failure
                    var infoFound = _reportingElements.FirstOrDefault(x => x.CentralPath == centralPath && x.ReportingElementId == id);
                    if(infoFound != null) ReportFailure(doc, infoFound);
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
                    case BuiltInCategory.OST_Grids:
                        var grid = (Grid)element;
                        if (GridUtils.ExtentGeometryChanged(centralPath, grid.Id, grid.GetExtents()))
                        {
                            ReportFailure(doc, reportingInfo);
                        }
                        else if (GridUtils.gridParameters.ContainsKey(centralPath))
                        {
                            //parameter changed
                            foreach (var paramId in GridUtils.gridParameters[centralPath])
                            {
                                if (!data.IsChangeTriggered(grid.Id, Element.GetChangeTypeParameter(paramId))) continue;
                                ReportFailure(doc, reportingInfo);
                            }
                        }
                        break;
                    default:
                        ReportFailure(doc, reportingInfo);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
