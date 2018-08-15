using System;
using System.Reflection;
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
            AddinUtilities.PublishAddinLog(
                new AddinLog("MissionControl-Communicator", commandData.Application.Application.VersionNumber));

            ToggleCommunicator(commandData.Application);

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        /// <summary>
        /// Shows or Hides the Communicator dockable pane.
        /// </summary>
        /// <param name="application">UIApp</param>
        public void ToggleCommunicator(UIApplication application)
        {
            var dpid = new DockablePaneId(new Guid(Properties.Resources.CommunicatorGuid));
            var dp = application.GetDockablePane(dpid);
            if (dp == null) return;

            var assembly = Assembly.GetExecutingAssembly();
            if (dp.IsShown())
            {
                dp.Hide();
                AppCommand.Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", "communicatorOff_32x32.png");
                AppCommand.Instance.CommunicatorButton.ItemText = "Show" + Environment.NewLine + "Communicator";
            }
            else
            {
                dp.Show();
                AppCommand.Instance.CommunicatorButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, "HOK.MissionControl", "communicatorOn_32x32.png");
                AppCommand.Instance.CommunicatorButton.ItemText = "Hide" + Environment.NewLine + "Communicator";
            }
        }
    }
}
