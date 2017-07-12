using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.Core;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Tools.SheetTracker
{
    public static class RevisionFailure
    {
        public static bool IsRevisionModified { get; set; } = false;
        public static ElementId FailingRevisionId { get; set; } = ElementId.InvalidElementId;
        public static Document CurrentDoc { get; set; } = null;

        public static void ProcessFailure(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                FailureProcessor.IsFailureProcessing = true;
                FailureProcessor.IsFailureProcessing = false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }
    }
}
