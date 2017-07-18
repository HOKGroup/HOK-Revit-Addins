using System;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using RevitDBManager.Classes;
using RevitDBManager.Forms;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Autodesk.Revit.Attributes;

namespace RevitDBManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class ViewerCommand:IExternalCommand
    {
        private UIApplication m_app;
        private bool dbExist;
        private string dbPath = "";
        private static form_Viewer viewerForm;
        private Autodesk.Revit.DB.Document doc;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                doc = m_app.ActiveUIDocument.Document;

                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("DataEditor", doc));

                var settings = new DefaultSettings(m_app);
                dbExist = settings.DatabaseExist;
                dbPath = settings.DefaultDBFile;

                if (dbExist&&File.Exists(dbPath))
                {
                    if (viewerForm == null || !viewerForm.Visible)
                    {
                        viewerForm = new form_Viewer(m_app, dbPath,false);
                        viewerForm.FormClosed += form_Viwer_FormClosed;
                        viewerForm.Show();
                    }
                }

                else
                {
                    MessageBox.Show("A linked database does not exist.\n"+settings.DefaultDBFile, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Log.AppendLog(LogMessageType.INFO, "Ended");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return Result.Failed;
            }
        }

        private static void form_Viwer_FormClosed(object sender, FormClosedEventArgs e)
        {
            viewerForm = null;
        }
    }
}
