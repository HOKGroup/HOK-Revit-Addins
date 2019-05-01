using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Links
{
    public class DwgFileInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("elementId")]
        public int ElementId { get; set; }

        [JsonProperty("instances")]
        public int Instances { get; set; }

        [JsonProperty("isViewSpecific")]
        public bool IsViewSpecific { get; set; }

        [JsonProperty("isLinked")]
        public bool IsLinked { get; set; }
    }
}
