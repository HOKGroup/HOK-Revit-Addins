using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public class FamilyTaskUpdatedMessage
    {
        public string FamilyName { get; set; }
        public FamilyStat FamilyStat { get; set; }
        public string OldTaskId { get; set; }
    }

    public class FamilyTaskDeletedMessage
    {
        public List<string> DeletedIds { get; set; }
    }

    public class FamilyTaskAddedMessage
    {
        public string FamilyName { get; set; }
        public FamilyStat FamilyStat { get; set; }
    }

    public class FamilyTaskAssistantClosedMessage
    {
        public bool IsClosed { get; set; }
    }

    public class SheetsTaskUpdateMessage
    {
        public SheetTask Task { get; set; }
    }
}
