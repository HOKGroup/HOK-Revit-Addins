using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.DTMTool.DTMUtils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.MissionControl.Tools.DTMTool
{
    public class DTMUpdater : IUpdater
    {
        private UpdaterId updaterId = null;
        private Dictionary<string, BuiltInCategory> catDictionary = new Dictionary<string, BuiltInCategory>();
        private ObservableCollection<ReportingElementInfo> reportingElements = new ObservableCollection<ReportingElementInfo>();

        private Guid updaterGuid = new Guid("A7483418-F1FF-4DBE-BB71-C6C8CEAE0FD4");
        public Guid UpdaterGuid { get { return updaterGuid; } set { updaterGuid = value; } }
        
        public DTMUpdater(AddInId addinId)
        {
            updaterId = new UpdaterId(addinId, updaterGuid);
            catDictionary.Add("Grids", BuiltInCategory.OST_Grids);
            catDictionary.Add("Levels", BuiltInCategory.OST_Levels);
            catDictionary.Add("Views", BuiltInCategory.OST_Views);
            catDictionary.Add("Scope Boxes", BuiltInCategory.OST_VolumeOfInterest);
            catDictionary.Add("RVT Links", BuiltInCategory.OST_RvtLinks);
        }

        public bool Register(Document doc, ProjectUpdater pUpdater)
        {
            bool registered = false;
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.RegisterUpdater(this, doc);
                    RefreshTriggers(doc, pUpdater);
                    LogUtil.AppendLog("DTM Updater Registered.");
                    registered = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-Register:" + ex.Message);
            }
            return registered;
        }

        public void Unregister(Document doc)
        {
            try
            {
                if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                    LogUtil.AppendLog("DTM Updater Removed.");
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-Unregister:" + ex.Message);
            }
        }

        public void RefreshTriggers(Document doc, ProjectUpdater pUpdater)
        {
            try
            {
                string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                string configId = "";
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    configId = AppCommand.Instance.ConfigDictionary[centralPath]._id;
                }

                UpdaterRegistry.RemoveDocumentTriggers(updaterId, doc);

                var elementsToDelete = from eInfo in reportingElements where eInfo.CentralPath == centralPath select eInfo;
                if (elementsToDelete.Count() > 0)
                {
                    List<ReportingElementInfo> elementsInfo = elementsToDelete.ToList();
                    foreach (ReportingElementInfo eInfo in elementsInfo)
                    {
                        reportingElements.Remove(eInfo);
                    }
                }

                foreach (CategoryTrigger trigger in pUpdater.CategoryTriggers)
                {
                    if (trigger.isEnabled)
                    {
                        ElementCategoryFilter catFilter = new ElementCategoryFilter(catDictionary[trigger.categoryName]);
                        UpdaterRegistry.AddTrigger(updaterId, catFilter, Element.GetChangeTypeAny());
                        UpdaterRegistry.AddTrigger(updaterId, catFilter, Element.GetChangeTypeElementAddition());
                        UpdaterRegistry.AddTrigger(updaterId, catFilter, Element.GetChangeTypeElementDeletion());
 
                        if (trigger.categoryName == "Grids")
                        {
                            GridUtils.CollectGridExtents(doc, centralPath);
                            if (GridUtils.gridParameters.ContainsKey(centralPath))
                            {
                                foreach (ElementId paramId in GridUtils.gridParameters[centralPath])
                                {
                                    UpdaterRegistry.AddTrigger(updaterId, catFilter, Element.GetChangeTypeParameter(paramId));
                                }
                            }

                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            List<Element> elements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();
                            foreach (Element element in elements)
                            {
                                ReportingElementInfo reportingInfo = new ReportingElementInfo(configId, updaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, element.Id, element.UniqueId);
                                reportingElements.Add(reportingInfo);
                            }
                        }
                        else if (trigger.categoryName == "Views")
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            List<View> views = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().Cast<View>().ToList();
                            var viewTemplates = from view in views where view.IsTemplate select view;
                            if (viewTemplates.Count() > 0)
                            {
                                foreach (Element view in viewTemplates)
                                {
                                    ReportingElementInfo reportingInfo = new ReportingElementInfo(configId, updaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, view.Id, view.UniqueId);
                                    reportingElements.Add(reportingInfo);
                                }
                            }
                        }
                        else
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            List<Element> elements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();
                            foreach (Element element in elements)
                            {
                                ReportingElementInfo reportingInfo = new ReportingElementInfo(configId, updaterGuid.ToString(), centralPath, trigger.categoryName, trigger.description, element.Id, element.UniqueId);
                                reportingElements.Add(reportingInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-RefreshTriggers:" + ex.Message);
            }
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                string configId = "";
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(centralPath))
                {
                    configId = AppCommand.Instance.ConfigDictionary[centralPath]._id;
                }

                List<ElementId> addedElementIds = data.GetAddedElementIds().ToList();
                foreach (ElementId id in addedElementIds)
                {
                    Element element = doc.GetElement(id);
                    if (null != element)
                    {
                        BuiltInCategory bltCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                        if (bltCategory == BuiltInCategory.OST_RvtLinks)
                        {
                            RunCategoryActionItems(centralPath, data, element);
                        }
                        else
                        {
                            AddCategoryCache(configId, centralPath, element);
                        }
                    }
                }

                List<ElementId> modifiedElementIds = data.GetModifiedElementIds().ToList();
                foreach (ElementId id in modifiedElementIds)
                {
                    Element element = doc.GetElement(id);
                    if (null != element)
                    {
                        RunCategoryActionItems(centralPath, data, element);
                    }
                }

                List<ElementId> deletedElementIds = data.GetDeletedElementIds().ToList();
                foreach (ElementId id in deletedElementIds)
                {
                    //Process Failure
                    var infoFound = from info in reportingElements where info.CentralPath == centralPath && info.ReportingElementId == id select info;
                    if (infoFound.Count() > 0)
                    {
                        ReportFailure(doc, infoFound.First());
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-Execute:" + ex.Message);
            }
        }

        private void AddCategoryCache(string configId, string centralPath, Element element)
        {
            try
            {
                if (element.Category.Name == "Grids")
                {
                    Grid grid = element as Grid;
                    if (!GridUtils.gridExtents.ContainsKey(centralPath))
                    {
                        Dictionary<ElementId, Outline> extents = new Dictionary<ElementId, Outline>();
                        extents.Add(grid.Id, grid.GetExtents());
                        GridUtils.gridExtents.Add(centralPath, extents);
                    }
                    else
                    {
                        GridUtils.gridExtents[centralPath].Add(grid.Id, grid.GetExtents());
                    }
                    AddElementToStorage(configId, centralPath, element);
                }
                else if (element.Category.Name == "Views")
                {
                    View view = element as View;
                    if (view.IsTemplate)
                    {
                        AddElementToStorage(configId, centralPath, element);
                    }
                }
                else
                {
                    AddElementToStorage(configId, centralPath, element);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-AddCategoryCache:" + ex.Message);
            }
        }

        private void AddElementToStorage(string configId, string centralPath, Element element)
        {
            try
            {
                string categoryName = element.Category.Name;
                string triggerMessage = GetTriggerMessage(configId, centralPath, element);
                ReportingElementInfo eInfo = new ReportingElementInfo(configId, updaterGuid.ToString(), centralPath, categoryName, triggerMessage, element.Id, element.UniqueId);
                reportingElements.Add(eInfo);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-AddElementToStorage:" + ex.Message);
            }
        }

        private string GetTriggerMessage(string configId, string centralPath, Element element)
        {
            string triggerMsg = "";
            try
            {
                if (AppCommand.Instance.ConfigDictionary.ContainsKey(configId))
                {
                    Configuration configFound = AppCommand.Instance.ConfigDictionary[configId];
                    var updaterFound = from updater in configFound.updaters where updater.updaterId.ToLower() == updaterGuid.ToString().ToLower() select updater;
                    if (updaterFound.Count() > 0)
                    {
                        ProjectUpdater dtmUpdater = updaterFound.First();
                        var triggerFound = from trigger in dtmUpdater.CategoryTriggers where trigger.categoryName == element.Category.Name select trigger;
                        if (triggerFound.Count() > 0)
                        {
                            triggerMsg = triggerFound.First().description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-GetTriggerMessage:" + ex.Message);
            }
            return triggerMsg;
        }

        private void RunCategoryActionItems(string centralPath, UpdaterData data, Element element)
        {
            try
            {
                ReportingElementInfo reportingInfo = null;
                Document doc = data.GetDocument();
                var infoFound = from info in reportingElements where info.CentralPath == centralPath && info.ReportingUniqueId == element.UniqueId select info;
                if (infoFound.Count() > 0)
                {
                    reportingInfo = infoFound.First();
                }

                BuiltInCategory bltCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                switch (bltCategory)
                {
                    case BuiltInCategory.OST_Grids:
                        Grid grid = element as Grid;
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
                            foreach (ElementId paramId in GridUtils.gridParameters[centralPath])
                            {
                                if (data.IsChangeTriggered(grid.Id, Element.GetChangeTypeParameter(paramId)))
                                {
                                    if (null != reportingInfo)
                                    {
                                        ReportFailure(doc, reportingInfo);
                                    }
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
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-RunCategoryActionItems:" + ex.Message);
            }
        }

        private void ReportFailure(Document doc, ReportingElementInfo eInfo)
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
                string message = ex.Message;
                LogUtil.AppendLog("DTMUpdater-ReportFailure:" + ex.Message);
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
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "DTMUpdater";
        }
    }
}
