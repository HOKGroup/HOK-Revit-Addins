using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using Microsoft.VisualBasic;

namespace HOK.ProjectSheetManager.Classes
{
    public class Settings
    {
        private ExternalCommandData cmdData;
        private string iniFileName = "SheetMaker.ini";
        private string iniPath;
        private string accessFilePath;
        private string excelFilePath;
        private UIApplication uiApp;
        private Document doc;
        private Dictionary<string, Autodesk.Revit.DB.View> m_Views;
        private Dictionary<string, ViewSheet> m_Sheets;
        private Dictionary<string, Sheet> m_AddinSheets;
        private Dictionary<string, Tblk> m_Tblks;

        public Settings(ExternalCommandData commandData)
        {
            cmdData = commandData;
            doc = cmdData.Application.ActiveUIDocument.Document;

            string dirName = string.Empty;
            string masterFilePath = doc.PathName;

            if (doc.IsWorkshared)
            {
                ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                if (modelPath != null)
                {
                    masterFilePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if(string.IsNullOrEmpty(masterFilePath))
                    {
                        masterFilePath = doc.PathName;
                    }
                    dirName = Path.GetDirectoryName(masterFilePath);
                }
            }
            else if (!string.IsNullOrEmpty(doc.PathName))
            {
                dirName = Path.GetDirectoryName(doc.PathName);
            }

            if(string.IsNullOrEmpty(dirName) == false)
            {
                iniPath = Path.Combine(dirName, iniFileName);
            }

            if(!File.Exists(iniPath))
            {
                TaskDialog dlg = new TaskDialog("Sheet Manager");
                dlg.MainInstruction = "SheetMaker.ini File Not Found";
                dlg.MainContent = "Failed to find INI file.\nPlease create a new INI file or open an existing file.";
                dlg.AllowCancellation = true;
                dlg.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Create a New INI File.");
                dlg.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Open an Existing INI File.");

                TaskDialogResult result = dlg.Show();
                switch(result)
                {
                    case TaskDialogResult.CommandLink1:
                        FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                        folderDialog.Description = "Select the directory that you want to use as the default.";
                        if(Directory.Exists(dirName))
                        {
                            folderDialog.SelectedPath = dirName;
                        }
                        DialogResult fdr = folderDialog.ShowDialog();
                        if(fdr == DialogResult.OK)
                        {
                            iniPath = folderDialog.SelectedPath + "\\" + iniFileName;
                            WriteIniFile();
                        }
                        return;
                    case TaskDialogResult.CommandLink2:
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.DefaultExt = "ini";
                        openFileDialog.Filter = "ini files (*.ini)|*.ini";
                        DialogResult odr = openFileDialog.ShowDialog();
                        if(odr == DialogResult.OK)
                        {
                            string openFileName = openFileDialog.FileName;
                            if(openFileName.Contains(iniFileName))
                            {
                                iniPath = openFileName;
                            }
                            else
                            {
                                MessageBox.Show("Please select a valid ini file.\nThe  file name should be SheetMaker.ini", "File Name: SheetMaker.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                odr = openFileDialog.ShowDialog();
                            }
                        }
                        return;
                }
            }
            string currentFolder = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf("\\"));
            excelFilePath = currentFolder + "\\SheetMakerSampleData.xlsx";

            // List Sheets and Titleblocks
            GetSheetsAndTitleblockInstances();

            // Get the views
            List<Autodesk.Revit.DB.View> views;
            ViewSheet viewSheet;
            Autodesk.Revit.DB.View view;
            FilteredElementCollector collViews = new FilteredElementCollector(doc);
            collViews.OfCategory(BuiltInCategory.OST_Views);
            views = collViews.Cast<Autodesk.Revit.DB.View>().ToList();

            // Verify what we have collected
            foreach(Element viewTest in views)
            {
                view = viewTest as Autodesk.Revit.DB.View;
                viewSheet = viewTest as ViewSheet;
                if (viewSheet != null)
                    continue;
                // Excluding sheets in case they have the same name
                if(!m_Views.ContainsKey(view.Name))
                {
                    m_Views.Add(view.Name, view);
                }
            }

            // Get the ViewSchedules
            FilteredElementCollector collectorSchedule = new FilteredElementCollector(doc);
            collectorSchedule.OfClass(typeof(ViewSchedule));
            IList<Element> m_dSchedules = collectorSchedule.ToElements();

            foreach(Element element in m_dSchedules)
            {
                ViewSchedule schedule = element as ViewSchedule;
                if(!m_Views.ContainsKey(schedule.Name))
                {
                    m_Views.Add(schedule.Name, schedule);
                }
            }

            // Reading ini overrides defaults
            ReadIniFile();
        }

