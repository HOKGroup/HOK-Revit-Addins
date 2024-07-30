using System;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.DoorRoom
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class DoorCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;
            Log.AppendLog(LogMessageType.INFO, "Started");

            // (Konrad) We are gathering information about the addin use. This allows us to
            // better maintain the most used plug-ins or discontiue the unused ones.
            AddinUtilities.PublishAddinLog(new AddinLog("Utilities-DoorRoom", commandData.Application.Application.VersionNumber));

            FindSharedParameters(out bool toNumberFound, out bool toNameFound, out bool fromNumberFound, out bool fromNameFound);

            if (toNumberFound && toNameFound && fromNumberFound && fromNameFound)
            {
                var taskDialog =
                    new TaskDialog("Door Link Command")
                    {
                        MainInstruction = "Select a mode for the Door Link Command",
                        MainContent =
                            "The shared parameters can be propagated with values from either pre-defined system room data or retrieved room data from linked models.",
                        AllowCancellation = true
                    };
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Copy To and From values to Shared Parameters.");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Retrieve room data from linked models.");

                var tResult = taskDialog.Show();
                if (tResult == TaskDialogResult.CommandLink1)
                {
                    var doorLinkManager = new DoorLinkManager(m_app, DoorLinkType.CopyFromHost);
                }
                else if (tResult == TaskDialogResult.CommandLink2)
                {
                    var doorLinkManager = new DoorLinkManager(m_app, DoorLinkType.FindFromLink);
                }
            }
            else
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine("The following parameters are required in door elements to run the Door Link command.\n");
                if (!toNumberFound)
                {
                    strBuilder.AppendLine("Parameter Name: ToRoomNumber\tParameter Type: Text\n");
                }
                if (!toNameFound)
                {
                    strBuilder.AppendLine("Parameter Name: ToRoomName\tParameter Type: Text\n");
                }
                if (!fromNumberFound)
                {
                    strBuilder.AppendLine("Parameter Name: FromRoomNumber\tParameter Type: Text\n");
                }
                if (!fromNameFound)
                {
                    strBuilder.AppendLine("Parameter Name: FromRoomName\tParameter Type: Text\n");
                }

                MessageBox.Show(strBuilder.ToString(), "Parameter Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Log.AppendLog(LogMessageType.INFO, "Ended");
            return Result.Succeeded;
        }

        private void FindSharedParameters(out bool toNumberFound, out bool toNameFound, out bool fromNumberFound, out bool fromNameFound)
        {
            toNumberFound = false;
            toNameFound = false;
            fromNumberFound = false;
            fromNameFound = false;

            try
            {
                var iter = m_doc.ParameterBindings.ForwardIterator();
                while (iter.MoveNext())
                {
                    var definition = iter.Key;
                    switch (definition.Name)
                    {
                        case "ToRoomNumber":
                            toNumberFound = true;
                            break;
                        case "ToRoomName":
                            toNameFound = true;
                            break;
                        case "FromRoomNumber":
                            fromNumberFound = true;
                            break;
                        case "FromRoomName":
                            fromNameFound = true;
                            break;
                        default:
                            continue;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sahred parameters cannot be found.\n" + ex.Message, "Find Shared Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
