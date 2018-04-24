using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Worksets
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class WorksetData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [JsonProperty("itemCount")]
        public List<WorksetItem> ItemCount { get; set; } = new List<WorksetItem>();

        [JsonProperty("onSynched")]
        public List<WorksetEvent> OnSynched { get; set; } = new List<WorksetEvent>();

        [JsonProperty("onOpened")]
        public List<WorksetEvent> OnOpened { get; set; } = new List<WorksetEvent>();
    }

    /// <summary>
    /// Event Object stored for each OnOpened/OnSynched events.
    /// </summary>
    public class WorksetEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("user")]
        public string User { get; set; } = "";

        [JsonProperty("opened")]
        public double Opened { get; set; }

        [JsonProperty("closed")]
        public double Closed { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Workset contained stored for each OnClosed event.
    /// </summary>
    public class WorksetItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("worksets")]
        public List<Item> Worksets { get; set; }
    }

    /// <summary>
    /// Individual Workset object stored inside WorksetItem.
    /// </summary>
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("count")]
        public double Count { get; set; }
    }
}