        public void GetSheetsAndTitleblockInstances()
        {
            // Get all sheets as titleblocks
            FilteredElementCollector m_ColTblk = new FilteredElementCollector(doc);
            m_ColTblk.WhereElementIsNotElementType();
            IList<Element> m_Tblk = m_ColTblk.OfCategory(BuiltInCategory.OST_TitleBlocks).ToElements();

            // List all parameters
            foreach(Element x in m_Tblk)
            {
                List<Parameter> m_ParaList = new List<Parameter>();
                string m_SheetNumber = "";

                foreach (Autodesk.Revit.DB.Parameter p in x.Parameters)
                {
                    if(p != null)
                    {
                        Parameter m_para = new Parameter(p);

                        if (m_para.Name == "Sheet Number")
                            m_SheetNumber = m_para.Value;

                        m_ParaList.Add(m_para);
                    }
                }

                Tblk m_clsTblkItem = new Tblk(m_ParaList, x);
                
                try
                {
                    if(!m_Tblks.ContainsKey(m_SheetNumber))
                    {
                        m_Tblks.Add(m_SheetNumber, m_clsTblkItem);
                    }
                }
                catch { }
            }

            // Get all sheets as sheets
            FilteredElementCollector m_Colsheets = new FilteredElementCollector(doc);
            m_Colsheets.WhereElementIsNotElementType();
            IList<Element> m_ListSheets = m_Colsheets.OfCategory(BuiltInCategory.OST_Sheets).ToElements();

            // List all parameters
            foreach(Element x in m_ListSheets)
            {
                try
                {
                    ViewSheet m_Sht = x as ViewSheet;
                    string m_SheetNumber = "";
                    if(!string.IsNullOrEmpty(m_Sht.SheetNumber))
                    {
                        m_SheetNumber = m_Sht.SheetNumber;
                    }

                    // Add to the source dictionary object
                    if(!m_Sheets.ContainsKey(m_SheetNumber))
                    {
                        m_Sheets.Add(m_SheetNumber, m_Sht);
                    }

                    List<Parameter> m_ParaList = new List<Parameter>();
                    foreach (Autodesk.Revit.DB.Parameter p in m_Sht.Parameters)
                    {
                        if(p != null)
                        {
                            Parameter m_para = new Parameter(p);
                            m_ParaList.Add(m_para);
                        }
                    }

                    Sheet m_clsSheetItem = new Sheet(m_ParaList, m_Sht);

                    try
                    {
                        if(!m_AddinSheets.ContainsKey(m_SheetNumber))
                        {
                            m_AddinSheets.Add(m_SheetNumber, m_clsSheetItem);
                        }
                    }
                    catch { }
                }
                catch { }
            }
        }

        public void WriteIniFile() // Add info for views
        {
            try
            {
                // Note, if file doesn't exist it is created
                // The using statement also closes the StreamWriter.
                if (iniPath != "")
                {
                    // incase project has not been saved yet
                    using (StreamWriter sw = new StreamWriter(iniPath))
                    {
                        sw.WriteLine(excelFilePath);
                        sw.WriteLine("");
                        sw.WriteLine("*** Only the first line of this file is read ***");
                        sw.WriteLine("    Format:");
                        sw.WriteLine("    String - Path and filename of Excel file");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public void ReadIniFile()
        {
            // Read the ini file if possible if not it will get written later when the user sets the path
            try
            {
                if(File.Exists(iniPath))
                {
                    string input = "";
                    using(StreamReader sr = File.OpenText(iniPath))
                    {
                        if(InlineAssignHelper(ref input, sr.ReadLine()) == "") {
                            MessageBox.Show("Unable to read Excel file path from file: \n" + iniPath, "Error Reading ini file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            excelFilePath = input;
                        }
                        sr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message , "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static T InlineAssignHelper<T>(ref T target, T value)
        {
            target = value;
            return value;
        }
        public Dictionary<string, Sheet> clsSheetsList
        {
            get
            {
                return m_AddinSheets;
            }
        }
        public Dictionary<string, Tblk> clsTblksList
        {
            get
            {
                return m_Tblks;
            }
        }
        public string ApplicationVersion()
        {
            return "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        public string ProgramName()
        {
            return "SheetManager";
        }
        public UIApplication Application
        {
            get
            {
                return this.cmdData.Application;
            }
        }
        public Document Document
        {
            get
            {
                return this.doc;
            }
        }
        public Dictionary<string, ViewSheet> Sheets
        {
            get
            {
                return m_Sheets;
            }
        }
        public Dictionary<string, Autodesk.Revit.DB.View> Views
        {
            get
            {
                return m_Views;
            }
        }
        public string IniPath()
        {
            return iniPath;
        }
        public string ExcelPath()
        {
            return excelFilePath;
        }
    }
}
