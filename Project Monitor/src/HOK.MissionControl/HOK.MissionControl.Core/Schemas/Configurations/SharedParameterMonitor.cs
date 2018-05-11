using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class for string Shared Parameter File Location.
    /// </summary>
    public class SharedParameterMonitor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("monitorId")]
        public string MonitorId { get; set; }

        [JsonProperty("monitorName")]
        public string MonitorName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("addInName")]
        public string AddInName { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("isMonitorOn")]
        public bool IsMonitorOn { get; set; }
    }
}
