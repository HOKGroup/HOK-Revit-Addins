using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Worksets;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class WorksetItemCount
    {
        /// <summary>
        /// Publishes Workset Item Count data when Document is closed.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="worksetsId">Worksets Document Id.</param>
        public void PublishData(Document doc, string worksetsId)
        {
            try
            {
                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .ToWorksets();

                var worksetInfo = new List<WorksetItem>();
                foreach (var w in worksets)
                {
                    var worksetFilter = new ElementWorksetFilter(w.Id, false);
                    var count = new FilteredElementCollector(doc)
                        .WherePasses(worksetFilter)
                        .GetElementCount();

                    worksetInfo.Add(new WorksetItem { Name = w.Name, Count = count });
                }

                if (!ServerUtilities.Post(new WorksetItemData { Worksets = worksetInfo },
                    "worksets/" + worksetsId + "/itemcount", out WorksetItemData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Worksets Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}