using System;
using System.Threading;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Links;
using HOK.MissionControl.Core.Schemas.Models;
using HOK.MissionControl.Core.Schemas.Styles;
using HOK.MissionControl.Core.Schemas.Views;
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
        public Document Doc { get; set; }
        public string CentralPath { get; set; }
        public Project Project { get; set; }
        public Configuration Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        public void CheckIn(Document doc)
        {
            Doc = doc;
            CentralPath = FileInfoUtil.GetCentralFilePath(doc);
            try
            {
                
                if (!ServerUtilities.GetByCentralPath(CentralPath, "configurations/centralpath", out Configuration configFound))
                {
                    DisableMissionControl();
                    return;
                }

                if (!ServerUtilities.Get("projects/configid/" + configFound.Id, out Project projectFound))
                {
                    DisableMissionControl();
                    return;
                }

                if (MissionControlSetup.Configurations.ContainsKey(CentralPath)) MissionControlSetup.Configurations.Remove(CentralPath);
                MissionControlSetup.Configurations.Add(CentralPath, configFound);
                Configuration = configFound;

                if (MissionControlSetup.Projects.ContainsKey(CentralPath)) MissionControlSetup.Projects.Remove(CentralPath);
                MissionControlSetup.Projects.Add(CentralPath, projectFound);
                Project = projectFound;

                // (Konrad) This might be a good time to let users know that Mission Control is ready to go.
                AppCommand.CommunicatorHandler.Status = Status.Success;
                AppCommand.CommunicatorHandler.Message = "Successfully connected to Mission Control!";
                AppCommand.CommunicatorHandler.Request.Make(RequestId.ReportStatus);
                AppCommand.CommunicatorEvent.Raise();

                // (Konrad) Register Updaters that are in the config file.
                ApplyConfiguration();

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
        private void ApplyConfiguration()
        {
            try
            {
                foreach (var updater in Configuration.updaters)
                {
                    if (!updater.isUpdaterOn) continue;

                    if (string.Equals(updater.updaterId, AppCommand.Instance.DoorUpdaterInstance.UpdaterGuid.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DoorUpdaterInstance.Register(Doc, updater);
                    }
                    else if (string.Equals(updater.updaterId, AppCommand.Instance.DtmUpdaterInstance.UpdaterGuid.ToString(), 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AppCommand.Instance.DtmUpdaterInstance.Register(Doc, updater);
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
                        //TODO: Refactor this! 
                        //new Thread(() => new SheetTracker.SheetTracker().SynchSheets(doc))
                        //{
                        //    Priority = ThreadPriority.BelowNormal,
                        //    IsBackground = true
                        //}.Start();
                    }
                    else if (string.Equals(updater.updaterId, Properties.Resources.HealthReportTrackerGuid, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessModels();
                        ProcessWorksets();
                        ProcessFamilies();
                        ProcessStyle();
                        ProcessLinks();
                        ProcessViews();

                        // (Konrad) in order not to become out of synch with the database we need a way
                        // to communicate live updates from the database to task assistant/communicator
                        new Thread(() => new MissionControlSocket().Main(Doc))
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
        private void ProcessViews()
        {
            if (!ServerUtilities.GetByCentralPath(CentralPath, "views/centralpath", out ViewsData vData))
            {
                if (ServerUtilities.Post(new WorksetData { CentralPath = CentralPath.ToLower() }, "views", out vData))
                {
                    ServerUtilities.Put(new { id = vData.Id }, "projects/" + Project.Id + "/addview");
                }
            }
            if (vData != null)
            {
                new Thread(() => new ViewMonitor().PublishData(Doc, vData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessWorksets()
        {
            if (!ServerUtilities.GetByCentralPath(CentralPath, "worksets/centralpath", out WorksetData wData))
            {
                if (ServerUtilities.Post(new WorksetData { CentralPath = CentralPath.ToLower() }, "worksets", out wData))
                {
                    ServerUtilities.Put(new { id = wData.Id }, "projects/" + Project.Id + "/addworkset");
                    MissionControlSetup.WorksetsIds.Add(CentralPath, wData.Id); // store workset record
                }
            }
            if (wData != null)
            {
                MissionControlSetup.WorksetsIds.Add(CentralPath, wData.Id); // store workset record
                new Thread(() => new WorksetItemCount().PublishData(Doc, wData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();

                new Thread(() => new WorksetOpenSynch().PublishData(Doc, wData.Id, WorksetMonitorState.onopened))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessFamilies()
        {
            // (Konrad) For families we only need to make sure that we have the collection id. It will be used 
            // by the Tasks. The actual data gets posted by FamilyPublis Tool.
            if (!ServerUtilities.GetByCentralPath(CentralPath, "families/centralpath", out FamilyData fData))
            {
                if (ServerUtilities.Post(new FamilyData { CentralPath = CentralPath.ToLower() }, "families", out fData))
                {
                    ServerUtilities.Put(new { id = fData.Id }, "projects/" + Project.Id + "/addfamilies");
                    MissionControlSetup.FamiliesIds.Add(CentralPath, fData.Id); // store families record
                }
            }
            else
            {
                MissionControlSetup.FamiliesIds.Add(CentralPath, fData.Id); // store families record
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessStyle()
        {
            if (!ServerUtilities.GetByCentralPath(CentralPath, "styles/centralpath", out StylesData sData))
            {
                if (ServerUtilities.Post(new StylesData { CentralPath = CentralPath.ToLower() }, "styles", out sData))
                {
                    ServerUtilities.Put(new { id = sData.Id }, "projects/" + Project.Id + "/addstyle");
                }
            }
            if (sData != null)
            {
                new Thread(() => new StylesMonitor().PublishData(Doc, sData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessLinks()
        {
            if (!ServerUtilities.GetByCentralPath(CentralPath, "links/centralpath", out LinkData lData))
            {
                if (ServerUtilities.Post(new LinkData { CentralPath = CentralPath.ToLower() }, "links", out lData))
                {
                    ServerUtilities.Put(new { id = lData.Id }, "projects/" + Project.Id + "/addlink");
                }
            }
            if (lData != null)
            {
                new Thread(() => new LinkMonitor().PublishData(Doc, lData.Id))
                {
                    Priority = ThreadPriority.BelowNormal,
                    IsBackground = true
                }.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ProcessModels()
        {
            if (!ServerUtilities.GetByCentralPath(CentralPath, "models/centralpath", out ModelData mData))
            {
                if (ServerUtilities.Post(new ModelData { CentralPath = CentralPath.ToLower() }, "models", out mData))
                {
                    ServerUtilities.Put(new { id = mData.Id }, "projects/" + Project.Id + "/addmodel");
                }
            }
            if (mData != null)
            {
                new Thread(() => new ModelMonitor().PublishModelSize(Doc, CentralPath, mData.Id, Doc.Application.VersionNumber))
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
