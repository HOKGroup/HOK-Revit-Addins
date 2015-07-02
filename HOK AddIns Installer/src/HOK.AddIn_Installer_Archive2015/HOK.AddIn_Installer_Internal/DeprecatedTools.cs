using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HOK.AddIn_Installer_Internal
{
    public class DeprecatedTools
    {
        private string installDirectory2013 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2013";
        private string installDirectory2014 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2014";
        private string installDirectory2015 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\2015";
        private string installDynamo = @"C:\Autodesk\Dynamo\Core\definitions";
        private Dictionary<Tool2013, RevitToolProperties> deprecated2013 = new Dictionary<Tool2013, RevitToolProperties>();
        private Dictionary<Tool2014, RevitToolProperties> deprecated2014 = new Dictionary<Tool2014, RevitToolProperties>();
        private Dictionary<Tool2015, RevitToolProperties> deprecated2015 = new Dictionary<Tool2015, RevitToolProperties>();
        private Dictionary<string, DynamoToolProperties> deprecatedDynamo = new Dictionary<string, DynamoToolProperties>();

        public Dictionary<Tool2013, RevitToolProperties> Deprecated2013 { get { return deprecated2013; } set { deprecated2013 = value; } }
        public Dictionary<Tool2014, RevitToolProperties> Deprecated2014 { get { return deprecated2014; } set { deprecated2014 = value; } }
        public Dictionary<Tool2015, RevitToolProperties> Deprecated2015 { get { return deprecated2015; } set { deprecated2015 = value; } }
        public Dictionary<string, DynamoToolProperties> DeprecatedDynamo { get { return deprecatedDynamo; } set { deprecatedDynamo = value; } }

        public DeprecatedTools()
        {
            CollectDeprecatedTools2013();
            CollectDeprecatedTools2014();
            CollectDeprecatedTools2015();

            CheckDeprecatedTools();
        }

        private void CollectDeprecatedTools2013()
        {
            try
            {
                //ExportRevision, BatchExport, DoorMark, ImportImage are deprecated.
                Array arrayTools = Enum.GetValues(typeof(Tool2013));
                foreach (Tool2013 tool in arrayTools)
                {
                    List<string> fileNames = new List<string>();
                    RevitToolProperties tp = new RevitToolProperties();
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2013;
                    tp.ToolEnum2013 = tool;

                    switch (tool)
                    {
                        case Tool2013.ExportRevisions:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevisionCloudExtraction.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\revision.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Export Revisions";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevisionCloudExtraction.dll";
                            tp.DLLName = "HOK.RevisionCloudExtraction.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;
                        case Tool2013.BatchExport:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BatchFamilyExporter.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\batch.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Batch Export";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BatchFamilyExporter.dll";
                            tp.DLLName = "HOK.BatchFamilyExporter.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;
                        case Tool2013.DoorMark:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.DoorRenumber.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\door.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Door Mark";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.DoorRenumber.dll";
                            tp.DLLName = "HOK.DoorRenumber.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;
                        case Tool2013.ImportImage:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ImageToDraftingView.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\image.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Import Image";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ImageToDraftingView.dll";
                            tp.DLLName = "HOK.ImageToDraftingView.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;
                        case Tool2013.CreateFinishes:
                            fileNames.Add("\\HOK.CreateFinishFloor.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.CreateFinishFloor.dll");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Create Finishes";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.CreateFinishFloor.dll";
                            tp.DLLName = "HOK.CreateFinishFloor.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;

                        case Tool2013.ModelReporting:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ModelReporting.dll");
                            fileNames.Add("\\HOK.ModelReporting.addin");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Model Reporting";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ModelReporting.dll";
                            tp.DLLName = "HOK.ModelReporting.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;

                        case Tool2013.HOKNavigator:
                            fileNames.Add("\\HOK.Navigator.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Navigator.dll");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "HOK Navigator";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Navigator.dll";
                            tp.DLLName = "HOK.Navigator.dll";

                            deprecated2013.Add(tp.ToolEnum2013, tp);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not collect deprecated tools.\n" + ex.Message, "CollectDeprecatedTools2013", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectDeprecatedTools2014()
        {
            try
            {
                //ExportRevision, BatchExport, DoorMark, ImportImage are deprecated.
                Array arrayTools = Enum.GetValues(typeof(Tool2014));
                foreach (Tool2014 tool in arrayTools)
                {
                    List<string> fileNames = new List<string>();
                    RevitToolProperties tp = new RevitToolProperties();
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2014;
                    tp.ToolEnum2014 = tool;

                    switch (tool)
                    {
                        case Tool2014.ExportRevisions:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevisionCloudExtraction.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\revision.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Export Revisions";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevisionCloudExtraction.dll";
                            tp.DLLName = "HOK.RevisionCloudExtraction.dll";
                            
                            deprecated2014.Add(tp.ToolEnum2014, tp);
                            break;
                        case Tool2014.BatchExport:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BatchFamilyExporter.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\batch.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Batch Export";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BatchFamilyExporter.dll";
                            tp.DLLName = "HOK.BatchFamilyExporter.dll";

                            deprecated2014.Add(tp.ToolEnum2014, tp);
                            break;
                        case Tool2014.DoorMark:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.DoorRenumber.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\door.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Door Mark";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.DoorRenumber.dll";
                            tp.DLLName = "HOK.DoorRenumber.dll";

                            deprecated2014.Add(tp.ToolEnum2014, tp);
                            break;
                        case Tool2014.ImportImage:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ImageToDraftingView.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\image.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Import Image";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ImageToDraftingView.dll";
                            tp.DLLName = "HOK.ImageToDraftingView.dll";

                            deprecated2014.Add(tp.ToolEnum2014, tp);
                            break;
                        case Tool2014.ModelReporting:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ModelReporting.dll");
                            fileNames.Add("\\HOK.ModelReporting.addin");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Model Reporting";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ModelReporting.dll";
                            tp.DLLName = "HOK.ModelReporting.dll";

                            deprecated2014.Add(tp.ToolEnum2014, tp);
                            break;
                        case Tool2014.HOKNavigator:
                            fileNames.Add("\\HOK.Navigator.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Navigator.dll");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "HOK Navigator";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Navigator.dll";
                            tp.DLLName = "HOK.Navigator.dll";

                            deprecated2014.Add(tp.ToolEnum2014, tp);
                            break;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not collect deprecated tools.\n" + ex.Message, "CollectDeprecatedTools2014", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectDeprecatedTools2015()
        {
            try
            {
                //ExportRevision, BatchExport, DoorMark, ImportImage are deprecated.
                Array arrayTools = Enum.GetValues(typeof(Tool2015));
                foreach (Tool2015 tool in arrayTools)
                {
                    List<string> fileNames = new List<string>();
                    RevitToolProperties tp = new RevitToolProperties();
                    tp.TargetSoftWareEnum = TargetSoftware.Revit_2015;
                    tp.ToolEnum2015 = tool;

                    switch (tool)
                    {
                        case Tool2015.ExportRevisions:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevisionCloudExtraction.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\revision.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Export Revisions";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevisionCloudExtraction.dll";
                            tp.DLLName = "HOK.RevisionCloudExtraction.dll";

                            deprecated2015.Add(tp.ToolEnum2015, tp);
                            break;
                        case Tool2015.BatchExport:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BatchFamilyExporter.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\batch.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Batch Export";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BatchFamilyExporter.dll";
                            tp.DLLName = "HOK.BatchFamilyExporter.dll";

                            deprecated2015.Add(tp.ToolEnum2015, tp);
                            break;
                        case Tool2015.DoorMark:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.DoorRenumber.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\door.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Door Mark";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.DoorRenumber.dll";
                            tp.DLLName = "HOK.DoorRenumber.dll";

                            deprecated2015.Add(tp.ToolEnum2015, tp);
                            break;
                        case Tool2015.ImportImage:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ImageToDraftingView.dll");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\image.ico");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Import Image";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ImageToDraftingView.dll";
                            tp.DLLName = "HOK.ImageToDraftingView.dll";

                            deprecated2015.Add(tp.ToolEnum2015, tp);
                            break;
                        case Tool2015.ModelReporting:
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ModelReporting.dll");
                            fileNames.Add("\\HOK.ModelReporting.addin");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "Model Reporting";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ModelReporting.dll";
                            tp.DLLName = "HOK.ModelReporting.dll";

                            deprecated2015.Add(tp.ToolEnum2015, tp);
                            break;
                        case Tool2015.HOKNavigator:
                            fileNames.Add("\\HOK.Navigator.addin");
                            fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Navigator.dll");
                            tp.FilePaths = fileNames;

                            tp.ToolName = "HOK Navigator";
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Navigator.dll";
                            tp.DLLName = "HOK.Navigator.dll";

                            deprecated2015.Add(tp.ToolEnum2015, tp);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not collect deprecated tools.\n" + ex.Message, "CollectDeprecatedTools2014", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CheckDeprecatedTools()
        {
            StringBuilder strBuilder = new StringBuilder();

            foreach (Tool2013 tool in deprecated2013.Keys)
            {
                RevitToolProperties tp = deprecated2013[tool];
                string fullPath = installDirectory2013 + tp.DllPath;

                if (File.Exists(fullPath))
                {
                    strBuilder.AppendLine(tp.ToolName + " 2013");
                }
            }

            foreach (Tool2014 tool in deprecated2014.Keys)
            {
                RevitToolProperties tp = deprecated2014[tool];
                string fullPath = installDirectory2014 + tp.DllPath;

                if (File.Exists(fullPath))
                {
                    strBuilder.AppendLine(tp.ToolName + " 2014");
                }
            }

            foreach (Tool2015 tool in deprecated2015.Keys)
            {
                RevitToolProperties tp = deprecated2015[tool];
                string fullPath = installDirectory2015 + tp.DllPath;

                if (File.Exists(fullPath))
                {
                    strBuilder.AppendLine(tp.ToolName + " 2015");
                }
            }

            if (strBuilder.Length > 0)
            {
                DialogResult dr = MessageBox.Show("The HOK Addins Installer has detected outdated tools.\nThe following addins have been replaced with newer tools.\nClick \"OK\" to uninstall the outdated tools from your computer.\n\n"+strBuilder.ToString(), 
                    "Outdated Tools Detected!", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                //DialogResult dr = MessageBox.Show("[Export Revision], [Batch Family Export], [Door Mark], and [Import Image] have been superseded by other tools.\nWould you like to uninstall the tools from your machine?", "Uninstallation of Deprecated Tools", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.OK)
                {
                    DeleteDeprecatedFiles();
                }
            }
        }

        private void DeleteDeprecatedFiles()
        {
            try
            {
                foreach (Tool2013 tool in deprecated2013.Keys)
                {
                    RevitToolProperties tp = deprecated2013[tool];
                    string fullPath = installDirectory2013 + tp.DllPath;

                    if (File.Exists(fullPath))
                    {
                        foreach (string filePath in tp.FilePaths)
                        {
                            fullPath = installDirectory2013 + filePath;
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
                        }
                    }
                }

                foreach (Tool2014 tool in deprecated2014.Keys)
                {
                    RevitToolProperties tp = deprecated2014[tool];
                    string fullPath = installDirectory2014 + tp.DllPath;

                    if (File.Exists(fullPath))
                    {
                        foreach (string filePath in tp.FilePaths)
                        {
                            fullPath = installDirectory2014 + filePath;
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
                        }
                    }
                }

                foreach (Tool2015 tool in deprecated2015.Keys)
                {
                    RevitToolProperties tp = deprecated2015[tool];
                    string fullPath = installDirectory2015 + tp.DllPath;

                    if (File.Exists(fullPath))
                    {
                        foreach (string filePath in tp.FilePaths)
                        {
                            fullPath = installDirectory2015 + filePath;
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete deprecated files.\n" + ex.Message, "DeleteDeprecatedFiles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
