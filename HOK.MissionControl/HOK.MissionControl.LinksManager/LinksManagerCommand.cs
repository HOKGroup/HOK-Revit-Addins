using System;
using System.Windows.Interop;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.MissionControl.LinksManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class LinksManagerCommand : ExternalCommand
    {
        public override void Execute()
        {
            var uiApp = Context.Application;
            var doc = Context.ActiveDocument;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-LinksManager", Application.VersionNumber));

                var viewModel = new LinksManagerViewModel(doc);
                var view = new LinksManagerView
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
