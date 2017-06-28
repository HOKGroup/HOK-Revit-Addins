using System;
using System.IO;
using System.Text;

namespace HOK.MissionControl.Core.Utils
{
    public static class LogUtil
    {
        public static bool Initialized { get; set; }
        public static string LogDirectory { get; set; } = "";
        public static string LogFilePath { get; set; } = "";
        public static StringBuilder LogBuilder { get; set; } = new StringBuilder();

        public static void InitializeLog()
        {
            try
            {
                LogBuilder.Clear();
                var tempFolder = Path.GetTempPath();
                var hokLogFolder = Path.Combine(tempFolder, "HOKReivtAddInLogs");
                LogDirectory = Path.Combine(hokLogFolder, "MissionControl");

                if (!Directory.Exists(hokLogFolder))
                {
                    Directory.CreateDirectory(hokLogFolder);
                }

                if (Directory.Exists(hokLogFolder) && !Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                if (!Directory.Exists(LogDirectory)) return;

                var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                LogFilePath = "log-" + dateStr + ".log";
                LogFilePath = Path.Combine(LogDirectory, LogFilePath);

                if (!File.Exists(LogFilePath))
                {
                    File.Create(LogFilePath);
                }

                LogBuilder.AppendLine("====================================================================================");
                Initialized = true;
            }
            catch
            {
                // ignored
            }
        }

        public static void AppendLog(string str)
        {
            try
            {
                LogBuilder.AppendLine(DateTime.Now + "\t" + str);
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteLog()
        {
            try
            {
                if (!Initialized) return;

                LogBuilder.AppendLine("");
                var oldText = File.ReadAllText(LogFilePath);
                var logText = LogBuilder + oldText;

                File.WriteAllText(LogFilePath, logText);
            }
            catch
            {
                // ignored
            }
        }
    }
}
