using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;

namespace HOK.LevelManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class LevelCommand : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public override void Execute()
        {
            try
            {
                m_app = Context.UiApplication;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-LevelManager", Application.VersionNumber));

                var managerForm = new LevelManagerForm(m_app);
                managerForm.ShowDialog();

                Log.AppendLog(LogMessageType.INFO, "Ended");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
