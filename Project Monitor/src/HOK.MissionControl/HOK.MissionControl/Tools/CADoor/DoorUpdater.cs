using Autodesk.Revit.DB;
using HOK.MissionControl.Classes;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.MissionControl.Tools.CADoor
{
    public class DoorUpdater : IUpdater
    {
        private UpdaterId updaterId = null;
        private List<Parameter> pullParameters = new List<Parameter>();
        private List<Parameter> pushParameters = new List<Parameter>();
        private List<Parameter> stateCAParameters = new List<Parameter>();

        private string pullParamName = "Approach @ Pull Side";
        private string pushParamName = "Approach @ Push Side";
        private string stateCAParamName = "StateCA";

        private Guid updaterGuid = new Guid("C2C658D7-EC43-4721-8D2C-2B8C10C340E2");
        public Guid UpdaterGuid { get { return updaterGuid; } set { updaterGuid = value; } }
        //private DoorFailureProcessor doorFailure = null;
        public DoorUpdater(AddInId addinId)
        {
            updaterId = new UpdaterId(addinId,updaterGuid);
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
                    LogUtil.AppendLog("Door Updater Registered.");
                    registered = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DoorUpdater-Register:" + ex.Message);
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
                    LogUtil.AppendLog("Door Updater Removed.");
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DoorUpdater-Unregister:" + ex.Message);
            }
        }

        public bool RefreshTriggers(Document doc, ProjectUpdater pUpdater)
        {
            bool refreshed = false;
            try
            {
                UpdaterRegistry.RemoveDocumentTriggers(updaterId, doc);
                ElementFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<FamilyInstance> doorInstances = collector.WherePasses(catFilter).WhereElementIsNotElementType().Cast<FamilyInstance>().ToList();

                collector = new FilteredElementCollector(doc);
                List<Family> doorFamilies = collector.OfClass(typeof(Family)).Cast<Family>().ToList();

                bool existParam = FindClearanceParameter(doorInstances, doorFamilies);
                if (existParam)
                {
                    if (pullParameters.Count > 0)
                    {
                        foreach (Parameter param in pullParameters)
                        {
                            UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                        }
                    }
                    if (pushParameters.Count > 0)
                    {
                        foreach (Parameter param in pushParameters)
                        {
                            UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                        }

                    }
                    if (stateCAParameters.Count > 0)
                    {
                        UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeElementAddition());
                        foreach (Parameter param in stateCAParameters)
                        {
                            UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DoorUpdater-RefreshTriggers:" + ex.Message);
            }
            return refreshed;
        }
    

        private bool FindClearanceParameter(List<FamilyInstance> doorInstances, List<Family> doorFamilies)
        {
            bool exist = false;
            try
            {
                foreach (Family doorFamily in doorFamilies)
                {
#if RELEASE2013 ||RELEASE2014
                    var doors = from door in doorInstances where door.Symbol.Family.Name == doorFamily.Name select door;
#elif RELEASE2015 || RELEASE2016
                    var doors = from door in doorInstances where door.Symbol.FamilyName == doorFamily.Name select door;
#endif

                    if (doors.Count() > 0)
                    {
                        FamilyInstance doorInstance = doors.First();
#if RELEASE2013 || RELEASE2014
                        Parameter pullParam = doorInstance.get_Parameter(pullParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter pullParam = doorInstance.LookupParameter(pullParamName);
#endif
                        if (null != pullParam)
                        {
                            pullParameters.Add(pullParam);
                        }

#if RELEASE2013||RELEASE2014
                        Parameter pushParam = doorInstance.get_Parameter(pushParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter pushParam = doorInstance.LookupParameter(pushParamName);
#endif
                        if (null != pushParam)
                        {
                            pushParameters.Add(pushParam);
                        }

#if RELEASE2013||RELEASE2014
                        Parameter caParam = doorInstance.get_Parameter(stateCAParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter caParam = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParam)
                        {
                            stateCAParameters.Add(caParam);
                        }
                    }
                }

                if (pullParameters.Count > 0 && pushParameters.Count > 0 && stateCAParameters.Count > 0)
                {
                    exist = true;
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DoorUpdater-FindClearanceParameter:" + ex.Message);
            }
            return exist;
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                if (data.GetModifiedElementIds().Count > 0)
                {
                    ElementId doorId = data.GetModifiedElementIds().First();
                    FamilyInstance doorInstance = doc.GetElement(doorId) as FamilyInstance;
                    if (null != doorInstance)
                    {
#if RELEASE2013||RELEASE2014
                        Parameter pushParameter = doorInstance.get_Parameter(pushParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter pushParameter = doorInstance.LookupParameter(pushParamName);
#endif

                        if (null != pushParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(pushParameter)))
                            {
                                string pushValue = pushParameter.AsValueString();
                                if (!pushValue.Contains("Approach"))
                                {
                                    DoorFailure.IsDoorFailed = true;
                                    DoorFailure.FailingDoorId = doorId;
                                    FailureProcessor.IsFailureFound = true;

                                    MessageBoxResult dr = MessageBox.Show(pushValue + " is not a correct value for the parameter " + pushParamName, "Invalid Door Parameter.", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
#if RELEASE2013||RELEASE2014
                        Parameter pullParameter = doorInstance.get_Parameter(pullParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter pullParameter = doorInstance.LookupParameter(pullParamName);
#endif


                        if (null != pullParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(pullParameter)))
                            {
                                string pullValue = pullParameter.AsValueString();
                                if (!pullValue.Contains("Approach"))
                                {
                                    DoorFailure.IsDoorFailed = true;
                                    DoorFailure.FailingDoorId = doorId;
                                    FailureProcessor.IsFailureFound = true;

                                    MessageBoxResult dr = MessageBox.Show(pullValue + " is not a correct value for the parameter " + pullParamName, "Invalid Door Parameter.", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }

#if RELEASE2013||RELEASE2014
                        Parameter caParameter = doorInstance.get_Parameter(stateCAParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter caParameter = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(caParameter)))
                            {
                                string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                                if (AppCommand.Instance.ProjectDictionary.ContainsKey(centralPath))
                                {
                                    Project project = AppCommand.Instance.ProjectDictionary[centralPath];
                                    if (project.address.state == "CA")
                                    {
                                        caParameter.Set(1);
                                    }
                                    else
                                    {
                                        caParameter.Set(0);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (data.GetAddedElementIds().Count > 0)
                {
                    ElementId doorId = data.GetAddedElementIds().First();
                    FamilyInstance doorInstance = doc.GetElement(doorId) as FamilyInstance;
                    if (null != doorInstance)
                    {
#if RELEASE2013||RELEASE2014
                        Parameter caParameter = doorInstance.get_Parameter(stateCAParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter caParameter = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParameter)
                        {
                            string centralPath = FileInfoUtil.GetCentralFilePath(doc);
                            if (AppCommand.Instance.ProjectDictionary.ContainsKey(centralPath))
                            {
                                Project project = AppCommand.Instance.ProjectDictionary[centralPath];
                                if (project.address.state == "CA")
                                {
                                    caParameter.Set(1);
                                }
                                else
                                {
                                    caParameter.Set(0);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.AppendLog("DoorUpdater-Execute:" + ex.Message);
            }
        }

        public string GetAdditionalInformation()
        {
            return "Monitor changes on door parameter values.";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DoorsOpeningsWindows;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "Door Parameter Updater";
        }
    }
}
