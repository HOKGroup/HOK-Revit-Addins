#region References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Links;
using HOK.MissionControl.Core.Schemas.Models;
using HOK.MissionControl.Core.Schemas.Styles;
using HOK.MissionControl.Core.Schemas.Views;
using HOK.MissionControl.Core.Schemas.Worksets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator.Messaging;
#endregion

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class CommunicatorHealthReportModel
    {
        /// <summary>
        /// Creates graphics for Health Report Summary as they are getting downloaded from internet.
        /// </summary>
        /// <param name="obj">Health Report Added Message object.</param>
        /// <returns></returns>
        public HealthReportSummaryViewModel CreateSummary(HealthReportSummaryAdded obj)
        {
            switch (obj.Type)
            {
                case SummaryType.Views:
                    var vData = (ViewsData)obj.Data;
                    return ProcessViews(vData);
                case SummaryType.Worksets:
                    var wData = (WorksetData)obj.Data;
                    return ProcessWorksets(wData);
                case SummaryType.Families:
                    var fData = (FamilyData)obj.Data;
                    return ProcessFamilies(fData);
                case SummaryType.Styles:
                    var sData = (StylesData)obj.Data;
                    return ProcessStyles(sData);
                case SummaryType.Links:
                    var lData = (LinkData)obj.Data;
                    return ProcessLinks(lData);
                case SummaryType.Models:
                    var mData = (ModelData)obj.Data;
                    return ProcessModels(mData);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Processes View Stats to create Summary.
        /// </summary>
        /// <param name="data">Views Data</param>
        /// <returns></returns>
        private static HealthReportSummaryViewModel ProcessViews(ViewsData data)
        {
            var viewStats = data.ViewStats.LastOrDefault();
            if (viewStats == null) return null;

            var viewsNotOnSheet = viewStats.TotalViews - viewStats.ViewsOnSheet;
            var schedulesNotOnSheet = viewStats.TotalSchedules - viewStats.SchedulesOnSheet;

            var viewsNotOnSheetP = Math.Round(
                (double)((viewsNotOnSheet * 100) / viewStats.TotalViews), 0);
            var schedulesNotOnSheetP = Math.Round(
                (double)((schedulesNotOnSheet * 100) / viewStats.TotalSchedules), 0);

            var passingChecks = 0;
            if (viewsNotOnSheetP <= 20) passingChecks += 2;
            else
            {
                if (viewsNotOnSheetP > 20 && viewsNotOnSheetP <= 40) passingChecks += 1;
            }
            if (schedulesNotOnSheetP <= 20) passingChecks += 2;
            else
            {
                if (schedulesNotOnSheetP > 20 && schedulesNotOnSheetP <= 40) passingChecks += 1;
            }
            var templateValue = Math.Round(
                (double)((viewStats.ViewsOnSheetWithTemplate * 100) / viewStats.ViewsOnSheet), 0);
            if (templateValue >= 80) passingChecks += 2;
            else
            {
                if (templateValue > 70 && templateValue <= 80) passingChecks += 1;
            }
            if (viewStats.UnclippedViews <= 20) passingChecks += 2;

            const int maxScore = 8;
            var vm = new HealthReportSummaryViewModel
            {
                Count = viewStats.TotalViews.ToString(),
                Title = "Views:",
                ToolName = string.Empty,
                ShowButton = false,
                Score = passingChecks + "/" + maxScore,
                FillColor = GetColor(passingChecks, maxScore)
            };
            return vm;
        }

        /// <summary>
        /// Processes Worksets Stats to create Summary.
        /// </summary>
        /// <param name="data">Worksets Data</param>
        /// <returns></returns>
        private static HealthReportSummaryViewModel ProcessWorksets(WorksetData data)
        {
            var worksetStats = data.ItemCount.LastOrDefault();
            if (worksetStats == null) return null;

            var unusedWorksets = 0;
            var workset1 = false;
            var sharedLevels = false;
            var contentOnSingleWorkset = false;
            var worksetCountTotal = worksetStats.Worksets.Sum(w => w.Count);

            foreach (var w in worksetStats.Worksets)
            {
                if (w.Count <= 0) unusedWorksets++;
                if (w.Name == "Workset1") workset1 = true;
                if (w.Name == "Shared Levels and Grids") sharedLevels = true;
                if ((w.Count * 100) / worksetCountTotal >= 50) contentOnSingleWorkset = true;
            }

            var passingChecks = 0;
            if (unusedWorksets == 0) passingChecks += 2;
            if (workset1 && sharedLevels && worksetStats.Worksets.Count == 2) passingChecks += 0;
            else passingChecks += 2;
            if (!contentOnSingleWorkset) passingChecks += 2;

            const int maxScore = 6;
            var vm = new HealthReportSummaryViewModel
            {
                Count = worksetStats.Worksets.Count.ToString(),
                Title = "Worksets:",
                ToolName = string.Empty,
                ShowButton = false,
                Score = passingChecks + "/" + maxScore,
                FillColor = GetColor(passingChecks, maxScore)
            };

            return vm;
        }

        /// <summary>
        /// Processes Families Stats to create Summary.
        /// </summary>
        /// <param name="data">Family Data</param>
        /// <returns></returns>
        private static HealthReportSummaryViewModel ProcessFamilies(FamilyData data)
        {
            if (data == null) return null;

            var passingChecks = 0;
            if (data.OversizedFamilies <= 20) passingChecks += 2;
            else
            {
                if (data.OversizedFamilies > 10 && data.OversizedFamilies < 20) passingChecks += 1;
            }

            var config = MissionControlSetup.Configurations.ContainsKey(data.CentralPath)
                ? MissionControlSetup.Configurations[data.CentralPath]
                : null;

            var familyNameCheck = new List<string> { "HOK_I", "HOK_M" }; //defaults
            if (config != null)
            {
                familyNameCheck = config.Updaters.First(x => string.Equals(x.UpdaterId,
                    Properties.Resources.HealthReportTrackerGuid, StringComparison.OrdinalIgnoreCase)).UserOverrides.FamilyNameCheck.Values;
            }

            var misnamed = 0;
            foreach (var family in data.Families)
            {
                if (!family.IsFailingChecks) continue;
                if (!familyNameCheck.Any(family.Name.Contains)) misnamed++;
            }

            if (misnamed < 10) passingChecks += 2;
            else
            {
                if (misnamed > 10 && misnamed < 20) passingChecks += 1;
            }
            if (data.UnusedFamilies <= 10) passingChecks += 2;
            else
            {
                if (data.UnusedFamilies > 10 && data.UnusedFamilies < 20) passingChecks += 1;
            }
            if (data.InPlaceFamilies <= 5) passingChecks += 2;
            else
            {
                if (data.InPlaceFamilies > 5 && data.InPlaceFamilies < 10) passingChecks += 1;
            }

            const int maxScore = 8;
            var vm = new HealthReportSummaryViewModel
            {
                Count = data.TotalFamilies.ToString(),
                Title = "Families:",
                ToolName = string.Empty,
                ShowButton = false,
                Score = passingChecks + "/" + maxScore,
                FillColor = GetColor(passingChecks, maxScore)
            };

            return vm;
        }

        /// <summary>
        /// Processes Styles Stats to create Summary.
        /// </summary>
        /// <param name="data">Styles Data</param>
        /// <returns></returns>
        private static HealthReportSummaryViewModel ProcessStyles(StylesData data)
        {
            var stylesStats = data.StyleStats.LastOrDefault();
            if (stylesStats == null) return null;

            var overridenDimensions = stylesStats.DimSegmentStats.Count;

            var passingChecks = 0;
            if (overridenDimensions <= 10) passingChecks += 2;
            else if (overridenDimensions > 10 && overridenDimensions <= 20) passingChecks += 1;

            var unusedTextTypes = true;
            var unusedDimensionTypes = true;
            var usesProjectUnits = true;
            var unusedTypes = 0;
            foreach (var ds in stylesStats.DimStats)
            {
                if (ds.Instances == 0)
                {
                    unusedTypes += 1;
                    unusedDimensionTypes = false;
                }
                if (!ds.UsesProjectUnits) usesProjectUnits = false;
            }
            foreach (var ts in stylesStats.TextStats)
            {
                if (ts.Instances == 0)
                {
                    unusedTypes += 1;
                    unusedTextTypes = false;
                }
            }

            if (usesProjectUnits) passingChecks += 2;
            if (unusedDimensionTypes) passingChecks += 2;
            if (unusedTextTypes) passingChecks += 2;

            const int maxScore = 8;
            var vm = new HealthReportSummaryViewModel
            {
                Count = unusedTypes.ToString(),
                Title = "Styles:",
                ToolName = string.Empty,
                ShowButton = false,
                Score = passingChecks + "/" + maxScore,
                FillColor = GetColor(passingChecks, maxScore)
            };

            return vm;
        }

        /// <summary>
        /// Processes Links Stats to create Summary.
        /// </summary>
        /// <param name="data">Links Data</param>
        /// <returns></returns>
        private static HealthReportSummaryViewModel ProcessLinks(LinkData data)
        {
            var linkStats = data.LinkStats.LastOrDefault();
            if (linkStats == null) return null;

            var passingChecks = 0;
            var imports = linkStats.ImportedDwgFiles.Select(x => !x.IsLinked).Count();
            if (imports == 0) passingChecks += 2;
            if (linkStats.UnusedLinkedImages == 0) passingChecks += 2;
            else
            {
                if (linkStats.UnusedLinkedImages <= 2) passingChecks += 1;
            }
            if (linkStats.TotalImportedStyles <= 25) passingChecks += 2;
            else
            {
                if (linkStats.TotalImportedStyles > 25 && linkStats.TotalImportedStyles <= 50) passingChecks += 1;
            }

            const int maxScore = 6;
            var vm = new HealthReportSummaryViewModel
            {
                Count = linkStats.TotalImportedDwg.ToString(),
                Title = "Links:",
                ToolName = "Links Manager",
                ShowButton = true,
                Score = passingChecks + "/" + maxScore,
                FillColor = GetColor(passingChecks, maxScore)
            };

            return vm;
        }

        /// <summary>
        /// Processes Models Stats to create Summary.
        /// </summary>
        /// <param name="data">Models Data</param>
        /// <returns></returns>
        private static HealthReportSummaryViewModel ProcessModels(ModelData data)
        {
            var modelStats = data.ModelSizes.LastOrDefault();
            if (modelStats == null) return null;

            var vm = new HealthReportSummaryViewModel
            {
                Count = StringUtilities.BytesToString(modelStats.Value),
                Title = "Model:",
                ToolName = string.Empty,
                Score = "0/0",
                FillColor = Color.FromRgb(119, 119, 119)
            };

            return vm;
        }

        #region Utilities 

        /// <summary>
        /// Returns a Color based on score and max value.
        /// </summary>
        /// <param name="score">Passing Checks score.</param>
        /// <param name="newMax">Max value that can be scored.</param>
        /// <returns></returns>
        private static Color GetColor(int score, int newMax)
        {
            var remapedScore = Math.Round(
                (double)((score * 6) / newMax));
            if (remapedScore <= 2)
            {
                return Color.FromRgb(217, 83, 79);
            }
            if (remapedScore >= 2 && remapedScore <= 4)
            {
                return Color.FromRgb(240, 173, 78);
            }
            return Color.FromRgb(92, 182, 92);
        }

        #endregion
    }
}
