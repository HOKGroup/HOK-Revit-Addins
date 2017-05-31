using System;
using System.Linq;
using System.Net;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.WorksetMonitor
{
    public static class WorksetOpenSynch
    {
        public static Guid UpdaterGuid { get; set; } = new Guid("56603be6-aeb2-45d0-9ebc-2830fad6368b");

        /// <summary>
        /// Publishes Workset Open Data to database for onOpened/onSynched events.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        /// <param name="state">State of the Revit model.</param>
        /// <param name="centralPath">Central File path.</param>
        /// <param name="refreshProject">Return value that indicates if we should make another call to server to retrieve updated Project Document.</param>
        public static void PublishData(Document doc, Configuration config, Project project, WorksetMonitorState state, string centralPath, out bool refreshProject)
        {
            refreshProject = false;
            try
            {
                var updaterFound = config.updaters
                    .Where(x => string.Equals(x.updaterId.ToLower(), UpdaterGuid.ToString().ToLower(),
                        StringComparison.Ordinal))
                    .ToList();

                if (!updaterFound.Any() || !updaterFound.First().isUpdaterOn) return;

                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .ToWorksets();

                var opened = 0;
                var closed = 0;
                var user = Environment.UserName;
                var worksetDocumentId = project.worksets.FirstOrDefault();
                foreach (var w in worksets)
                {
                    if (w.IsOpen) opened++;
                    else closed++;
                }

                var worksetInfo = new WorksetEvent
                {
                    user = user,
                    opened = opened,
                    closed = closed
                };

                // (Konrad) It's possible that Workset Document doesn't exist in database yet.
                // Create it and set the reference to it in Project if that's the case.
                if (string.IsNullOrEmpty(worksetDocumentId))
                {
                    worksetDocumentId = ServerUtil.PostWorksetData();
                    var status = ServerUtil.AddWorksetToProject(project, worksetDocumentId);

                    if (status == HttpStatusCode.Created)
                    {
                        refreshProject = true;
                    }
                }

                // (Konrad) Publish Workset information to database based on current state.
                switch (state)
                {
                    case WorksetMonitorState.onOpen:
                        ServerUtil.PostWorksetInfo(worksetInfo, worksetDocumentId, state);
                        break;
                    case WorksetMonitorState.onSynch:
                        ServerUtil.PostWorksetInfo(worksetInfo, worksetDocumentId, state);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
            catch (Exception e)
            {
                LogUtil.AppendLog("WorksetOpenMonitor-PublishData: " + e.Message);
            }
        }
    }
}

