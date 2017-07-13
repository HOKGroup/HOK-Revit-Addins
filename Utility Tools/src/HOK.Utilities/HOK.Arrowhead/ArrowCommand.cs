using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.Arrowhead
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ArrowCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started.");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-Arrowhead", m_doc));

            var assignerWindow = new HeadAssignerWindow(m_app);
            if (assignerWindow.ShowDialog() == true)
            {
                assignerWindow.Close();
            }

            Log.AppendLog(LogMessageType.INFO, "Ended.");
            return Result.Succeeded;
        }
    }
}
