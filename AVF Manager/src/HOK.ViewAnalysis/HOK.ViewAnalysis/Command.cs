using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.ViewAnalysis
{
    public enum LEEDParameters
    {
        LEED_AreaWithViews,
        LEED_NonRegularyOccupied,
        LEED_IsExteriorWall
    }

    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        private const string sharedParameterFileName = "Addins Shared Parameters.txt";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            if (AddSharedParameters() == false)
            {
                MessageBox.Show("LEED Parameters cannot be found.\n" + LEEDParameters .LEED_AreaWithViews + "\n" + LEEDParameters.LEED_NonRegularyOccupied, 
                    "LEED View Analysis - Shared Parameters", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }

            if (IsViewForAnalysis(m_doc.ActiveView))
            {
                //two modes: fast run for simple geometry, slow run for complex geometry
                var uidoc = m_app.ActiveUIDocument;

                var result = MessageBox.Show("Start selecting rooms to calculate area with views for LEED EQc 8.2.", 
                    "LEED EQc 8.2 - View Analysis",
                    MessageBoxButtons.OKCancel, 
                    MessageBoxIcon.Information);

                if (result != DialogResult.OK) return Result.Succeeded;

                var selectedElements = uidoc.Selection.PickObjects(ObjectType.Element, 
                    new RoomElementFilter(), 
                    "Select rooms to calculate the area with views. Click Finish on the options bar when you're done selecting rooms.");

                if (!selectedElements.Any()) return Result.Succeeded;

                var selectedRooms = new List<Room>();
                //ray tracing is only available in 3d views
                foreach (var reference in selectedElements)
                {
                    var room = m_doc.GetElement(reference.ElementId) as Room;
                    if (null != room)
                    {
                        selectedRooms.Add(room);
                    }
                }

                if (!selectedRooms.Any()) return Result.Succeeded;

                var mainWindow = new MainWindow(m_app, selectedRooms);
                if (mainWindow.ShowDialog() == true)
                {
                    MessageBox.Show("View analysis has been successfully run.", 
                        "LEED EQc 8.2", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("A floor plan should be an active view for the view anlaysis.\n", 
                    "View Analysis - Active View", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            var addinInfo = new AddinLog
            {
                pluginName = "ViewAnalysis-LEED Analysis",
                user = Environment.UserName,
                revitVersion = BasicFileInfo.Extract(m_doc.PathName).SavedInVersion
            };
            AddinUtilities.PublishAddinLog(addinInfo);

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        private bool IsViewForAnalysis(Autodesk.Revit.DB.View activeView)
        {
            var result = false;
            try
            {
                if (activeView.ViewType == ViewType.FloorPlan)
                {
                    AnalysisDisplayStyle displayStyle = null;

                    var collector = new FilteredElementCollector(m_doc);
                    var displayStyles = collector.OfClass(typeof(AnalysisDisplayStyle)).ToElements().Cast<AnalysisDisplayStyle>().ToList();
                    var viewAnlysisStyle = from style in displayStyles where style.Name == "View Analysis" select style;
                    if (viewAnlysisStyle.Any())
                    {
                        displayStyle = viewAnlysisStyle.First();
                    }
                    else
                    {
                        var coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
                        coloredSurfaceSettings.ShowGridLines = false;

                        var colorSettings = new AnalysisDisplayColorSettings();
                        var yellow = new Color(255,255, 0);
                        var blue = new Color(0, 128, 255);
                        var colorEntries = new List<AnalysisDisplayColorEntry> {new AnalysisDisplayColorEntry(blue)};
                        colorSettings.SetIntermediateColors(colorEntries);

                        colorSettings.MinColor = blue;
                        colorSettings.MaxColor = yellow;
                        colorSettings.ColorSettingsType = AnalysisDisplayStyleColorSettingsType.SolidColorRanges;

                        var legendSettings = new AnalysisDisplayLegendSettings
                        {
                            NumberOfSteps = 1,
                            ShowDataDescription = false,
                            ShowLegend = false
                        };

                        using (var trans = new Transaction(m_doc))
                        {
                            trans.Start("Create an Analysis Display Style");
                            try
                            {
                                displayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(m_doc, "View Analysis", coloredSurfaceSettings, colorSettings, legendSettings);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                trans.RollBack();
                            }
                        }
                    }

                    if (null != displayStyle)
                    {
                        using (var trans = new Transaction(m_doc))
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
                                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
            var added = false;
            try
            {
                var areaWithViewParamExist = false;
                var nonRegularlyOccupiedParamExist = false;
                var isExteriorWallExist = false;

                var iter = m_doc.ParameterBindings.ForwardIterator();
                while (iter.MoveNext())
                {
                    var definition = iter.Key;
                    var elemBinding = (ElementBinding)iter.Current;
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
                    var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var definitionPath = Path.GetDirectoryName(currentAssembly) + "/Resources/" + sharedParameterFileName;

                    var originalDefinitionFile = m_app.Application.SharedParametersFilename;

                    using (var trans = new Transaction(m_doc))
                    {
                        trans.Start("Add Shared Parameters");
                        try
                        {
                            m_app.Application.SharedParametersFilename = definitionPath;
                            var definitionFile = m_app.Application.OpenSharedParameterFile();

                            var catSet = m_app.Application.Create.NewCategorySet();
                            var category = m_doc.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms);
                            catSet.Insert(category);
                            var binding = m_app.Application.Create.NewInstanceBinding(catSet);

                            var group = definitionFile.Groups.get_Item("HOK LEED");
                            if (!areaWithViewParamExist)
                            {
                                var definition = group.Definitions.get_Item(LEEDParameters.LEED_AreaWithViews.ToString());
                                if (null != definition)
                                {
                                    m_doc.ParameterBindings.Insert(definition, binding);
                                }
                            }
                            if (!nonRegularlyOccupiedParamExist)
                            {
                                var definition = group.Definitions.get_Item(LEEDParameters.LEED_NonRegularyOccupied.ToString());
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

                                var definition = group.Definitions.get_Item(LEEDParameters.LEED_IsExteriorWall.ToString());
                                if (null != definition)
                                {
                                    m_doc.ParameterBindings.Insert(definition, binding);
                                }
                            }
                            added = true;

                            //set to the original shared parameter file
                            m_app.Application.SharedParametersFilename = originalDefinitionFile;
                            m_app.Application.OpenSharedParameterFile();
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                            trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return added;
        }
    }
}
