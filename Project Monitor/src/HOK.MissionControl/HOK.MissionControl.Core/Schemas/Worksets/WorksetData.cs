using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Worksets
{
    /// <summary>
    /// 
    /// </summary>
    public class WorksetData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [JsonProperty("itemCount")]
        public List<WorksetItemData> ItemCount { get; set; } = new List<WorksetItemData>();

        [JsonProperty("onSynched")]
        public List<WorksetEvent> OnSynched { get; set; } = new List<WorksetEvent>();

        [JsonProperty("onOpened")]
        public List<WorksetEvent> OnOpened { get; set; } = new List<WorksetEvent>();
    }
}
