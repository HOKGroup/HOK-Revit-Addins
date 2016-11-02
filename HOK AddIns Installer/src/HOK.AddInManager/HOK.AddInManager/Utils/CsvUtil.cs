using HOK.AddInManager.Classes;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.AddInManager.Utils
{
    public static class CsvUtil
    {
        public static string[] columnNames = new string[] { "icon", "name", "addin", "order", "tooltip", "url"};

        public static ObservableCollection<AddinInfo> ReadAddInList(string csvFile, string sourceDirectory, string installDirectory)
        {
            ObservableCollection<AddinInfo> addinCollection = new ObservableCollection<AddinInfo>();
            using (TextFieldParser parser = new TextFieldParser(csvFile))
            {
                try
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    bool firstRow = true;
                    bool formatMatched = true;

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (firstRow)
                        {
                            for (int i = 0; i < columnNames.Length; ++i)
                            {
                                if (fields[i] != columnNames[i])
                                {
                                    formatMatched = false; break;
                                }
                            }
                            firstRow = false;
                        }
                        else if (formatMatched)
                        {
                            AddinInfo addinInfo = new AddinInfo()
                            {
                                IconName = fields[0],
                                ToolName = fields[1],

                                Index = int.Parse(fields[3]),
                                Tooltip = fields[4],
                                Url = fields[5]
                            };
                            string names = fields[2];
                            string[] splitter = { ", " , ","};
                            string[] splitNames = names.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                            addinInfo.AddInNames = splitNames;
                            addinInfo.GetDetailInfo(sourceDirectory, installDirectory);

                            bool addinExist = true; //to make sure all addin files exist
                            foreach (string addinPath in addinInfo.AddInPaths)
                            {
                                if (!File.Exists(addinPath))
                                {
                                    addinExist = false;
                                }
                            }
                            if (addinExist)
                            {
                                addinCollection.Add(addinInfo);
                            }
                        }
                    }
                    addinCollection = new ObservableCollection<AddinInfo>(addinCollection.OrderBy(o => o.Index));
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
            return addinCollection;
        }
    }
}
