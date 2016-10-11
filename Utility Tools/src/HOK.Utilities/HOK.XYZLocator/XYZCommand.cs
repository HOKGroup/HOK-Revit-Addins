using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.XYZLocator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class XYZCommand:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        private string locationFamilyName = "XYZ_Location_HOK";
        private List<ElementId> instanceIds = new List<ElementId>();
        
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                bool found = FindLocatorFamily();
                if (found)
                {
                    MainWindow mainWindow = new MainWindow(m_app);
                    if (mainWindow.ShowDialog() == true)
                    {
                        XYZLocation xyzLocation = mainWindow.Location;
                        bool assigned = AssignParameter(xyzLocation);
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
                MessageBox.Show("Failed to run XYZ Locator.\n" + ex.Message, "XYZ Locator", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }

        private bool FindLocatorFamily()
        {
            bool found = false;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<FamilySymbol> symbols = collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).WhereElementIsElementType().ToElements().Cast<FamilySymbol>().ToList();
                var symbolFound = from symbol in symbols where symbol.Family.Name == locationFamilyName select symbol;
                if (symbolFound.Count() > 0)
                {
                    List<FamilySymbol> familySymbols = symbolFound.ToList();
                    List<ElementFilter> filters = new List<ElementFilter>();
                    foreach (FamilySymbol symbol in familySymbols)
                    {
                        FamilyInstanceFilter filter = new FamilyInstanceFilter(m_doc, symbol.Id);
                        filters.Add(filter);
                    }
                    LogicalOrFilter orFilter = new LogicalOrFilter(filters);

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
                string message = ex.Message;
            }
            return found;
        }

        private bool AssignParameter(XYZLocation xyzLocation)
        {
            bool assigned = false;
            using (TransactionGroup tg = new TransactionGroup(m_doc))
            {
                tg.Start("Assign Parameters");
                try
                {
                    foreach (ElementId eId in instanceIds)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Assign Parameter");
                            try
                            {
                                FamilyInstance instance = m_doc.GetElement(eId) as FamilyInstance;
                                if (null != instance)
                                {
                                    LocationPoint locationPt = instance.Location as LocationPoint;
                                    XYZ point = locationPt.Point;

                                    XYZ location1;
                                    XYZ location2;

                                    xyzLocation.GetTransformedValues(point, out location1, out location2);

                                    //assign parameters
                                    bool paramSet = SetParameterValue(instance, "XYZ_Location_1_Description", xyzLocation.Description1);
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
                                string messag = ex.Message;
                            }
                        }
                    }
                    tg.Assimilate();
                }
                catch (Exception ex)
                {
                    tg.RollBack();
                    string message = ex.Message;
                }
            }
            return assigned;
        }


        private bool SetParameterValue(FamilyInstance instance, string paramName, object value)
        {
            bool result = false;
            try
            {
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                Parameter param = instance.LookupParameter(paramName);
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
#else
                Parameter param = instance.get_Parameter(paramName);
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
#endif
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }
    }
}
