using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;

namespace HOK.Arrowhead
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ArrowCommand : IExternalCommand
    {
        private UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            Log.AppendLog(LogMessageType.INFO, "Started.");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            var unused1 = AddinUtilities.PublishAddinLog(
                new AddinLog("Utilities-Arrowhead", m_app.Application.VersionNumber), LogPosted);

            var assignerWindow = new HeadAssignerWindow(m_app);
            if (assignerWindow.ShowDialog() == true)
            {
                assignerWindow.Close();
            }

            Log.AppendLog(LogMessageType.INFO, "Ended.");
            return Result.Succeeded;
        }

        /// <summary>
        /// Callback method for when Addin-info is published.
        /// </summary>
        /// <param name="data"></param>
        private static void LogPosted(AddinData data)
        {
            Log.AppendLog(LogMessageType.INFO, "Addin info was published: " 
                + (string.IsNullOrEmpty(data.Id) ? "Unsuccessfully." : "Successfully."));
        }
    }
}
