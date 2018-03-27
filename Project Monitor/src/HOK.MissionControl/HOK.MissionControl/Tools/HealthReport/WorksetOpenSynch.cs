using System;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class WorksetOpenSynch
    {
        /// <summary>
        /// Publishes Workset Open Data to database for onOpened/onSynched events.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="recordId">Health Record Document Id.</param>
        /// <param name="state">State of the Revit model.</param>
        public void PublishData(Document doc, string recordId, WorksetMonitorState state)
        {
            try
            {
                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .ToWorksets();

                var opened = 0;
                var closed = 0;
                foreach (var w in worksets)
                {
                    if (w.IsOpen) opened++;
                    else closed++;
                }

                var worksetInfo = new WorksetEvent
                {
                    user = Environment.UserName.ToLower(),
                    opened = opened,
                    closed = closed
                };

                var unused = ServerUtilities.Post<WorksetEvent>(worksetInfo, "healthrecords/" + recordId + "/" + state);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}