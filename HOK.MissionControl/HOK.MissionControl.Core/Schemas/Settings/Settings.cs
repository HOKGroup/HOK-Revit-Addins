using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable UnusedMember.Global

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

        [JsonProperty("userLocation")]
        public UserLocation UserLocation { get; set; } = new UserLocation();
    }

    public class UserLocation
    {
        [JsonProperty("source")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserLocationSources Source { get; set; }

        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("group")]
        public int Group { get; set; }
    }

    public enum UserLocationSources
    {
        MachineName
    }

    public class Office
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("code")]
        public List<string> Code { get; set; } = new List<string>();
    }
}
