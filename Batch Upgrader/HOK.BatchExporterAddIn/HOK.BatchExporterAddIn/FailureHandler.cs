using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.BatchExporterAddIn
{
    public class FailureHandler : IFailuresPreprocessor
    {
        private List<FailureMessageInfo> failureMessageInfoList = new List<FailureMessageInfo>();

        public List<FailureMessageInfo> FailureMessageInfoList { get { return failureMessageInfoList; } set { failureMessageInfoList = value; } }

        public FailureHandler()
        {
        }

        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {

            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            //if (failureMessages.Count == 0) { return FailureProcessingResult.Continue; }

            bool needRollBack = false;
            string transactionName = failuresAccessor.GetTransactionName();
            foreach (FailureMessageAccessor fma in failureMessages)
            {
                FailureMessageInfo messageInfo = new FailureMessageInfo();
                try 
                { 
                    messageInfo.ErrorMessage = fma.GetDescriptionText();
                    messageInfo.ErrorSeverity = fma.GetSeverity().ToString();
                    messageInfo.FailingElementIds = fma.GetFailingElementIds().ToList();
                    
                }
                catch { messageInfo.ErrorMessage = "Unknown Error"; }

                FailureSeverity severity = fma.GetSeverity();
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
                catch { }
            }

            if (needRollBack) { return FailureProcessingResult.ProceedWithRollBack; }
            else { return FailureProcessingResult.Continue; }
        }
    }

    public class FailureMessageInfo
    {
        public string ErrorMessage { get; set; }
        public string ErrorSeverity { get; set; }
        public List<ElementId> FailingElementIds { get; set; }
    }
}
