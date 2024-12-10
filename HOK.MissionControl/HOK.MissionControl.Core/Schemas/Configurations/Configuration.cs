using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Main Configuration class.
    /// </summary>
    public class Configuration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("files")]
        public List<RvtFile> Files { get; set; } = new List<RvtFile>();

        [JsonPropertyName("sheetDatabase")]
        public string SheetDatabase { get; set; }

        [JsonPropertyName("sharedParamMonitor")]
        public SharedParameterMonitor SharedParamMonitor { get; set; }

        [JsonPropertyName("updaters")]
        public List<ProjectUpdater> Updaters { get; set; } = new List<ProjectUpdater>();
    }
}
