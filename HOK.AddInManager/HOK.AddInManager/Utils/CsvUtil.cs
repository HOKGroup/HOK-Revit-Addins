using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using HOK.AddInManager.Classes;
using HOK.Core.Utilities;

namespace HOK.AddInManager.Utils
{
    public static class CsvUtil
    {
        public static string[] ColumnNames =
        {
            "icon", "name", "addin", "order", "tooltip", "url", "dropdownOptionsFlag"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="sourceDirectory"></param>
        /// <param name="installDirectory"></param>
        /// <returns></returns>
        public static ObservableCollection<AddinInfo> ReadAddInList(string csvFile, string sourceDirectory, string installDirectory)
        {
            var addinCollection = new ObservableCollection<AddinInfo>();
            using (var parser = new TextFieldParser(csvFile))
            {
                try
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    var firstRow = true;
                    var formatMatched = true;

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        if (fields == null) continue;

                        // (Konrad) We first verify that our CSV file has the same headers as anticipated.
                        if (firstRow)
                        {
                            for (var i = 0; i < ColumnNames.Length; ++i)
                            {
                                if (fields[i] == ColumnNames[i]) continue;
                                formatMatched = false;
                                break;
                            }
                            firstRow = false;
                        }
                        else if (formatMatched)
                        {
                            var restart = int.Parse(fields[6]);
                            var addinInfo = new AddinInfo
                            {
                                IconName = fields[0],
                                ToolName = fields[1],
                                Index = int.Parse(fields[3]),
                                Tooltip = fields[4],
                                Url = fields[5],
                                DropdownOptionsFlag = restart,
                                LoadTypes = restart == 1
                                    ? new[]{LoadType.Never, LoadType.Always } 
                                    : new[]{LoadType.Never, LoadType.Always, LoadType.ThisSessionOnly} 
                            };

                            var names = fields[2];
                            string[] splitter = { ", " , ","};
                            var splitNames = names.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                            addinInfo.AddInNames = splitNames;
                            addinInfo.GetDetailInfo(sourceDirectory, installDirectory);

                            var addinExist = true; //to make sure all addin files exist
                            foreach (var addinPath in addinInfo.AddInPaths)
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
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }
            }
            return addinCollection;
        }
    }
}
