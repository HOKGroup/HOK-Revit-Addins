using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace HOK.LevelManager
{
    public static class LogFileManager
    {
        public static Document m_doc;
        public static string documentPath = "";
        public static string logFileName = "";
        public static string logDirectory = "";
        public static string logFullName = "";
        public static int maxLog = 200;
        public static StringBuilder logBuilder = new StringBuilder();
        public static string splitter = "------------------------------------------------------------";
        private static List<StringBuilder> stringBuilderList = new List<StringBuilder>();

        public static string LogDirectory { get { return logDirectory; } set { logDirectory = value; } }


        public static bool CreateLogFile(Document doc)
        {
            try
            {
                bool directoryFound = FindLogDirectory(doc);
                if (directoryFound)
                {
                    logFullName = Path.Combine(logDirectory, logFileName);
                    if (File.Exists(logFullName))
                    {
                        ReadLogFile(logFullName);
                    }
                    else
                    {
                        try { FileStream fs = File.Create(logFullName); fs.Close(); }
                        catch { return false; }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return false;
            }
        }

        private static bool FindLogDirectory(Document doc)
        {
            bool found = false;
            try
            {
                m_doc = doc;
                if (m_doc.IsWorkshared)
                {
                    ModelPath modelPath = m_doc.GetWorksharingCentralModelPath();
                    if (null != modelPath)
                    {
                        documentPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    }
                    else
                    {
                        documentPath = m_doc.PathName;
                    }
                }
                else
                {
                    documentPath = m_doc.PathName;
                }

                if (!string.IsNullOrEmpty(documentPath))
                {
                    string directoryName = Path.GetDirectoryName(documentPath);
                    logDirectory = Path.Combine(directoryName, "Logs");
                    logFileName = Path.GetFileNameWithoutExtension(documentPath) + "_LevelManager.log";
                    if (!Directory.Exists(logDirectory))
                    {
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(logDirectory);
                        if (null != directoryInfo)
                        {
                            found = true;
                        }
                    }
                    else
                    {
                        found = true;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find a log directory for this document.\n" + ex.Message, "Find Log Directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return found;
        }

        public static void ReadLogFile(string logPath)
        {
            try
            {
                stringBuilderList = new List<StringBuilder>();
                StringBuilder strBuilder = new StringBuilder();
                using (StreamReader sr = new StreamReader(logPath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(splitter))
                        {
                            stringBuilderList.Add(strBuilder);
                            strBuilder = new StringBuilder();
                        }
                        else
                        {
                            strBuilder.AppendLine(line);
                        }
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void WriteLogFile()
        {
            if (logBuilder.Length > 0)
            {
                if (File.Exists(logFullName))
                {
                    string tempFile = Path.GetTempFileName();
                    using (StreamReader sr = new StreamReader(logFullName))
                    {
                        using (StreamWriter sw = new StreamWriter(tempFile))
                        {
                            string line;

                            while ((line = sr.ReadLine()) != null)
                            {
                                sw.WriteLine("");
                            }
                            sw.Close();
                        }
                        sr.Close();
                    }
                    File.Delete(logFullName);
                    File.Move(tempFile, logFullName);

                    FileStream fs = File.Open(logFullName, FileMode.Create);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(logBuilder.ToString());
                        sw.WriteLine(splitter);

                        for (int i = 0; i < stringBuilderList.Count; i++)
                        {
                            if (i > maxLog) { break; }
                            else
                            {
                                sw.Write(stringBuilderList[i].ToString());
                                sw.WriteLine(splitter);
                            }

                        }
                        sw.Close();
                    }
                    fs.Close();
                }
            }
        }

        public static void AppendLog(string message)
        {
            logBuilder.AppendLine(message);
        }

        public static void ClearLogFile()
        {
            logBuilder.Clear();
        }
    }
}
