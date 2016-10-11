using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using RevitDBManager.Forms;
using System.Windows.Forms;
using RevitDBManager.Classes;
using Microsoft.Office.Interop.Access.Dao;
using RevitDBManager.ViewerClasses;
using System.IO;
using Autodesk.Revit.DB;

namespace RevitDBManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class Command:IExternalCommand
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private string dbPath = "";
        private Database database;
        private Autodesk.Revit.DB.Document doc;
        private static form_Viewer viewerForm = null;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                doc = m_app.ActiveUIDocument.Document;

                form_FileSelection fielSelectionForm = new form_FileSelection(m_app);
                if (fielSelectionForm.ShowDialog() == DialogResult.OK)
                {
                    dbPath = fielSelectionForm.DefualtFilePath;
                    if (string.Empty != dbPath)
                    {
                        RevitDBViewer dbViewer = new RevitDBViewer(m_app, dbPath, false, false);
                        database = dbViewer.DaoDB;

                        ExternalReference externalReference = new ExternalReference(database);

                        ReadFromRevit readRevit = new ReadFromRevit(doc, database);
                        readRevit.NonVisibleFields = dbViewer.NonVisibleFields;
                        readRevit.LockTypeFields = dbViewer.LockTypeFields;
                        readRevit.ParamIDMap = dbViewer.ParamIdMap;
                        readRevit.ExcludeInstSettings = dbViewer.ExcludeInstanceSettings;
                        readRevit.ExternalFields = externalReference.ExternalFields;
                        readRevit.SourceTables = externalReference.SourceTables;
                        readRevit.ProgressbarText = "Synchronizing...";

                        readRevit.CollectRevitElementsData();
                        readRevit.SaveRevitData(); //Save Revit Data into Database (ReadOnly, RevitOnly, LockAll)

                        dbViewer.CloseDatabase();

                        if (readRevit.FixMode && File.Exists(dbPath))
                        {
                            if (viewerForm == null || !viewerForm.Visible)
                            {
                                viewerForm = new form_Viewer(m_app, dbPath, true);
                                viewerForm.WarningTables = readRevit.WarningTables;
                                viewerForm.FormClosed += new FormClosedEventHandler(form_Viwer_FormClosed);
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
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: cannot start Revit DB Link." + ex.Message, "Revit DB Link");
                return Result.Failed;
            }
        }

        private void form_Viwer_FormClosed(object sender, FormClosedEventArgs e)
        {
            viewerForm = null;
        }
    }

     static class MessageManager
     {
         static StringBuilder m_messageBuff = new StringBuilder();
         // store the warning/error messages
         public static StringBuilder MessageBuff
         {
             get { return m_messageBuff; }
             set { m_messageBuff = value; }
         }
     }
}
