using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] columnNames = { "key", "description", "parentKey", "keynoteSet_id" };

                List<KeynoteInfo> keynoteList = new List<KeynoteInfo>();
                string path = @"B:\Revit Projects\Keynote Manager\Keynote.csv";
                if (File.Exists(path))
                {
                    using (TextFieldParser parser = new TextFieldParser(path))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        bool firstRow = true;
                        bool formatMismatched = false;

                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();
                            if (firstRow)
                            {
                                for (int i = 0; i < columnNames.Length; i++)
                                {
                                    if (fields[i] != columnNames[i])
                                    {
                                        formatMismatched = true; break;
                                    }
                                }

                                if (formatMismatched)
                                {
                                    Console.WriteLine("Column names should be the following orders.\n[" + columnNames[0] + "], [" + columnNames[1] + "], [" + columnNames[2] + "], [" + columnNames[3] + "]"); 
                                }
                                firstRow = false;
                            }
                            else
                            {
                                string key = fields[0];
                                string description = fields[1];
                                string parentKey = fields[2];
                                string keynoteset_id = fields[3];

                                KeynoteInfo keynoteInfo = new KeynoteInfo(Guid.NewGuid().ToString(), key, parentKey, description, keynoteset_id);
                                keynoteList.Add(keynoteInfo);
                            }
                        }

                        string content = "";
                        string errorMsg = "";
                        HttpStatusCode status = ServerUtil.PostKeynote(out content, out errorMsg, keynoteList);
                    }

                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            
        }
    }
}
