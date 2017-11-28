using System.Diagnostics;
using Autodesk.Revit.UI;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.Communicator.HealthReport
{
    public class HealthReportSummaryModel
    {
        public void LaunchCommand(string text)
        {
            var commandName = string.Empty;
            switch (text)
            {
                case "Links Manager":
                    commandName =
                        "CustomCtrl_%CustomCtrl_%  HOK - Beta%Mission Control%Links Manager_Command";
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

        public void LaunchWebsite()
        {
            AppCommand.EnqueueTask(app =>
            {
                var doc = app.ActiveUIDocument.Document;
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrEmpty(centralPath)) return;

                if (MissionControlSetup.Projects.ContainsKey(centralPath))
                {
                    Process.Start("http://missioncontrol.hok.com/#/projects/healthreport/" + MissionControlSetup.Projects[centralPath].Id);
                }
            });
        }
    }
}
