using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace HOK.LPDCalculator.Schedule
{
    class ScheduleDataParser
    {
        //delimiter
        static char[] tabs = new char[] { '\t' };
        static char[] quotes = new char[] { '"' };

        
        DataTable table = null;

      
        public DataTable Table
        {
            get { return table; }
        }

        public ScheduleDataParser(string filename)
        {
            try
            {
                StreamReader stream = File.OpenText(filename);

                string line;
                string[] a;

                while (null != (line = stream.ReadLine()))
                {
                    a = line
                      .Split(tabs)
                      .Select<string, string>(s => s.Trim(quotes))
                      .ToArray();

                    // Second line of text file contains 
                    // schedule column names

                    if (null == table)
                    {
                        table = new DataTable();

                        foreach (string column_name in a)
                        {
                            DataColumn column = new DataColumn();
                            column.DataType = typeof(string);
                            column.ColumnName = column_name;
                            table.Columns.Add(column);
                        }

                        table.BeginLoadData();

                        continue;
                    }

                    // Remaining lines define schedula data

                    DataRow dr = table.LoadDataRow(a, true);
                }
                table.EndLoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read text file."+filename+"\n"+ex.Message, "ScheduleDataParser", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
