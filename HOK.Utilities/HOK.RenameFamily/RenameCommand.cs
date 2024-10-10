using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.Core.Utilities;
using Autodesk.Revit.DB;
using Nice3point.Revit.Toolkit.External;

namespace HOK.RenameFamily
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class RenameCommand : ExternalCommand
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
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-RenameFamily", Application.VersionNumber));

                var viewModel = new RenameViewModel(m_app);
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
        }
    }
}
