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
using Nice3point.Revit.Toolkit.External;
// ReSharper disable UnusedMember.Global

#endregion

namespace HOK.MissionControl.Tools.WebsiteLink
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class WebsiteLinkCommand : ExternalCommand
    {
        public override void Execute()
        {
            var doc = Context.ActiveDocument;
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontinue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("MissionControl-WebsiteLink", Context.Application.VersionNumber));

                var launchHome = false;
                if (!string.IsNullOrWhiteSpace(doc.PathName))
                {
                    var centralPath = FileInfoUtil.GetCentralFilePath(doc);
                    if (!string.IsNullOrWhiteSpace(centralPath))
                    {
                        if (MissionControlSetup.Projects.ContainsKey(centralPath))
                        {
                            var id = MissionControlSetup.Projects[centralPath].Id;
                            var url = ServerUtilities.RestApiBaseUrl + "/#/projects/edit/" + id;
                            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        }
                        else launchHome = true;
                    }
                    else launchHome = true;
                }
                else launchHome = true;

                if(launchHome) Process.Start(new ProcessStartInfo(ServerUtilities.RestApiBaseUrl + "/#/home") { UseShellExecute = true });
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
