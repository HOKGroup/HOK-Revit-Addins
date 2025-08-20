namespace HOK.ProjectSheetManager.Classes
{
    class ExcelUtility
    {
        private Settings m_Settings;
        private DataTable m_DataTable;

        public ExcelUtility(Settings settings)
        {
            m_Settings = settings;
        }

        public bool FillDataTableFromExcelSheetNames(string nameColumn)
        {
            List<SheetInfo> sheets = MiniExcel.GetSheetInformations(m_Settings.ExcelPath);
            DataRow row = null;
            try
            {
                m_DataTable = new DataTable();
                m_DataTable.Columns.Add(nameColumn, typeof(string));
                for (int i = 0; i < sheets.Count; i++)
                {
                    string worksheetName = sheets[i].Name;
                    if (worksheetName != "Renumber Sheets" && worksheetName != "Rename Views")
                    {
                        row = m_DataTable.NewRow();
                        row[0] = worksheetName;
                        m_DataTable.Rows.Add(row);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void FillDataTableFromExcelWorksheet(string nameWorksheet)
        {
            // This may need to be updated in the future once it's no longer supported in a future release
            // Until then, this was how it was implemented in the previous version of the add-in
            m_DataTable = MiniExcel.QueryAsDataTable(m_Settings.ExcelPath, sheetName: nameWorksheet);
        }

        public void FillExcelWorksheetFromDataTable(string nameWorksheet)
        {
            var sheets = new DataSet();
            foreach (SheetInfo sheet in MiniExcel.GetSheetInformations(m_Settings.ExcelPath))
            {
                if (sheet.Name == nameWorksheet)
                {
                    sheets.Tables.Add(this.DataTable);
                }
                else
                {
                    sheets.Tables.Add(MiniExcel.QueryAsDataTable(m_Settings.ExcelPath, sheetName: sheet.Name));
                }
            }
            MiniExcel.SaveAs(m_Settings.ExcelPath, sheets, overwriteFile: true, excelType: ExcelType.XLSX);
        }

        public DataTable DataTable
        {
            get { return m_DataTable; }
            set { m_DataTable = value; }
        }
    }
}
