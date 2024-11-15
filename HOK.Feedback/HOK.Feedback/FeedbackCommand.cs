using System;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.Feedback
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FeedbackCommand : ExternalCommand
    {
        public override void Execute()
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontinue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("Feedback Tool", Application.VersionNumber));

                var title = "HOK Feedback Tool v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var model = new FeedbackModel();
                var viewModel = new FeedbackViewModel(model, title);
                var view = new FeedbackView
                {
                    DataContext = viewModel
                };

                var unused = new WindowInteropHelper(view)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                view.ShowDialog();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
