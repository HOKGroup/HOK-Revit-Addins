using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewsData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        [JsonPropertyName("viewStats")]
        public List<ViewsDataItem> ViewStats { get; set; } = new List<ViewsDataItem>();
    }
}
