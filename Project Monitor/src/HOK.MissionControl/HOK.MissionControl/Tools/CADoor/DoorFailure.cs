using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Tools.CADoor
{
    public static class DoorFailure
    {
        private static bool isDoorFailed = false;
        private static ElementId failingDoorId = ElementId.InvalidElementId;
        private static Document currentDoc = null;

        public static bool IsDoorFailed { get { return isDoorFailed; } set { isDoorFailed = value; } }
        public static ElementId FailingDoorId { get { return failingDoorId; } set { failingDoorId = value; } }
        public static Document CurrentDoc { get {  return currentDoc; } set { currentDoc = value; } }

        public static void ProcessFailure(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                if (isDoorFailed)
                {
                    FailureProcessor.IsFailureProcessing = true;
                    FailuresAccessor fa = args.GetFailuresAccessor();
                    IList<FailureMessageAccessor> failList = new List<FailureMessageAccessor>();
                    failList = fa.GetFailureMessages();
                    bool foundFailingElement = false;
                    foreach (FailureMessageAccessor failure in failList)
                    {
                        foreach (ElementId id in failure.GetFailingElementIds())
                        {
                            if (failingDoorId.IntegerValue == id.IntegerValue) { foundFailingElement = true; }
                        }
                    }
                    if (foundFailingElement)
                    {
                        args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                        FailureHandlingOptions option = fa.GetFailureHandlingOptions();
                        option.SetClearAfterRollback(true);
                        fa.SetFailureHandlingOptions(option);
                    }

                    isDoorFailed = false;
                    FailureProcessor.IsFailureProcessing = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("DoorFailure-ProcessFailure:" + ex.Message);
            }
        }
    }
}
