using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas
{


    /// <summary>
    /// View Stats Data Collection
    /// </summary>
    public class ViewStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public int totalViews { get; set; }
        public int totalSheets { get; set; }
        public int totalSchedules { get; set; }
        public int viewsOnSheet { get; set; }
        public int viewsOnSheetWithTemplate { get; set; }
        public int schedulesOnSheet { get; set; }
        public int unclippedViews { get; set; }
        public DateTime createdOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Model size data, model session event data.
    /// </summary>
    public class EventData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public long value { get; set; }
        public string user { get; set; }
        public DateTime createdOn { get; set; } = DateTime.UtcNow;
    }

    ///// <summary>
    ///// Main MongoDB Collection for Health Report Data.
    ///// </summary>
    //public class HealthReportData
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    [JsonProperty("_id")]
    //    public string Id { get; set; }
    //    public string centralPath { get; set; }
    //    public List<WorksetEvent> onOpened { get; set; } = new List<WorksetEvent>();
    //    public List<WorksetEvent> onSynched { get; set; } = new List<WorksetEvent>();
    //    public List<WorksetItem> itemCount { get; set; } = new List<WorksetItem>();
    //    public List<ViewStat> viewStats { get; set; } = new List<ViewStat>();
    //    public List<StylesStat> styleStats { get; set; } = new List<StylesStat>();
    //    public List<LinkStat> linkStats { get; set; } = new List<LinkStat>();
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string familyStats { get; set; }
    //    public List<EventData> openTimes { get; set; } = new List<EventData>();
    //    public List<EventData> synchTimes { get; set; } = new List<EventData>();
    //    public List<EventData> modelSizes { get; set; } = new List<EventData>();
    //}
}
