using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HOK.LPDCalculator.Schedule
{
    class ScheduleDataParser
    {
        //delimiter
        static char[] tabs = { '\t' };
        static char[] quotes = { '"' };

        
        DataTable table;

      
        public DataTable Table
        {
            get { return table; }
        }

        public ScheduleDataParser(string filename)
        {
            try
            {
                var stream = File.OpenText(filename);

                string line;
                string[] a;

                while (null != (line = stream.ReadLine()))
                {
                    a = line
                      .Split(tabs)
                      .Select(s => s.Trim(quotes))
                      .ToArray();

                    // Second line of text file contains 
                    // schedule column names

                    if (null == table)
                    {
                        table = new DataTable();

                        foreach (var column_name in a)
                        {
                            var column = new DataColumn();
                            column.DataType = typeof(string);
                            column.ColumnName = column_name;
                            table.Columns.Add(column);
                        }

                        table.BeginLoadData();

                        continue;
                    }

                    // Remaining lines define schedula data

                    var dr = table.LoadDataRow(a, true);
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
