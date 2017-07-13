using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HOK.Core.Utilities
{
    /// <summary>
    /// Indicates message type to be posted to log.
    /// </summary>
    public enum LogMessageType
    {
        NONE, INFO, WARNING, EXCEPTION, ERROR
    }

    public static class Log
    {
        public static bool Initialized { get; set; }
        public static string LogDirectory { get; set; } = "";
        public static string LogFilePath { get; set; } = "";
        public static StringBuilder LogBuilder { get; set; } = new StringBuilder();

        public static void Initialize(string folderName, string modelPath)
        {
            LogBuilder.Clear();
            var tempFolder = Path.GetTempPath();
            var hokLogFolder = Path.Combine(tempFolder, "HOK.RevitAddinLogs");
            LogDirectory = Path.Combine(hokLogFolder, folderName);

            if (!Directory.Exists(hokLogFolder))
            {
                Directory.CreateDirectory(hokLogFolder);
            }

            if (Directory.Exists(hokLogFolder) && !Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
            LogFilePath = "log-" + dateStr + ".log";
            LogFilePath = Path.Combine(LogDirectory, LogFilePath);

            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath);
            }

            LogBuilder.AppendLine("====================================================================================");
            LogBuilder.AppendLine("Model Path: " + modelPath);
            LogBuilder.AppendLine("====================================================================================");

            Initialized = true;
        }

        public static void AppendLog(LogMessageType logType, string msg)
        {
            try
            {
                var method = new StackFrame(1).GetMethod();
                if (method.ReflectedType != null)
                {
                    LogBuilder.AppendLine($"{logType} \t {DateTime.Now} \t {method.ReflectedType.FullName}.{method.Name}: {msg}");
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Writes out the combined log string into file.
        /// </summary>
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
