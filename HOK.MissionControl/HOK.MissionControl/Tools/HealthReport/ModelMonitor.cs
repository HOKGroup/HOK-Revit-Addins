using System;
using System.IO;
using System.Linq;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Models;

namespace HOK.MissionControl.Tools.HealthReport
{
    public class ModelMonitor
    {
        /// <summary>
        /// Publishes data about Model Synch duration.
        /// </summary>
        /// <param name="filePath">Revit Document central file path.</param>
        public void PublishSynchTime(string filePath)
        {
            try
            {
                var from = AppCommand.SynchTime["from"];
                var to = DateTime.UtcNow;
                var span = to - from;
                var ms = (int)span.TotalMilliseconds;

                var eventItem = new ModelEventData
                {
                    CentralPath = filePath.ToLower(),
                    Value = ms,
                    User = Environment.UserName.ToLower()
                };

                if (!ServerUtilities.Post(eventItem, "model/synchtimes", out ModelEventData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Model Synch Times Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Publishes data about Model Opening duration.
        /// </summary>
        /// <param name="filePath">Revit Document central file path.</param>
        public void PublishOpenTime(string filePath)
        {
            try
            {
                var from = AppCommand.OpenTime["from"];
                var to = DateTime.UtcNow;
                var span = to - from;
                var ms = (int)span.TotalMilliseconds;

                var eventItem = new ModelEventData
                {
                    CentralPath = filePath.ToLower(),
                    Value = ms,
                    User = Environment.UserName.ToLower()
                };

                if (!ServerUtilities.Post(eventItem, "model/opentimes", out ModelEventData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Model Open Times Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Publishes information about linked models/images/object styles in the model.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="filePath">Revit Document central file path.</param>
        /// <param name="version">Revit file version.</param>
        public void PublishModelSize(Document doc, string filePath, string version)
        {
            try
            {
                long fileSize;
                try
                {
                    bool isRevitServer = filePath.StartsWith("rsn://", StringComparison.OrdinalIgnoreCase);
                    if (isRevitServer)
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

                        var result = ServerUtilities.GetFileInfoFromRevitServer<RsFileInfo>(clientPath, requestPath);
                        fileSize = result.ModelSize - result.SupportSize;
                    }
                    else if (!isRevitServer && filePath.Contains("://"))
                    {
                        // (Konrad) For BIM 360 files, we can just pull the file size from local cached file.
                        // It's pretty much what is stored in the web, so will work for our purpose.
                        var fileName = doc.WorksharingCentralGUID + ".rvt";
                        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var collaborationDir = Path.Combine(localAppData,
                            "Autodesk\\Revit\\Autodesk Revit " + doc.Application.VersionNumber, "CollaborationCache");

                        var file = Directory.GetFiles(collaborationDir, fileName, SearchOption.AllDirectories)
                            .FirstOrDefault(x => new FileInfo(x).Directory?.Name != "CentralCache");
                        if (file == null) return;

                        var fileInfo = new FileInfo(file);
                        fileSize = fileInfo.Length;
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

                var eventItem = new ModelEventData
                {
                    CentralPath = filePath.ToLower(),
                    Value = fileSize,
                    User = Environment.UserName.ToLower()
                };

                if (!ServerUtilities.Post(eventItem, "model/modelsizes", out ModelEventData unused))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish Model Size Data.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
