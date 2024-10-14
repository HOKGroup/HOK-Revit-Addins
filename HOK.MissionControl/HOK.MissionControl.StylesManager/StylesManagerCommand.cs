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

namespace HOK.MissionControl.StylesManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class StylesManagerCommand : ExternalCommand
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
                    new AddinLog("MissionControl-StylesManager", Application.VersionNumber));

                var vm = new StylesManagerViewModel(doc);
                var v = new StylesManagerView
                {
                    DataContext = vm
                };

                var unused = new WindowInteropHelper(v)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                v.Show();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
