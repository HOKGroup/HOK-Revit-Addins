using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Configurations;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.CADoor
{
    public class DoorUpdater : IUpdater
    {
        private UpdaterId updaterId;
        private List<Parameter> pullParameters = new List<Parameter>();
        private List<Parameter> pushParameters = new List<Parameter>();
        private List<Parameter> stateCAParameters = new List<Parameter>();
        private string v1pullParamName = "Approach @ Pull Side";
        private string v1pushParamName = "Approach @ Push Side";
        private string v1clearanceTypeName = "Approach";
        private string v2ParamName = "COMPONENT VERSION";
        private string v2pullParamName = "CLEARANCE TYPE_ PULL SIDE";
        private string v2pushParamName = "CLEARANCE TYPE_ PUSH SIDE";
        private string v2clearanceTypeName = "SWING,";
        private string stateCAParamName = "StateCA";
        private Guid _updaterGuid = new Guid("C2C658D7-EC43-4721-8D2C-2B8C10C340E2");
        public Guid UpdaterGuid { get { return _updaterGuid; } set { _updaterGuid = value; } }

        public DoorUpdater(AddInId addinId)
        {
            updaterId = new UpdaterId(addinId,_updaterGuid);
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
                if (!UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.RegisterUpdater(this, doc);
                    RefreshTriggers(doc, pUpdater);
                    Log.AppendLog(LogMessageType.INFO, "Door Updater Registered.");
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
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void Unregister(Document doc)
        {
            try
            {
                if (UpdaterRegistry.IsUpdaterRegistered(updaterId, doc))
                {
                    UpdaterRegistry.UnregisterUpdater(updaterId, doc);
                    Log.AppendLog(LogMessageType.INFO, "Door Updater Removed.");
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
        /// <param name="pUpdater"></param>
        /// <returns></returns>
        public bool RefreshTriggers(Document doc, ProjectUpdater pUpdater)
        {
            var refreshed = false;
            try
            {
                UpdaterRegistry.RemoveDocumentTriggers(updaterId, doc);
                ElementFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
                var collector = new FilteredElementCollector(doc);
                var doorInstances = collector.WherePasses(catFilter).WhereElementIsNotElementType().Cast<FamilyInstance>().ToList();

                collector = new FilteredElementCollector(doc);
                var doorFamilies = collector.OfClass(typeof(Family)).Cast<Family>().ToList();

                var existParam = FindClearanceParameter(doorInstances, doorFamilies);
                if (existParam)
                {
                    if (pullParameters.Count > 0)
                    {
                        foreach (var param in pullParameters)
                        {
                            UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                        }
                    }
                    if (pushParameters.Count > 0)
                    {
                        foreach (var param in pushParameters)
                        {
                            UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                        }

                    }
                    if (stateCAParameters.Count > 0)
                    {
                        UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeElementAddition());
                        foreach (var param in stateCAParameters)
                        {
                            UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return refreshed;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doorInstances"></param>
        /// <param name="doorFamilies"></param>
        /// <returns></returns>
        private bool FindClearanceParameter(List<FamilyInstance> doorInstances, List<Family> doorFamilies)
        {
            var exist = false;
            try
            {
                foreach (var doorFamily in doorFamilies)
                {
                    var doors = doorInstances.Where(x => x.Symbol.FamilyName == doorFamily.Name).ToList();
                    if (doors.Any())
                    {
                        var doorInstance = doors.First();
                        bool isV2Door = null != doorInstance.Symbol.LookupParameter(v2ParamName);
                        string pullParamName = isV2Door ? v2pullParamName : v1pullParamName;
                        string pushParamName = isV2Door ? v2pushParamName : v1pushParamName;
                        doorInstance.LookupParameter(v2ParamName);
                        var pullParam = doorInstance.LookupParameter(pullParamName);
                        if (null != pullParam)
                        {
                            pullParameters.Add(pullParam);
                        }
                        var pushParam = doorInstance.LookupParameter(pushParamName);
                        if (null != pushParam)
                        {
                            pushParameters.Add(pushParam);
                        }
                        var caParam = doorInstance.LookupParameter(stateCAParamName);
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return exist;
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
                if (data.GetModifiedElementIds().Count > 0)
                {
                    var doorId = data.GetModifiedElementIds().First();
                    var doorInstance = doc.GetElement(doorId) as FamilyInstance;
                    bool isV2Door = null != doorInstance.Symbol.LookupParameter(v2ParamName);
                    string pullParamName = isV2Door ? v2pullParamName : v1pullParamName;
                    string pushParamName = isV2Door ? v2pushParamName : v1pushParamName;
                    string typeNameContains = isV2Door ? v2clearanceTypeName : v1clearanceTypeName;
                    if (null != doorInstance)
                    {
                        var pushParameter = doorInstance.LookupParameter(pushParamName);

                        if (null != pushParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(pushParameter)))
                            {
                                var pushValue = pushParameter.AsValueString();
                                if (!pushValue.Contains(typeNameContains))
                                {
                                    DoorFailure.IsDoorFailed = true;
                                    DoorFailure.FailingDoorId = doorId;
                                    DoorFailure.CurrentDoc = doc;
                                    FailureProcessor.IsFailureFound = true;

                                    var dr = MessageBox.Show(pushValue + " is not a correct value for the parameter " + pushParamName, "Invalid Door Parameter.", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                        var pullParameter = doorInstance.LookupParameter(pullParamName);
                        if (null != pullParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(pullParameter)))
                            {
                                var pullValue = pullParameter.AsValueString();
                                if (!pullValue.Contains(typeNameContains))
                                {
                                    DoorFailure.IsDoorFailed = true;
                                    DoorFailure.FailingDoorId = doorId;
                                    FailureProcessor.IsFailureFound = true;

                                    var dr = MessageBox.Show(pullValue + " is not a correct value for the parameter " + pullParamName, "Invalid Door Parameter.", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                        }
                        var caParameter = doorInstance.LookupParameter(stateCAParamName);
                        if (null != caParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(caParameter)))
                            {
                                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                                if (MissionControlSetup.Projects.ContainsKey(centralPath))
                                {
                                    var project = MissionControlSetup.Projects[centralPath];
                                    if (project.Address.State == "CA")
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
                    var doorId = data.GetAddedElementIds().First();
                    var doorInstance = doc.GetElement(doorId) as FamilyInstance;
                    if (null != doorInstance)
                    {
                        var caParameter = doorInstance.LookupParameter(stateCAParamName);
                        if (null != caParameter)
                        {
                            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                            if (MissionControlSetup.Projects.ContainsKey(centralPath))
                            {
                                var project = MissionControlSetup.Projects[centralPath];
                                if (project.Address.State == "CA")
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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetAdditionalInformation()
        {
            return "Monitor changes on door parameter values.";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DoorsOpeningsWindows;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUpdaterName()
        {
            return "Door Parameter Updater";
        }
    }
}
