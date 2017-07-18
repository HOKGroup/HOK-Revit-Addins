using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.IO;
using System.Windows.Forms;
using HOK.RoomsToMass.ToMass;

namespace HOK.RoomsToMass
{
    public enum MassTool { MassCreate = 0, UpdateData, MassCommands }

    public static class LogFileManager
    {
        public static Document m_doc;
        public static string documentPath = "";
        public static MassTool massTool;
        public static string logFileName = "";
        public static string logDirectory = "";
        //public static string logFilePath = "";
        public static string projectFileName = "";
        public static string logFullName = "";
        public static int maxLog = 100;
        public static StringBuilder logBuilder = new StringBuilder();
        public static string splitter = "------------------------------------------------------------";
        private static List<StringBuilder> stringBuilderList = new List<StringBuilder>();

        public static string LogDirectory { get { return logDirectory; } set { logDirectory = value; } }
        public static MassTool SelectedMassTool { get { return massTool; } set { massTool = value; } }

        //public static bool CreateLogFile(Document doc, MassTool selectedTool)
        //{
        //    try
        //    {
        //        massTool = selectedTool;
        //        bool directoryFound = FindLogDirectory(doc);
        //        if (directoryFound)
        //        {
        //            logFullName = Path.Combine(logDirectory, logFileName);
        //            if (File.Exists(logFullName))
        //            {
        //                ReadLogFile(logFullName);
        //            }
        //            else
        //            {
        //                try { FileStream fs = File.Create(logFullName); fs.Close(); }
        //                catch { return false; }
        //            }
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string message = ex.Message;
        //        return false;
        //    }
        //}

        //private static bool FindLogDirectory(Document doc)
        //{
        //    bool found = false;
        //    try
        //    {
        //        m_doc = doc;
        //        if (m_doc.IsWorkshared)
        //        {
        //            ModelPath modelPath = m_doc.GetWorksharingCentralModelPath();
        //            if (null != modelPath)
        //            {
        //                documentPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
        //                if (string.IsNullOrEmpty(documentPath))
        //                {
        //                    documentPath = m_doc.PathName;
        //                }
        //            }
        //            else
        //            {
        //                documentPath = m_doc.PathName;
        //            }
        //        }
        //        else
        //        {
        //            documentPath = m_doc.PathName;
        //        }

        //        if (!string.IsNullOrEmpty(documentPath))
        //        {
        //            string directoryName = Path.GetDirectoryName(documentPath);
        //            logDirectory = Path.Combine(directoryName, "Logs");
        //            string surfix = FindSurfix(massTool);
        //            if (!string.IsNullOrEmpty(surfix))
        //            {
        //                logFileName = Path.GetFileNameWithoutExtension(documentPath) + surfix;
        //                if (!Directory.Exists(logDirectory))
        //                {
        //                    DirectoryInfo directoryInfo = Directory.CreateDirectory(logDirectory);
        //                    if (null != directoryInfo)
        //                    {
        //                        found = true;
        //                    }
        //                }
        //                else
        //                {
        //                    found = true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Failed to find a log directory for this document.\n" + ex.Message, "Find Log Directory", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //    return found;
        //}

        //private static string FindSurfix(MassTool toolType)
        //{
        //    string surfix = "";
        //    switch (toolType)
        //    {
        //        case MassTool.MassCreate:
        //            surfix = "_MassCreate.log";
        //            break;
        //        case MassTool.UpdateData:
        //            surfix = "_UpdateData.log";
        //            break;
        //        case MassTool.MassCommands:
        //            surfix = "_MassCommands.log";
        //            break;
        //    }
        //    return surfix;
        //}
        //public static void ReadLogFile(string logPath)
        //{
        //    try
        //    {
        //        StringBuilder strBuilder = new StringBuilder();
        //        using (StreamReader sr = new StreamReader(logPath))
        //        {
        //            string line;
        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                if (line.Contains(splitter))
        //                {
        //                    stringBuilderList.Add(strBuilder);
        //                    strBuilder = new StringBuilder();
        //                }
        //                else
        //                {
        //                    strBuilder.AppendLine(line);
        //                }
        //            }
        //            sr.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Failed to read the logFile.\n" + ex.Message, "LogFileManager:ReadLogFile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}

        //public static void WriteLogFile()
        //{
        //    if (logBuilder.Length > 0)
        //    {
        //        if (File.Exists(logFullName))
        //        {
        //            string tempFile = Path.GetTempFileName();
        //            using (StreamReader sr = new StreamReader(logFullName))
        //            {
        //                using (StreamWriter sw = new StreamWriter(tempFile))
        //                {
        //                    string line;

