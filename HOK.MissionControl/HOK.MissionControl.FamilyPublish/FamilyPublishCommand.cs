#region References

using System;
using System.Windows.Interop;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.FamilyPublish.Utilities;
using Visibility = System.Windows.Visibility;
using Nice3point.Revit.Toolkit.External;

#endregion

namespace HOK.MissionControl.FamilyPublish
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FamilyPublishCommand : ExternalCommand
    {
        public override void Execute()
        {
            var uiApp = Context.UiApplication;
            var doc = Context.ActiveDocument;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-PublishFamilyData", Context.Application.VersionNumber));

                var pathName = doc.PathName;
                if (string.IsNullOrEmpty(pathName))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Path is Empty. File was not saved yet.");
                    var dialog = new FamilyMonitorView
                    {
                        DataContext =
                            new FamilyMonitorViewModel(null,
                                "...establish a connection to Mission Control for unsaved files."),
                        CancelButton = { Visibility = Visibility.Collapsed }
                    };
                    dialog.ShowDialog();

                    Result = Result.Failed;
                    return;
                }

                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrEmpty(centralPath))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Could not get Central Path.");
                    var dialog = new FamilyMonitorView
                    {
                        DataContext =
                            new FamilyMonitorViewModel(null,
                                "...get a Central File Path. Only Workshared projects can be added to Mission Control."),
                        CancelButton = { Visibility = Visibility.Collapsed }
                    };
                    dialog.ShowDialog();

                    Result = Result.Failed;
                    return;
                }

                if (!MissionControlSetup.Projects.ContainsKey(centralPath) || 
                    !MissionControlSetup.Configurations.ContainsKey(centralPath) || 
                    !MissionControlSetup.FamilyData.ContainsKey(centralPath))
                {
                    Log.AppendLog(LogMessageType.ERROR, "No Config Found.");
                    var dialog = new FamilyMonitorView
                    {
                        DataContext =
                            new FamilyMonitorViewModel(null,
                                "...find your project in Mission Control database. Please make sure that it was added."),
                        CancelButton = { Visibility = Visibility.Collapsed }
                    };
                    dialog.ShowDialog();

                    Result = Result.Failed;
                    return;
                }

                uiApp.Application.FailuresProcessing += FailureProcessing;
                var familiesId = MissionControlSetup.FamilyData[centralPath].Id;
                var model = new FamilyMonitorModel(doc, MissionControlSetup.Configurations[centralPath], MissionControlSetup.Projects[centralPath], familiesId, centralPath);
                var viewModel = new FamilyMonitorViewModel(model, "...make this any faster. Hang in there!")
                {
                    ExecuteFamilyPublish = true
                };
                var view = new FamilyMonitorView
                {
                    DataContext = viewModel,
                    CloseButton = { Visibility = Visibility.Collapsed }
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

            uiApp.Application.FailuresProcessing -= FailureProcessing;
            Log.AppendLog(LogMessageType.INFO, "Ended");
        }

        /// <summary>
        /// Error handler for "Constraints are not satisfied" that pops up frequently w/ Families.
        /// </summary>
        private static void FailureProcessing(object sender, FailuresProcessingEventArgs args)
        {
            var fa = args.GetFailuresAccessor();
            var fmas = fa.GetFailureMessages();
            var count = 0;

            if (fmas.Count == 0)
            {
                args.SetProcessingResult(FailureProcessingResult.Continue);
                return;
            }
            foreach (var fma in fmas)
            {
                if (fma.GetSeverity() == FailureSeverity.Warning)
                {
                    fa.DeleteWarning(fma);
                }
                else
                {
                    fa.ResolveFailure(fma);
                    count++;
                }
            }

            if (count > 0)
            {
                args.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            }
        }
    }
}
