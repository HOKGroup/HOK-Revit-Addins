#region References
using System;
using System.IO;
using System.Threading;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Links;
using HOK.MissionControl.Core.Schemas.Models;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Schemas.Styles;
using HOK.MissionControl.Core.Schemas.Views;
using HOK.MissionControl.Core.Schemas.Worksets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Tools.Communicator.Socket;
using HOK.MissionControl.Tools.HealthReport;
using HOK.MissionControl.Utils;
#endregion

namespace HOK.MissionControl.Tools.MissionControl
{
    public class MissionControl
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void CheckIn(Document doc)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                if (!ServerUtilities.GetByCentralPath(centralPath, "configurations/centralpath", out Configuration configFound))
                {
                    DisableMissionControl();
                    return;
                }

                if (!ServerUtilities.Get("projects/configid/" + configFound.Id, out Project projectFound))
                {
                    DisableMissionControl();
                    return;
                }

                if (MissionControlSetup.Configurations.ContainsKey(centralPath)) MissionControlSetup.Configurations.Remove(centralPath);
                MissionControlSetup.Configurations.Add(centralPath, configFound);

                if (MissionControlSetup.Projects.ContainsKey(centralPath)) MissionControlSetup.Projects.Remove(centralPath);
                MissionControlSetup.Projects.Add(centralPath, projectFound);

                // (Konrad) This might be a good time to let users know that Mission Control is ready to go.
                AppCommand.CommunicatorHandler.Status = Status.Success;
                AppCommand.CommunicatorHandler.Message = "Successfully connected to Mission Control!";
                AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                AppCommand.CommunicatorEvent.Raise();

                // (Konrad) Register Updaters that are in the config file.
                ApplyConfiguration(doc);

