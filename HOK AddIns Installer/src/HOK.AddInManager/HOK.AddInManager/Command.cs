using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.AddInManager.Classes;
using HOK.AddInManager.UserControls;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.AddInManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        private readonly Dictionary<string/*toolName*/, LoadType> tempSettings = new Dictionary<string, LoadType>();

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                var addins = AppCommand.thisApp.addins;
                StoreTempCollection(addins.AddinCollection);
                var viewModel = new AddInViewModel(addins);
                var mainWindow = new MainWindow { DataContext = viewModel };
                if (mainWindow.ShowDialog() == true)
                {
                    var vm = (AddInViewModel)mainWindow.DataContext;
                    try
                    {
                        // (Konrad) We are gathering information about the addin use. This allows us to
                        // better maintain the most used plug-ins or discontiue the unused ones.
                        // If Window was closed using the OK button we can collect more details about the app to publish.
                        var log = new AddinLog("AddinManager", commandData.Application.Application.VersionNumber)
                        {
                            detailInfo = vm.AddinsObj.AddinCollection
                                .Select(x => new InfoItem { name = x.ToolName, value = x.ToolLoadType.ToString() })
                                .ToList()
                        };
                        var unused1 = AddinUtilities.PublishAddinLog(log, LogPosted);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }

                    // write setting and load addins.
                    AppCommand.thisApp.addins = vm.AddinsObj;
                    AppCommand.thisApp.ProcessPlugins();
                }
                else
                {
                    // If user cancelled out of this window, we don't need to log all the details, other than that it was opened.
                    var unused1 = AddinUtilities.PublishAddinLog(
                        new AddinLog("AddinManager", commandData.Application.Application.VersionNumber), LogPosted);

                    OverrideTempSettings();
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origCollection"></param>
        private void StoreTempCollection(ObservableCollection<AddinInfo> origCollection)
        {
            try
            {
                tempSettings.Clear();
                foreach (var addin in origCollection)
                {
                    tempSettings.Add(addin.ToolName, addin.ToolLoadType);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OverrideTempSettings()
        {
            try
            {
                foreach (var addinName in tempSettings.Keys)
                {
                    var addinFound = AppCommand.thisApp.addins.AddinCollection
                        .FirstOrDefault(x => x.ToolName == addinName);
                    if (addinFound == null) continue;

                    var index = AppCommand.thisApp.addins.AddinCollection.IndexOf(addinFound);
                    AppCommand.thisApp.addins.AddinCollection[index].ToolLoadType = tempSettings[addinName];
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }
}
