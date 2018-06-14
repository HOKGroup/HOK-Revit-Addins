using System;
using System.Diagnostics;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.StylesManager
{
    [Name(nameof(Properties.Resources.StylesManager_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.StylesManager_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.StylesManager_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.StylesManager_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.StylesManager_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.StylesManager_Namespace), typeof(Properties.Resources))]
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class StylesManagerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var unused1 = AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-StylesManager", commandData.Application.Application.VersionNumber), LogPosted);

                var vm = new StylesManagerViewModel(doc);
                var v = new StylesManagerView
                {
                    DataContext = vm
                };

                var unused = new WindowInteropHelper(v)
                {
                    Owner = Process.GetCurrentProcess().MainWindowHandle
                };

                v.ShowDialog();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        /// <summary>
        /// Callback method for when Addin-info is published.
        /// </summary>
        /// <param name="data"></param>
        private static void LogPosted(AddinData data)
        {
            Log.AppendLog(LogMessageType.INFO, "Addin info was published: "
                + (string.IsNullOrEmpty(data.Id) ? "Unsuccessfully." : "Successfully."));
        }
    }
}
