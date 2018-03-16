using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class StylesMonitor
    {
        public void PublishStylesInfo(Document doc, string recordId)
        {
            try
            {
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

                var dimSegmentStats = new List<DimensionSegmentInfo>();
                foreach (var d in dimInstances)
                {
                    var key = d.GetTypeId().IntegerValue;
                    if (dimTypes.ContainsKey(key))
                    {
                        // increment instance count
                        dimTypes[key] = new Tuple<DimensionType, int>(dimTypes[key].Item1, dimTypes[key].Item2 + 1);
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.INFO, "Givent TextNoteType Id doesn't exist in the model. It will be skipped.");
                    }

                    foreach (DimensionSegment s in d.Segments)
                    {
                        if (string.IsNullOrEmpty(s.ValueOverride)) continue;

                        dimSegmentStats.Add(new DimensionSegmentInfo(s));
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


                var result = ServerUtilities.Post<StylesStat>(stylesStats, "healthrecords/" + recordId + "/stylestats");
                if (result.Id == null)
                {
                    Log.AppendLog(LogMessageType.INFO, "Raising Status Window event. Status: Error.");
                    AppCommand.CommunicatorHandler.Status = Status.Error;
                    AppCommand.CommunicatorHandler.Message = "Styles Info failed to post.";
                    AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                    AppCommand.CommunicatorEvent.Raise();
                }
                else
                {
                    Log.AppendLog(LogMessageType.INFO, "Raising Status Window event. Status: Success.");
                    AppCommand.CommunicatorHandler.Status = Status.Success;
                    AppCommand.CommunicatorHandler.Message = "Styles Info posted successfully!";
                    AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                    AppCommand.CommunicatorEvent.Raise();
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
