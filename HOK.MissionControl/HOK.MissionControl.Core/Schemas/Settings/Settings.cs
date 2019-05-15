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

        [JsonProperty("projectInfo")]
        public ProjectInfo ProjectInfo { get; set; } = new ProjectInfo();
    }

    public class ProjectInfo
    {
        [JsonProperty("source")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectInfoSources Source { get; set; }

        [JsonProperty("projectName")]
        public Dictionary<string, object> ProjectName { get; set; }

        [JsonProperty("projectNumber")]
        public Dictionary<string, object> ProjectNumber { get; set; }

        [JsonProperty("projectLocation")]
        public Dictionary<string, object> ProjectLocation { get; set; }
    }

    public class UserLocation
    {
        [JsonProperty("source")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UserLocationSources Source { get; set; }

        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("match")]
        public int Match { get; set; }

        [JsonProperty("group")]
        public int Group { get; set; }
    }

    public enum ProjectInfoSources
    {
        FilePath
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
