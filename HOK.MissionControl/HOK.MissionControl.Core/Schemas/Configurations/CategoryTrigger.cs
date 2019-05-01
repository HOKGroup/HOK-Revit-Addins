using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class posted to MongoDB when user overrides any of the DTM Tools.
    /// </summary>
    public class CategoryTrigger
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("categoryName")]
        public string CategoryName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; } = DateTime.UtcNow;
    }
}
