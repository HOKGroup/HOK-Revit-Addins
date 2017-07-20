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
    [Name(nameof(Properties.Resources.FamilyPublish_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.FamilyPublish_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.FamilyPublish_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.FamilyPublish_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.FamilyPublish_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.FamilyPublish_Namespace), typeof(Properties.Resources))]
    [AddinName(nameof(Properties.Resources.FamilyPublish_AddinName), typeof(Properties.Resources))]
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
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("MissionControl-PublishFamilyData", doc));

                var pathName = doc.PathName;
                if (string.IsNullOrEmpty(pathName))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Path is Null or Empty.");
                    return Result.Failed;
                }
                

                var centralPath = BasicFileInfo.Extract(pathName).CentralPath;
                if (string.IsNullOrEmpty(centralPath))
                {
                    Log.AppendLog(LogMessageType.ERROR, "Could not get Central Path.");
                    return Result.Failed;
                }
                

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

                if (!ProjectDictionary.ContainsKey(centralPath) || !ConfigDictionary.ContainsKey(centralPath))
                {
                    Log.AppendLog(LogMessageType.ERROR, "No Config Found.");
                    return Result.Failed;
                }
                

                FamilyMonitor.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended.");
            return Result.Succeeded;
        }
    }
}
