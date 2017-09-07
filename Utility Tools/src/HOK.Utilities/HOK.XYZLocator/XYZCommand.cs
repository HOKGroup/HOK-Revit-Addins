using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.XYZLocator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class XYZCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        private const string locationFamilyName = "XYZ_Location_HOK";
        private List<ElementId> instanceIds = new List<ElementId>();
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-WorksetView", commandData.Application.Application.VersionNumber));

                var found = FindLocatorFamily();
                if (found)
                {
                    var mainWindow = new MainWindow(m_app);
                    if (mainWindow.ShowDialog() == true)
                    {
                        var xyzLocation = mainWindow.Location;
                        var assigned = AssignParameter(xyzLocation);
                        if (assigned)
                        {
                            MessageBox.Show("Parameter values for XYZ locaiton have been assigned.", "XYZ Locator", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please load a Generic Model family, " + locationFamilyName, "Family Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        private bool FindLocatorFamily()
        {
            var found = false;
            try
            {
                var collector = new FilteredElementCollector(m_doc);
                var symbols = collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsElementType().ToElements().Cast<FamilySymbol>().ToList();
                var symbolFound = from symbol in symbols where symbol.Family.Name == locationFamilyName select symbol;
                if (symbolFound.Any())
                {
                    var familySymbols = symbolFound.ToList();
                    var filters = new List<ElementFilter>();
                    foreach (var symbol in familySymbols)
                    {
                        var filter = new FamilyInstanceFilter(m_doc, symbol.Id);
                        filters.Add(filter);
                    }
                    var orFilter = new LogicalOrFilter(filters);

                    collector = new FilteredElementCollector(m_doc);
                    instanceIds = collector.OfClass(typeof(FamilyInstance)).WherePasses(orFilter).ToElementIds().ToList();
                    if (instanceIds.Count > 0)
                    {
                        found = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return found;
        }

        private bool AssignParameter(XYZLocation xyzLocation)
        {
            var assigned = false;
            using (var tg = new TransactionGroup(m_doc))
            {
                tg.Start("Assign Parameters");
                try
                {
                    foreach (var eId in instanceIds)
                    {
                        using (var trans = new Transaction(m_doc))
                        {
                            trans.Start("Assign Parameter");
                            try
                            {
                                var instance = m_doc.GetElement(eId) as FamilyInstance;
                                if (null != instance)
                                {
                                    var locationPt = instance.Location as LocationPoint;
                                    var point = locationPt.Point;

                                    XYZ location1;
                                    XYZ location2;

                                    xyzLocation.GetTransformedValues(point, out location1, out location2);

                                    //assign parameters
                                    var paramSet = SetParameterValue(instance, "XYZ_Location_1_Description", xyzLocation.Description1);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_1_X", location1.X);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_1_Y", location1.Y);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_1_Z", location1.Z);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_2_Description", xyzLocation.Description2);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_2_X", location2.X);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_2_Y", location2.Y);
                                    paramSet = SetParameterValue(instance, "XYZ_Location_2_Z", location2.Z);
                                }
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                var messag = ex.Message;
                            }
                        }
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    var message = ex.Message;
                }
            }
            return assigned;
        }


        private bool SetParameterValue(FamilyInstance instance, string paramName, object value)
        {
            var result = false;
            try
            {
                var param = instance.LookupParameter(paramName);
                if (null != param)
                {
                    if (param.StorageType == StorageType.Double)
                    {
                        param.Set((double)value);
                        result = true;
                    }
                    else if (param.StorageType == StorageType.String)
                    {
                        param.Set((string)value);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return result;
        }
    }
}
