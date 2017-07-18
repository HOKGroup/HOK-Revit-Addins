using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Office.Interop.Access.Dao;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using RevitDBManager.Forms;
using RevitDBManager.Classes;
using RevitDBManager.ViewerClasses;

namespace RevitDBManager
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        private UIApplication m_app;
        private string dbPath = "";
        private Database database;
        private Autodesk.Revit.DB.Document doc;
        private static form_Viewer viewerForm = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("DataSynch", doc));

                var fielSelectionForm = new form_FileSelection(m_app);
                if (fielSelectionForm.ShowDialog() == DialogResult.OK)
                {
                    dbPath = fielSelectionForm.DefualtFilePath;
                    if (string.Empty != dbPath)
                    {
                        var dbViewer = new RevitDBViewer(m_app, dbPath, false, false);
                        database = dbViewer.DaoDB;

                        var externalReference = new ExternalReference(database);

                        var readRevit = new ReadFromRevit(doc, database)
                            {
                                NonVisibleFields = dbViewer.NonVisibleFields,
                                LockTypeFields = dbViewer.LockTypeFields,
                                ParamIDMap = dbViewer.ParamIdMap,
                                ExcludeInstSettings = dbViewer.ExcludeInstanceSettings,
                                ExternalFields = externalReference.ExternalFields,
                                SourceTables = externalReference.SourceTables,
                                ProgressbarText = "Synchronizing..."
                            };

                        readRevit.CollectRevitElementsData();
                        readRevit.SaveRevitData(); //Save Revit Data into Database (ReadOnly, RevitOnly, LockAll)

                        dbViewer.CloseDatabase();

                        if (readRevit.FixMode && File.Exists(dbPath))
                        {
                            if (viewerForm == null || !viewerForm.Visible)
                            {
                                viewerForm = new form_Viewer(m_app, dbPath, true)
                                {
                                    WarningTables = readRevit.WarningTables
                                };
                                viewerForm.FormClosed += form_Viwer_FormClosed;
                                viewerForm.Show();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Successfully Synchronized. All Revit elements are up to date.", "Synchronization: ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("A database file linked to this project dose not exist.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
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

        private void form_Viwer_FormClosed(object sender, FormClosedEventArgs e)
        {
            viewerForm = null;
        }
    }

    internal static class MessageManager
    {
        // store the warning/error messages
        public static StringBuilder MessageBuff { get; set; } = new StringBuilder();
    }
}
