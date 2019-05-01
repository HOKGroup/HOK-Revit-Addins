using System;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Worksets;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class WorksetOpenSynch
    {
        /// <summary>
        /// Publishes Workset Open Data to database for onOpened/onSynched events.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="filePath">Revit Document central file path.</param>
        /// <param name="state">State of the Revit model.</param>
        public void PublishData(Document doc, string filePath, WorksetMonitorState state)
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
                    CentralPath = filePath.ToLower(),
                    User = Environment.UserName.ToLower(),
                    Opened = opened,
                    Closed = closed
                };

                var route = state == WorksetMonitorState.onopened ? "onopened" : "onsynched";
                if (!ServerUtilities.Post(worksetInfo, "worksets/" + route, out WorksetEvent unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Worksets Data: " + state + ".");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}