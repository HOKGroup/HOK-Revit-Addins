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
            List<SheetInfo> sheets = MiniExcel.GetSheetInformations(m_Settings.ExcelPath());
            DataRow row = null;
            try
            {
                m_DataTable = new DataTable();
                m_DataTable.Columns.Add(nameColumn, typeof(string));
                for(int i = 0; i < sheets.Count; i++)
                {
                    string worksheetName = sheets[0].Name;
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
            m_DataTable = MiniExcel.QueryAsDataTable(m_Settings.ExcelPath(), sheetName: nameWorksheet);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void FillExcelWorksheetFromDataTable(string nameWorksheet)
        {
            var values = new List<Dictionary<string, object>>();

            foreach(DataRow dataRow in m_DataTable.Rows)
            {
                for(int i = 0; i < m_DataTable.Columns.Count; i ++)
                {
                    values.Add(new Dictionary<string, object> { { m_DataTable.Columns[i].ColumnName.ToString(), dataRow.ItemArray[i].ToString() } });
                }
            }

            MiniExcel.SaveAs(m_Settings.ExcelPath(), values, sheetName: nameWorksheet);
        }

        public DataTable DataTable
        {
            get
            {
                return m_DataTable;
            }
        }
    }
}