        //                    while ((line = sr.ReadLine()) != null)
        //                    {
        //                        sw.WriteLine("");
        //                    }
        //                    sw.Close();
        //                }
        //                sr.Close();
        //            }
        //            File.Delete(logFullName);
        //            File.Move(tempFile, logFullName);

        //            FileStream fs = File.Open(logFullName, FileMode.Create);
        //            using (StreamWriter sw = new StreamWriter(fs))
        //            {
        //                sw.WriteLine(DateTime.Now.ToString() + ":\t" + projectFileName);
        //                sw.Write(logBuilder.ToString());
        //                sw.WriteLine(splitter);

        //                for (int i = 0; i < stringBuilderList.Count; i++)
        //                {
        //                    if (i > maxLog) { break; }
        //                    else
        //                    {
        //                        sw.Write(stringBuilderList[i].ToString());
        //                        sw.WriteLine(splitter);
        //                    }

        //                }
        //                sw.Close();
        //            }
        //            fs.Close();
        //        }
        //    }
        //}

        //public static void AppendLog(string logString)
        //{
        //    logBuilder.AppendLine(logString);
        //}

        //public static void AppendLog(string methodName, string errorMessage)
        //{
        //    try
        //    {
        //        logBuilder.AppendLine(methodName + ": " + errorMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Failed to append log line.\n" + ex.Message, "LogFileManager:AppendLog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}

        //public static void AppendLog(string methodName, Document doc, List<FailureMessageInfo> failureMessages)
        //{
        //    try
        //    {
        //        logBuilder.AppendLine(methodName + ": " );
        //        foreach (FailureMessageInfo messageInfo in failureMessages)
        //        {
        //            logBuilder.AppendLine(messageInfo.ErrorSeverity + ": " + messageInfo.ErrorMessage+": ");
        //            foreach (ElementId elementId in messageInfo.FailingElementIds)
        //            {
        //                Element element = doc.GetElement(elementId);
        //                if (null != element)
        //                {
        //                    if(null!=element.Name)
        //                    {
        //                        logBuilder.AppendLine(" [" + element.Id.IntegerValue + "] " + element.Name);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Failed to append log line.\n" + ex.Message, "LogFileManager:AppendLog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }
        //}

        //    public static void AppendLog(string methodName, Document doc, Element errorElement, List<FailureMessageInfo> failureMessages)
        //    {
        //        try
        //        {
        //            string elementName = "";
        //            if (errorElement.Name != null)
        //            {
        //                elementName = errorElement.Name;
        //            }

        //            logBuilder.AppendLine(methodName + ": ["+errorElement.Id.IntegerValue.ToString()+"] "+elementName);
        //            foreach (FailureMessageInfo messageInfo in failureMessages)
        //            {
        //                logBuilder.AppendLine(messageInfo.ErrorSeverity + ": " + messageInfo.ErrorMessage + ": ");
        //                foreach (ElementId elementId in messageInfo.FailingElementIds)
        //                {
        //                    Element element = doc.GetElement(elementId);
        //                    if (null != element)
        //                    {
        //                        if (null != element.Name)
        //                        {
        //                            logBuilder.AppendLine(" [" + element.Id.IntegerValue + "] " + element.Name);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Failed to append log line.\n" + ex.Message, "LogFileManager:AppendLog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        }
        //    }

        //    public static void ClearLogFile()
        //    {
        //        logBuilder.Clear();
        //    }
    }

    public class FailureHandler : IFailuresPreprocessor
    {
        private List<FailureMessageInfo> failureMessageInfoList = new List<FailureMessageInfo>();

        public List<FailureMessageInfo> FailureMessageInfoList { get { return failureMessageInfoList; } set { failureMessageInfoList = value; } }


        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var failureMessages = failuresAccessor.GetFailureMessages();
            if (failureMessages.Count == 0) { return FailureProcessingResult.Continue; }

            var needRollBack = false;
            //string transactionName = failuresAccessor.GetTransactionName();
            foreach (var fma in failureMessages)
            {
                FailureMessageInfo messageInfo = new FailureMessageInfo();
                try { messageInfo.ErrorMessage = fma.GetDescriptionText(); }
                catch { messageInfo.ErrorMessage = "Unknown Error"; }

                var severity = fma.GetSeverity();
                try
                {
                    if (severity == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(fma);
                    }
                    else
                    {
                        messageInfo.ErrorSeverity = severity.ToString();
                        messageInfo.FailingElementIds = fma.GetFailingElementIds().ToList();
                        failureMessageInfoList.Add(messageInfo);
                        needRollBack = true;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return needRollBack ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
        }
    }

    public class FailureMessageInfo
    {
        public string ErrorMessage { get; set; }
        public string ErrorSeverity { get; set; }
        public List<ElementId> FailingElementIds { get; set; }
    }
}
