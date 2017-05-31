using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Classes;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.WorksetMonitor
{
    public class WorksetItemCount
    {
        public static Guid UpdaterGuid { get; set; } = new Guid("56603be6-aeb2-45d0-9ebc-2830fad6368b");

        /// <summary>
        /// Publishes Workset Item Count data when Document is closed.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        /// <param name="centralPath">Document central file path.</param>
        /// <param name="refreshProject"></param>
        public static void PublishData(Document doc, Configuration config, Project project, string centralPath, out bool refreshProject)
        {
            refreshProject = false;
            try
            {
                var worksetDocumentId = project.worksets.FirstOrDefault();
                var updaterFound = config.updaters
                    .Where(x => string.Equals(x.updaterId.ToLower(), UpdaterGuid.ToString().ToLower(),
                        StringComparison.Ordinal))
                    .ToList();

                if (!updaterFound.Any() || !updaterFound.First().isUpdaterOn) return;

                var worksets = new FilteredWorksetCollector(doc)
                    .OfKind(WorksetKind.UserWorkset)
                    .ToWorksets();

                var worksetInfo = new List<Item>();
                foreach (var w in worksets)
                {
                    var worksetFilter = new ElementWorksetFilter(w.Id, false);
                    var count = new FilteredElementCollector(doc)
                        .WherePasses(worksetFilter)
                        .Count();

                    worksetInfo.Add(new Item {name = w.Name, count = count});
                }

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

                // (Konrad) Publish information about workset counts in the model.
                ServerUtil.PostWorksetCounts(new WorksetItem{worksets = worksetInfo}, worksetDocumentId);
            }
            catch (Exception e)
            {
                LogUtil.AppendLog("WorksetOpenMonitor-PublishData: " + e.Message);
            }
        }
    }
}
