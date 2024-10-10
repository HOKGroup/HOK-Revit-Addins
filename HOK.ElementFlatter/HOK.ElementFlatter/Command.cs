using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.ElementFlatter
{
    [Transaction(TransactionMode.Manual)]
    public class Command : ExternalCommand
    {
        private UIApplication m_app;

        public override void Execute()
        {
            m_app = Context.UiApplication;
            Log.AppendLog(LogMessageType.INFO, "Started.");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(
                new AddinLog("ElementFlatter", m_app.Application.VersionNumber));

            var viewModel = new CommandViewModel(m_app);
            var cmdWindow = new CommandWindow
            {
                DataContext = viewModel
            };
            cmdWindow.ShowDialog();

            Log.AppendLog(LogMessageType.INFO, "Ended.");
        }
    }
}
