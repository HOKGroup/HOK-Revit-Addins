using System;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.BetaToolsManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class AddinInstallerCommand : IExternalCommand
    {
        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var unused1 = AddinUtilities.PublishAddinLog(new AddinLog("Beta-BetaInstaller", commandData.Application.Application.VersionNumber), LogPosted);

                var model = AppCommand.Instance.ViewModel;
                var viewModel = new AddinInstallerViewModel(model);
                var view = new AddinInstallerWindow
                {
                    DataContext = viewModel
                };
                var unused = new WindowInteropHelper(view)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                view.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        private static void LogPosted(AddinData data)
        {
            StatusBarManager.StatusLabel.Text = string.IsNullOrEmpty(data.Id) ? "Failed to publish Addin Log." : "Ready";
        }
    }
}
