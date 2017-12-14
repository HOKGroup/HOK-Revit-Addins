using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;

namespace HOK.MissionControl.Tools.Communicator.Messaging
{
    #region Family Tasks

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

    #endregion

    #region Sheet Tasks

    /// <summary>
    /// Message sent by the socket when sheet task is created/added.
    /// </summary>
    public class SheetsTaskAddedMessage
    {
        public SheetItem Sheet { get; set; }
        public SheetTask Task { get; set; }
    }

    /// <summary>
    /// Message sent by the socket when sheet task is updated.
    /// </summary>
    public class SheetsTaskUpdatedMessage
    {
        public SheetItem Sheet { get; set; }
        public SheetTask Task { get; set; }
    }

    /// <summary>
    /// Message sent by the socket when sheet task is deleted.
    /// </summary>
    public class SheetsTaskDeletedMessage
    {
        public string Identifier { get; set; }
        public List<string> Deleted { get; set; }
    }

    /// <summary>
    /// Message sent by the socket when new Sheet is created in Mission Control.
    /// </summary>
    public class SheetTaskSheetsCreatedMessage
    {
        public List<SheetItem> Sheets { get; set; }
    }

    /// <summary>
    /// Message sent by External Command handler when sheet task is completed.
    /// </summary>
    public class SheetTaskCompletedMessage
    {
        public bool Completed { get; set; }
        public string Message { get; set; }
    }

    #endregion

    /// <summary>
    /// Message sent by the Task Assistant when window is closing.
    /// It will trigger selection reset in Communicator window.
    /// </summary>
    public class TaskAssistantClosedMessage
    {
        public bool IsClosed { get; set; }
    }

    //public class SheetsTaskSheetAddedMessage
    //{
    //    public List<SheetItem> NewSheets { get; set; }
    //}

    //public class SheetsTaskApprovedMessage
    //{
    //    public string Identifier { get; set; }
    //    public SheetItem Sheet { get; set; }
    //}

    ///// <summary>
    ///// Message sent by the socket when sheet task is approved.
    ///// </summary>
    //public class SheetsTaskApprovedNewSheetMessage
    //{
    //    public string Identifier { get; set; }
    //    public SheetItem Sheet { get; set; }
    //}

    //public class TaskAssistantClosingMessage
    //{
    //}
}
