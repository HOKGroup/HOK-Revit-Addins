using System;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.MissionControl.Tools.Communicator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CommunicatorCommand : ExternalCommand
    {
         public override void Execute()
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(
                new AddinLog("MissionControl-Communicator", Application.VersionNumber));

            ToggleCommunicator(Context.UiApplication);

            Log.AppendLog(LogMessageType.INFO, "Ended");
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
