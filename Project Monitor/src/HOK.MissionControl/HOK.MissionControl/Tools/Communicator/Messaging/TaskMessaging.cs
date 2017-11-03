using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    public class FamilyTaskUpdatedMessage
    {
        public string FamilyName { get; set; }
        public FamilyData FamilyStat { get; set; }
        public string OldTaskId { get; set; }
    }

    public class FamilyTaskDeletedMessage
    {
        public List<string> DeletedIds { get; set; }
    }

    public class FamilyTaskAddedMessage
    {
        public string FamilyName { get; set; }
        public FamilyData FamilyStat { get; set; }
    }

    public class FamilyTaskAssistantClosedMessage
    {
        public bool IsClosed { get; set; }
    }

    public class SheetsTaskUpdateMessage
    {
        public SheetItem Task { get; set; }
    }

    public class SheetsTaskApprovedMessage
    {
        public string Identifier { get; set; }
        public SheetItem Sheet { get; set; }
    }

    public class SheetsTaskDeletedMessage
    {
        public string Identifier { get; set; }
    }

    public class TaskAssistantClosingMessage
    {
    }

    public class SheetTaskCompletedMessage
    {
        public bool Completed { get; set; }
        public string Message { get; set; }
    }
}
