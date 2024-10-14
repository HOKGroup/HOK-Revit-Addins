using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Windows.Forms;
using HOK.Core.Utilities;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.RoomsToMass.ParameterAssigner
{
    class SplitINIDataManager
    {
        private UIApplication m_app;
        private Document m_doc;
        private string iniPath = "";
        private Dictionary<long/*elementId*/, SplitINIData> splitDictionary = new Dictionary<long, SplitINIData>();
        private string[] splitter = new string[] { "##" };

        public Dictionary<long, SplitINIData> SplitDictionary { get { return splitDictionary; } set { splitDictionary = value; } }

        public SplitINIDataManager(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;

            if (FindINI())
            {
                ReadINI();
            }
        }

        private bool FindINI()
        {
            try
            {
                string masterFilePath = "";
                ModelPath modelPath=m_doc.GetWorksharingCentralModelPath();
                if (m_doc.IsWorkshared && null != modelPath)
                {
                    if (modelPath.ServerPath)
                    {
                        masterFilePath = modelPath.CentralServerPath;
                    }
                    else
                    {
                        masterFilePath = m_doc.PathName;
                    }
                }
                else
                {
                    masterFilePath = m_doc.PathName;
                }
                iniPath = masterFilePath.Replace(".rvt", "_split.ini");
                if (!File.Exists(iniPath))
                {
                    try { FileStream fs = File.Create(iniPath); fs.Close(); }
                    catch { return false; }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        private void ReadINI()
        {
            try
            {
                //CategoryName##OriginalElementId##PrimaryElementIds##SecondaryElementIds
                if (File.Exists(iniPath))
                {
                    using (StreamReader sr = new StreamReader(iniPath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] strSplitInfo = line.Split(splitter, StringSplitOptions.None);
                            if (strSplitInfo.Length == 4)
                            {
                                SplitINIData splitData = new SplitINIData();
                                splitData.CategoryName = strSplitInfo[0];
                                splitData.ElementId = int.Parse(strSplitInfo[1]);

                                string[] strPrimary = strSplitInfo[2].Split(',');
                                List<long> primaryElements = new List<long>();
                                foreach (string primary in strPrimary)
                                {
                                    long elementId = long.Parse(primary);
                                    primaryElements.Add(elementId);
                                }
                                splitData.PrimaryElementIds = primaryElements;

                                string[] strSecondary = strSplitInfo[3].Split(',');
                                List<long> secondaryElements = new List<long>();
                                foreach (string secondary in strSecondary)
                                {
                                    long elementId = long.Parse(secondary);
                                    secondaryElements.Add(elementId);
                                }
                                splitData.SecondaryElementIds = secondaryElements;

                                if (!splitDictionary.ContainsKey(splitData.ElementId))
                                {
                                    splitDictionary.Add(splitData.ElementId, splitData);
                                }
                            }
                        }
                        sr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public void AppendINI(ElementProperties ep)
        {
            try
            {
                SplitINIData splitData = new SplitINIData();
                splitData.CategoryName = ep.CategoryName;
                splitData.ElementId = ep.ElementId;

                List<long> primaryElementIds = new List<long>();
                foreach (Element element in ep.PrimaryElements)
                {
                    primaryElementIds.Add(GetElementIdValue(element.Id));
                }
                splitData.PrimaryElementIds = primaryElementIds;

                List<long> secondaryElementIds = new List<long>();
                foreach (Element element in ep.SecondaryElements)
                {
                    secondaryElementIds.Add(GetElementIdValue(element.Id));
                }
                splitData.SecondaryElementIds = secondaryElementIds;

                if (!splitDictionary.ContainsKey(splitData.ElementId))
                {
                    splitDictionary.Add(splitData.ElementId, splitData);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public void WriteINI()
        {
            try
            {
                if (File.Exists(iniPath))
                {
                    //delete all
                    string tempFile = Path.GetTempFileName();
                    using (StreamReader sr = new StreamReader(iniPath))
                    {
                        using (StreamWriter sw = new StreamWriter(tempFile))
                        {
                            string line;

                            while ((line = sr.ReadLine()) != null)
                            {
                                sw.WriteLine("");
                            }
                        }
                    }
                    File.Delete(iniPath);
                    File.Move(tempFile, iniPath);

                    FileStream fs = File.Open(iniPath, FileMode.Create);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string sp = "##";
                        foreach (int elemnetId in splitDictionary.Keys)
                        {
                            SplitINIData splitData = splitDictionary[elemnetId];
                            string line = splitData.CategoryName + sp + splitData.ElementId + sp;

                            string primaryIds= "";
                            for (int i = 0; i < splitData.PrimaryElementIds.Count; i++)
                            {
                                if (i == 0)
                                {
                                    primaryIds += splitData.PrimaryElementIds[i];
                                }
                                else
                                {
                                    primaryIds += "," + splitData.PrimaryElementIds[i];
                                }
                            }
                            line += primaryIds + sp;

                            string secondaryIds = "";
                            for (int i = 0; i < splitData.SecondaryElementIds.Count; i++)
                            {
                                if (i == 0)
                                {
                                    secondaryIds += splitData.SecondaryElementIds[i];
                                }
                                else
                                {
                                    secondaryIds += "," + splitData.SecondaryElementIds[i];
                                }
                            }
                            line += secondaryIds;
                            sw.WriteLine(line);
                        }
                        sw.Close();
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

    }

    public class SplitINIData
    {
        public long ElementId { get; set; }
        public string CategoryName { get; set; }
        public List<long> PrimaryElementIds { get; set; }
        public List<long> SecondaryElementIds { get; set; }
    }
}
