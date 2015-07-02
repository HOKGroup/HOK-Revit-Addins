using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace HOK.LPDCalculator.Schedule
{
    public class ScheduleDataManager
    {
        private UIApplication m_app;
        private Document m_doc;
        private CalculationTypes calculationType;
        private ViewSchedule selectedSchedule;
        private string extension = ".txt";
        private string txtPath = "";
        private DataTable scheduleData = null;
        private string areaUnit = "SF";
        private string lpdUnit = "W/ft²";
        private string loadUnit = "VA";
        private Dictionary<string/*fieldName*/, FieldProperties> fieldDictionary = new Dictionary<string, FieldProperties>();

        private string[] buildingAreaFields = new string[] { "ASHRAE LPD Category (BAM)", "ASHRAE Allowable LPD_BAM", "25% Below ASHRAE_BAM", "ActualLightingLoad", "Area" };
        private string[] spaceBySpaceFields = new string[] { "AllowableLightingLoad ", "ActualLightingLoad", "Area" };

        public ViewSchedule SelectedSchedule { get { return selectedSchedule; } set { selectedSchedule = value; } }
        public string TextFilePath { get { return txtPath; } set { txtPath = value; } }
        public DataTable ScheduleData { get { return scheduleData; } set { scheduleData = value; } }
        
        public ScheduleDataManager(UIApplication uiapp, CalculationTypes calType)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            calculationType = calType;
            selectedSchedule = FindViewSchedule(calculationType);
            if (null != selectedSchedule)
            {
                if (ValidateViewSchedule(selectedSchedule))
                {
                    if (ExportViewSchedule(selectedSchedule))
                    {
                        scheduleData = GetScheduleDataTable();
                        RestoreFields(selectedSchedule);
                    }
                }
            }
        }

        private ViewSchedule FindViewSchedule(CalculationTypes calType)
        {
            ViewSchedule viewSchedule=null;
            try
            {
                string viewName = "";
                if (calType == CalculationTypes.BuildingArea)
                {
                    viewName = "LPD (BUILDING AREA METHOD)";
                }
                else if (calType == CalculationTypes.SpaceBySpace)
                {
                    viewName = "LPD (SPACE BY SPACE METHOD)";
                }

                FilteredElementCollector collector = new FilteredElementCollector(m_doc).OfClass(typeof(ViewSchedule));
                foreach (ViewSchedule vs in collector)
                {
                    if (vs.ViewName == viewName)
                    {
                        viewSchedule = vs;
                        break;
                    }
                }

                return viewSchedule;
            }
            catch
            {
                MessageBox.Show("LPD schedule was not found.", "Not Found Schedule", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return viewSchedule;
            }
        }

        private bool ExportViewSchedule(ViewSchedule viewSchedule)
        {
            bool result = false;
            try
            {
                ViewScheduleExportOptions options = new ViewScheduleExportOptions();
                options.ColumnHeaders = ExportColumnHeaders.OneRow;
#if RELEASE2014 || RELEASE2015 || RELEASE2016
                options.Title = false;
#endif

                string centralPath = GetCentralPath(m_doc);
                if (!string.IsNullOrEmpty(centralPath))
                {
                    string directoryName = Path.GetDirectoryName(centralPath);
                    using (Transaction trans = new Transaction(m_doc))
                    {
                        try
                        {
                            trans.Start("Export Schedule");
                            txtPath = Path.Combine(directoryName, viewSchedule.Name + extension);
                            if (File.Exists(txtPath))
                            {
                                File.Delete(txtPath);
                            }
                            viewSchedule.Export(directoryName, viewSchedule.Name + extension, options);
                            trans.Commit();
                        }
                        catch { trans.RollBack(); }
                    }
                }

                if (File.Exists(txtPath))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(viewSchedule.Name+": Failed to export the selected schedule.\n"+ex.Message, "Export Schedule", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private bool ValidateViewSchedule(ViewSchedule viewSchedule)
        {
            bool result = false;
            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Check Validation of Schedule Definition");
                try
                {
                    string[] requiredFields = new string[] { };
                    ScheduleDefinition definition = viewSchedule.Definition;
                    if (null != definition)
                    {
                        if (calculationType == CalculationTypes.BuildingArea)
                        {
                            definition.IsItemized = false;
                            definition.ShowGrandTotal = true;
                            requiredFields = buildingAreaFields;
                        }
                        else if (calculationType == CalculationTypes.SpaceBySpace)
                        {
                            definition.IsItemized = true;
                            definition.ShowGrandTotal = true;
                            requiredFields = spaceBySpaceFields;
                        }

                        int count = definition.GetFieldCount();
                        for (int i = 0; i < count; i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            string fieldName = field.GetName();
                            if (!fieldDictionary.ContainsKey(fieldName))
                            {
                                FieldProperties fp = new FieldProperties();
                                fp.ColumnHeading = field.ColumnHeading;
                                fp.HasTotals = field.HasTotals;
                                fp.IsHidden = field.IsHidden;
                                fieldDictionary.Add(fieldName, fp);
                            }
                           
                            if (requiredFields.Contains(fieldName))
                            {
                                if (fieldName.Contains("ASHRAE Allowable LPD") || fieldName.Contains("25% Below ASHRAE"))
                                {
                                    continue;
                                }
                                if (field.CanTotal()) { field.HasTotals = true; }
                                field.IsHidden = false;
                            }
                        }
                    }
                    trans.Commit();
                    result = true;
                }
                catch
                {
                    trans.RollBack();
                    result = false;
                }
            }
            return result;
        }

        private void RestoreFields(ViewSchedule viewSchedule)
        {
            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Restore Fields");
                try
                {
                    ScheduleDefinition definition = viewSchedule.Definition;
                    int count = definition.GetFieldCount();
                    for (int i = 0; i < count; i++)
                    {
                        ScheduleField field = definition.GetField(i);
                        string fieldName = field.GetName();
                        if (fieldDictionary.ContainsKey(fieldName))
                        {
                            FieldProperties originalField = fieldDictionary[fieldName];
                            if (field.CanTotal()) { field.HasTotals = originalField.HasTotals; }
                            field.IsHidden = originalField.IsHidden;
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    MessageBox.Show("Failed to restore fields settings.\n" + ex.Message, "Restore Schedule Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private DataTable GetScheduleDataTable()
        {
            DataTable dataTable = null;
            try
            {
                ScheduleDataParser parser = new ScheduleDataParser(txtPath);
                dataTable = parser.Table;
            }
            catch
            {
                MessageBox.Show("Failed to get the data table from the exported schedule.\n"+txtPath, "Schedule Data Table", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return dataTable;
        }

        private string GetCentralPath(Document doc)
        {
            string docCentralPath = "";
            try
            {
                if (doc.IsWorkshared)
                {
                    ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                    string centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        docCentralPath = centralPath;
                    }
                    else
                    {
                        //detached
                        docCentralPath = doc.PathName;
                    }
                }
                else
                {
                    docCentralPath = doc.PathName;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                docCentralPath = doc.PathName;
            }
            return docCentralPath;
        }

        private string GetHeaderText(string fieldName)
        {
            string header = "";
            if (fieldDictionary.ContainsKey(fieldName))
            {
                header = fieldDictionary[fieldName].ColumnHeading;
            }
            return header;
        }

        public double GetAllowableLightingLoad()
        {
            double allowableLoad = 0;
            try
            {
                string headerText = GetHeaderText("AllowableLightingLoad");
                string strValue = scheduleData.Rows[scheduleData.Rows.Count - 1][headerText].ToString();
                strValue = strValue.Replace(loadUnit, "");
                double.TryParse(strValue, out allowableLoad);
            }
            catch { }
            return allowableLoad;
        }

        public string GetLPDCategoryBAM()
        {
            string categoryName = "";
            try
            {
                string headerText = GetHeaderText("ASHRAE LPD Category (BAM)");
                categoryName = scheduleData.Rows[scheduleData.Rows.Count - 2][headerText].ToString();
            }
            catch { }
            return categoryName;
        }

        public double GetAllowableLPDBAM()
        {
            double allowableLPD = 0;
            try
            {
                string headerText = GetHeaderText("ASHRAE Allowable LPD_BAM");
                string strValue = scheduleData.Rows[scheduleData.Rows.Count - 2][headerText].ToString();
                strValue = strValue.Replace(lpdUnit, "");
                double.TryParse(strValue, out allowableLPD);
            }
            catch { }
            return allowableLPD;
        }

        public double GetTargetLPDBAM()
        {
            double allowableLPD = 0;
            try
            {
                string headerText = GetHeaderText("25% Below ASHRAE_BAM");
                string strValue = scheduleData.Rows[scheduleData.Rows.Count - 2][headerText].ToString();
                strValue = strValue.Replace(lpdUnit, "");
                double.TryParse(strValue, out allowableLPD);
            }
            catch { }
            return allowableLPD;
        }

        public double GetActualLightingLoad()
        {
            double actualLightingLoad = 0;
            try
            {
                string headerText = GetHeaderText("ActualLightingLoad");
                string strValue = scheduleData.Rows[scheduleData.Rows.Count - 1][headerText].ToString();
                strValue = strValue.Replace(loadUnit, "");
                double.TryParse(strValue, out actualLightingLoad);
            }
            catch { }
            return actualLightingLoad;
        }

        public double GetArea()
        {
            double area = 0;
            try
            {
                string headerText = GetHeaderText("Area");
                string areaValue = scheduleData.Rows[scheduleData.Rows.Count - 1][headerText].ToString();
                areaValue = areaValue.Replace(areaUnit, "");
                double.TryParse(areaValue, out area);
            }
            catch { }
            return area;
        }

        public double GetSavings()
        {
            double savings = 0;
            try
            {
                string headerText = GetHeaderText("Savings/Overage");
                string strValue = scheduleData.Rows[scheduleData.Rows.Count - 1][headerText].ToString();
                strValue = strValue.Replace(loadUnit, "");
                double.TryParse(strValue, out savings);
            }
            catch { }
            return savings;
        }

    }

    public class FieldProperties
    {
        public string ColumnHeading{get;set;}
        public bool IsHidden{get;set;}
        public bool HasTotals { get; set; }
    }
}


