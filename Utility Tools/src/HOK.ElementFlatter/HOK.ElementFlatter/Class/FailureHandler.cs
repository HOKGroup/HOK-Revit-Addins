using Autodesk.Revit.DB;
using HOK.ElementFlatter.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementFlatter.Class
{
    public class FailureHandler : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();

            bool needRollBack = false;
            string transactionName = failuresAccessor.GetTransactionName();
            foreach (FailureMessageAccessor fma in failureMessages)
            {
                LogMessageInfo messageInfo = new LogMessageInfo();
                try { messageInfo.Message = fma.GetDescriptionText(); }
                catch { messageInfo.Message = "Unknown Error"; }

                FailureSeverity severity = fma.GetSeverity();
                try
                {
                 
                    messageInfo.RelatedElementIds = fma.GetFailingElementIds().ToList();

                    if (severity == FailureSeverity.Warning)
                    {
                        messageInfo.MessageType = LogMessageType.WARNING;
                        failuresAccessor.DeleteWarning(fma);
                    }
                    else if (severity == FailureSeverity.Error) 
                    {
                        messageInfo.MessageType = LogMessageType.ERROR;
                        needRollBack = true;
                    }

                    LogManager.AppendLog(messageInfo);
                }
                catch { }
            }

            if (needRollBack) { return FailureProcessingResult.ProceedWithRollBack; }
            else { return FailureProcessingResult.Continue; }
        }
    }
}
