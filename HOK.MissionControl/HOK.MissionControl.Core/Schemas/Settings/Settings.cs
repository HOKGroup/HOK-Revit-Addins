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

        [JsonProperty("httpAddress")]
        public string HttpAddress { get; set; } = "http://missioncontrol.hok.com";
    }
}
