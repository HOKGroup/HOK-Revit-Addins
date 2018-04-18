using System;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.WebsiteLink
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class WebsiteLinkCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                var unused1 = AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-WebsiteLink", commandData.Application.Application.VersionNumber), LogPosted);

                var launchHome = false;
                if (!string.IsNullOrEmpty(doc.PathName))
                {
                    var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                    if (!string.IsNullOrEmpty(centralPath))
                    {
                        if (MissionControlSetup.Projects.ContainsKey(centralPath))
                        {
                            var id = MissionControlSetup.Projects[centralPath].Id;
                            Process.Start("http://missioncontrol.hok.com/#/projects/edit/" + id);
                        }
                        else launchHome = true;
                    }
                    else launchHome = true;
                }
                else launchHome = true;

                if(launchHome) Process.Start("http://missioncontrol.hok.com/#/home");
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        private static void LogPosted(AddinData data)
        {
            Log.AppendLog(LogMessageType.INFO, "Addin info was published: " 
                + (string.IsNullOrEmpty(data.Id) ? "Unsuccessfully." : "Successfully."));
        }
    }
}
