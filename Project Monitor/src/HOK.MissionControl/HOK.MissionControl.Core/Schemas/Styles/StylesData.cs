using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class StylesData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [JsonProperty("styleStats")]
        public List<StylesDataItem> StyleStats { get; set; } = new List<StylesDataItem>();
    }
}
