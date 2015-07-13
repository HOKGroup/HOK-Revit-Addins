using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HOK.ViewAnalysis
{
    public enum LEEDParameters
    {
        LEED_AreaWithViews, LEED_NonRegularyOccupied, LEED_IsExteriorWall
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Command:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        
        private string sharedParameterFileName = "Addins Shared Parameters.txt";

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;

            if (AddSharedParameters() == false)
            {
                MessageBox.Show("LEED Parameters cannot be found.\n" + LEEDParameters .LEED_AreaWithViews.ToString()+ "\n" + LEEDParameters.LEED_NonRegularyOccupied.ToString(), "LEED View Analysis - Shared Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (IsViewForAnalysis(m_doc.ActiveView))
            {
                //two modes: fast run for simple geometry, slow run for complex geometry
                UIDocument uidoc = m_app.ActiveUIDocument;
                IList<Reference> selectedElements = uidoc.Selection.PickObjects(ObjectType.Element, new RoomElementFilter(), "Select rooms to calculate the area with views. Click Finish on the options bar when you're done selecting rooms.");
                if (selectedElements.Count > 0)
                {
                    List<Room> selectedRooms = new List<Room>();
                    //ray tracing is only available in 3d views
                    foreach (Reference reference in selectedElements)
                    {
                        Room room = m_doc.GetElement(reference.ElementId) as Room;
                        if (null != room)
                        {
                            selectedRooms.Add(room);
                        }
                    }

                    if (selectedRooms.Count > 0)
                    {
                        MainWindow mainWindow = new MainWindow(m_app, selectedRooms);
                        if (mainWindow.ShowDialog() == true)
                        {
                            MessageBox.Show("View analysis has been successfully run.", "LEED EQc 8.2", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("A floor plan should be an active view for the view anlaysis.\n", "View Analysis - Active View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return Result.Succeeded;
        }

        private bool IsViewForAnalysis(Autodesk.Revit.DB.View activeView)
        {
            bool result = false;
            try
            {
                if (activeView.ViewType == ViewType.FloorPlan)
                {
                    AnalysisDisplayStyle displayStyle = null;

                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<AnalysisDisplayStyle> displayStyles = collector.OfClass(typeof(AnalysisDisplayStyle)).ToElements().Cast<AnalysisDisplayStyle>().ToList();
                    var viewAnlysisStyle = from style in displayStyles where style.Name == "View Analysis" select style;
                    if (viewAnlysisStyle.Count() > 0)
                    {
                        displayStyle = viewAnlysisStyle.First();
                    }
                    else
                    {
                        AnalysisDisplayColoredSurfaceSettings coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
                        coloredSurfaceSettings.ShowGridLines = false;

                        AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
                        Color yellow = new Color(255,255, 0);
                        Color blue = new Color(0, 128, 255);
                        List<AnalysisDisplayColorEntry> colorEntries = new List<AnalysisDisplayColorEntry>();
                        colorEntries.Add(new AnalysisDisplayColorEntry(blue));
                        colorSettings.SetIntermediateColors(colorEntries);

                        colorSettings.MinColor = blue;
                        colorSettings.MaxColor = yellow;
                        colorSettings.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.SolidColorRanges;

                        AnalysisDisplayLegendSettings legendSettings = new AnalysisDisplayLegendSettings();
                        legendSettings.NumberOfSteps = 1;
                        legendSettings.ShowDataDescription = false;
                        legendSettings.ShowLegend = false;

                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Create an Analysis Display Style");
                            try
                            {
                                displayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(m_doc, "View Analysis", coloredSurfaceSettings, colorSettings, legendSettings);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.RollBack();
                                MessageBox.Show("Failed to create an analysis display style.\n" + ex.Message, "Analysis Display Style", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }

                    if (null != displayStyle)
                    {
                        using (Transaction trans = new Transaction(m_doc))
                        {
                            trans.Start("Set Display Style");
                            try
                            {
                                m_doc.ActiveView.AnalysisDisplayStyleId = displayStyle.Id;
                                trans.Commit();
                                result = true;
                            }
                            catch (Exception ex)
                            {
                                string message = ex.Message;
                                trans.RollBack();
                                result = false;
                            }
                        }
                    }
                    else
                    {
                        result = false;
                    }

                }
                else
                {
                    result = false;
                    MessageBox.Show("A floor plan should be an active view for the view anlaysis.\n", "View Analysis - Active View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get information of the active view.\n"+ex.Message, "View Analysis - Active View", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool AddSharedParameters()
        {
            bool added = false;
            try
            {
                bool areaWithViewParamExist = false;
                bool nonRegularlyOccupiedParamExist = false;
                bool isExteriorWallExist = false;

                DefinitionBindingMapIterator iter = m_doc.ParameterBindings.ForwardIterator();
                while (iter.MoveNext())
                {
                    Definition definition = iter.Key;
                    ElementBinding elemBinding = (ElementBinding)iter.Current;
                    if (definition.Name == LEEDParameters.LEED_AreaWithViews.ToString())
                    {
                        areaWithViewParamExist = true;
                    }
                    else if (definition.Name == LEEDParameters.LEED_NonRegularyOccupied.ToString())
                    {
                        nonRegularlyOccupiedParamExist = true;
                    }
                    else if (definition.Name == LEEDParameters.LEED_IsExteriorWall.ToString())
                    {
                        isExteriorWallExist = true;
                    }
                }

                if (areaWithViewParamExist && nonRegularlyOccupiedParamExist && isExteriorWallExist)
                {
                    added = true;
                }
                else
                {
                    string currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string definitionPath = Path.GetDirectoryName(currentAssembly) + "/Resources/" + sharedParameterFileName;

                    string originalDefinitionFile = m_app.Application.SharedParametersFilename;

                    using (Transaction trans = new Transaction(m_doc))
                    {
                        trans.Start("Add Shared Parameters");
                        try
                        {
                            m_app.Application.SharedParametersFilename = definitionPath;
                            DefinitionFile definitionFile = m_app.Application.OpenSharedParameterFile();

                            CategorySet catSet = m_app.Application.Create.NewCategorySet();
                            Category category = m_doc.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms);
                            catSet.Insert(category);
                            InstanceBinding binding = m_app.Application.Create.NewInstanceBinding(catSet);

                            DefinitionGroup group = definitionFile.Groups.get_Item("HOK LEED");
                            if (!areaWithViewParamExist)
                            {
                                Definition definition = group.Definitions.get_Item(LEEDParameters.LEED_AreaWithViews.ToString());
                                if (null != definition)
                                {
                                    m_doc.ParameterBindings.Insert(definition, binding);
                                }
                            }
                            if (!nonRegularlyOccupiedParamExist)
                            {
                                Definition definition = group.Definitions.get_Item(LEEDParameters.LEED_NonRegularyOccupied.ToString());
                                if (null != definition)
                                {
                                    m_doc.ParameterBindings.Insert(definition, binding);
                                }
                            }
                            if (!isExteriorWallExist)
                            {
                                catSet = m_app.Application.Create.NewCategorySet();
                                category = m_doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
                                catSet.Insert(category);
                                binding = m_app.Application.Create.NewInstanceBinding(catSet);

                                Definition definition = group.Definitions.get_Item(LEEDParameters.LEED_IsExteriorWall.ToString());
                                if (null != definition)
                                {
                                    m_doc.ParameterBindings.Insert(definition, binding);
                                }
                            }
                            added = true;

                            //set to the original shared parameter file
                            m_app.Application.SharedParametersFilename = originalDefinitionFile;
                            definitionFile = m_app.Application.OpenSharedParameterFile();
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
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return added;
        }
    }

    public class RoomElementFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (null != elem.Category)
            {
                if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Rooms)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
