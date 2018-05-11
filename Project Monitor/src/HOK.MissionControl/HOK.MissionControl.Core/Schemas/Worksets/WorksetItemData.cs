using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Worksets
{
    /// <summary>
    /// 
    /// </summary>
    public class WorksetItemData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("worksets")]
        public List<WorksetItem> Worksets { get; set; }
    }
}
