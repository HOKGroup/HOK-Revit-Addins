using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class CommunicatorHealthReportModel
    {
        public ObservableCollection<HealthReportSummaryViewModel> ProcessData(HealthReportData data, FamilyData familyStats)
        {
            var output = new ObservableCollection<HealthReportSummaryViewModel>();
            if (familyStats != null)
            {
                var passingChecks = 0;
                if (familyStats.oversizedFamilies <= 20) passingChecks += 2;
                else
                {
                    if (familyStats.oversizedFamilies > 10 && familyStats.oversizedFamilies < 20) passingChecks += 1;
                }
                var misnamed = familyStats.families.Count(f => f.isFailingChecks && !f.name.Contains("_HOK_I") && !f.name.Contains("_HOK_M"));
                if (misnamed < 10) passingChecks += 2;
                else
                {
                    if (misnamed > 10 && misnamed < 20) passingChecks += 1;
                }
                if (familyStats.unusedFamilies <= 10) passingChecks += 2;
                else
                {
                    if (familyStats.unusedFamilies > 10 && familyStats.unusedFamilies < 20) passingChecks += 1;
                }
                if (familyStats.inPlaceFamilies <= 5) passingChecks += 2;
                else
                {
                    if (familyStats.inPlaceFamilies > 5 && familyStats.inPlaceFamilies < 10) passingChecks += 1;
                }

                const int maxScore = 8;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = familyStats.totalFamilies.ToString(),
                    Title = "Families:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var worksetStats = data.itemCount.LastOrDefault();
            if (worksetStats != null)
            {
                var unusedWorksets = 0;
                var workset1 = false;
                var sharedLevels = false;
                var contentOnSingleWorkset = false;
                var worksetCountTotal = worksetStats.worksets.Sum(w => w.count);

                foreach (var w in worksetStats.worksets)
                {
                    if (w.count <= 0) unusedWorksets++;
                    if (w.name == "Workset1") workset1 = true;
                    if (w.name == "Shared Levels and Grids") sharedLevels = true;
                    if ((w.count * 100) / worksetCountTotal >= 50) contentOnSingleWorkset = true;
                }

                var passingChecks = 0;
                if (unusedWorksets == 0) passingChecks += 2;
                if (workset1 && sharedLevels && worksetStats.worksets.Count == 2) passingChecks += 0;
                else passingChecks += 2;
                if (!contentOnSingleWorkset) passingChecks += 2;

                const int maxScore = 6;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = worksetStats.worksets.Count.ToString(),
                    Title = "Worksets:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var linkStats = data.linkStats.LastOrDefault();
            if (linkStats != null)
            {
                var passingChecks = 0;
                var imports = linkStats.importedDwgFiles.Select(x => !x.isLinked).Count();
                if (imports == 0) passingChecks += 2;
                if (linkStats.unusedLinkedImages == 0) passingChecks += 2;
                else
                {
                    if (linkStats.unusedLinkedImages <= 2) passingChecks += 1;
                }
                if (linkStats.totalImportedStyles <= 25) passingChecks += 2;
                else
                {
                    if (linkStats.totalImportedStyles > 25 && linkStats.totalImportedStyles <= 50) passingChecks += 1;
                }

                const int maxScore = 6;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = linkStats.totalImportedDwg.ToString(),
                    Title = "Links:",
                    ToolName = "Links Manager",
                    ShowButton = true,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var viewStats = data.viewStats.LastOrDefault();
            if (viewStats != null)
            {
                var viewsNotOnSheet = viewStats.totalViews - viewStats.viewsOnSheet;
                var schedulesNotOnSheet = viewStats.totalSchedules - viewStats.schedulesOnSheet;

                var viewsNotOnSheetP = Math.Round(
                    (double)((viewsNotOnSheet * 100) / viewStats.totalViews), 0);
                var schedulesNotOnSheetP = Math.Round(
                    (double)((schedulesNotOnSheet * 100) / viewStats.totalSchedules), 0);

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
                    (double)((viewStats.viewsOnSheetWithTemplate * 100) / viewStats.viewsOnSheet), 0);
                if (templateValue >= 80) passingChecks += 2;
                else
                {
                    if (templateValue > 70 && templateValue <= 80) passingChecks += 1;
                }
                if (viewStats.unclippedViews <= 20) passingChecks += 2;

                const int maxScore = 8;
                var vm = new HealthReportSummaryViewModel
                {
                    Count = viewStats.totalViews.ToString(),
                    Title = "Views:",
                    ToolName = string.Empty,
                    ShowButton = false,
                    Score = passingChecks + "/" + maxScore,
                    FillColor = GetColor(passingChecks, maxScore)
                };
                output.Add(vm);
            }

            var modelStats = data.modelSizes.FirstOrDefault();
            if (modelStats != null)
            {
                var vm = new HealthReportSummaryViewModel
                {
                    Count = StringUtilities.BytesToString(modelStats.value),
                    Title = "Model:",
                    ToolName = string.Empty,
                    Score = "0/0",
                    FillColor = Color.FromRgb(119, 119, 119)
                };
                output.Add(vm);
            }

            return output;
        }

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

        /// <summary>
        /// Shows or Hides the Communicator dockable pane.
        /// </summary>
        /// <param name="application">UIApp</param>
        public void ToggleCommunicator(UIApplication application)
        {
            var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
            var dp = application.GetDockablePane(dpid);
            if (dp == null) return;

            var assembly = Assembly.GetExecutingAssembly();
            if (dp.IsShown())
            {
                dp.Hide();
                AppCommand.Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", "communicatorOff_32x32.png");
                AppCommand.Instance.CommunicatorButton.ItemText = "Show" + Environment.NewLine + "Communicator";
            }
            else
            {
                dp.Show();
                AppCommand.Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", "communicatorOn_32x32.png");
                AppCommand.Instance.CommunicatorButton.ItemText = "Hide" + Environment.NewLine + "Communicator";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetCommunicatorImage()
        {
            // (Konrad) This needs to run after the doc is opened, because UI elements don't get created until then.
            AppCommand.EnqueueTask(app =>
            {
                var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
                var dp = app.GetDockablePane(dpid);
                var assembly = Assembly.GetExecutingAssembly();
                if (dp != null)
                {
                    AppCommand.Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", dp.IsShown()
                        ? "communicatorOn_32x32.png"
                        : "communicatorOff_32x32.png");
                    AppCommand.Instance.CommunicatorButton.ItemText = dp.IsShown()
                        ? "Hide" + Environment.NewLine + "Communicator"
                        : "Show" + Environment.NewLine + "Communicator";
                }
            });
        }
    }
}
