using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.DoorRoom
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class DoorCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            m_doc = m_app.ActiveUIDocument.Document;

            bool toNumberFound = false;
            bool toNameFound = false;
            bool fromNumberFound = false;
            bool fromNameFound = false;

            FindSharedParameters(out toNumberFound, out toNameFound, out fromNumberFound, out fromNameFound);

            if (toNumberFound && toNameFound && fromNumberFound && fromNameFound)
            {
                TaskDialog taskDialog = new TaskDialog("Door Link Command");
                taskDialog.MainInstruction = "Select a mode for the Door Link Command";
                taskDialog.MainContent = "The shared parameters can be propagated with values from either pre-defined system room data or retrieved room data from linked models.";
                taskDialog.AllowCancellation = true;
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Copy To and From values to Shared Parameters.");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Retrieve room data from linked models.");

                TaskDialogResult tResult = taskDialog.Show();
                if (tResult == TaskDialogResult.CommandLink1)
                {
                    DoorLinkManager doorLinkManager = new DoorLinkManager(m_app, DoorLinkType.CopyFromHost);
                }
                else if (tResult == TaskDialogResult.CommandLink2)
                {
                    DoorLinkManager doorLinkManager = new DoorLinkManager(m_app, DoorLinkType.FindFromLink);
                }
            }
            else
            {
                StringBuilder strBuilder = new StringBuilder();
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

                DefinitionBindingMapIterator iter = m_doc.ParameterBindings.ForwardIterator();
                while (iter.MoveNext())
                {
                    Definition definition = iter.Key;
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
