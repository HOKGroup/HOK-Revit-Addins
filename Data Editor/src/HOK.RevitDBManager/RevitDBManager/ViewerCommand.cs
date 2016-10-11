using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using RevitDBManager.Classes;
using RevitDBManager.Forms;
using System.IO;

namespace RevitDBManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class ViewerCommand:IExternalCommand
    {
        private UIApplication m_app;
        private bool dbExist = false;
        private string dbPath = "";
        private static form_Viewer viewerForm = null;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;

                DefaultSettings settings = new DefaultSettings(m_app);
                dbExist = settings.DatabaseExist;
                dbPath = settings.DefaultDBFile;

                if (dbExist&&File.Exists(dbPath))
                {
                    if (viewerForm == null || !viewerForm.Visible)
                    {
                        viewerForm = new form_Viewer(m_app, dbPath,false);
                        viewerForm.FormClosed += new FormClosedEventHandler(form_Viwer_FormClosed);
                        viewerForm.Show();
                    }
                }

                else
                {
                    MessageBox.Show("A linked database does not exist.\n"+settings.DefaultDBFile, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Database Viewer." + ex.Message);
                return Result.Failed;
            }
        }

        private void form_Viwer_FormClosed(object sender, FormClosedEventArgs e)
        {
            viewerForm = null;
        }
    }
}
