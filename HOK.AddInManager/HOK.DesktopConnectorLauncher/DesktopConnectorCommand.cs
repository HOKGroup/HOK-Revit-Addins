using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using System.Diagnostics;
using Microsoft.Win32;

namespace HOK.DesktopConnectorLauncher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class DesktopConnectorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                string keyName = @"HKEY_CURRENT_USER\SOFTWARE\HOK\BIM\";
                string valueName = "DesktopConnectorPath_Citrix";
                string path = (string)Registry.GetValue(keyName, valueName, null);
                if (path != null && System.IO.File.Exists(path)) {
                    Process.Start(path);
                }
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
