using System;
using System.Threading;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Worksets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator;
using HOK.MissionControl.Tools.Communicator.Socket;
using HOK.MissionControl.Tools.HealthReport;
using HOK.MissionControl.Utils;

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
                ApplyConfiguration(doc, configFound, projectFound);

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
        /// <param name="doc">Revit Document</param>
        /// <param name="config">Mission Control Configuration</param>
        /// <param name="project"></param>
        private static void ApplyConfiguration(Document doc, Configuration config, Project project)
        {
            try
            {
                var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                foreach (var updater in config.updaters)
                {
                    if (!updater.isUpdaterOn) continue;

                    if (string.Equals(updater.updaterId, AppCommand.Instance.DoorUpdaterInstance.UpdaterGuid.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DoorUpdaterInstance.Register(doc, updater);
                    }
                    else if (string.Equals(updater.updaterId, AppCommand.Instance.DtmUpdaterInstance.UpdaterGuid.ToString(), 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DtmUpdaterInstance.Register(doc, updater);
                        AppCommand.Instance.DtmUpdaterInstance.CreateReloadLatestOverride();
                        AppCommand.Instance.DtmUpdaterInstance.CreateSynchToCentralOverride();
                    }
                    else if (string.Equals(updater.updaterId, AppCommand.Instance.LinkUnloadInstance.UpdaterGuid.ToString(), 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.LinkUnloadInstance.CreateLinkUnloadOverride();
                    }
                    else if (string.Equals(updater.updaterId, Properties.Resources.SheetsTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        //new Thread(() => new SheetTracker.SheetTracker().SynchSheets(doc))
                        //{
                        //    Priority = ThreadPriority.BelowNormal,
                        //    IsBackground = true
                        //}.Start();
                    }
                    else if (string.Equals(updater.updaterId, Properties.Resources.HealthReportTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ServerUtilities.GetByCentralPath(centralPath, "worksets/centralpath", out WorksetData wData))
                        {
                            if (ServerUtilities.Post(new WorksetData { CentralPath = centralPath.ToLower() }, "worksets", out wData))
                            {
                                ServerUtilities.Put(new { id = wData.Id }, "projects/" + project.Id + "/addworkset");
                                MissionControlSetup.WorksetsIds.Add(centralPath, wData.Id); // store workset record
                            }
                        }
                        if (wData != null)
                        {
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

                        if (!ServerUtilities.GetByCentralPath(centralPath, "families/centralpath", out FamilyData fData))
                        {
                            if (ServerUtilities.Post(new FamilyData { CentralPath = centralPath.ToLower() }, "families", out fData))
                            {
                                ServerUtilities.Put(new { id = fData.Id }, "projects/" + project.Id + "/addfamilies");
                                MissionControlSetup.FamiliesIds.Add(centralPath, fData.Id); // store families record
                            }
                        }

                        //if (!MissionControlSetup.HealthRecordIds.ContainsKey(centralPath))
                        //{
                        //    HrData = ServerUtilities.GetByCentralPath<HealthReportData>(centralPath, "healthrecords/centralpath");
                        //    if (HrData == null)
                        //    {
                        //        HrData = ServerUtilities.Post<HealthReportData>(new HealthReportData { centralPath = centralPath.ToLower() }, "healthrecords");
                        //        ServerUtilities.AddHealthRecordToProject(currentProject, HrData.Id);


                        //        var projectFound = ServerUtilities.Get<Project>("projects/configid/" + currentConfig.Id);
                        //        if (null == projectFound) return;
                        //        MissionControlSetup.Projects[centralPath] = projectFound; // this won't be null since we checked before.

                        //        MissionControlSetup.HealthRecordIds.Add(centralPath, HrData.Id); // store health record
                        //        MissionControlSetup.FamiliesIds.Add(centralPath, HrData.familyStats); // store families record
                        //    }
                        //    else
                        //    {
                        //        MissionControlSetup.HealthRecordIds.Add(centralPath, HrData.Id); // store health record
                        //        MissionControlSetup.FamiliesIds.Add(centralPath, HrData.familyStats); // store families record
                        //    }
                        //}



                        AppCommand.OpenTime["from"] = DateTime.UtcNow;

                        // (Konrad) in order not to become out of synch with the database we need a way
                        // to communicate live updates from the database to task assistant/communicator
                        new Thread(() => new MissionControlSocket().Main(doc))
                        {
                            Priority = ThreadPriority.BelowNormal,
                            IsBackground = true
                        }.Start();
                    }
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
        private static void DisableMissionControl()
        {
            // kill website button
            AppCommand.Instance.WebsiteButton.Enabled = false;

            // kill communicator panel and button
            AppCommand.CommunicatorHandler.Request.Make(RequestId.Disable);
            AppCommand.CommunicatorEvent.Raise();
        }
    }
}
