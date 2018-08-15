using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.SheetManager.AddIn
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        private UIApplication m_app;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Beta-SheetManager", commandData.Application.Application.VersionNumber));

                m_app = commandData.Application;
                AppCommand.thisApp.ShowWindow(m_app);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }
    }
}
