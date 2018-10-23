#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using GalaSoft.MvvmLight.Messaging;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Configurations;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.FilePaths;
using HOK.MissionControl.Core.Schemas.Groups;
using HOK.MissionControl.Core.Schemas.Links;
using HOK.MissionControl.Core.Schemas.Models;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Schemas.Styles;
using HOK.MissionControl.Core.Schemas.Users;
using HOK.MissionControl.Core.Schemas.Views;
using HOK.MissionControl.Core.Schemas.Warnings;
using HOK.MissionControl.Core.Schemas.Worksets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Tools.Communicator.Messaging;
using HOK.MissionControl.Tools.Communicator.Socket;
using HOK.MissionControl.Tools.HealthReport;
using HOK.MissionControl.Utils;

#endregion

namespace HOK.MissionControl.Tools.MissionControl
{
    public class MissionControl
    {
        /// <summary>
        /// Method to "check into" mission control. It posts all initial data, and stores all needed references.
        /// </summary>
        /// <param name="doc"></param>
        public void CheckIn(Document doc)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);

                // (Konrad) We can publish a file path to the DB.
                // That will make it easier to create Configurations.
                if (!doc.IsDetached && !doc.IsFamilyDocument && doc.IsWorkshared)
                {
                    if (!ServerUtilities.Post(new FilePathItem(centralPath, doc.Application.VersionNumber), "filepaths/add", out FilePathItem unused))
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to publish File Path: " + centralPath);
                    }
                }

                // (Konrad) Get Configuration/Project.
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
                CommunicatorUtilities.LaunchCommunicator();
                ApplyConfiguration(doc);
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                // (Konrad) We are not going to process warnings here.
#else
                CollectWarnings(doc);
