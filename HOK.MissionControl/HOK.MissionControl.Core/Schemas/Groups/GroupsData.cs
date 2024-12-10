using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupsData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        [JsonPropertyName("groupStats")]
        public List<GroupDataItem> GroupStats { get; set; } = new List<GroupDataItem>();
    }
}
