using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Core.Utils
{
    public static class LogUtil
    {
        private static bool initialized = false;
        private static string logDirectory = "";
        private static string logFilePath = "";
        private static StringBuilder logBuilder = new StringBuilder();

        public static bool Initialized { get { return initialized; } set { initialized = value; } }
        public static string LogDirectory { get { return logDirectory; } set { logDirectory = value; } }
        public static string LogFilePath { get { return logFilePath; } set { logFilePath = value; } }
        public static StringBuilder LogBuilder { get { return logBuilder; } set { logBuilder = value; } }

        public static void InitializeLog()
        {
            try
            {
                logBuilder.Clear();
                //AppData/Local/Temp/HOKReivtAddInLogs/MissionControl
                string tempFolder = Path.GetTempPath();
                string hokLogFolder = Path.Combine(tempFolder, "HOKReivtAddInLogs");
                logDirectory = Path.Combine(hokLogFolder, "MissionControl");

                if (!Directory.Exists(hokLogFolder))
                {
                    Directory.CreateDirectory(hokLogFolder);
                }

                if (Directory.Exists(hokLogFolder) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                if (Directory.Exists(logDirectory))
                {
                    string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                    logFilePath = "log-" + dateStr + ".log";
                    logFilePath = Path.Combine(logDirectory, logFilePath);

                    if (!File.Exists(logFilePath))
                    {
                        File.Create(logFilePath);
                    }

                    logBuilder.AppendLine("====================================================================================");
                    initialized = true;
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void AppendLog(string str)
        {
            try
            {
                logBuilder.AppendLine(DateTime.Now.ToString() + "\t" + str);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void WriteLog()
        {
            try
            {
                if (initialized)
                {
                    logBuilder.AppendLine("");
                    string oldText = File.ReadAllText(logFilePath);
                    string logText = logBuilder.ToString() + oldText;

                    File.WriteAllText(logFilePath, logText);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
