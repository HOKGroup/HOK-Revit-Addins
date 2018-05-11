using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Worksets
{
    /// <summary>
    /// 
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
}
