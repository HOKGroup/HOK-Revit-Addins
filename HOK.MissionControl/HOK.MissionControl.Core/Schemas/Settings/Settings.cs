#region References

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable UnusedMember.Global

#endregion

namespace HOK.MissionControl.Core.Schemas.Settings
{
    /// <summary>
    /// Global settings class for Mission Control.
    /// </summary>
    public class Settings
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; } = "Settings";

        [JsonPropertyName("offices")]
        public List<Office> Offices { get; set; } = new List<Office>();

        [JsonPropertyName("states")]
        public List<string> States { get; set; } = new List<string>();

        [JsonPropertyName("localPathRgx")]
        public List<string> LocalPathRgx { get; set; } = new List<string>();

        [JsonPropertyName("userLocation")]
        public UserLocation UserLocation { get; set; } = new UserLocation();

        [JsonPropertyName("projectInfo")]
        public ProjectInfo ProjectInfo { get; set; } = new ProjectInfo();

        [JsonPropertyName("tempLocation")]
        public TempLocation TempLocation { get; set; } = new TempLocation();
    }

    public class ProjectInfo
    {
        [JsonPropertyName("source")]
        public ProjectInfoSources Source { get; set; }

        [JsonPropertyName("projectName")]
        public Dictionary<string, object> ProjectName { get; set; }

        [JsonPropertyName("projectNumber")]
        public Dictionary<string, object> ProjectNumber { get; set; }

        [JsonPropertyName("projectLocation")]
        public Dictionary<string, object> ProjectLocation { get; set; }
    }

    public class UserLocation
    {
        [JsonPropertyName("source")]
        public UserLocationSources Source { get; set; }

        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }

        [JsonPropertyName("match")]
        public int Match { get; set; }

        [JsonPropertyName("group")]
        public int Group { get; set; }
    }

    public class TempLocation
    {
        [JsonPropertyName("source")]
        public TempLocationSources Source { get; set; }

        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }

        [JsonPropertyName("tempPath")]
        public string TempPath { get; set; }
    }

    public enum ProjectInfoSources
    {
        FilePath
    }

    public enum UserLocationSources
    {
        MachineName
    }

    public enum TempLocationSources
    {
        MachineName
    }

    public class Office
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public List<string> Code { get; set; } = new List<string>();
    }
}
