using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Main Configuration class.
    /// </summary>
    public class Configuration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("files")]
        public List<RvtFile> Files { get; set; } = new List<RvtFile>();

        [JsonProperty("sheetDatabase")]
        public string SheetDatabase { get; set; }

        [JsonProperty("sharedParamMonitor")]
        public SharedParameterMonitor SharedParamMonitor { get; set; }

        [JsonProperty("updaters")]
        public List<ProjectUpdater> Updaters { get; set; } = new List<ProjectUpdater>();
    }
}
