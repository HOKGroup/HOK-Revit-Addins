using System.Diagnostics;
using Autodesk.Revit.UI;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class HealthReportSummaryModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void LaunchCommand(string text)
        {
            var commandName = string.Empty;
            switch (text)
            {
                case "Links Manager":
                    commandName =
                        "CustomCtrl_%CustomCtrl_%   HOK   %Mission Control%LinksManager_Command";
                    break;
                case "Groups Manager":
                    commandName =
                        "CustomCtrl_%CustomCtrl_%   HOK   %Mission Control%GroupManager_Command";
                    break;
                case "Styles Manager":
                    commandName =
                        "CustomCtrl_%CustomCtrl_%   HOK   %Mission Control%StylesManager_Command";
                    break;
            }

            if (string.IsNullOrEmpty(commandName)) return;

            var addinId = RevitCommandId.LookupCommandId(commandName);
            if (addinId == null) return;

            AppCommand.EnqueueTask(app =>
            {
                app.PostCommand(addinId);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void LaunchWebsite()
        {
            AppCommand.EnqueueTask(app =>
            {
                var doc = app.ActiveUIDocument.Document;
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrWhiteSpace(centralPath)) return;

                if (MissionControlSetup.Projects.ContainsKey(centralPath))
                {
                    Process.Start(ServerUtilities.RestApiBaseUrl + "/#/projects/healthreport/" + MissionControlSetup.Projects[centralPath].Id);
                }
            });
        }
    }
}
