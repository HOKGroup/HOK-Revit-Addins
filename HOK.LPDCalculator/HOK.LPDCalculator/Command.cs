using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.LPDCalculator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public override void Execute()
        {
            m_app = Context.UiApplication;
            m_doc = Context.ActiveDocument;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("ViewAnalysis-LPD Analysis", Application.VersionNumber));

            var docPath = RevitDocument.GetCentralPath(m_doc);
            if (!string.IsNullOrEmpty(docPath))
            {
                var commandForm = new CommandForm(m_app);
                commandForm.ShowDialog();
            }
            else
            {
                Log.AppendLog(LogMessageType.WARNING, "File not saved");
                MessageBox.Show(Properties.Resources.Command_FileNotSaved, Properties.Resources.Command_FileNotSavedHeader, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