                Log.AppendLog(LogMessageType.INFO, "Raising Status Window event. Status: Success. Message: Mission Control check in succeeded.");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Registers availble configuration based on Central Model path match.
        /// </summary>
        private static void ApplyConfiguration(Document doc)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                var config = MissionControlSetup.Configurations[centralPath];
                foreach (var updater in config.updaters)
                {
                    if (!updater.isUpdaterOn) continue;

                    if (string.Equals(updater.updaterId, AppCommand.Instance.DoorUpdaterInstance.UpdaterGuid.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        // (Konrad) We need to register updaters from within Revit context.
                        // That's why we are running this in the Idling Event.
                        AppCommand.EnqueueTask(app =>
                        {
                            AppCommand.Instance.DoorUpdaterInstance.Register(doc, updater);
                        });
                    }
                    else if (string.Equals(updater.updaterId, AppCommand.Instance.DtmUpdaterInstance.UpdaterGuid.ToString(), 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessTriggerRecords(centralPath);

                        AppCommand.EnqueueTask(app =>
                        {
                            AppCommand.Instance.DtmUpdaterInstance.Register(doc, updater);
                            DTMTool.DtmSynchOverrides.CreateReloadLatestOverride(app);
                            DTMTool.DtmSynchOverrides.CreateSynchToCentralOverride(app);
                        });
                    }
                    else if (string.Equals(updater.updaterId, Properties.Resources.LinkUnloadTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.EnqueueTask(app =>
                        {
                            LinkUnloadMonitor.LinkUnloadMonitor.CreateLinkUnloadOverride(app);
                        });
                    }
                    else if (string.Equals(updater.updaterId, Properties.Resources.SheetsTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessSheets(ActionType.CheckIn, doc, centralPath);
                    }
                    else if (string.Equals(updater.updaterId, Properties.Resources.HealthReportTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        // (Konrad) These are read-only methods so they don't need to run in Revit context.
                        ProcessModels(ActionType.CheckIn, doc, centralPath);
                        ProcessWorksets(ActionType.CheckIn, doc, centralPath);
                        ProcessFamilies(centralPath);
                        ProcessStyle(doc, centralPath);
                        ProcessLinks(doc, centralPath);
                        ProcessViews(doc, centralPath);

                        // (Konrad) in order not to become out of synch with the database we need a way
                        // to communicate live updates from the database to task assistant/communicator
                        new Thread(() => new MissionControlSocket().Main(doc))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                }

                // (Konrad) This tool will reset Shared Parameters Location to one specified in Mission Control
                if (config.GetType().GetProperty("sharedParamMonitor") != null && 
                    config.sharedParamMonitor.isMonitorOn)
                {
                    if (File.Exists(config.sharedParamMonitor.filePath))
                    {
                        doc.Application.SharedParametersFilename = config.sharedParamMonitor.filePath;
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to reset Shared Parameter location. Could not find file specified.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Checks if Trigger Records collection exists and creates one if it doesn't. Since trigger
        /// records are published on demand by another tool (DTM) there is no need to publish them here.
        /// </summary>
        /// <param name="centralPath"></param>
        public static void ProcessTriggerRecords(string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            if (!ServerUtilities.GetByCentralPath(centralPath, "triggerrecords/centralpath", out TriggerRecordData trData))
            {
                if (ServerUtilities.Post(new TriggerRecordData { CentralPath = centralPath.ToLower() }, "triggerrecords", out trData))
                {
                    ServerUtilities.Put(new { id = trData.Id }, "projects/" + project.Id + "/addtriggerrecord");

                    if (MissionControlSetup.TriggerRecords.ContainsKey(centralPath))
                        MissionControlSetup.TriggerRecords.Remove(centralPath);
                    MissionControlSetup.TriggerRecords.Add(centralPath, trData.Id); // store TriggerRecords record
                }
            }
            if (trData != null)
            {
                if (MissionControlSetup.TriggerRecords.ContainsKey(centralPath))
                    MissionControlSetup.TriggerRecords.Remove(centralPath);
                MissionControlSetup.TriggerRecords.Add(centralPath, trData.Id); // store TriggerRecords record
            }
        }

        /// <summary>
        /// Adds Sheets data to collection if such exists, otherwise creates a new one.
        /// </summary>
        public static void ProcessSheets(ActionType action, Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            switch (action)
            {
                case ActionType.CheckIn:
                    if (!ServerUtilities.GetByCentralPath(centralPath, "sheets/centralpath", out SheetData sData))
                    {
                        if (ServerUtilities.Post(new SheetData { CentralPath = centralPath.ToLower() }, "sheets", out sData))
                        {
                            ServerUtilities.Put(new { id = sData.Id }, "projects/" + project.Id + "/addsheet");
                            AppCommand.SheetsData = sData;
                            if (MissionControlSetup.SheetsIds.ContainsKey(centralPath))
                                MissionControlSetup.SheetsIds.Remove(centralPath);
                            MissionControlSetup.SheetsIds.Add(centralPath, sData.Id); // store sheets record
                        }
                    }
                    if (sData != null)
                    {
                        AppCommand.SheetsData = sData;
                        if (MissionControlSetup.SheetsIds.ContainsKey(centralPath))
                            MissionControlSetup.SheetsIds.Remove(centralPath);
                        MissionControlSetup.SheetsIds.Add(centralPath, sData.Id); // store sheets record

                        new Thread(() => new SheetTracker.SheetTracker().SynchSheets(doc))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                    break;
                case ActionType.Synch:
                    if (MissionControlSetup.SheetsIds.ContainsKey(centralPath))
                    {
                        new Thread(() => new SheetTracker.SheetTracker().SynchSheets(doc))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        /// <summary>
        /// Adds Views data to collection if such exists, otherwise creates a new one.
        /// </summary>
        private static void ProcessViews(Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            if (!ServerUtilities.GetByCentralPath(centralPath, "views/centralpath", out ViewsData vData))
            {
                if (ServerUtilities.Post(new WorksetData { CentralPath = centralPath.ToLower() }, "views", out vData))
                {
                    ServerUtilities.Put(new { id = vData.Id }, "projects/" + project.Id + "/addview");
                }
            }
            if (vData != null)
            {
                new Thread(() => new ViewMonitor().PublishData(doc, vData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// Adds Worksets data to collection if such exists, otherwise creates a new one.
        /// </summary>
        public static void ProcessWorksets(ActionType action, Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            switch (action)
            {
                case ActionType.CheckIn:
                    if (!ServerUtilities.GetByCentralPath(centralPath, "worksets/centralpath", out WorksetData wData))
                    {
                        if (ServerUtilities.Post(new WorksetData { CentralPath = centralPath.ToLower() }, "worksets", out wData))
                        {
                            ServerUtilities.Put(new { id = wData.Id }, "projects/" + project.Id + "/addworkset");
                            if (MissionControlSetup.WorksetsIds.ContainsKey(centralPath))
                                MissionControlSetup.WorksetsIds.Remove(centralPath);
                            MissionControlSetup.WorksetsIds.Add(centralPath, wData.Id); // store workset record
                        }
                    }
                    if (wData != null)
                    {
                        if (MissionControlSetup.WorksetsIds.ContainsKey(centralPath))
                            MissionControlSetup.WorksetsIds.Remove(centralPath);
                        MissionControlSetup.WorksetsIds.Add(centralPath, wData.Id); // store workset record

                        new Thread(() => new WorksetItemCount().PublishData(doc, wData.Id))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();

                        new Thread(() => new WorksetOpenSynch().PublishData(doc, wData.Id, WorksetMonitorState.onopened))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                    break;
                case ActionType.Synch:
                    if (MissionControlSetup.WorksetsIds.ContainsKey(centralPath))
                    {
                        var worksetsId = MissionControlSetup.WorksetsIds[centralPath];
                        new Thread(() => new WorksetOpenSynch().PublishData(doc, worksetsId, WorksetMonitorState.onsynched))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        /// <summary>
        /// Checks if Families collection exists and creates one if it doesn't. Since families
        /// stats are published on demand by another tool there is no need to publish them here.
        /// </summary>
        private static void ProcessFamilies(string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            // (Konrad) For families we only need to make sure that we have the collection id. It will be used 
            // by the Tasks. The actual data gets posted by FamilyPublis Tool.
            if (!ServerUtilities.GetByCentralPath(centralPath, "families/centralpath", out FamilyData fData))
            {
                if (ServerUtilities.Post(new FamilyData { CentralPath = centralPath.ToLower() }, "families", out fData))
                {
                    ServerUtilities.Put(new { id = fData.Id }, "projects/" + project.Id + "/addfamilies");
                    if (MissionControlSetup.FamiliesIds.ContainsKey(centralPath))
                        MissionControlSetup.FamiliesIds.Remove(centralPath);
                    MissionControlSetup.FamiliesIds.Add(centralPath, fData.Id); // store families record
                }
            }
            else
            {
                if (MissionControlSetup.FamiliesIds.ContainsKey(centralPath))
                    MissionControlSetup.FamiliesIds.Remove(centralPath);
                MissionControlSetup.FamiliesIds.Add(centralPath, fData.Id); // store families record
            }
        }

        /// <summary>
        /// Adds Styles data to collection if such exists, otherwise creates a new one.
        /// </summary>
        private static void ProcessStyle(Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            if (!ServerUtilities.GetByCentralPath(centralPath, "styles/centralpath", out StylesData sData))
            {
                if (ServerUtilities.Post(new StylesData { CentralPath = centralPath.ToLower() }, "styles", out sData))
                {
                    ServerUtilities.Put(new { id = sData.Id }, "projects/" + project.Id + "/addstyle");
                }
            }
            if (sData != null)
            {
                new Thread(() => new StylesMonitor().PublishData(doc, sData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// Adds Links data to collection if such exists, otherwise creates a new one.
        /// </summary>
        private static void ProcessLinks(Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            if (!ServerUtilities.GetByCentralPath(centralPath, "links/centralpath", out LinkData lData))
            {
                if (ServerUtilities.Post(new LinkData { CentralPath = centralPath.ToLower() }, "links", out lData))
                {
                    ServerUtilities.Put(new { id = lData.Id }, "projects/" + project.Id + "/addlink");
                }
            }
            if (lData != null)
            {
                new Thread(() => new LinkMonitor().PublishData(doc, lData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// Adds Models data to collection if such exists, otherwise creates a new one.
        /// </summary>
        public static void ProcessModels(ActionType action, Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            switch (action)
            {
                case ActionType.CheckIn:
                    if (!ServerUtilities.GetByCentralPath(centralPath, "models/centralpath", out ModelData mData))
                    {
                        if (ServerUtilities.Post(new ModelData { CentralPath = centralPath.ToLower() }, "models", out mData))
                        {
                            ServerUtilities.Put(new { id = mData.Id }, "projects/" + project.Id + "/addmodel");
                            if (MissionControlSetup.ModelsIds.ContainsKey(centralPath))
                                MissionControlSetup.ModelsIds.Remove(centralPath);
                            MissionControlSetup.ModelsIds.Add(centralPath, mData.Id); // store model record
                        }
                    }
                    if (mData != null)
                    {
                        if (MissionControlSetup.ModelsIds.ContainsKey(centralPath))
                            MissionControlSetup.ModelsIds.Remove(centralPath);
                        MissionControlSetup.ModelsIds.Add(centralPath, mData.Id); // store model record

                        new Thread(() => new ModelMonitor().PublishModelSize(doc, centralPath, mData.Id, doc.Application.VersionNumber))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();

                        if (AppCommand.OpenTime.ContainsKey("from"))
                        {
                            new Thread(() => new ModelMonitor().PublishOpenTime(mData.Id))
                            {
                                Priority = ThreadPriority.BelowNormal,
                                IsBackground = true
                            }.Start();
                        }
                    }
                    break;
                case ActionType.Synch:
                    if (MissionControlSetup.ModelsIds.ContainsKey(centralPath))
                    {
                        var modelsId = MissionControlSetup.ModelsIds[centralPath];
                        if (AppCommand.SynchTime.ContainsKey("from"))
                        {
                            new Thread(() => new ModelMonitor().PublishSynchTime(modelsId))
                            {
                                Priority = ThreadPriority.BelowNormal,
                                IsBackground = true
                            }.Start();
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        /// <summary>
        /// Disables Mission Control specific tools if it failed to connect to server.
        /// </summary>
        private static void DisableMissionControl()
        {
            // kill website button
            AppCommand.Instance.WebsiteButton.Enabled = false;

            // kill communicator panel and button
            AppCommand.CommunicatorHandler.Request.Make(RequestId.Disable);
            AppCommand.CommunicatorEvent.Raise();
        }

        /// <summary>
        /// Unregisters all updaters that might have been registered when we checked into Mission Control.
        /// Also cleans up any static variables that might cause issues on re-open.
        /// </summary>
        public static void UnregisterUpdaters(Document doc)
        {
            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            if (MissionControlSetup.Configurations.ContainsKey(centralPath))
            {
                var currentConfig = MissionControlSetup.Configurations[centralPath];
                foreach (var updater in currentConfig.updaters)
                {
                    if (!updater.isUpdaterOn) continue;

                    if (string.Equals(updater.updaterId,
                        AppCommand.Instance.DoorUpdaterInstance.UpdaterGuid.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DoorUpdaterInstance.Unregister(doc);
                    }
                    else if (string.Equals(updater.updaterId,
                        AppCommand.Instance.DtmUpdaterInstance.UpdaterGuid.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DtmUpdaterInstance.Unregister(doc);
                    }
                }
            }

            // (Konrad) Clean up all static classes that would be holding any relevant information. 
            // This would cause issues in case that user closes a MissionControl registered project without
            // closing Revit app. These static classes retain their values, and then would trick rest of app
            // to think that it is registered in Mission Control.
            MissionControlSetup.ClearAll();
            AppCommand.ClearAll();
        }
    }

    /// <summary>
    /// Specified the type of interaction that we want to execute. 
    /// </summary>
    public enum ActionType
    {
        CheckIn,
        Synch
    }
}
