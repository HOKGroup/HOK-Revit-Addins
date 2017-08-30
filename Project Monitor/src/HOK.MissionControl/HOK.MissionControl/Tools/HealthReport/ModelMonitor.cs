using System;
using System.IO;
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
        /// <param name="recordId">Id of the HealthRecord in MongoDB.</param>
        /// <param name="state">Current document state.</param>
        public static void PublishSessionInfo(string recordId, SessionEvent state)
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
                        var guid = ServerUtilities.PostToMongoDB(AppCommand.SessionInfo, "healthrecords", recordId, "sessioninfo").Id;
                        AppCommand.SessionInfo.Id = guid;
                        break;
                    case SessionEvent.documentSynched:
                        if (AppCommand.SessionInfo != null)
                        {
                            var objectId = AppCommand.SessionInfo.Id;
                            ServerUtilities.UpdateSessionInfo(recordId, objectId, "putSynchTime");
                        }
                        break;
                    case SessionEvent.documentClosed:
                        if (AppCommand.SessionInfo != null)
                        {
                            var objectId = AppCommand.SessionInfo.Id;
                            ServerUtilities.UpdateSessionInfo(recordId, objectId, "putToTime");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Publishes data about Model Synch duration.
        /// </summary>
        public static void PublishSynchTime(string recordId)
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

                ServerUtilities.PostToMongoDB(eventItem, "healthrecords", recordId, "modelsynchtime");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Publishes data about Model Opening duration.
        /// </summary>
        /// <param name="recordId">Id of the HealthRecord in MongoDB.</param>
        public static void PublishOpenTime(string recordId)
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

                ServerUtilities.PostToMongoDB(eventItem, "healthrecords", recordId, "modelopentime");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        /// <param name="filePath">Revit Document central file path.</param>
        /// <param name="recordId">Id of the HealthRecord in MongoDB.</param>
        /// <param name="config">Configuration for the model.</param>
        /// <param name="project">Project for the model.</param>
        public static void PublishModelSize(string filePath, string recordId, Configuration config, Project project)
        {
            try
            {
                if (!MonitorUtilities.IsUpdaterOn(project, config, UpdaterGuid)) return;

                long fileSize;
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    fileSize = fileInfo.Length;
                }
                catch (Exception ex)
                {
                    fileSize = 0;
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                var eventItem = new EventTime
                {
                    value = fileSize
                };

                ServerUtilities.PostToMongoDB(eventItem, "healthrecords", recordId, "modelsize");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
