using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Tools.RevisionTracker
{
    public static class RevisionFailure
    {
        private static bool isRevisionModified = false;
        private static ElementId failingRevisionId = ElementId.InvalidElementId;
        private static Document currentDoc = null;

        public static bool IsRevisionModified { get { return isRevisionModified; } set { isRevisionModified = value; } }
        public static ElementId FailingRevisionId { get { return failingRevisionId; } set { failingRevisionId = value; } }
        public static Document CurrentDoc { get { return currentDoc; } set { currentDoc = value; } }

        public static void ProcessFailure(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
                FailureProcessor.IsFailureProcessing = true;
                FailureProcessor.IsFailureProcessing = false;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                LogUtil.AppendLog("RevisionFailure-ProcessFailure:" + ex.Message);
            }
        }
    }
}
