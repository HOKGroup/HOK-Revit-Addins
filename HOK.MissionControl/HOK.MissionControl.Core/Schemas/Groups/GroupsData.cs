using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupsData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [JsonProperty("groupStats")]
        public List<GroupDataItem> GroupStats { get; set; } = new List<GroupDataItem>();
    }
}
