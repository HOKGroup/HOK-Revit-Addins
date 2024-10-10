using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.ViewDepth
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ViewCommand : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public override void Execute()
        {
            m_app = Context.UiApplication;
            m_doc = m_app.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-ViewDepth", Application.VersionNumber));

            var overrideViewDepth = new OverrideViewDepth(m_app);

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
