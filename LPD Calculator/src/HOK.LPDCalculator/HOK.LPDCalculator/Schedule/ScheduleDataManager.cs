using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.LPDCalculator.Schedule
{
    public class ScheduleDataManager
    {
        private readonly UIApplication m_app;
        private readonly Document m_doc;
        private readonly CalculationTypes calculationType;
        private const string extension = ".txt";
        private const string areaUnit = "SF";
        private const string lpdUnit = "W/ft²";
        private const string loadUnit = "VA";
        private readonly Dictionary<string/*fieldName*/, FieldProperties> fieldDictionary = new Dictionary<string, FieldProperties>();
        private readonly string[] buildingAreaFields = { "ASHRAE LPD Category (BAM)", "ASHRAE Allowable LPD_BAM", "25% Below ASHRAE_BAM", "ActualLightingLoad", "Area" };
        private readonly string[] spaceBySpaceFields = { "AllowableLightingLoad ", "ActualLightingLoad", "Area" };
        public ViewSchedule SelectedSchedule { get; set; }
        public string TextFilePath { get; set; } = "";
        public DataTable ScheduleData { get; set; }

        public ScheduleDataManager(UIApplication uiapp, CalculationTypes calType)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            calculationType = calType;
            SelectedSchedule = FindViewSchedule(calculationType);
            if (null == SelectedSchedule) return;
            if (!ValidateViewSchedule(SelectedSchedule)) return;
            if (!ExportViewSchedule(SelectedSchedule)) return;

            ScheduleData = GetScheduleDataTable();
            RestoreFields(SelectedSchedule);
        }

        private ViewSchedule FindViewSchedule(CalculationTypes calType)
        {
            ViewSchedule viewSchedule=null;
            try
            {
                var viewName = "";
                switch (calType)
                {
                    case CalculationTypes.BuildingArea:
                        viewName = "LPD (BUILDING AREA METHOD)";
                        break;
                    case CalculationTypes.SpaceBySpace:
                        viewName = "LPD (SPACE BY SPACE METHOD)";
                        break;
                }

                var collector = new FilteredElementCollector(m_doc).OfClass(typeof(ViewSchedule));
                foreach (var element in collector)
                {
                    var vs = (ViewSchedule) element;
                    if (vs.ViewName != viewName) continue;

                    viewSchedule = vs;
                    break;
                }

                return viewSchedule;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return viewSchedule;
            }
        }

        private bool ExportViewSchedule(ViewSchedule viewSchedule)
        {
            var result = false;
            try
            {
                var options = new ViewScheduleExportOptions
                {
                    ColumnHeaders = ExportColumnHeaders.OneRow,
                    Title = false
                };

                var centralPath = GetCentralPath(m_doc);
                if (!string.IsNullOrEmpty(centralPath))
                {
                    var directoryName = Path.GetDirectoryName(centralPath);
                    using (var trans = new Transaction(m_doc))
                    {
                        try
                        {
                            trans.Start("Export Schedule");
                            if (directoryName != null)
                            {
                                TextFilePath = Path.Combine(directoryName, viewSchedule.Name + extension);
                                if (File.Exists(TextFilePath))
                                {
                                    File.Delete(TextFilePath);
                                }
                                viewSchedule.Export(directoryName, viewSchedule.Name + extension, options);
                            }
                            trans.Commit();
                        }
                        catch { trans.RollBack(); }
                    }
                }

                if (File.Exists(TextFilePath))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                result = false;
            }
            return result;
        }

        private bool ValidateViewSchedule(ViewSchedule viewSchedule)
        {
            bool result;
            using (var trans = new Transaction(m_doc))
            {
                trans.Start("Check Validation of Schedule Definition");
                try
                {
                    var requiredFields = new string[] { };
                    var definition = viewSchedule.Definition;
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

                        var count = definition.GetFieldCount();
                        for (var i = 0; i < count; i++)
                        {
                            var field = definition.GetField(i);
                            var fieldName = field.GetName();
                            if (!fieldDictionary.ContainsKey(fieldName))
                            {
                                var fp = new FieldProperties
                                {
                                    ColumnHeading = field.ColumnHeading,
                                    IsHidden = field.IsHidden
                                };
#if RELEASE2015 || RELEASE2016
                                fp.HasTotals = field.HasTotals;
#else
                                fp.HasTotals = field.DisplayType == ScheduleFieldDisplayType.Totals;
#endif
                                fieldDictionary.Add(fieldName, fp);
                            }

                            if (!requiredFields.Contains(fieldName)) continue;

                            if (fieldName.Contains("ASHRAE Allowable LPD") || fieldName.Contains("25% Below ASHRAE"))
                            {
                                continue;
                            }
#if RELEASE2015 || RELEASE2016
                            if (field.CanTotal()) { field.HasTotals = true; }
#else
                            if (field.CanTotal()) { field.DisplayType = ScheduleFieldDisplayType.Totals; }
#endif
                            field.IsHidden = false;
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
            using (var trans = new Transaction(m_doc))
            {
                trans.Start("Restore Fields");
                try
                {
                    var definition = viewSchedule.Definition;
                    var count = definition.GetFieldCount();
                    for (var i = 0; i < count; i++)
                    {
                        var field = definition.GetField(i);
                        var fieldName = field.GetName();
                        if (fieldDictionary.ContainsKey(fieldName))
                        {
                            var originalField = fieldDictionary[fieldName];
#if RELEASE2015 || RELEASE2016
                            if (field.CanTotal()) { field.HasTotals = originalField.HasTotals; }
#else
                            if (field.CanTotal())
                            {
                                field.DisplayType = originalField.HasTotals 
                                    ? ScheduleFieldDisplayType.Totals 
                                    : ScheduleFieldDisplayType.Standard;
                            }
#endif
                            field.IsHidden = originalField.IsHidden;
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    trans.RollBack();
                }
            }
        }

        private DataTable GetScheduleDataTable()
        {
            DataTable dataTable = null;
            try
            {
                var parser = new ScheduleDataParser(TextFilePath);
                dataTable = parser.Table;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return dataTable;
        }

        private string GetCentralPath(Document doc)
        {
            string docCentralPath;
            try
            {
                if (doc.IsWorkshared)
                {
                    var modelPath = doc.GetWorksharingCentralModelPath();
                    var centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    docCentralPath = !string.IsNullOrEmpty(centralPath) ? centralPath : doc.PathName;
                }
                else
                {
                    docCentralPath = doc.PathName;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                docCentralPath = doc.PathName;
            }
            return docCentralPath;
        }

        private string GetHeaderText(string fieldName)
        {
            var header = "";
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
                var headerText = GetHeaderText("AllowableLightingLoad");
                var strValue = ScheduleData.Rows[ScheduleData.Rows.Count - 1][headerText].ToString();
                strValue = strValue.Replace(loadUnit, "");
                double.TryParse(strValue, out allowableLoad);
            }
            catch
            {
                // ignored
            }
            return allowableLoad;
        }

        public string GetLPDCategoryBAM()
        {
            var categoryName = "";
            try
            {
                var headerText = GetHeaderText("ASHRAE LPD Category (BAM)");
                categoryName = ScheduleData.Rows[ScheduleData.Rows.Count - 2][headerText].ToString();
            }
            catch
            {
                // ignored
            }
            return categoryName;
        }

        public double GetAllowableLPDBAM()
        {
            double allowableLPD = 0;
            try
            {
                var headerText = GetHeaderText("ASHRAE Allowable LPD_BAM");
                var strValue = ScheduleData.Rows[ScheduleData.Rows.Count - 2][headerText].ToString();
                strValue = strValue.Replace(lpdUnit, "");
                double.TryParse(strValue, out allowableLPD);
            }
            catch
            {
                // ignored
            }
            return allowableLPD;
        }

        public double GetTargetLPDBAM()
        {
            double allowableLPD = 0;
            try
            {
                var headerText = GetHeaderText("25% Below ASHRAE_BAM");
                var strValue = ScheduleData.Rows[ScheduleData.Rows.Count - 2][headerText].ToString();
                strValue = strValue.Replace(lpdUnit, "");
                double.TryParse(strValue, out allowableLPD);
            }
            catch
            {
                // ignored
            }
            return allowableLPD;
        }

        public double GetActualLightingLoad()
        {
            double actualLightingLoad = 0;
            try
            {
                var headerText = GetHeaderText("ActualLightingLoad");
                var strValue = ScheduleData.Rows[ScheduleData.Rows.Count - 1][headerText].ToString();
                strValue = strValue.Replace(loadUnit, "");
                double.TryParse(strValue, out actualLightingLoad);
            }
            catch
            {
                // ignored
            }
            return actualLightingLoad;
        }

        public double GetArea()
        {
            double area = 0;
            try
            {
                var headerText = GetHeaderText("Area");
                var areaValue = ScheduleData.Rows[ScheduleData.Rows.Count - 1][headerText].ToString();
                areaValue = areaValue.Replace(areaUnit, "");
                double.TryParse(areaValue, out area);
            }
            catch
            {
                // ignored
            }
            return area;
        }

        public double GetSavings()
        {
            double savings = 0;
            try
            {
                var headerText = GetHeaderText("Savings/Overage");
                var strValue = ScheduleData.Rows[ScheduleData.Rows.Count - 1][headerText].ToString();
                strValue = strValue.Replace(loadUnit, "");
                double.TryParse(strValue, out savings);
            }
            catch
            {
                // ignored
            }
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