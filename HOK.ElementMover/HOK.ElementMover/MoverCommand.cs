using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.ElementMover
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class MoverCommand : ExternalCommand
    {
        private UIApplication m_app;

        private Dictionary<ElementId, LinkedInstanceProperties> linkInstances = new Dictionary<ElementId, LinkedInstanceProperties>();

        public override void Execute()
        {
            m_app = Context.UiApplication;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(
                new AddinLog("ElementMover", Application.VersionNumber));

            AppCommand.thisApp.ShowMover(m_app);

            Log.AppendLog(LogMessageType.INFO, "Ended.");
        }
    }
}
