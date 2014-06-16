using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using System.IO;
using HOK.BCFReader.GenericForms;
using HOK.BCFReader.GenericClasses;

namespace HOK.BCFReader
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Command:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;
        private INIDataManager iniDataManager;
        private string bcfFile;
        private CommandForm commandForm;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                iniDataManager = new INIDataManager(m_app);

                if (iniDataManager.FindINI())
                {
                    TaskDialog taskdialog = new TaskDialog("BCF Reader");
                    taskdialog.MainInstruction = "Open a BCF file.";
                    taskdialog.MainContent = "Some bcf files have ever saved in this Revit project.\n You can open a new BCF file or choose a linked file on the file lists.";
                    taskdialog.AllowCancellation = true;
                    taskdialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Open a new BCF file.");
                    taskdialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Select a linked BCF file.");
                    iniDataManager.ReadINI();//collect reference data into referencedictionary

                    TaskDialogResult tResult = taskdialog.Show();
                    if (TaskDialogResult.CommandLink1 == tResult)
                    {
                        bcfFile = OpenBCF(iniDataManager.MasterFilePath);
                    }
                    else if (TaskDialogResult.CommandLink2 == tResult)
                    {
                        BcfListForm bcfForm = new BcfListForm();
                        bcfForm.RefDictionary = iniDataManager.RefDictionary;
                        bcfForm.DisplayBcfFiles();
                        if (DialogResult.OK == bcfForm.ShowDialog())
                        {
                            bcfFile = bcfForm.BCFPath;
                        }
                    }
                    iniDataManager.SelectedFilePath = bcfFile;
                    if (File.Exists(bcfFile))
                    {
                        commandForm = new CommandForm(m_app, bcfFile, iniDataManager);
                        commandForm.ShowDialog();
                    }
                    else if (bcfFile == null)
                    {

                    }
                    else
                    {
                        MessageBox.Show("BCF file dose not exist.\n" + bcfFile, "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (null != iniDataManager.MasterFilePath)
                {
                    bcfFile = OpenBCF(iniDataManager.MasterFilePath);
                    iniDataManager.SelectedFilePath = bcfFile;
                    if (null != bcfFile)
                    {
                        commandForm = new CommandForm(m_app, bcfFile, iniDataManager);
                        commandForm.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("This tool Cannot open the BCF file. Please select an appropriate file. \n"+ex.Message, "Failure: Opening BCF file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Result.Succeeded;
        }

        private string OpenBCF(string iniDirectory)
        {
            string bcfPath = "";
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Path.GetDirectoryName(iniDirectory);
                openFileDialog.Filter = "bcf files (*.bcfzip)|*.bcfzip";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.CheckFileExists)
                    {
                        bcfPath = openFileDialog.FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot open the BCF file. Please select an appropriate file. \n" + ex.Message, "Failure: Openning BCF file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return bcfPath;
        }

        
    }
}
