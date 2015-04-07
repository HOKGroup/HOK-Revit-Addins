using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.IO;

namespace HOK.BatchExporterAddIn
{
    public static class LogFileManager
    {
        public static string logDirectory = @"V:\HOK-Tools\BatchUpgrader\Logs";
        public static string logFullName = "";
        public static int maxLog = 100;
        public static StringBuilder logBuilder = new StringBuilder();
        public static string splitter = "------------------------------------------------------------";
        private static List<StringBuilder> stringBuilderList = new List<StringBuilder>();
        private static ProjectSettings selectedProject;

        public static ProjectSettings SelectedProject { get { return selectedProject; } set { selectedProject = value; } }
        public static string LogDirectory { get { return logDirectory; } set { logDirectory = value; } }

        public static bool CreateLogFile()
        {
            try
            {
                if (Directory.Exists(selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.LogLocation))
                {
                    logDirectory = selectedProject.UpgradeOptions.UpgradeVersionSaveAsOptions.LogLocation;
                }

                string fileName = "BatchUpgrader_" + selectedProject.Office + "_" + selectedProject.ProjectName + ".log";
                logFullName = logDirectory + "\\" + fileName;

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
            catch (Exception ex)
            {
                string message = ex.Message;
                return false;
            }
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

        public static void AppendLog(string methodName, string errorMessage)
        {
            try
            {
                logBuilder.AppendLine(methodName + ": " + errorMessage);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void AppendLog(string message)
        {
            logBuilder.AppendLine(message);
        }

        public static void AppendLog(FailureMessageInfo messageInfo)
        {
            try
            {
                logBuilder.AppendLine("*****************************************************");
                if (!string.IsNullOrEmpty(messageInfo.ErrorSeverity) && !string.IsNullOrEmpty(messageInfo.ErrorMessage))
                {
                    logBuilder.AppendLine(messageInfo.ErrorSeverity + ": " + messageInfo.ErrorMessage);
                    if (messageInfo.FailingElementIds.Count > 0)
                    {
                        string Id = "Element Id: ";
                        foreach (ElementId eId in messageInfo.FailingElementIds)
                        {
                            Id += " [" + eId.IntegerValue + "] ";
                        }
                        logBuilder.AppendLine(Id);
                    }
                }
                else
                {
                    logBuilder.AppendLine(messageInfo.ErrorMessage);
                }
                logBuilder.AppendLine("*****************************************************");
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void ClearLogFile()
        {
            logBuilder.Clear();
        }
    }
}
