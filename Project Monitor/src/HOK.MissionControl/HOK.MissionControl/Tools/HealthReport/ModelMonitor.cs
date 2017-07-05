using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.HealthReport
{
    /// <summary>
    /// Three different Document States that Model info gets updated during.
    /// </summary>
    public enum SessionEvent
    {
        documentOpened = 0,
        documentSynched = 1,
        documentClosed = 2
    }

    public class ModelMonitor
    {
        public static Guid UpdaterGuid { get; set; } = new Guid(Properties.Resources.HealthReportTrackerGuid);

        /// <summary>
        /// Publishes data about Session duration and Synch intervals.
        /// </summary>
        /// <param name="worksetDocumentId">ObjectId of the Workset document.</param>
        /// <param name="state">Current document state.</param>
        public static void PublishSessionInfo(string worksetDocumentId, SessionEvent state)
        {
            try
            {
                switch (state)
                {
                    case SessionEvent.documentOpened:
                        AppCommand.SessionInfo = new SessionInfo
                        {
                            user = Environment.UserName,
                            from = DateTime.Now
                        };
                        var guid = ServerUtilities.PostToMongoDB(AppCommand.SessionInfo, "worksets", worksetDocumentId, "sessioninfo").Id;
                        AppCommand.SessionInfo.Id = guid;
                        break;
                    case SessionEvent.documentSynched:
                        if (AppCommand.SessionInfo != null)
                        {
                            var objectId = AppCommand.SessionInfo.Id;
                            ServerUtilities.UpdateSessionInfo(worksetDocumentId, objectId, "putSynchTime");
                        }
                        break;
                    case SessionEvent.documentClosed:
                        if (AppCommand.SessionInfo != null)
                        {
                            var objectId = AppCommand.SessionInfo.Id;
                            ServerUtilities.UpdateSessionInfo(worksetDocumentId, objectId, "putToTime");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
            catch (Exception e)
            {
                Log.AppendLog("ModelMonitor-PublishSessionInfo: " + e.Message);
            }
        }

        /// <summary>
        /// Publishes data about Model Synch duration.
        /// </summary>
        /// <param name="worksetDocumentId">ObjectId of the Workset document.</param>
        public static void PublishSynchTime(string worksetDocumentId)
        {
            try
            {
                var from = AppCommand.SynchTime["from"];
                var to = DateTime.Now;
                var span = to - from;
                var ms = (int)span.TotalMilliseconds;

                var eventItem = new EventTime
                {
                    value = ms
                };

                ServerUtilities.PostStats(eventItem, worksetDocumentId, "modelsynchtime");
            }
            catch (Exception e)
            {
                Log.AppendLog("ModelMonitor-PublishOpenTime: " + e.Message);
            }
        }

        /// <summary>
        /// Publishes data about Model Opening duration.
        /// </summary>
        /// <param name="worksetDocumentId">ObjectId of the Workset document.</param>
        public static void PublishOpenTime(string worksetDocumentId)
        {
            try
            {
                var from = AppCommand.OpenTime["from"];
                var to = DateTime.Now;
                var span = to - from;
                var ms = (int)span.TotalMilliseconds;

                var eventItem = new EventTime
                {
                    value = ms
                };

                ServerUtilities.PostStats(eventItem, worksetDocumentId, "modelopentime");
            }
            catch (Exception e)
            {
                Log.AppendLog("ModelMonitor-PublishOpenTime: " + e.Message);
            }
        }

        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        /// <param name="centralPath"></param>
        public static void PublishModelSize(Document doc, Configuration config, Project project, string centralPath)
        {
            try
            {
                if (!MonitorUtilities.IsUpdaterOn(project, config, UpdaterGuid)) return;
                var worksetDocumentId = project.worksets.FirstOrDefault();
                if (string.IsNullOrEmpty(worksetDocumentId)) return;

                long fileSize = 0;
                try
                {
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        var fileInfo = new FileInfo(centralPath);
                        fileSize = fileInfo.Length;
                    }
                }
                catch (Exception e)
                {
                    fileSize = 0;
                    Log.AppendLog("ModelMonitor-PublishModelSize-GetFileInfo: " + e.Message);
                }

                var eventItem = new EventTime
                {
                    value = fileSize
                };

                ServerUtilities.PostStats(eventItem, worksetDocumentId, "modelsize");
            }
            catch (Exception e)
            {
                Log.AppendLog("ModelMonitor-PublishModelSize: " + e.Message);
            }
        }
    }
}
