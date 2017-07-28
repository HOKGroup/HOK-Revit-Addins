using System;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.CitrixTools
{
    [Name(nameof(Properties.Resources.CopyFiles_Name), typeof(Properties.Resources))]
    [Description(nameof(Properties.Resources.CopyFiles_Description), typeof(Properties.Resources))]
    [Image(nameof(Properties.Resources.CopyFiles_ImageName), typeof(Properties.Resources))]
    [PanelName(nameof(Properties.Resources.CopyFiles_PanelName), typeof(Properties.Resources))]
    [ButtonText(nameof(Properties.Resources.CopyFiles_ButtonText), typeof(Properties.Resources))]
    [Namespace(nameof(Properties.Resources.CopyFiles_Namespace), typeof(Properties.Resources))]
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CopyFilesCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Citrix-CopyFiles", commandData.Application.Application.VersionNumber));

                Process.Start(@"C:\HOK\FileCopyTool\CitrixCopyBdrive.exe");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }
    }
}
