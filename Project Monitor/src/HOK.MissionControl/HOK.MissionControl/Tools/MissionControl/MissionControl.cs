using System;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.MissionControl
{
    public class MissionControl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void CheckIn(Document doc)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var configFound = ServerUtilities.GetByCentralPath<Configuration>(centralPath, "configurations/centralpath");
                if (configFound == null)
                {
                    DisableMissionControl();
                    return;
                }

                var projectFound = ServerUtilities.Get<Project>("projects/configid/" + configFound.Id);
                if (projectFound == null)
                {
                    DisableMissionControl();
                    return;
                }

                if (MissionControlSetup.Configurations.ContainsKey(centralPath)) MissionControlSetup.Configurations.Remove(centralPath);
                MissionControlSetup.Configurations.Add(centralPath, configFound);

                if (MissionControlSetup.Projects.ContainsKey(centralPath)) MissionControlSetup.Projects.Remove(centralPath);
                MissionControlSetup.Projects.Add(centralPath, projectFound);

                AppCommand.OpenTime["from"] = DateTime.UtcNow;
                AppCommand.CommunicatorHandler.Status = Status.Success;
                AppCommand.CommunicatorHandler.Message = "Successfully connected to Mission Control!";
                AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                AppCommand.CommunicatorEvent.Raise();

                Log.AppendLog(LogMessageType.INFO, "Raising Status Window event. Status: Success. Message: Mission Control check in succeeded.");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static void DisableMissionControl()
        {
            AppCommand.CommunicatorHandler.Request.Make(RequestId.Disable);
            AppCommand.CommunicatorEvent.Raise();
        }
    }
}
