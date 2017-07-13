using System;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.ElementFlatter.Class
{
    public class FailureHandler : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            var failureMessages = failuresAccessor.GetFailureMessages();
            var needRollBack = false;

            foreach (var fma in failureMessages)
            {
                var severity = fma.GetSeverity();
                try
                {
                 
                    var items = fma.GetFailingElementIds().ToList();

                    if (severity == FailureSeverity.Warning)
                    {
                        Log.AppendLog(LogMessageType.WARNING, "Deleted Warning");
                        failuresAccessor.DeleteWarning(fma);
                    }
                    else if (severity == FailureSeverity.Error) 
                    {
                        Log.AppendLog(LogMessageType.ERROR, string.Join(",", items));
                        needRollBack = true;
                    }
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }
            }

            return needRollBack ? FailureProcessingResult.ProceedWithRollBack : FailureProcessingResult.Continue;
        }
    }
}
