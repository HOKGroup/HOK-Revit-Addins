using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.Tools.Communicator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CommunicatorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("MissionControl-Communicator", commandData.Application.Application.VersionNumber));

            AppCommand.Instance.ToggleCommunicator(commandData.Application);

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }
    }
}
