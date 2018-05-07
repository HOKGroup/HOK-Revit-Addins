#region References
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
#endregion

namespace HOK.MissionControl.Tools.DTMTool
{
    public static class DtmSynchOverrides
    {
        /// <summary>
        /// Creates an idling task that will bind our own Reload Latest command to existing one.
        /// </summary>
        public static void CreateReloadLatestOverride()
        {
            AppCommand.EnqueueTask(app =>
            {
                try
                {
                    var commandId = RevitCommandId.LookupCommandId("ID_WORKSETS_RELOAD_LATEST");
                    if (commandId == null || !commandId.CanHaveBinding) return;

                    var binding = app.CreateAddInCommandBinding(commandId);
                    binding.Executed += OnReloadLatest;
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                }
            });
        }

        /// <summary>
        /// Replacement method for reloading latest which disables the DTM Tool.
        /// </summary>
        public static void OnReloadLatest(object sender, ExecutedEventArgs args)
        {
            // (Konrad) This will disable the DTM Tool when we are reloading latest.
            AppCommand.IsSynching = true;

            // (Konrad) Reloads Latest.
            AppCommand.EnqueueTask(app =>
            {
                try
                {
                    Log.AppendLog(LogMessageType.INFO, "Reloading Latest...");

                    var reloadOptions = new ReloadLatestOptions();
                    var doc = app.ActiveUIDocument.Document;
                    doc.ReloadLatest(reloadOptions);
                    if (!doc.HasAllChangesFromCentral())
                    {
                        doc.ReloadLatest(reloadOptions);
                    }
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                }
            });

            // (Konrad) Turns the DTM Tool back on when reload is done.
            AppCommand.EnqueueTask(app =>
            {
                AppCommand.IsSynching = false;
            });
        }

        /// <summary>
        /// Creates and idling task that will bind our own Synch to Central command to existing one.
        /// </summary>
        public static void CreateSynchToCentralOverride()
        {
            AppCommand.EnqueueTask(app =>
            {
                try
                {
                    var commandId = RevitCommandId.LookupCommandId("ID_FILE_SAVE_TO_MASTER");
                    var commandId2 = RevitCommandId.LookupCommandId("ID_FILE_SAVE_TO_MASTER_SHORTCUT");
                    if (commandId == null || commandId2 == null || !commandId2.CanHaveBinding || !commandId.CanHaveBinding) return;

                    // (Konrad) We shouldn't be registering the same event handler more than once. It will throw an exception if we do.
                    // It would also potentially cause the event to be fired multiple times. 
                    if (!AppCommand.IsSynchOverriden)
                    {
                        var binding = app.CreateAddInCommandBinding(commandId);
                        binding.Executed += (sender, e) => OnSynchToCentral(sender, e, SynchType.Synch);
                        AppCommand.IsSynchOverriden = true;
                    }
                    if (!AppCommand.IsSynchNowOverriden)
                    {
                        var binding2 = app.CreateAddInCommandBinding(commandId2);
                        binding2.Executed += (sender, e) => OnSynchToCentral(sender, e, SynchType.SynchNow);
                        AppCommand.IsSynchNowOverriden = true;
                    }
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                }
            });
        }

        /// <summary>
        /// Ovveride method for when user synchs to central.
        /// The goal here is to disable the DTM Tool and prevent pop-ups while synching.
        /// </summary>
        public static void OnSynchToCentral(object sender, ExecutedEventArgs args, SynchType synchType)
        {
            // (Konrad) This will disable the DTM Tool when we are synching to central.
            AppCommand.IsSynching = true;

            RevitCommandId commandId;
            switch (synchType)
            {
                case SynchType.Synch:
                    commandId = RevitCommandId.LookupCommandId("ID_FILE_SAVE_TO_MASTER");
                    break;
                case SynchType.SynchNow:
                    commandId = RevitCommandId.LookupCommandId("ID_FILE_SAVE_TO_MASTER_SHORTCUT");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(synchType), synchType, null);
            }
            if (commandId == null || !commandId.CanHaveBinding) return;

            AppCommand.EnqueueTask(app =>
            {
                try
                {
                    app.RemoveAddInCommandBinding(commandId);

                    switch (synchType)
                    {
                        case SynchType.Synch:
                            AppCommand.IsSynchOverriden = false;
                            break;
                        case SynchType.SynchNow:
                            AppCommand.IsSynchNowOverriden = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(synchType), synchType, null);
                    }
                }
                catch (Exception e)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                }
            });

            AppCommand.EnqueueTask(app =>
            {
                // (Konrad) We can now post the same Command we were overriding since the override is OFF.
                app.PostCommand(commandId);

                // (Konrad) Once the command executes this will turn the override back ON.
                AppCommand.IsSynching = false;

                var doc = app.ActiveUIDocument.Document;
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (string.IsNullOrEmpty(centralPath)) return;

                // (Konrad) Let's turn the synch command override back on.
                var config = MissionControlSetup.Configurations[centralPath];
                foreach (var updater in config.Updaters)
                {
                    if (!updater.IsUpdaterOn) continue;

                    if (string.Equals(updater.UpdaterId.ToLower(),
                        AppCommand.Instance.DtmUpdaterInstance.UpdaterGuid.ToString().ToLower(), StringComparison.Ordinal))
                    {
                        CreateSynchToCentralOverride();
                    }
                }
            });
        }
    }

    public enum SynchType
    {
        Synch,
        SynchNow
    }
}
