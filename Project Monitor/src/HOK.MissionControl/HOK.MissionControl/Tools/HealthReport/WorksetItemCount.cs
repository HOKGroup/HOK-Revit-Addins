using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class WorksetItemCount
    {
        /// <summary>
        /// Publishes Workset Item Count data when Document is closed.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="recordId">Health Record Document Id.</param>
        public void PublishData(Document doc, string recordId)
        {
            try
            {
                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .ToWorksets();

                var worksetInfo = new List<Item>();
                foreach (var w in worksets)
                {
                    var worksetFilter = new ElementWorksetFilter(w.Id, false);
                    var count = new FilteredElementCollector(doc)
                        .WherePasses(worksetFilter)
                        .GetElementCount();

                    worksetInfo.Add(new Item { name = w.Name, count = count });
                }

                var unused = ServerUtilities.Post<WorksetItem>(new WorksetItem { worksets = worksetInfo }, "healthrecords/" + recordId + "/itemcount");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}