//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Autodesk.Revit.UI;
//using System.Windows.Forms;
//using RevitDBManager.Classes;
//using RevitDBManager.Forms;

//namespace RevitDBManager
//{
//    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
//    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
//    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

//    public class EditorCommand:IExternalCommand
//    {
//        private UIApplication m_app;
//        private bool dbExist = false;
//        private string dbPath = "";
//        private static form_Editor createDBForm = null;

//        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
//        {
//            try
//            {
//                m_app = commandData.Application;

//                if (createDBForm == null || !createDBForm.Visible)
//                {
//                    DefaultSettings settings = new DefaultSettings(m_app);
//                    dbExist = settings.DatabaseExist;

//                    if (dbExist)
//                    {
//                        dbPath = settings.DefaultDBFile;
//                        createDBForm = new form_Editor(m_app, true, dbPath);
//                        createDBForm.FormClosed += new FormClosedEventHandler(form_Editor_FormClosed);
//                        createDBForm.Text = "Edit Revit Database: " + dbPath;
//                        createDBForm.Show();
//                    }
//                    else
//                    {
//                        DialogResult dr;
//                        dr = MessageBox.Show("A linked database file dose not exist.\n Would you like to create a new database for this project?", "File Not Found", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
//                        if (dr == DialogResult.OK)
//                        {
//                            createDBForm = new form_Editor(m_app, false, "");
//                            createDBForm.FormClosed += new FormClosedEventHandler(form_Editor_FormClosed);
//                            createDBForm.Show();
//                        }
//                    }
//                }
//                return Result.Succeeded;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Revit Data Editor Error: \n" + ex.Message, "Revit Data Editor");
//                return Result.Failed;
//            }
//        }

//        private void form_Editor_FormClosed(object sender, FormClosedEventArgs e)
//        {
//            createDBForm = null;
//        }
//    }
//}
