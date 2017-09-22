using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Schemas
{
    /// <summary>
    /// Event Object stored for each OnOpened/OnSynched events.
    /// </summary>
    public class WorksetEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string user { get; set; } = "";
        public double opened { get; set; } = 0;
        public double closed { get; set; } = 0;
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Individual Workset object stored inside WorksetItem.
    /// </summary>
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; }
        public double count { get; set; }
    }

    /// <summary>
    /// Workset contained stored for each OnClosed event.
    /// </summary>
    public class WorksetItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<Item> worksets { get; set; }
    }

    /// <summary>
    /// View Stats Data Collection
    /// </summary>
    public class ViewStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int totalViews { get; set; }
        public int totalSheets { get; set; }
        public int totalSchedules { get; set; }
        public int viewsOnSheet { get; set; }
        public int viewsOnSheetWithTemplate { get; set; }
        public int schedulesOnSheet { get; set; }
        public int unclippedViews { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Imported DWG File info.
    /// </summary>
    public class DwgFileInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; }
        public int elementId { get; set; }
        public int instances { get; set; }
        public bool isViewSpecific { get; set; }
        public bool isLinked { get; set; }
    }

    /// <summary>
    /// Link Stats Data Collection.
    /// </summary>
    public class LinkStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int totalImportedDwg { get; set; }
        public List<DwgFileInfo> importedDwgFiles { get; set; }
        public int unusedLinkedImages { get; set; }
        public int totalDwgStyles { get; set; }
        public int totalImportedStyles { get; set; }
        public int totalLinkedModels { get; set; }
        public int totalLinkedDwg { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Task object added to Family.
    /// </summary>
    public class FamilyTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; }
        public string assignedTo { get; set; }
        public string message { get; set; }
        public string submittedBy { get; set; }
        public string completedBy { get; set; }
        public DateTime submittedOn { get; set; }
        public DateTime completedOn { get; set; }
    }

    /// <summary>
    /// Individual Family object data.
    /// </summary>
    public class FamilyItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; }
        public string size { get; set; }
        public long sizeValue { get; set; }
        public int instances { get; set; }
        public int elementId { get; set; }
        public int refPlaneCount { get; set; }
        public int arrayCount { get; set; }
        public int voidCount { get; set; }
        public int nestedFamilyCount { get; set; }
        public int parametersCount { get; set; }
        public bool isFailingChecks { get; set; }
        public bool isDeleted { get; set; }
        public List<FamilyTask> tasks { get; set; } = new List<FamilyTask>();
    }

    /// <summary>
    /// Summarized Family stats for Dashbord generation.
    /// </summary>
    public class FamilyStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string centralPath { get; set; }
        public int totalFamilies { get; set; }
        public int unusedFamilies { get; set; }
        public int oversizedFamilies { get; set; }
        public int inPlaceFamilies { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
        public List<FamilyItem> families { get; set; } = new List<FamilyItem>();
    }

    /// <summary>
    /// Model size data, model session event data.
    /// </summary>
    public class EventData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public long value { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Session monitoring data.
    /// </summary>
    public class SessionInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string user { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        public List<DateTime> synched { get; set; } = new List<DateTime>();
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Main MongoDB Collection for Health Report Data.
    /// </summary>
    public class HealthReportData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string centralPath { get; set; }
        public List<WorksetEvent> onOpened { get; set; } = new List<WorksetEvent>();
        public List<WorksetEvent> onSynched { get; set; } = new List<WorksetEvent>();
        public List<WorksetItem> itemCount { get; set; } = new List<WorksetItem>();
        public List<ViewStat> viewStats { get; set; } = new List<ViewStat>();
        public List<LinkStat> linkStats { get; set; } = new List<LinkStat>();
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string familyStats { get; set; }
        public List<EventData> openTimes { get; set; } = new List<EventData>();
        public List<EventData> synchTimes { get; set; } = new List<EventData>();
        public List<EventData> modelSizes { get; set; } = new List<EventData>();
        public List<SessionInfo> sessionLogs { get; set; } = new List<SessionInfo>();
    }
}
