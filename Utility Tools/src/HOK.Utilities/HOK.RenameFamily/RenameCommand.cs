using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;
using Autodesk.Revit.DB;

namespace HOK.RenameFamily
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class RenameCommand : IExternalCommand
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
                    new AddinLog("Utilities-RenameFamily", commandData.Application.Application.VersionNumber), LogPosted);

                var viewModel = new RenameViewModel(commandData.Application);
                var window = new RenameWindow
                {
                    DataContext = viewModel
                };
                window.ShowDialog();

                Log.AppendLog(LogMessageType.INFO, "Ended");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
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
