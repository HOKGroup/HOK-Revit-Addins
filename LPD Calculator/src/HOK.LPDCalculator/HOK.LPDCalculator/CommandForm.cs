using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;


namespace HOK.LPDCalculator
{
    public enum ModelSelection { Both = 0, Host = 1, Link = 2 }

    public partial class CommandForm : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document m_doc;
        private FamilySymbol spaceAnnotation = null;
        private ModelSelection spaceModel;

        public FamilySymbol SpaceAnnotationSymbol { get { return spaceAnnotation; } set { spaceAnnotation = value; } }
        public ModelSelection SpaceModelSelection { get { return spaceModel; } set { spaceModel = value; } }

        public CommandForm(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            Text = "LPD Analysis - v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            progressBar.Visible = false;
            statusLabel.Text = "Ready";
        }

        private void bttnBuilding_Click(object sender, EventArgs e)
        {
            try
            {
                var selection = GetModelSelection();

                var collector = new FilteredElementCollector(m_doc);
                var area = collector.OfCategory(BuiltInCategory.OST_Areas).ToElements().ToList().First() as Area;

#if RELEASE2022 || RELEASE2023 || RELEASE2024
                var param1Found = FindParameter(area, "ActualLightingLoad", SpecTypeId.ApparentPower);
                var param2Found = FindParameter(area, "ActualLPD", SpecTypeId.ElectricalPowerDensity);
#else
                var param1Found = FindParameter(area, "ActualLightingLoad", ParameterType.ElectricalApparentPower);
                var param2Found = FindParameter(area, "ActualLPD", ParameterType.ElectricalPowerDensity);
#endif

                if (param1Found && param2Found)
                {
                    FamilySymbol annotationType = null;
                    var annotationFamilyName = "LPD_BuildingAreaMethod_HOK_I";
                    var annotationFound = FindAnnotationFamily(m_doc, annotationFamilyName, out annotationType);
                    if (annotationFound && null != annotationType)
                    {
                        var areaElements = FindAreasOnLPDAreaSchemes("LPD Building");
                        if (areaElements.Count > 0)
                        {
                            progressBar.Visible = true;
                            statusLabel.Text = "Calculating ...";
                            var calculator = new CalculatorMethods(m_app, areaElements, annotationType, selection);
                            if (calculator.CalculateLPD(progressBar))
                            {
                                if (calculator.UpdateBuildingAnnotationFamily())
                                {
                                    MessageBox.Show("Lighting Power Density was successfully calculated based on Area elements.\n Please review the schedule and annotation family.", annotationFamilyName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    Close();
                                }
                            }
                            else
                            {
                                progressBar.Visible = false;
                                statusLabel.Text = "";
                            }
                        }
                        else
                        {
                            MessageBox.Show("Areas on LPD Building scheme cannot be found.", "Area Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please load a standard LPD annotation family.\n" + annotationFamilyName, "Annotation Symbol Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    var strBuilder = new StringBuilder();
                    strBuilder.AppendLine("The following parameters are required in Area elements. Please create those two parameters to run the LPD calculator.\n");
                    strBuilder.AppendLine("1. Parameter Name: ActualLightingLoad");
                    strBuilder.AppendLine("Parameter Type: Apparent Power");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("2. Parameter Name: ActualLPD");
                    strBuilder.AppendLine("Parameter Type: Power Density");
                    MessageBox.Show(strBuilder.ToString(), "Parameter Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run Building Area method.\n" + ex.Message, "Building Area Method", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnSpace_Click(object sender, EventArgs e)
        {
            try
            {

                var selection = GetModelSelection();

                var collector = new FilteredElementCollector(m_doc);
                var area = collector.OfCategory(BuiltInCategory.OST_Areas).ToElements().ToList().First() as Area;

#if RELEASE2022 || RELEASE2023 || RELEASE2024
                var param1Found = FindParameter(area, "ActualLightingLoad", SpecTypeId.ApparentPower);
                var param2Found = FindParameter(area, "ActualLPD", SpecTypeId.ElectricalPowerDensity);
#else
                var param1Found = FindParameter(area, "ActualLightingLoad", ParameterType.ElectricalApparentPower);
                var param2Found = FindParameter(area, "ActualLPD", ParameterType.ElectricalPowerDensity);
#endif

                if (param1Found && param2Found)
                {
                    FamilySymbol annotationType = null;
                    var annotationFamilyName = "LPD_SpaceBySpaceMethod_HOK_I";
                    var annotationFound = FindAnnotationFamily(m_doc, annotationFamilyName, out annotationType);
                    if (annotationFound && null != annotationType)
                    {
                        var areaElements = FindAreasOnLPDAreaSchemes("LPD Space");
                        if (areaElements.Count > 0)
                        {
                            progressBar.Visible = true;
                            statusLabel.Text = "Calculating ...";
                            var calculator = new CalculatorMethods(m_app, areaElements, annotationType, selection);
                            if (calculator.CalculateLPD(progressBar))
                            {
                                if (calculator.UpdateSpaceAnnotationFamily())
                                {
                                    statusLabel.Text = "Completed.";
                                    MessageBox.Show("Lighting Power Density was successfully calculated based on Area elements.\n Please review the schedule and annotation family.", annotationFamilyName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    Close();
                                }
                            }
                            else
                            {
                                progressBar.Visible = false;
                                statusLabel.Text = "";
                            }
                        }
                        else
                        {
                            MessageBox.Show("Areas on LPD Space scheme cannot be found.", "Area Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please load a standard LPD annotation family.\n" + annotationFamilyName, "Annotation Symbol Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    var strBuilder = new StringBuilder();
                    strBuilder.AppendLine("The following parameters are required in Area elements. Please create those two parameters to run the LPD calculator.\n");
                    strBuilder.AppendLine("1. Parameter Name: ActualLightingLoad");
                    strBuilder.AppendLine("Parameter Type: Apparent Power");
                    strBuilder.AppendLine("");
                    strBuilder.AppendLine("2. Parameter Name: ActualLPD");
                    strBuilder.AppendLine("Parameter Type: Power Density");
                    MessageBox.Show(strBuilder.ToString(), "Parameter Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run Space by Space method.\n" + ex.Message, "Space by Space Method", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<Element> FindAreasOnLPDAreaSchemes(string schemeName)
        {
            var areasOnLPD = new List<Element>();
            try
            {
                var collector = new FilteredElementCollector(m_doc);
                var viewPlans = collector.OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().ToElements().Cast<ViewPlan>().ToList();
                var areaPlans = from view in viewPlans where view.ViewType == ViewType.AreaPlan select view;

                var lpdAreaPlans = new List<ViewPlan>();
                foreach (var viewPlan in areaPlans)
                {
                    if (null != viewPlan.AreaScheme)
                    {
                        if (viewPlan.AreaScheme.Name == schemeName)
                        {
                            lpdAreaPlans.Add(viewPlan);
                        }
                    }
                }

                if (lpdAreaPlans.Count > 0)
                {
                    foreach (var areaPlan in lpdAreaPlans)
                    {
                        var areaCollector = new FilteredElementCollector(m_doc, areaPlan.Id);
                        var areas = areaCollector.OfCategory(BuiltInCategory.OST_Areas).ToElements().ToList();
                        areasOnLPD.AddRange(areas);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(schemeName + ": Failed to find areas on LPD schemes.\n" + ex.Message, "Areas on LPD scheme Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return areasOnLPD;
        }

        private ModelSelection GetModelSelection()
        {
            var selection = ModelSelection.Both;

            if (radioButtonBoth.Checked)
            {
                selection = ModelSelection.Both;
            }
            else if (radioButtonHost.Checked)
            {
                selection = ModelSelection.Host;
            }
            else if (radioButtonLink.Checked)
            {
                selection = ModelSelection.Link;
            }

            return selection;
        }

#if RELEASE2022 || RELEASE2023 || RELEASE2024
        private bool FindParameter(Area area, string paramName, ForgeTypeId paramType)
        {
#else
        private bool FindParameter(Area area, string paramName, ParameterType paramType)
        {
#endif
            var result = false;
            try
            {
                var param = area.LookupParameter(paramName);

                if (null != param)
                {
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                    if (param.Definition.GetDataType() == paramType)
#else
                    if (param.Definition.ParameterType == paramType)
#endif
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find parameters.\n" + ex.Message, "Find Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

#if RELEASE2022 || RELEASE2023 || RELEASE2024
        private bool FindParameter(Room room, string paramName, ForgeTypeId paramType)
        {
#else
        private bool FindParameter(Room room, string paramName, ParameterType paramType)
        {
#endif
            var result = false;
            try
            {
                var param = room.LookupParameter(paramName);

                if (null != param)
                {
#if RELEASE2022 || RELEASE2023 || RELEASE2024
                    if (param.Definition.GetDataType() == paramType)
#else
                    if (param.Definition.ParameterType == paramType)
#endif
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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find parameters.\n" + ex.Message, "Find Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private bool FindAnnotationFamily(Document doc, string familyName, out FamilySymbol symbolType)
        {
            var found = false;
            symbolType = null;
            try
            {
                var collector = new FilteredElementCollector(doc);
                var annoSymbolTypes = collector.OfClass(typeof(FamilySymbol)).ToElements().Cast<FamilySymbol>().ToList();
                var symbolTypes = from annoType in annoSymbolTypes where annoType.Name.Contains(familyName) select annoType;
                if (symbolTypes.Count() > 0)
                {
                    symbolType = symbolTypes.First();
                    found = true;
                }
            }
            catch { }
            return found;
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var helpPath = @"V:\RVT-Data\HOK Program\Documentation\LPD Calculator_Instruction.pdf";
                System.Diagnostics.Process.Start(helpPath);
            }
            catch
            {
                MessageBox.Show("Help file cannot be found.", "Hel File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


    }
}
