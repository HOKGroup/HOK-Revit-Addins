using System;
using System.IO;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class ModelMonitor
    {
        /// <summary>
        /// Publishes data about Model Synch duration.
        /// </summary>
        public void PublishSynchTime(string recordId)
        {
            try
            {
                var from = AppCommand.SynchTime["from"];
                var to = DateTime.UtcNow;
                var span = to - from;
                var ms = (int)span.TotalMilliseconds;

                var eventItem = new EventData
                {
                    value = ms,
                    user = Environment.UserName.ToLower()
                };

                var unused = ServerUtilities.Post<EventData>(eventItem, "healthrecords/" + recordId + "/modelsynchtime");
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
        public void PublishOpenTime(string recordId)
        {
            try
            {
                var from = AppCommand.OpenTime["from"];
                var to = DateTime.UtcNow;
                var span = to - from;
                var ms = (int)span.TotalMilliseconds;

                var eventItem = new EventData
                {
                    value = ms,
                    user = Environment.UserName.ToLower()
                };

                var unused = ServerUtilities.Post<EventData>(eventItem, "healthrecords/" + recordId + "/modelopentime");
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
        /// <param name="version">Revit file version.</param>
        public void PublishModelSize(string filePath, string recordId, string version)
        {
            try
            {
                long fileSize;
                try
                {
                    if (filePath.StartsWith("RSN://"))
                    {
                        var server = string.Empty;
                        var subfolder = string.Empty;

                        // (Konrad) This is a Revit Server project. To get it's size one must use REST API,
                        // and ping the server that it's hosted on.
                        var splits = filePath.Split('/');
                        for (var i = 2; i < splits.Length; i++)
                        {
                            if (i == 2) server = splits[i];
                            else subfolder = subfolder + splits[i] + (i == splits.Length - 1 ? string.Empty : "|");
                        }
                        var clientPath = "http://" + server;
                        var requestPath = "RevitServerAdminRestService" + version + "/" + "AdminRestService.svc/" +
                                          subfolder + "/modelinfo";

                        fileSize = ServerUtilities.GetFileInfoFromRevitServer(clientPath, requestPath);
                    }
                    else
                    {
                        var fileInfo = new FileInfo(filePath);
                        fileSize = fileInfo.Length;
                    }
                }
                catch (Exception ex)
                {
                    fileSize = 0;
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                var eventItem = new EventData
                {
                    value = fileSize,
                    user = Environment.UserName.ToLower()
                };

                ServerUtilities.Post<EventData>(eventItem, "healthrecords/" + recordId + "/modelsize");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
