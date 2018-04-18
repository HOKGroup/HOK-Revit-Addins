using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Autodesk.Revit.DB;

namespace HOK.LevelManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class LevelCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var unused1 = AddinUtilities.PublishAddinLog(
                    new AddinLog("Utilities-LevelManager", commandData.Application.Application.VersionNumber), LogPosted);

                var managerForm = new LevelManagerForm(m_app);
                managerForm.ShowDialog();

                Log.AppendLog(LogMessageType.INFO, "Ended");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return Result.Cancelled;
            }
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
