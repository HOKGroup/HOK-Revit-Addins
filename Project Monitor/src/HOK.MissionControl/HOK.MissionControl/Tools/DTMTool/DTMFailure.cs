using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Tools.DTMTool
{
    public static class DTMFailure
    {
        private static bool isElementModified = false;
        private static ReportingElementInfo elementModified = null;
        private static Document currentDoc = null;

        public static bool IsElementModified { get { return isElementModified; } set { isElementModified = value; } }
        public static ReportingElementInfo ElementModified { get { return elementModified; } set { elementModified = value; } }
        public static Document CurrentDoc { get { return currentDoc; } set { currentDoc = value; } }

        public static void ProcessFailure(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                if (isElementModified && null!=currentDoc)
                {
                    FailureProcessor.IsFailureProcessing = true;
                    var fa = args.GetFailuresAccessor();

                    var dtmWindow = new DTMWindow(currentDoc, elementModified);
                    if ((bool)dtmWindow.ShowDialog())
                    {
                        args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        var option = fa.GetFailureHandlingOptions();
                        option.SetClearAfterRollback(true);
                        fa.SetFailureHandlingOptions(option);
                    }
                    isElementModified = false;
                    FailureProcessor.IsFailureProcessing = false;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                LogUtil.AppendLog("DTMFailure-ProcessFailure:" + ex.Message);
            }
        }

    }
}
