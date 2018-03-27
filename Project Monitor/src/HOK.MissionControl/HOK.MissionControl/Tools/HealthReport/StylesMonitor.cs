using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class StylesMonitor
    {
        private static Document _doc { get; set; }

        public void PublishStylesInfo(Document doc, string recordId)
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
                    else
                    {
                        Log.AppendLog(LogMessageType.INFO, "Givent TextNoteType Id doesn't exist in the model. It will be skipped.");
                    }
                }

                var textStats = textTypes.Select(x => new TextNoteTypeInfo(x.Value.Item1) {instances = x.Value.Item2})
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
                var centralPath = BasicFileInfo.Extract(doc.PathName).CentralPath;
                var config = MissionControlSetup.Configurations.ContainsKey(centralPath)
                    ? MissionControlSetup.Configurations[centralPath]
                    : null;

                var dimensionValueCheck = new List<string> { "EQ" }; //defaults
                if (config != null)
                {
                    dimensionValueCheck = config.updaters.First(x => string.Equals(x.updaterId,
                        Properties.Resources.HealthReportTrackerGuid, StringComparison.OrdinalIgnoreCase)).userOverrides.dimensionValueCheck.values;
                }

                var dimSegmentStats = new List<DimensionSegmentInfo>();
                foreach (var d in dimInstances)
                {
                    DimensionType dType;
                    var key = d.GetTypeId().IntegerValue;
                    if (dimTypes.ContainsKey(key))
                    {
                        // increment instance count
                        dimTypes[key] = new Tuple<DimensionType, int>(dimTypes[key].Item1, dimTypes[key].Item2 + 1);
                        dType = dimTypes[key].Item1;
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.INFO, "Given Dimension Type Id doesn't exist in the model. It will be skipped.");
                        continue; //without dimension type we can't get units so just break out
                    }

                    var ut = GetUnitType(dType);
                    if (d.Segments.Size == 0 && 
                        !string.IsNullOrEmpty(d.ValueOverride) && 
                        !dimensionValueCheck.Any(d.ValueOverride.Contains))
                    {
                        // dim w/ zero segments
                        dimSegmentStats.Add(new DimensionSegmentInfo(d)
                        {
                            valueString = InternalUnitsToProjectUnits(d.Value, ut),
                            ownerViewType = d.ViewSpecific 
                                ? ((View)doc.GetElement(d.OwnerViewId)).ViewType.ToString() 
                                : string.Empty,
                            ownerViewId = d.OwnerViewId.IntegerValue
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
                                valueString = InternalUnitsToProjectUnits(s.Value, ut),
                                ownerViewType = d.ViewSpecific 
                                    ? ((View)doc.GetElement(d.OwnerViewId)).ViewType.ToString() 
                                    : string.Empty,
                                ownerViewId = d.OwnerViewId.IntegerValue
                            });
                        }
                    }
                    
                }

                var dimStats = dimTypes.Select(x => new DimensionTypeInfo(x.Value.Item1) { instances = x.Value.Item2 })
                    .ToList();

                #endregion

                #region Line Style stats

                //TODO: Finish this out.

                #endregion

                var stylesStats = new StylesStat
                {
                    user = Environment.UserName.ToLower(),
                    textStats = textStats,
                    dimStats = dimStats,
                    dimSegmentStats = dimSegmentStats
                };

                var unused = ServerUtilities.Post<StylesStat>(stylesStats, "healthrecords/" + recordId + "/stylestats");
                //if (result.Id == null)
                //{
                //    Log.AppendLog(LogMessageType.INFO, "Raising Status Window event. Status: Error.");
                //    AppCommand.CommunicatorHandler.Status = Status.Error;
                //    AppCommand.CommunicatorHandler.Message = "Styles Info failed to post.";
                //    AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                //    AppCommand.CommunicatorEvent.Raise();
                //}
                //else
                //{
                //    Log.AppendLog(LogMessageType.INFO, "Raising Status Window event. Status: Success.");
                //    AppCommand.CommunicatorHandler.Status = Status.Success;
                //    AppCommand.CommunicatorHandler.Message = "Styles Info posted successfully!";
                //    AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                //    AppCommand.CommunicatorEvent.Raise();
                //}
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Get UnitType from DimensionType.
        /// </summary>
        /// <param name="dType">Dimension Type.</param>
        /// <returns>Unit Type.</returns>
        private static UnitType GetUnitType(DimensionType dType)
        {
            UnitType ut;
            switch (dType.StyleType)
            {
                case DimensionStyleType.Linear:
                case DimensionStyleType.ArcLength:
                case DimensionStyleType.Diameter:
                case DimensionStyleType.LinearFixed:
                case DimensionStyleType.SpotElevation:
                case DimensionStyleType.Radial:
                    ut = UnitType.UT_Length;
                    break;
                case DimensionStyleType.Angular:
                    ut = UnitType.UT_Angle;
                    break;
                case DimensionStyleType.SpotCoordinate:
                    ut = UnitType.UT_Undefined;
                    break;
                case DimensionStyleType.SpotSlope:
                    ut = UnitType.UT_Slope;
                    break;
                default:
                    ut = UnitType.UT_Undefined;
                    break;
            }
            return ut;
        }

        /// <summary>
        /// Formats values from internal units to project units.
        /// </summary>
        /// <param name="value">Numerical value to format./</param>
        /// <param name="type">Unit Type to convert to.</param>
        /// <returns>String representation of the value.</returns>
        public static string InternalUnitsToProjectUnits(double? value, UnitType type)
        {
            return value == null
                ? string.Empty
                : UnitFormatUtils.Format(_doc.GetUnits(), type, (double)value, false, false);
        }
    }
}
