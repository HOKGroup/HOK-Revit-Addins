using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using System.Diagnostics;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External;

namespace HOK.DesktopConnectorLauncher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class DesktopConnectorCommand : ExternalCommand
    {
        public override void Execute()
        {
            Log.AppendLog(LogMessageType.INFO, "Started");

            try
            {
                // (Dan) All HOK specific settings are stored in a Settings.json file and referenced
                // inside of the HOK.Core. We can retrieve them by deserializing the resource.
                var settingsString = Resources.StreamEmbeddedResource("HOK.Core.Resources.Settings.json");
                string RegistryKey = Json.Deserialize<Settings>(settingsString)?.CitrixDesktopConnectorKey;
                string RegistryValue = Json.Deserialize<Settings>(settingsString)?.CitrixDesktopConnectorValue;
                string path = (string)Registry.GetValue(RegistryKey, RegistryValue, null);
                if (path != null && System.IO.File.Exists(path))
                {
                    Process.Start(path);
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
