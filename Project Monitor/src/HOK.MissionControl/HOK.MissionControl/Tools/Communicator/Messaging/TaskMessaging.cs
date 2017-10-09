using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public class TaskUpdatedMessage
    {
        public string FamilyName { get; set; }
        public FamilyStat FamilyStat { get; set; }
        public string OldTaskId { get; set; }
    }

    public class TaskDeletedMessage
    {
        public List<string> DeletedIds { get; set; }
    }

    public class TaskAddedMessage
    {
        public string FamilyName { get; set; }
        public FamilyStat FamilyStat { get; set; }
    }

    public class TaskAssistantClosedMessage
    {
        public bool IsClosed { get; set; }
    }
}
