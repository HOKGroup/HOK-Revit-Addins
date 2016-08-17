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

        public static bool IsRevisionModified { get { return isRevisionModified; } set { isRevisionModified = value; } }
        public static ElementId FailingRevisionId { get { return failingRevisionId; } set { failingRevisionId = value; } }

        public static void ProcessFailure(object sender, FailuresProcessingEventArgs args)
        {
            try
            {
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                LogUtil.AppendLog("RevisionFailure-ProcessFailure:" + ex.Message);
            }
        }
    }
}
