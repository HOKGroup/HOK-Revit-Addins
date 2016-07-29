using Autodesk.Revit.DB;
using HOK.ElementWatcher.Classes;
using HOK.ElementWatcher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.ElementWatcher.Updaters
{
    public class DTMUpdater : IUpdater
    {
        private UpdaterId updaterId = null;
        private Dictionary<string, BuiltInCategory> catDictionary = new Dictionary<string, BuiltInCategory>();
        private ObservableCollection<ReportingElementInfo> reportingElements = new ObservableCollection<ReportingElementInfo>();
        private Dictionary<Guid/*docId*/, ProjectUpdater> registeredUpdaters = new Dictionary<Guid, ProjectUpdater>();
        public static Guid updaterGuid = new Guid("A7483418-F1FF-4DBE-BB71-C6C8CEAE0FD4");
        

        public DTMUpdater(AddInId addinId)
        {
            updaterId = new UpdaterId(addinId, updaterGuid);
            catDictionary.Add("Grids", BuiltInCategory.OST_Grids);
            catDictionary.Add("Levels", BuiltInCategory.OST_Levels);
            catDictionary.Add("Views", BuiltInCategory.OST_Views);
            catDictionary.Add("Scope Boxes", BuiltInCategory.OST_VolumeOfInterest);
            catDictionary.Add("RVT Links", BuiltInCategory.OST_RvtLinks);
        }

        internal void Register(Document doc, ProjectUpdater pUpdater)
        {
            try
            {
                if (!UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.RegisterUpdater(this, doc);
                    RefreshTriggers(doc, pUpdater);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to register DTM updater\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal void Unregister(Document doc)
        {
            try
            {
                if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to unregister DTM updater\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal void RefreshTriggers(Document doc, ProjectUpdater pUpdater)
        {
            try
            {
                Guid docId = DataStorageUtil.GetProjectFileId(doc);
                UpdaterRegistry.RemoveDocumentTriggers(updaterId, doc);

                var elementsToDelete = from eInfo in reportingElements where eInfo.DocId == docId select eInfo;
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
                    if (trigger.IsEnabled)
                    {
                        ElementCategoryFilter catFilter = new ElementCategoryFilter(catDictionary[trigger.CategoryName]);
                        UpdaterRegistry.AddTrigger(updaterId, catFilter, Element.GetChangeTypeAny());

                        if (trigger.CategoryName == "Grids")
                        {
                            GridUtils.CollectGridExtents(doc, docId);
                            if (GridUtils.gridParameters.ContainsKey(docId))
                            {
                                foreach (ElementId paramId in GridUtils.gridParameters[docId])
                                {
                                    UpdaterRegistry.AddTrigger(updaterId, catFilter, Element.GetChangeTypeParameter(paramId));
                                }
                            }

                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            List<Element> elements = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().ToList();
                            foreach (Element element in elements)
                            {
                                ReportingElementInfo reportingInfo = new ReportingElementInfo(docId, trigger._id, trigger.CategoryName, element.Id, element.UniqueId);
                                reportingElements.Add(reportingInfo);
                            }
                        }
                        else if (trigger.CategoryName == "Views")
                        {
                            FilteredElementCollector collector = new FilteredElementCollector(doc);
                            List<View> views = collector.WherePasses(catFilter).WhereElementIsNotElementType().ToElements().Cast<View>().ToList();
                            var viewTemplates = from view in views where view.IsTemplate select view;
                            if (viewTemplates.Count() > 0)
                            {
                                foreach (Element view in viewTemplates)
                                {
                                    ReportingElementInfo reportingInfo = new ReportingElementInfo(docId, trigger._id, trigger.CategoryName, view.Id, view.UniqueId);
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
                                ReportingElementInfo reportingInfo = new ReportingElementInfo(docId, trigger._id, trigger.CategoryName, element.Id, element.UniqueId);
                                reportingElements.Add(reportingInfo);
                            }
                        }
                    }
                }

                if (registeredUpdaters.ContainsKey(docId))
                {
                    registeredUpdaters.Remove(docId);
                }
                registeredUpdaters.Add(docId, pUpdater);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to refresh triggers\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                Guid docId = DataStorageUtil.GetProjectFileId(doc);

                List<ElementId> addedElementIds = data.GetAddedElementIds().ToList();
                foreach (ElementId id in addedElementIds)
                {
                    Element element = doc.GetElement(id);
                    if (null != element)
                    {
                        AddCategoryCache(docId, element);
                    }
                }

                List<ElementId> modifiedElementIds = data.GetModifiedElementIds().ToList();
                foreach (ElementId id in modifiedElementIds)
                {
                    Element element = doc.GetElement(id);
                    if (null != element)
                    {
                        RunCategoryActionItems(docId, data, element);
                    }
                }

                List<ElementId> deletedElementIds = data.GetDeletedElementIds().ToList();
                foreach (ElementId id in deletedElementIds)
                {
                    //Process Failure
                    var infoFound = from info in reportingElements where info.DocId == docId && info.ReportingElementId == id select info;
                    if (infoFound.Count() > 0)
                    {
                        ReportFailure(infoFound.First());
                    }
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to execute updater.\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddCategoryCache(Guid docId, Element element)
        {
            try
            {
                if (element.Category.Name == "Grids")
                {
                    Grid grid = element as Grid;
                    if (!GridUtils.gridExtents.ContainsKey(docId))
                    {
                        Dictionary<ElementId, Outline> extents = new Dictionary<ElementId, Outline>();
                        extents.Add(grid.Id, grid.GetExtents());
                        GridUtils.gridExtents.Add(docId, extents);
                    }
                    else
                    {
                        GridUtils.gridExtents[docId].Add(grid.Id, grid.GetExtents());
                    }
                    AddElementToStorage(docId, element);
                }
                else if (element.Category.Name == "Views")
                {
                    View view = element as View;
                    if (view.IsTemplate)
                    {
                        AddElementToStorage(docId, element);
                    }
                }
                else
                {
                    AddElementToStorage(docId, element);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to add cache items.\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddElementToStorage(Guid docId, Element element)
        {
            try
            {
                string categoryName = element.Category.Name;
                var triggerFound = from trigger in registeredUpdaters[docId].CategoryTriggers where trigger.CategoryName == categoryName select trigger;
                if (triggerFound.Count() > 0)
                {
                    CategoryTrigger trigger = triggerFound.First();
                    ReportingElementInfo eInfo = new ReportingElementInfo(docId, trigger._id, trigger.CategoryName, element.Id, element.UniqueId);
                    reportingElements.Add(eInfo);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to add element.\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RunCategoryActionItems(Guid docId, UpdaterData data, Element element)
        {
            try
            {
                ReportingElementInfo reportingInfo = null;
                var infoFound = from info in reportingElements where info.DocId == docId && info.ReportingUniqueId == element.UniqueId select info;
                if (infoFound.Count() > 0)
                {
                    reportingInfo = infoFound.First();
                }

                BuiltInCategory bltCategory = (BuiltInCategory)element.Category.Id.IntegerValue;
                switch (bltCategory)
                {
                    case BuiltInCategory.OST_Grids:
                        Grid grid = element as Grid;
                        if (GridUtils.ExtentGeometryChanged(docId, grid.Id, grid.GetExtents()))
                        {
                            if (null != reportingInfo)
                            {
                                ReportFailure(reportingInfo);
                            }
                        }
                        else if(GridUtils.gridParameters.ContainsKey(docId))
                        {
                            foreach (ElementId paramId in GridUtils.gridParameters[docId])
                            {
                                if (data.IsChangeTriggered(grid.Id, Element.GetChangeTypeParameter(paramId)))
                                {
                                    if (null != reportingInfo)
                                    {
                                        ReportFailure(reportingInfo);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        if (null != reportingInfo)
                        {
                            ReportFailure(reportingInfo);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to run category action items.\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReportFailure(ReportingElementInfo eInfo)
        {
            try
            {
                AppCommand.Instance.IsElementChanged = true;
                AppCommand.Instance.ReportingInfo = eInfo;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("Failed to report failure.\n" + ex.Message, "DTM Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
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
