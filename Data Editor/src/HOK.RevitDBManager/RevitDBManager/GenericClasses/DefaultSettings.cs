using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Windows.Forms;

namespace RevitDBManager.Classes
{
    public class DefaultSettings
    {
        private UIApplication m_app;
        private Document doc;
        private bool dbExist=false;
        private string iniPath = "";
        private string defaultDBPath = "";

        private Dictionary<string/*filePath*/, DBFileInfo> dbInfoDictionary = new Dictionary<string, DBFileInfo>();
        private string[] splitter = new string[] { "##" };

        public string DefaultDBFile { get { return defaultDBPath; } set { defaultDBPath = value; } }
        public bool DatabaseExist { get { return dbExist; } }
        public Dictionary<string, DBFileInfo> DBInfoDictionary { get { return dbInfoDictionary; } set { dbInfoDictionary = value; } }

        public DefaultSettings(UIApplication uiapp)
        {
            m_app = uiapp;
            doc = m_app.ActiveUIDocument.Document;
            FindINI();
            ReadINI();
            SearchDefaultFile();
        }


        private void FindINI()
        {
            string masterFilePath = "";

            if (doc.IsWorkshared && null != doc.GetWorksharingCentralModelPath())
            {
                masterFilePath = doc.GetWorksharingCentralModelPath().ToString();
            }
            else
            {
                masterFilePath = doc.PathName;
            }

            iniPath = Path.GetDirectoryName(masterFilePath) + @"\" + Path.GetFileNameWithoutExtension(masterFilePath) + ".ini";

            if (!File.Exists(iniPath)) 
            {
                FileStream file = File.Create(iniPath);
                file.Close();
            }
        }

        private void ReadINI()
        {
            try
            {
                if (File.Exists(iniPath))
                {
                    using (StreamReader sr = new StreamReader(iniPath))
                    {
                        bool firstLine = true;
                        string line;
                        while((line=sr.ReadLine())!=null)
                        {
                            DBFileInfo dbFileInfo = new DBFileInfo();
                            dbFileInfo.isDefault = firstLine;

                            string[] strFileInfo = line.Split(splitter, StringSplitOptions.None);
                            if(strFileInfo.Length==4)
                            {
                                dbFileInfo.DateModified = strFileInfo[0];
                                dbFileInfo.ModifiedBy = strFileInfo[1];
                                dbFileInfo.FilePath = strFileInfo[2];
                                dbFileInfo.Comments = strFileInfo[3];
                                if (File.Exists(dbFileInfo.FilePath)) { dbFileInfo.FileName = Path.GetFileName(dbFileInfo.FilePath); }

                                if (!dbInfoDictionary.ContainsKey(dbFileInfo.FilePath)) { dbInfoDictionary.Add(dbFileInfo.FilePath, dbFileInfo); }
                                firstLine = false;
                            }
                        }
                        sr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read ini file: \n" + ex.Message);
            }
        }

        private void SearchDefaultFile()
        {
            if (dbInfoDictionary.Count > 0)
            {
                foreach (string filePath in dbInfoDictionary.Keys)
                {
                    DBFileInfo dbFileInfo = dbInfoDictionary[filePath];
                    if (dbFileInfo.isDefault)
                    {
                        if (File.Exists(dbFileInfo.FilePath))
                        {
                            dbExist = true;
                            DefaultDBFile = dbFileInfo.FilePath;
                        }
                        break;
                    }
                }
            }
        }

        //from SetUp
        public void SetDefualtFile(DBFileInfo fileInfo)
        {
            foreach (string filePath in dbInfoDictionary.Keys)
            {
                dbInfoDictionary[filePath].isDefault = false;
            }

            if(dbInfoDictionary.ContainsKey(fileInfo.FilePath))
            {
                dbInfoDictionary.Remove(fileInfo.FilePath);
            }
            dbInfoDictionary.Add(fileInfo.FilePath, fileInfo);
        }

        public void WriteINI()
        {
            try
            {
                if (File.Exists(iniPath))
                {   
                    FileStream fs = File.Open(iniPath, FileMode.Create);

                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string sp = "##";

                        foreach (string filePath in dbInfoDictionary.Keys)
                        {
                            DBFileInfo dbfile = dbInfoDictionary[filePath];
                            if (dbfile.isDefault)
                            {
                                sw.WriteLine(dbfile.DateModified + sp + dbfile.ModifiedBy + sp + dbfile.FilePath + sp + dbfile.Comments);
                                break;
                            }
                        }

                        foreach (string filePath in dbInfoDictionary.Keys)
                        {
                            DBFileInfo dbfile = dbInfoDictionary[filePath];
                            if (dbfile.isDefault) { continue; }
                            if (File.Exists(filePath))
                            {
                                sw.WriteLine(dbfile.DateModified + sp + dbfile.ModifiedBy + sp + dbfile.FilePath + sp + dbfile.Comments);
                            }
                            else
                            {
                                sw.WriteLine("Not Exist: " + filePath);
                            }
                        }
                        sw.Close();
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write ini file: \n" + ex.Message);
            }
        }
    }

    public class DBFileInfo
    {
        public string FileName { get; set; }
        public string DateModified { get; set; }
        public string ModifiedBy { get; set; }
        public string FilePath { get; set; }
        public string Comments { get; set; }
        public bool isDefault { get; set; }
    }
}
