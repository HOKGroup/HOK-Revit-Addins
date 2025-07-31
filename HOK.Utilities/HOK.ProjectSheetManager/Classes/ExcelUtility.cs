using System.Data;
using System.IO;
using System.Windows.Shapes;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using MiniExcelLibs.OpenXml.Models;

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
                for(int i = 0; i < sheets.Count; i++)
                {
                    string worksheetName = sheets[i].Name;
                    if(worksheetName != "Renumber Sheets" && worksheetName != "Rename Views")
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
#pragma warning disable CS0618 // Type or member is obsolete
            m_DataTable = MiniExcel.QueryAsDataTable(m_Settings.ExcelPath, sheetName: nameWorksheet);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void FillExcelWorksheetFromDataTable(string nameWorksheet)
        {
            var sheets = new DataSet();
            foreach(SheetInfo sheet in MiniExcel.GetSheetInformations(m_Settings.ExcelPath))
            {
                if(sheet.Name == nameWorksheet)
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
            get
            {
                return m_DataTable;
            }
            set
            {
                m_DataTable = value;
            }
        }
    }
}
