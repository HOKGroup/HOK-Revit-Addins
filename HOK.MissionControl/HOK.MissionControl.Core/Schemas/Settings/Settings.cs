using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Settings
{
    /// <summary>
    /// Global settings class for Mission Control.
    /// </summary>
    public class Settings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; } = "Settings";

        [JsonProperty("offices")]
        public List<Office> Offices { get; set; } = new List<Office>();

        [JsonProperty("states")]
        public List<string> States { get; set; } = new List<string>();

        [JsonProperty("localPathRgx")]
        public List<string> LocalPathRgx { get; set; } = new List<string>();
    }

    public class Office
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("code")]
        public List<string> Code { get; set; } = new List<string>();
    }
}
