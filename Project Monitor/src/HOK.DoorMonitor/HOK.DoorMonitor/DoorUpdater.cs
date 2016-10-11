using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace HOK.DoorMonitor
{
    public class DoorUpdater:IUpdater
    {
        private UpdaterId updaterId = null;
        private List<Parameter> pullParameters = new List<Parameter>();
        private List<Parameter> pushParameters = new List<Parameter>();
        private List<Parameter> stateCAParameters = new List<Parameter>();

        private string pullParamName = "Approach @ Pull Side";
        private string pushParamName = "Approach @ Push Side";
        private string stateCAParamName = "StateCA";

        //private DoorFailureProcessor doorFailure = null;
        public DoorUpdater(AddInId addinId)
        {
            updaterId = new UpdaterId(addinId, new Guid("C2C658D7-EC43-4721-8D2C-2B8C10C340E2"));
            
        }

        internal void Register(Document doc, MonitorProjectSetup projectSetup)
        {
            ElementFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<FamilyInstance> doorInstances = collector.WherePasses(catFilter).WhereElementIsNotElementType().Cast<FamilyInstance>().ToList();

            collector = new FilteredElementCollector(doc);
            List<Family> doorFamilies = collector.OfClass(typeof(Family)).Cast<Family>().ToList();

            bool existParam = FindClearanceParameter(doorInstances, doorFamilies);
            if (existParam)
            {
                UpdaterRegistry.RegisterUpdater(this, doc);
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
                if (stateCAParameters.Count > 0 )
                {
                    UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeElementAddition());
                    foreach (Parameter param in stateCAParameters)
                    {
                        UpdaterRegistry.AddTrigger(updaterId, doc, catFilter, Element.GetChangeTypeParameter(param));
                    }
                }
            }

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
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                    var doors = from door in doorInstances where door.Symbol.FamilyName == doorFamily.Name select door;
#endif

                    if (doors.Count() > 0)
                    {
                        FamilyInstance doorInstance = doors.First();
#if RELEASE2013 || RELEASE2014
                        Parameter pullParam = doorInstance.get_Parameter(pullParamName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter pullParam = doorInstance.LookupParameter(pullParamName);
#endif
                        if (null != pullParam)
                        {
                            pullParameters.Add(pullParam);
                        }

#if RELEASE2013||RELEASE2014
                        Parameter pushParam = doorInstance.get_Parameter(pushParamName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter pushParam = doorInstance.LookupParameter(pushParamName);
#endif
                        if (null != pushParam)
                        {
                            pushParameters.Add(pushParam);
                        }

#if RELEASE2013||RELEASE2014
                        Parameter caParam = doorInstance.get_Parameter(stateCAParamName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter caParam = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParam)
                        {
                            stateCAParameters.Add(caParam);
                        }
                    }
                }

                if (pullParameters.Count  > 0 && pushParameters.Count > 0 && stateCAParameters.Count > 0)
                {
                    exist = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find clearance related parameters.\n" + ex.Message, "Door Updater: Find Clearance Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return exist;
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                MonitorProjectSetup projectSetup = ProjectSetupDataStroageUtil.GetProjectSetup(doc);
                if (data.GetModifiedElementIds().Count > 0)
                {
                    ElementId doorId = data.GetModifiedElementIds().First();
                    FamilyInstance doorInstance = doc.GetElement(doorId) as FamilyInstance;
                    if (null != doorInstance)
                    {
#if RELEASE2013||RELEASE2014
                        Parameter pushParameter = doorInstance.get_Parameter(pushParamName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter pushParameter = doorInstance.LookupParameter(pushParamName);
#endif

                        if (null != pushParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(pushParameter)))
                            {
                                string pushValue = pushParameter.AsValueString();
                                if (!pushValue.Contains("Approach"))
                                {
                                    AppCommand.Instance.IsDoorFail = true;
                                    AppCommand.Instance.FailingDoorId = doorId;
                                    DialogResult dr = MessageBox.Show(pushValue + " is not a correct value for the parameter " + pushParamName, "Invalid Door Parameter.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
#if RELEASE2013||RELEASE2014
                        Parameter pullParameter = doorInstance.get_Parameter(pullParamName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter pullParameter = doorInstance.LookupParameter(pullParamName);
#endif

                        if (null != pullParameter)
                        {
                            if(data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(pullParameter)))
                            {
                                string pullValue = pullParameter.AsValueString();
                                if (!pullValue.Contains("Approach"))
                                {
                                    AppCommand.Instance.IsDoorFail = true;
                                    AppCommand.Instance.FailingDoorId = doorId;
                                    DialogResult dr = MessageBox.Show(pullValue + " is not a correct value for the parameter " + pullParamName, "Invalid Door Parameter.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }

#if RELEASE2013||RELEASE2014
                        Parameter caParameter = doorInstance.get_Parameter(stateCAParamName);
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter caParameter = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParameter)
                        {
                            if (data.IsChangeTriggered(doorId, Element.GetChangeTypeParameter(caParameter)))
                            {
                                int caValue = caParameter.AsInteger();
                                int projectVal = Convert.ToInt32(projectSetup.IsStateCA);
                                if (caValue != projectVal)
                                {
                                    caParameter.Set(projectVal);
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
#elif RELEASE2015 || RELEASE2016 || RELEASE2017
                        Parameter caParameter = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParameter)
                        {
                            int caValue = caParameter.AsInteger();
                            int projectVal = Convert.ToInt32(projectSetup.IsStateCA);
                            if (caValue != projectVal)
                            {
                                caParameter.Set(projectVal);//default value 1
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute the door updater.\n" + ex.Message, "DoorUpdater:Execute", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        internal void UpdateParameter()
        {

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
