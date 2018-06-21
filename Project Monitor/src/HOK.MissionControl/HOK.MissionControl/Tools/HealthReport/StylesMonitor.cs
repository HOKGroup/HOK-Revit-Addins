using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Styles;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class StylesMonitor
    {
        private static Document _doc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="stylesId"></param>
        public void PublishData(Document doc, string stylesId)
        {
            try
            {
                _doc = doc;

                #region Text Note Type stats

                var textTypes = new FilteredElementCollector(doc)
                    .OfClass(typeof(TextNoteType))
                    .ToDictionary(x => x.Id.IntegerValue, x => new Tuple<Element,int>(x, 0));

                var textInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(TextNote))
                    .WhereElementIsNotElementType();

                foreach (var t in textInstances)
                {
                    var key = t.GetTypeId().IntegerValue;
                    if (textTypes.ContainsKey(key))
                    {
                        // increment instance count
                        textTypes[key] = new Tuple<Element, int>(textTypes[key].Item1, textTypes[key].Item2 + 1);
                    }
                }

                var textStats = textTypes.Select(x => new TextNoteTypeInfo(x.Value.Item1) {Instances = x.Value.Item2})
                    .ToList();

                #endregion

                #region Dimension Type stats

                var dimTypes = new FilteredElementCollector(doc)
                    .OfClass(typeof(DimensionType))
                    .Cast<DimensionType>()
                    .Where(x => !string.IsNullOrEmpty(x.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString()))
                    .ToDictionary(x => x.Id.IntegerValue, x => new Tuple<DimensionType, int>(x, 0));

                var dimInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(Dimension))
                    .WhereElementIsNotElementType()
                    .Cast<Dimension>();

                // (Konrad) There is a user override in Configuration that controls what dimension overrides are ignored
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var config = MissionControlSetup.Configurations.ContainsKey(centralPath)
                    ? MissionControlSetup.Configurations[centralPath]
                    : null;

                var dimensionValueCheck = new List<string> { "EQ" }; //defaults
                if (config != null)
                {
                    dimensionValueCheck = config.Updaters.First(x => string.Equals(x.UpdaterId,
                        Properties.Resources.HealthReportTrackerGuid, StringComparison.OrdinalIgnoreCase)).UserOverrides.DimensionValueCheck.Values;
                }

                var units = _doc.GetUnits();
                var dimSegmentStats = new List<DimensionSegmentInfo>();
                foreach (var d in dimInstances)
                {
                    var key = d.GetTypeId().IntegerValue;
                    if (dimTypes.ContainsKey(key))
                    {
                        // increment instance count
                        dimTypes[key] = new Tuple<DimensionType, int>(dimTypes[key].Item1, dimTypes[key].Item2 + 1);
                    }

                    if (d.NumberOfSegments == 0)
                    {
                        if (string.IsNullOrEmpty(d.ValueOverride)) continue;
                        if (dimensionValueCheck.Any(d.ValueOverride.Contains)) continue;

                        // dim w/ zero segments
                        dimSegmentStats.Add(new DimensionSegmentInfo(d)
                        {
                            ValueString = UnitFormatUtils.Format(units, d.DimensionType.UnitType, (double)d.Value, false, false),
                            OwnerViewType = d.ViewSpecific 
                                ? ((View)doc.GetElement(d.OwnerViewId)).ViewType.ToString() 
                                : string.Empty,
                            OwnerViewId = d.OwnerViewId.IntegerValue
                        });
                    }
                    else
                    {
                        // dim w/ multiple segments
                        foreach (DimensionSegment s in d.Segments)
                        {
                            if (string.IsNullOrEmpty(s.ValueOverride)) continue;
                            if (dimensionValueCheck.Any(s.ValueOverride.Contains)) continue;

                            dimSegmentStats.Add(new DimensionSegmentInfo(s)
                            {
                                ValueString = UnitFormatUtils.Format(units, d.DimensionType.UnitType, (double)s.Value, false, false),
                                OwnerViewType = d.ViewSpecific 
                                    ? ((View)doc.GetElement(d.OwnerViewId)).ViewType.ToString() 
                                    : string.Empty,
                                OwnerViewId = d.OwnerViewId.IntegerValue
                            });
                        }
                    }
                }

                var dimStats = dimTypes.Select(x => new DimensionTypeInfo(x.Value.Item1) { Instances = x.Value.Item2 })
                    .ToList();

                #endregion

                #region Line Style stats

                //TODO: Finish this out.

                #endregion

                var stylesStats = new StylesDataItem
                {
                    User = Environment.UserName.ToLower(),
                    TextStats = textStats,
                    DimStats = dimStats,
                    DimSegmentStats = dimSegmentStats
                };

                if (!ServerUtilities.Post(stylesStats, "styles/" + stylesId + "/stylestats",
                    out StylesDataItem unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Styles Data.");
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
