using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.DoorMonitor
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class SettingCommand:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        private string pullParamName = "Approach @ Pull Side";
        private string pushParamName = "Approach @ Push Side";
        private string stateCAParamName = "StateCA";

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;


            try
            {
                MonitorProjectSetup projectSetup = ProjectSetupDataStroageUtil.GetProjectSetup(m_doc);
                SettingWindow settingWindow = new SettingWindow(m_doc, projectSetup);
                if (settingWindow.ShowDialog() == true)
                {
                    projectSetup = settingWindow.ProjectSetup;
                    settingWindow.Close();

                    if (projectSetup.IsMonitorOn)
                    {
                        List<MonitorMessage> errorMessages = VerifyDoorParameters(projectSetup);
                        if (errorMessages.Count > 0)
                        {
                            MessageWindow msgWindow = new MessageWindow(errorMessages);
                            if (msgWindow.ShowDialog() == true)
                            {
                                List<MonitorMessage> selectedMessages = msgWindow.SelectedItems;
                                msgWindow.Close();
                                ShowElements(selectedMessages);
                            }
                        }
                    }

                    AppCommand.Instance.RefreshMonitorSetup(m_doc, projectSetup);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to execute Setting Commands.\n"+ex.Message, "Setting Command", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            return Result.Succeeded;
        }

        private List<MonitorMessage> VerifyDoorParameters(MonitorProjectSetup projectSetup)
        {
            List<MonitorMessage> warningMessages = new List<MonitorMessage>();
            try
            {
                using (TransactionGroup transGroup = new TransactionGroup(m_doc))
                {
                    transGroup.Start("Verify Doors");
                    ElementFilter catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<FamilyInstance> doorInstances = collector.WherePasses(catFilter).WhereElementIsNotElementType().Cast<FamilyInstance>().ToList();

                    foreach (FamilyInstance doorInstance in doorInstances)
                    {

#if RELEASE2013 || RELEASE2014
                    Parameter pullParam = doorInstance.get_Parameter(pullParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter pullParam = doorInstance.LookupParameter(pullParamName);
#endif
                        if (null != pullParam)
                        {
                            string pullValue = pullParam.AsValueString();
                            if (!pullValue.Contains("Approach"))
                            {
                                MonitorMessage message = new MonitorMessage(doorInstance, pullParamName, "Incorrect parameter value has been set.");
                                warningMessages.Add(message);
                            }
                        }


#if RELEASE2013||RELEASE2014
                    Parameter pushParam = doorInstance.get_Parameter(pushParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter pushParam = doorInstance.LookupParameter(pushParamName);
#endif
                        if (null != pushParam)
                        {
                            string pushValue = pushParam.AsValueString();
                            if (!pushValue.Contains("Approach"))
                            {
                                MonitorMessage message = new MonitorMessage(doorInstance, pushParamName, "Incorrect parameter value has been set.");
                                warningMessages.Add(message);
                            }
                        }

#if RELEASE2013||RELEASE2014
                            Parameter caParam = doorInstance.get_Parameter(stateCAParamName);
#elif RELEASE2015 || RELEASE2016
                        Parameter caParam = doorInstance.LookupParameter(stateCAParamName);
#endif
                        if (null != caParam)
                        {
                            int caVal = caParam.AsInteger();
                            int projectVal = Convert.ToInt32(projectSetup.IsStateCA);
                            if (caVal != projectVal)
                            {
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Set State Param");
                                    try
                                    {
                                        caParam.Set(projectVal);
                                        MonitorMessage message = new MonitorMessage(doorInstance, stateCAParamName, "Parameter Value has been changed. (solved) ");
                                        warningMessages.Add(message);
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        string message = ex.Message;
                                        trans.RollBack();
                                    }
                                }
                            }
                        }
                    }
                    transGroup.Assimilate();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to verify door parameters.\n"+ex.Message, "Verify Door Parameters", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return warningMessages;
        }

        public void ShowElements(List<MonitorMessage> selectedMessages)
        {
            try
            {
                UIDocument uidoc = m_app.ActiveUIDocument;
                List<ElementId> selectedIds = new List<ElementId>();

#if RELEASE2013||RELEASE2014
                SelElementSet selElements = SelElementSet.Create();
                foreach (MonitorMessage message in selectedMessages)
                {
                    selElements.Add(message.ElementObj);
                    selectedIds.Add(message.ElementObj.Id);
                }
                uidoc.Selection.Elements = selElements;
                uidoc.ShowElements(selectedIds);
#elif RELEASE2015 || RELEASE2016
                foreach (MonitorMessage message in selectedMessages)
                {
                    selectedIds.Add(message.ElementObj.Id);
                }
                uidoc.Selection.SetElementIds(selectedIds);
                uidoc.ShowElements(selectedIds);
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to show elements.\n"+ex.Message, "Show Elements", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
