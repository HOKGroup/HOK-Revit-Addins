using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Views;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class ViewMonitor
    {
        /// <summary>
        /// Publishes View count data when Document is closed.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="viewsId">Id of the Views Document in MongoDB.</param>
        public void PublishData(Document doc, string viewsId)
        {
            try
            {
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
                foreach (var v in new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Where(x => !x.IsTemplate))
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

                var viewStats = new ViewsDataItem
                {
                    TotalViews = viewTotalCount,
                    TotalSheets = sheetTotalCount,
                    TotalSchedules = scheduleTotalCount,
                    ViewsOnSheet = viewsOnSheet,
                    ViewsOnSheetWithTemplate = viewsOnSheetWithTemplate,
                    SchedulesOnSheet = schedulesOnSheet,
                    UnclippedViews = unclippedViews
                };

                if (!ServerUtilities.Post(viewStats, "views/" + viewsId + "/viewstats", 
                    out ViewsData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Views Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}