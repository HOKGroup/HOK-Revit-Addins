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
        private string excelFilePath;
        private UIApplication uiApp;
        private Document doc;
        private Dictionary<string, Autodesk.Revit.DB.View> m_Views;
        private Dictionary<string, ViewSheet> m_Sheets;
        private Dictionary<string, Sheet> m_AddinSheets;
        private Dictionary<string, Tblk> m_Tblks;

        const string HOK_EXCEL_PATH_PARAM_NAME = "Sheet Manager Excel File Path";

        public Settings(ExternalCommandData commandData)
        {
            cmdData = commandData;
            doc = cmdData.Application.ActiveUIDocument.Document;
            m_Tblks = new Dictionary<string, Tblk>();
            m_Sheets = new Dictionary<string, ViewSheet>();
            m_AddinSheets = new Dictionary<string, Sheet>();
            m_Views = new Dictionary<string, Autodesk.Revit.DB.View>();

            // Get the global parameter, create it if it doesn't exist
            ElementId hokExcelFilePathParamId = GlobalParametersManager.FindByName(doc, HOK_EXCEL_PATH_PARAM_NAME);

            // If it doesn't exist, create it
            if (hokExcelFilePathParamId == ElementId.InvalidElementId)
            {
                CreateGlobalParamExcelPath(HOK_EXCEL_PATH_PARAM_NAME);
                excelFilePath = "";
            }
            else
            {
                GlobalParameter hokExcelFilePathParam = doc.GetElement(hokExcelFilePathParamId) as GlobalParameter;

                string paramValue = (hokExcelFilePathParam.GetValue() as StringParameterValue).Value;

                excelFilePath = paramValue;
            }

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
        }

        private void CreateGlobalParamExcelPath(string paramName)
        {
            using (Transaction tr = new Transaction(doc, "Create Sheet Manager Excel Path Parameter"))
            {
                tr.Start();
                GlobalParameter.Create(doc, paramName, SpecTypeId.String.Text);
                tr.Commit();
            }
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
        public string ExcelPath()
        {
            return excelFilePath;
        }
    }
}
