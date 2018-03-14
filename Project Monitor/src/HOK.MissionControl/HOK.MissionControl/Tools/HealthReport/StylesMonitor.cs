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

                    dimSegmentStats.AddRange(from DimensionSegment s in d.Segments select new DimensionSegmentInfo(s));
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


                ServerUtilities.Post<StylesStat>(stylesStats, "healthrecords/" + recordId + "/stylestats");
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
