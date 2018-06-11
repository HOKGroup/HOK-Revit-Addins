using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass
{
    public enum MassTool
    {
        MassCreate = 0,
        UpdateData,
        MassCommands
    }

    public static class LogFileManager
    {
        public static Document m_doc;
        public static string documentPath = "";
        public static MassTool massTool;
        public static string logFileName = "";
        public static string logDirectory = "";
        public static string projectFileName = "";
        public static string logFullName = "";
        public static int maxLog = 100;
        public static StringBuilder logBuilder = new StringBuilder();
        public static string splitter = "------------------------------------------------------------";
        private static List<StringBuilder> stringBuilderList = new List<StringBuilder>();

        public static string LogDirectory { get { return logDirectory; } set { logDirectory = value; } }
        public static MassTool SelectedMassTool { get { return massTool; } set { massTool = value; } }
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
