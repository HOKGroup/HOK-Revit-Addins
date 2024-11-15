using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;
using Nice3point.Revit.Toolkit.External;

namespace HOK.Arrowhead
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ArrowCommand : ExternalCommand
    {
        private UIApplication m_app;

        public override void Execute()
        {
            m_app = Context.UiApplication;
            Log.AppendLog(LogMessageType.INFO, "Started.");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-Arrowhead", m_app.Application.VersionNumber));

            var assignerWindow = new HeadAssignerWindow(m_app);
            if (assignerWindow.ShowDialog() == true)
            {
                assignerWindow.Close();
            }

            Log.AppendLog(LogMessageType.INFO, "Ended.");
        }
    }
}
