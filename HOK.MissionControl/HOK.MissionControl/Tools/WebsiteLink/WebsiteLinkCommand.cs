#region References

using System;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
// ReSharper disable UnusedMember.Global

#endregion

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
                // better maintain the most used plug-ins or discontinue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-WebsiteLink", commandData.Application.Application.VersionNumber));

                var launchHome = false;
                if (!string.IsNullOrWhiteSpace(doc.PathName))
                {
                    var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                    if (!string.IsNullOrWhiteSpace(centralPath))
                    {
                        if (MissionControlSetup.Projects.ContainsKey(centralPath))
                        {
                            var id = MissionControlSetup.Projects[centralPath].Id;
                            Process.Start(ServerUtilities.RestApiBaseUrl + "/#/projects/edit/" + id);
                        }
                        else launchHome = true;
                    }
                    else launchHome = true;
                }
                else launchHome = true;

                if(launchHome) Process.Start(ServerUtilities.RestApiBaseUrl + "/#/home");
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }
    }
}