#endif
                EnableMissionControl();

                Log.AppendLog(LogMessageType.INFO, "Mission Control check in succeeded.");
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
                var launchSockets = false;
                foreach (var updater in config.Updaters)
                {
                    if (!updater.IsUpdaterOn) continue;

                    if (string.Equals(updater.UpdaterId, AppCommand.Instance.DoorUpdaterInstance.UpdaterGuid.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        // (Konrad) We need to register updaters from within Revit context.
                        // That's why we are running this in the Idling Event.
                        AppCommand.EnqueueTask(app =>
                        {
                            AppCommand.Instance.DoorUpdaterInstance.Register(doc, updater);
                        });
                    }
                    else if (string.Equals(updater.UpdaterId, AppCommand.Instance.DtmUpdaterInstance.UpdaterGuid.ToString(), 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessTriggerRecords(centralPath);

                        AppCommand.EnqueueTask(app =>
                        {
                            AppCommand.Instance.DtmUpdaterInstance.Register(doc, updater);
                        });

                        DTMTool.DtmSynchOverrides.CreateReloadLatestOverride();
                        DTMTool.DtmSynchOverrides.CreateSynchToCentralOverride();
                    }
                    else if (string.Equals(updater.UpdaterId, Properties.Resources.LinkUnloadTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.EnqueueTask(app =>
                        {
                            LinkUnloadMonitor.LinkUnloadMonitor.CreateLinkUnloadOverride(app);
                        });
                    }
                    else if (string.Equals(updater.UpdaterId, Properties.Resources.SheetsTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessSheets(ActionType.CheckIn, doc, centralPath);

                        launchSockets = true;
                    }
                    else if (string.Equals(updater.UpdaterId, Properties.Resources.HealthReportTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        // (Konrad) These are read-only methods so they don't need to run in Revit context.
                        ProcessModels(ActionType.CheckIn, doc, centralPath);
                        ProcessWorksets(ActionType.CheckIn, doc, centralPath);
#if RELEASE2015 || RELEASE2016 || RELEASE2017
                        // (Konrad) We are not going to process warnings here.
#else
                        ProcessWarnings(ActionType.CheckIn, doc, centralPath);
#endif
                        ProcessFamilies(centralPath);
                        ProcessStyle(doc, centralPath);
                        ProcessLinks(doc, centralPath);
                        ProcessViews(doc, centralPath);
                        ProcessGroups(doc, centralPath);

                        launchSockets = true;
                    }
                }

                // (Konrad) This tool will reset Shared Parameters Location to one specified in Mission Control
                if (config.GetType().GetProperty("sharedParamMonitor") != null && 
                    config.SharedParamMonitor.IsMonitorOn)
                {
                    if (File.Exists(config.SharedParamMonitor.FilePath))
                    {
                        doc.Application.SharedParametersFilename = config.SharedParamMonitor.FilePath;
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to reset Shared Parameter location. Could not find file specified.");
                    }
                }

                if (launchSockets)
                {
                    // (Konrad) in order not to become out of synch with the database we need a way
                    // to communicate live updates from the database to task assistant/communicator
                    var socket = new MissionControlSocket(doc);
                    socket.Start();
                    AppCommand.Socket = socket;
                }

                // Publish user/machine info to be used by Zombie
                var userInfo = new UserItem
                {
                    User = Environment.UserName.ToLower(),
                    Machine = Environment.MachineName
                };

                if (!ServerUtilities.Post(userInfo, "users/add", out ResponseCreated unused1))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Failed to publish User/Machine Data.");
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

                            if (MissionControlSetup.SheetsData.ContainsKey(centralPath))
                                MissionControlSetup.SheetsData.Remove(centralPath);
                            MissionControlSetup.SheetsData.Add(centralPath, sData); // store sheets record
                        }
                    }
                    if (sData != null)
                    {
                        if (MissionControlSetup.SheetsData.ContainsKey(centralPath))
                            MissionControlSetup.SheetsData.Remove(centralPath);
                        MissionControlSetup.SheetsData.Add(centralPath, sData); // store sheets record

                        Messenger.Default.Send(new CommunicatorDataDownloaded
                        {
                            CentralPath = centralPath,
                            Type = DataType.Sheets
                        });

                        new Thread(() => new SheetTracker.SheetTracker().SynchSheets(doc))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                    break;
                case ActionType.Synch:
                    if (MissionControlSetup.SheetsData.ContainsKey(centralPath))
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
            var data = new DataRangeRequest(centralPath.ToLower());
            if (!ServerUtilities.Post(data, "views/viewstats", out ViewsData vData))
            {
                if (ServerUtilities.Post(new ViewsData { CentralPath = centralPath.ToLower() }, "views", out vData))
                {
                    ServerUtilities.Put(new { id = vData.Id }, "projects/" + project.Id + "/addview");
                }
                if (MissionControlSetup.ViewsData.ContainsKey(centralPath))
                    MissionControlSetup.ViewsData.Remove(centralPath);
                MissionControlSetup.ViewsData.Add(centralPath, vData); // store views record
            }
            if (vData != null)
            {
                if (MissionControlSetup.ViewsData.ContainsKey(centralPath))
                    MissionControlSetup.ViewsData.Remove(centralPath);
                MissionControlSetup.ViewsData.Add(centralPath, vData); // store views record

                Messenger.Default.Send(new HealthReportSummaryAdded { Data = vData, Type = SummaryType.Views });

                new Thread(() => new ViewMonitor().PublishData(doc, vData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }
#if RELEASE2015 || RELEASE2016 || RELEASE2017
        // (Konrad) We are not going to process warnings here.
#else
        /// <summary>
        /// Adds Warnings to a collection in database. If warnings exist it updates their status.
        /// </summary>
        public static void ProcessWarnings(ActionType action, Document doc, string centralPath)
        {
            var current = doc.GetWarnings().Select(x => new WarningItem(x, doc)).ToList();

            switch (action)
            {
                case ActionType.CheckIn:
                    if (!ServerUtilities.Post(current, "warnings/add", out ResponseCreated unused1))
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to publish Views Data.");
                    }
                    break;
                case ActionType.Synch:
                    var newW = AppCommand.Warnings.Values
                        .Where(x => !string.IsNullOrEmpty(x.CreatedBy) && current.Any(y => y.UniqueId == x.UniqueId)).ToList();
                    var existingW = current.Except(newW).Select(x => x.UniqueId);

                    var payload = new WarningData(Environment.UserName, centralPath, newW, existingW);
                    if (!ServerUtilities.Post(payload, "warnings/update", out ResponseCreated unused))
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to publish Views Data.");
                    }
                    else
                    {
                        CollectWarnings(doc);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
#endif
        /// <summary>
        /// Adds Groups data to collection if such exists, otherwise creates a new one.
        /// </summary>
        private static void ProcessGroups(Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            var data = new DataRangeRequest(centralPath.ToLower());
            if (!ServerUtilities.Post(data, "groups/groupstats", out GroupsData gData))
            {
                if (ServerUtilities.Post(new GroupsData { CentralPath = centralPath.ToLower() }, "groups", out gData))
                {
                    ServerUtilities.Put(new { id = gData.Id }, "projects/" + project.Id + "/addgroup");
                }
            }

            if (gData != null)
            {
                if (MissionControlSetup.GroupsData.ContainsKey(centralPath))
                    MissionControlSetup.GroupsData.Remove(centralPath);
                MissionControlSetup.GroupsData.Add(centralPath, gData); // store groups record

                Messenger.Default.Send(new HealthReportSummaryAdded { Data = gData, Type = SummaryType.Groups });

                new Thread(() => new GroupMonitor().PublishData(doc, gData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
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
            // by the Tasks. The actual data gets posted by FamilyPublish Tool.
            if (!ServerUtilities.GetByCentralPath(centralPath, "families/centralpath", out FamilyData fData))
            {
                if (ServerUtilities.Post(new FamilyData { CentralPath = centralPath.ToLower() }, "families", out fData))
                {
                    ServerUtilities.Put(new { id = fData.Id }, "projects/" + project.Id + "/addfamilies");

                    if (MissionControlSetup.FamilyData.ContainsKey(centralPath))
                        MissionControlSetup.FamilyData.Remove(centralPath);
                    MissionControlSetup.FamilyData.Add(centralPath, fData); // store families record
                }
            }
            else
            {
                if (MissionControlSetup.FamilyData.ContainsKey(centralPath))
                    MissionControlSetup.FamilyData.Remove(centralPath);
                MissionControlSetup.FamilyData.Add(centralPath, fData); // store families record

                Messenger.Default.Send(new HealthReportSummaryAdded { Data = fData, Type = SummaryType.Families });
                Messenger.Default.Send(new CommunicatorDataDownloaded { CentralPath = centralPath, Type = DataType.Families });
            }
        }

        /// <summary>
        /// Adds Styles data to collection if such exists, otherwise creates a new one.
        /// </summary>
        private static void ProcessStyle(Document doc, string centralPath)
        {
            var project = MissionControlSetup.Projects[centralPath];
            var data = new DataRangeRequest(centralPath.ToLower());
            if (!ServerUtilities.Post(data, "styles/stylestats", out StylesData sData))
            {
                if (ServerUtilities.Post(new StylesData { CentralPath = centralPath.ToLower() }, "styles", out sData))
                {
                    ServerUtilities.Put(new { id = sData.Id }, "projects/" + project.Id + "/addstyle");

                    if (MissionControlSetup.StylesData.ContainsKey(centralPath))
                        MissionControlSetup.StylesData.Remove(centralPath);
                    MissionControlSetup.StylesData.Add(centralPath, sData); // store styles record
                }
            }
            if (sData != null)
            {
                if (MissionControlSetup.StylesData.ContainsKey(centralPath))
                    MissionControlSetup.StylesData.Remove(centralPath);
                MissionControlSetup.StylesData.Add(centralPath, sData); // store styles record

                Messenger.Default.Send(new HealthReportSummaryAdded { Data = sData, Type = SummaryType.Styles });

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
            var data = new DataRangeRequest(centralPath.ToLower());
            if (!ServerUtilities.Post(data, "links/linkstats", out LinkData lData))
            {
                if (ServerUtilities.Post(new LinkData { CentralPath = centralPath.ToLower() }, "links", out lData))
                {
                    ServerUtilities.Put(new { id = lData.Id }, "projects/" + project.Id + "/addlink");

                    if (MissionControlSetup.LinksData.ContainsKey(centralPath))
                        MissionControlSetup.LinksData.Remove(centralPath);
                    MissionControlSetup.LinksData.Add(centralPath, lData); // store links record
                }
            }
            if (lData != null)
            {
                if (MissionControlSetup.LinksData.ContainsKey(centralPath))
                    MissionControlSetup.LinksData.Remove(centralPath);
                MissionControlSetup.LinksData.Add(centralPath, lData); // store links record

                Messenger.Default.Send(new HealthReportSummaryAdded { Data = lData, Type = SummaryType.Links });

                new Thread(() => new LinkMonitor().PublishData(doc, lData.Id))
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
            var data = new DataRangeRequest(centralPath.ToLower()){ From = null, To = null };
            switch (action)
            {
                case ActionType.CheckIn:
                    if (!ServerUtilities.Post(data, "worksets/getworksetsdata", out List<WorksetStats> wData))
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to get Workset Stats.");
                        return;
                    }
                    if (wData != null && wData.Any())
                    {
                        if (MissionControlSetup.WorksetsData.ContainsKey(centralPath))
                            MissionControlSetup.WorksetsData.Remove(centralPath);
                        MissionControlSetup.WorksetsData.Add(centralPath, wData.First()); // store workset record

                        Messenger.Default.Send(new HealthReportSummaryAdded { Data = wData.First(), Type = SummaryType.Worksets });

                        new Thread(() => new WorksetItemCount().PublishData(doc, centralPath))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();

                        new Thread(() => new WorksetOpenSynch().PublishData(doc, centralPath, WorksetMonitorState.onopened))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
                    break;
                case ActionType.Synch:
                    new Thread(() => new WorksetOpenSynch().PublishData(doc, centralPath, WorksetMonitorState.onsynched))
                    {
                        Priority = ThreadPriority.BelowNormal,
                        IsBackground = true
                    }.Start();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        /// <summary>
        /// Adds Models data to collection if such exists, otherwise creates a new one.
        /// </summary>
        public static void ProcessModels(ActionType action, Document doc, string centralPath)
        {
            var data = new DataRangeRequest(centralPath.ToLower()){ From = null, To = null };
            switch (action)
            {
                case ActionType.CheckIn:
                    if (!ServerUtilities.Post(data, "model/getmodelsdata", out List<ModelStats> mData) || mData == null)
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Failed to get Model Stats.");
                        return;
                    }
                    if (mData.Any())
                    {
                        if (MissionControlSetup.ModelsData.ContainsKey(centralPath))
                            MissionControlSetup.ModelsData.Remove(centralPath);
                        MissionControlSetup.ModelsData.Add(centralPath, mData.First()); // store model record

                        Messenger.Default.Send(new HealthReportSummaryAdded { Data = mData.First(), Type = SummaryType.Models });

                        new Thread(() => new ModelMonitor().PublishModelSize(doc, centralPath, doc.Application.VersionNumber))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();

                        if (AppCommand.OpenTime.ContainsKey("from"))
                        {
                            new Thread(() => new ModelMonitor().PublishOpenTime(centralPath))
                            {
                                Priority = ThreadPriority.BelowNormal,
                                IsBackground = true
                            }.Start();
                        }
                    }
                    break;
                case ActionType.Synch:
                    if (AppCommand.SynchTime.ContainsKey("from"))
                    {
                        new Thread(() => new ModelMonitor().PublishSynchTime(centralPath))
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
        /// Disables Mission Control specific tools if it failed to connect to server.
        /// </summary>
        private static void DisableMissionControl()
        {
            // kill website button
            AppCommand.Instance.WebsiteButton.Enabled = false;

            // kill family publish button
            AppCommand.Instance.FamilyPublishButton.Enabled = false;

            // kill communicator panel and button
            AppCommand.CommunicatorHandler.Request.Make(RequestId.Disable);
            AppCommand.CommunicatorEvent.Raise();
        }

        /// <summary>
        /// Re-enables mission control buttons.
        /// </summary>
        private static void EnableMissionControl()
        {
            AppCommand.Instance.WebsiteButton.Enabled = true;
            AppCommand.Instance.CommunicatorButton.Enabled = true;
            AppCommand.Instance.FamilyPublishButton.Enabled = true;
        }

        /// <summary>
        /// Unregisters all updaters that might have been registered when we checked into Mission Control.
        /// Also cleans up any static variables that might cause issues on re-open.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        public static void UnregisterUpdaters(Document doc)
        {
            var centralPath = FileInfoUtil.GetCentralFilePath(doc);
            if (MissionControlSetup.Configurations.ContainsKey(centralPath))
            {
                var currentConfig = MissionControlSetup.Configurations[centralPath];
                foreach (var updater in currentConfig.Updaters)
                {
                    if (!updater.IsUpdaterOn) continue;

                    if (string.Equals(updater.UpdaterId,
                        AppCommand.Instance.DoorUpdaterInstance.UpdaterGuid.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DoorUpdaterInstance.Unregister(doc);
                    }
                    else if (string.Equals(updater.UpdaterId,
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
#if RELEASE2015 || RELEASE2016 || RELEASE2017
        // (Konrad) We are not going to process warnings here.
#else
        /// <summary>
        /// Collects all currently existing warnings in the file and sets them on AppCommand.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        private static void CollectWarnings(Document doc)
        {
            AppCommand.Warnings = doc.GetWarnings()
                .Select(x => new WarningItem(x, doc))
                .ToDictionary(x => x.UniqueId, x => x);
        }
#endif
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
