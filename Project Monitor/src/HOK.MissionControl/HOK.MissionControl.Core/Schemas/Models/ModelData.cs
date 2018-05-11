using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ModelData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [JsonProperty("modelSizes")]
        public List<ModelEventData> ModelSizes { get; set; } = new List<ModelEventData>();

        [JsonProperty("synchTimes")]
        public List<ModelEventData> SynchTimes { get; set; } = new List<ModelEventData>();

        [JsonProperty("openTimes")]
        public List<ModelEventData> OpenTimes { get; set; } = new List<ModelEventData>();
    }
}
