namespace HOK.ProjectSheetManager.Classes
{
    public class Settings
    {
        private ExternalCommandData cmdData;
        private string excelFilePath;
        private Document doc;
        private Dictionary<string, Autodesk.Revit.DB.View> m_Views;
        private Dictionary<string, ViewSheet> m_Sheets;
        private Dictionary<string, Sheet> m_AddinSheets;
        private Dictionary<string, Titleblock> m_Tblks;

        const string HOK_EXCEL_PATH_PARAM_NAME = "Sheet Manager Excel File Path";

        public Settings(ExternalCommandData commandData)
        {
            cmdData = commandData;
            doc = Context.ActiveDocument;
            if (doc == null)
            {
                throw new Exception(
                    "No active document found. Please open a Revit project before running the command."
                );
            }

            m_Tblks = new Dictionary<string, Titleblock>();
            m_Sheets = new Dictionary<string, ViewSheet>();
            m_AddinSheets = new Dictionary<string, Sheet>();
            m_Views = new Dictionary<string, Autodesk.Revit.DB.View>();

            // Get the global parameter, create it if it doesn't exist
            ElementId hokExcelFilePathParamId = GlobalParametersManager.FindByName(
                doc,
                HOK_EXCEL_PATH_PARAM_NAME
            );

            // If it doesn't exist, create it
            if (hokExcelFilePathParamId == ElementId.InvalidElementId)
            {
                CreateGlobalParamExcelPath(HOK_EXCEL_PATH_PARAM_NAME);
                excelFilePath = "";
            }
            else
            {
                GlobalParameter hokExcelFilePathParam = (GlobalParameter)
                    doc.GetElement(hokExcelFilePathParamId);

                string paramValue = ((StringParameterValue)hokExcelFilePathParam.GetValue()).Value;

                excelFilePath = paramValue;
            }

            // List Sheets and Titleblocks
            GetSheetsAndTitleblockInstances();

            // Get the views
            ViewSheet viewSheet;
            Autodesk.Revit.DB.View view;
            IList<Autodesk.Revit.DB.View> views = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .Cast<Autodesk.Revit.DB.View>()
                .ToList();

            // Verify what we have collected
            foreach (var viewTest in views)
            {
                view = viewTest;
                viewSheet = viewTest as ViewSheet;
                if (viewSheet != null)
                    continue;
                // Excluding sheets in case they have the same name
                m_Views.TryAdd(view.Name, view);
            }

            // Get the ViewSchedules
            IList<Element> m_dSchedules = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSchedule))
                .ToElements();

            foreach (Element element in m_dSchedules)
            {
                ViewSchedule schedule = (ViewSchedule)element;
                if (!m_Views.ContainsKey(schedule.Name))
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
            IList<Element> m_Tblk = m_ColTblk
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .ToElements();

            // List all parameters
            foreach (Element x in m_Tblk)
            {
                List<Parameter> m_ParaList = new List<Parameter>();
                string m_SheetNumber = "";

                foreach (Autodesk.Revit.DB.Parameter p in x.Parameters)
                {
                    if (p != null)
                    {
                        Parameter m_para = new Parameter(p);

                        if (m_para.Name == "Sheet Number")
                            m_SheetNumber = m_para.Value;

                        m_ParaList.Add(m_para);
                    }
                }

                Titleblock m_clsTblkItem = new Titleblock(m_ParaList, x);

                try
                {
                    if (!m_Tblks.ContainsKey(m_SheetNumber))
                    {
                        m_Tblks.Add(m_SheetNumber, m_clsTblkItem);
                    }
                }
                catch { }
            }

            // Get all sheets as sheets
            var sheets = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Sheets)
                .ToElements();

            // List all parameters
            foreach (Element x in sheets)
            {
                try
                {
                    ViewSheet m_Sht = (ViewSheet)x;
                    string m_SheetNumber = "";
                    if (!string.IsNullOrEmpty(m_Sht.SheetNumber))
                    {
                        m_SheetNumber = m_Sht.SheetNumber;
                    }

                    // Add to the source dictionary object
                    if (!m_Sheets.ContainsKey(m_SheetNumber))
                    {
                        m_Sheets.Add(m_SheetNumber, m_Sht);
                    }

                    List<Parameter> m_ParaList = new List<Parameter>();
                    foreach (Autodesk.Revit.DB.Parameter p in m_Sht.Parameters)
                    {
                        if (p != null)
                        {
                            Parameter m_para = new Parameter(p);
                            m_ParaList.Add(m_para);
                        }
                    }

                    Sheet m_clsSheetItem = new Sheet(m_ParaList, m_Sht);

                    try
                    {
                        if (!m_AddinSheets.ContainsKey(m_SheetNumber))
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

        public Dictionary<string, Sheet> clsSheetsList => m_AddinSheets;
        public Dictionary<string, Titleblock> clsTblksList => m_Tblks;

        public string ApplicationVersion()
        {
            return $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        public string ProgramName()
        {
            return "SheetManager";
        }

        public UIApplication Application => cmdData.Application;
        public Document Document => doc;
        public Dictionary<string, ViewSheet> Sheets => m_Sheets;
        public Dictionary<string, Autodesk.Revit.DB.View> Views => m_Views;
        public string ExcelPath
        {
            get { return excelFilePath; }
            set { excelFilePath = value; }
        }
    }
}
