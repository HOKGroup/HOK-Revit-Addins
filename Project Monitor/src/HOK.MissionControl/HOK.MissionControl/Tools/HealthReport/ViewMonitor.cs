using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class ViewMonitor
    {
        public static Guid UpdaterGuid { get; set; } = new Guid(Properties.Resources.HealthReportTrackerGuid);

        /// <summary>
        /// Publishes View count data when Document is closed.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        public static void PublishData(Document doc, Configuration config, Project project)
        {
            try
            {
                if (!MonitorUtilities.IsUpdaterOn(project, config, UpdaterGuid)) return;
                var worksetDocumentId = project.worksets.FirstOrDefault();
                if (string.IsNullOrEmpty(worksetDocumentId)) return;

                // (Konrad) Collect info about views.
                // Views on sheet, schedules on sheet, views total etc.
                var scheduleTotalCount = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .GetElementCount();

                var viewTotalCount = 0;
                var unclippedViews = 0;
                var viewsOnSheet = 0;
                var viewsOnSheetWithTemplate = 0;
                var sheetTotalCount = 0;
                foreach (var v in new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>())
                {
                    viewTotalCount++;
                    switch (v.ViewType)
                    {
                        case ViewType.DrawingSheet:
                            sheetTotalCount++;
                            var viewIds = ((ViewSheet)v).GetAllPlacedViews();
                            foreach (var id in viewIds)
                            {
                                viewsOnSheet++;
                                var view = (View)doc.GetElement(id);
                                if (view.ViewTemplateId != ElementId.InvalidElementId) viewsOnSheetWithTemplate++;
                            }
                            break;
                        case ViewType.FloorPlan:
                        case ViewType.EngineeringPlan:
                        case ViewType.AreaPlan:
                        case ViewType.CeilingPlan:
                            if (v.get_Parameter(BuiltInParameter.VIEW_BACK_CLIPPING).AsInteger() == 0)
                            {
                                unclippedViews++;
                            }
                            break;
                        case ViewType.Elevation:
                        case ViewType.Section:
                            if (v.get_Parameter(BuiltInParameter.VIEWER_BOUND_FAR_CLIPPING).AsInteger() == 0)
                            {
                                unclippedViews++;
                            }
                            break;
                        case ViewType.ThreeD:
                            if (v.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_FAR).AsInteger() == 0)
                            {
                                unclippedViews++;
                            }
                            break;
                    }
                }

                var scheduleSet = new HashSet<ElementId>();
                foreach (var ssi in new FilteredElementCollector(doc)
                    .OfClass(typeof(ScheduleSheetInstance))
                    .Cast<ScheduleSheetInstance>())
                {
                    scheduleSet.Add(ssi.ScheduleId);
                }
                var schedulesOnSheet = scheduleSet.Count;
                viewsOnSheet += schedulesOnSheet;

                var viewStats = new ViewStat
                {
                    totalViews = viewTotalCount,
                    totalSheets = sheetTotalCount,
                    totalSchedules = scheduleTotalCount,
                    viewsOnSheet = viewsOnSheet,
                    viewsOnSheetWithTemplate = viewsOnSheetWithTemplate,
                    schedulesOnSheet = schedulesOnSheet,
                    unclippedViews = unclippedViews
                };

                ServerUtilities.PostStats(viewStats, worksetDocumentId, "viewstats");
            }
            catch (Exception e)
            {
                LogUtilities.AppendLog("ViewMonitor-PublishData: " + e.Message);
            }
        }
    }
}