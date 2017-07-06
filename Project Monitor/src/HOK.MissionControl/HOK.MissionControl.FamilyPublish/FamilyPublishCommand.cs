using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.MissionControl.FamilyPublish
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class FamilyPublishCommand : IExternalCommand
    {
        public Dictionary<string, Configuration> ConfigDictionary { get; set; } = new Dictionary<string, Configuration>();
        public Dictionary<string, Project> ProjectDictionary { get; set; } = new Dictionary<string, Project>();

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            Log.AppendLog("HOK.MissionControl.FamilyPublish.FamilyPublishCommand: Started.");

            try
            {
                var pathName = doc.PathName;
                if (string.IsNullOrEmpty(pathName)) return Result.Failed;

                var centralPath = BasicFileInfo.Extract(pathName).CentralPath;
                if (string.IsNullOrEmpty(centralPath)) return Result.Failed;

                var configFound = ServerUtilities.GetConfigurationByCentralPath(centralPath);
                if (configFound != null)
                {
                    if (ConfigDictionary.ContainsKey(centralPath))
                    {
                        ConfigDictionary.Remove(centralPath);
                    }
                    ConfigDictionary.Add(centralPath, configFound);

                    var projectFound = ServerUtilities.GetProjectByConfigurationId(configFound.Id);
                    if (null != projectFound)
                    {
                        if (ProjectDictionary.ContainsKey(centralPath))
                        {
                            ProjectDictionary.Remove(centralPath);
                        }
                        ProjectDictionary.Add(centralPath, projectFound);
                    }
                }

                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath)) return Result.Failed;

                FamilyMonitor.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var addinInfo = new AddinLog
                {
                    pluginName = "MissionControl-PublishFamilyData",
                    user = Environment.UserName,
                    revitVersion = BasicFileInfo.Extract(doc.PathName).SavedInVersion
                };

                AddinUtilities.PublishAddinLog(addinInfo);
            }
            catch (Exception e)
            {
                Log.AppendLog("HOK.MissionControl.FamilyPublish.FamilyPublishCommand: " + e.Message);
            }
            Log.AppendLog("HOK.MissionControl.FamilyPublish.FamilyPublishCommand: Ended.");
            return Result.Succeeded;
        }
    }
}
