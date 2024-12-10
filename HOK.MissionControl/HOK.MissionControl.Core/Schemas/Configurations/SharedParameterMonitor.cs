using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class for string Shared Parameter File Location.
    /// </summary>
    public class SharedParameterMonitor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("monitorId")]
        public string MonitorId { get; set; }

        [JsonPropertyName("monitorName")]
        public string MonitorName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("addInName")]
        public string AddInName { get; set; }

        [JsonPropertyName("filePath")]
        public string FilePath { get; set; }

        [JsonPropertyName("isMonitorOn")]
        public bool IsMonitorOn { get; set; }
    }
}
