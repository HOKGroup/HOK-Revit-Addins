using System;
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
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;

            try
            {
                var pathName = doc.PathName;
                if (string.IsNullOrEmpty(pathName)) return Result.Failed;

                var centralPath = BasicFileInfo.Extract(pathName).CentralPath;
                if (string.IsNullOrEmpty(centralPath)) return Result.Failed;

                var appCommand = AppCommand.Instance;
                if (!appCommand.ProjectDictionary.ContainsKey(centralPath) || !appCommand.ConfigDictionary.ContainsKey(centralPath)) return Result.Failed;

                FamilyMonitor.PublishData(doc, appCommand.ConfigDictionary[centralPath], appCommand.ProjectDictionary[centralPath]);

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var addinInfo = new AddinLog
                {
                    pluginName = "MissionControl-PublishFamilyData",
                    user = Environment.UserName,
                    revitVersion = BasicFileInfo.Extract(doc.PathName).SavedInVersion,
                };

                AddinUtilities.PublishAddinLog(addinInfo);
            }
            catch (Exception e)
            {
                LogUtil.AppendLog("FamilyPublishCommand:" + e.Message);
            }
            return Result.Succeeded;
        }
    }
}
