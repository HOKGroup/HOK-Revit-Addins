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
    }

    public class FamilyStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<FamilyItem> suspectFamilies { get; set; }
        public int totalFamilies { get; set; }
        public int unusedFamilies { get; set; }
        public int oversizedFamilies { get; set; }
        public int inPlaceFamilies { get; set; }
        public string createdBy { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;

    }

    public class EventTime
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public double value { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
    }

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
        public List<FamilyStat> familyStats { get; set; } = new List<FamilyStat>();
        public List<EventTime> openTimes { get; set; } = new List<EventTime>();
        public List<EventTime> synchTimes { get; set; } = new List<EventTime>();
        public List<EventTime> modelSizes { get; set; } = new List<EventTime>();
        public List<SessionInfo> sessionLogs { get; set; } = new List<SessionInfo>();
    }

    #region GET

    /// <summary>
    /// Response returned by MongoDB when queried for /familystats
    /// </summary>
    public class FamilyResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<FamilyStat> familyStats { get; set; } = new List<FamilyStat>();
    }

    #endregion
}
