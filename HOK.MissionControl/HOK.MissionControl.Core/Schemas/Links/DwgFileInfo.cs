using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Links
{
    public class DwgFileInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("elementId")]
        public long ElementId { get; set; }

        [JsonPropertyName("instances")]
        public int Instances { get; set; }

        [JsonPropertyName("isViewSpecific")]
        public bool IsViewSpecific { get; set; }

        [JsonPropertyName("isLinked")]
        public bool IsLinked { get; set; }
    }
}
