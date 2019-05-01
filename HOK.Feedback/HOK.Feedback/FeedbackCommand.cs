using System;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.Feedback
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FeedbackCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontinue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("Feedback Tool", commandData.Application.Application.VersionNumber));

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
            return Result.Succeeded;
        }
    }
}
