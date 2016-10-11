using Autodesk.Revit.DB.Events;
using HOK.MissionControl.Tools.CADoor;
using HOK.MissionControl.Tools.DTMTool;
using HOK.MissionControl.Tools.RevisionTracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.MissionControl.Utils
{
    public static class FailureProcessor
    {
        public static bool IsFailureFound = false;
        public static bool IsSynchronizing = false;
        public static bool IsFailureProcessing = false;

        public static void CheckFailure(object sender, FailuresProcessingEventArgs args)
        {
            if (IsFailureProcessing) { return; }
            if (IsSynchronizing) { return; }
            if (IsFailureFound)
            {
                if (DoorFailure.IsDoorFailed)
                {
                    DoorFailure.ProcessFailure(sender, args);
                }
                else if (DTMFailure.IsElementModified)
                {
                    DTMFailure.ProcessFailure(sender, args);
                }
                else if (RevisionFailure.IsRevisionModified)
                {
                    RevisionFailure.ProcessFailure(sender, args);
                }
                IsFailureFound = false;
            }
        }
    }
}
