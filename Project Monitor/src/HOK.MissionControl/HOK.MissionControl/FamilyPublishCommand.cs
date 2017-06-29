using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.HealthReport.ObjectTrackers;

namespace HOK.MissionControl
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

            try
            {
                // TODO: There is potential here to optimize this part and wrap it into utility code.
                var pathName = doc.PathName;
                if (string.IsNullOrEmpty(pathName)) return Result.Failed;

                var fileInfo = BasicFileInfo.Extract(pathName);
                if (!fileInfo.IsWorkshared) return Result.Failed;

                var centralPath = fileInfo.CentralPath;
                if (string.IsNullOrEmpty(centralPath)) return Result.Failed;

                var configFound = ServerUtil.GetConfigurationByCentralPath(centralPath);
                if (configFound != null)
                {
                    if (ConfigDictionary.ContainsKey(centralPath))
                    {
                        ConfigDictionary.Remove(centralPath);
                    }
                    ConfigDictionary.Add(centralPath, configFound);

                    var projectFound = ServerUtil.GetProjectByConfigurationId(configFound.Id);
                    if (projectFound != null)
                    {
                        if (ProjectDictionary.ContainsKey(centralPath))
                        {
                            ProjectDictionary.Remove(centralPath);
                        }
                        ProjectDictionary.Add(centralPath, projectFound);
                    }

                    FamilyMonitor.PublishData(doc, ConfigDictionary[centralPath], ProjectDictionary[centralPath]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return Result.Succeeded;
        }
    }
}
