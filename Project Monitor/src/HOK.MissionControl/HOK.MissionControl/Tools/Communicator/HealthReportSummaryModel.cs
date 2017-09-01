using Autodesk.Revit.UI;

namespace HOK.MissionControl.Tools.Communicator
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
    }
}
