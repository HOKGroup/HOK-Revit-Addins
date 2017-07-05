using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Autodesk.Revit.DB;
using HOK.Core;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class WorksetItemCount
    {
        public static Guid UpdaterGuid { get; set; } = new Guid(Properties.Resources.HealthReportTrackerGuid);

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
                if (!MonitorUtilities.IsUpdaterOn(project, config, UpdaterGuid)) return;

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

                // (Konrad) It's possible that Workset Document doesn't exist in database yet.
                // Create it and set the reference to it in Project if that's the case.
                var worksetDocumentId = project.worksets.FirstOrDefault();
                if (string.IsNullOrEmpty(worksetDocumentId))
                {
                    worksetDocumentId = ServerUtilities.PostDataScheme(new HealthReportData(), "worksets").Id;
                    var status = ServerUtilities.AddWorksetToProject(project, worksetDocumentId);
                    if (status == HttpStatusCode.Created) refreshProject = true;
                }

                // (Konrad) Publish information about workset counts in the model.
                ServerUtilities.PostWorksetCounts(new WorksetItem { worksets = worksetInfo }, worksetDocumentId);
            }
            catch (Exception e)
            {
                LogUtilities.AppendLog("WorksetOpenMonitor-PublishData: " + e.Message);
            }
        }
    }
}