using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Users
{
    public class UserItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("user")] public string User { get; set; }
        [JsonProperty("machine")] public string Machine { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonProperty("updatedAt")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
