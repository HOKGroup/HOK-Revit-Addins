using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewsData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [JsonProperty("viewStats")]
        public List<ViewsDataItem> ViewStats { get; set; } = new List<ViewsDataItem>();
    }
}
