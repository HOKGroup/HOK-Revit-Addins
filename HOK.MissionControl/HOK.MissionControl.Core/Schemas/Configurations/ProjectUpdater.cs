using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class wrapper around most of the category based updaters.
    /// </summary>
    public class ProjectUpdater
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("updaterId")]
        public string UpdaterId { get; set; }

        [JsonPropertyName("updaterName")]
        public string UpdaterName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("addInId")]
        public string AddInId { get; set; }

        [JsonPropertyName("addInName")]
        public string AddInName { get; set; }

        [JsonPropertyName("isUpdaterOn")]
        public bool IsUpdaterOn { get; set; }

        [JsonPropertyName("categoryTriggers")]
        public List<CategoryTrigger> CategoryTriggers { get; set; } = new List<CategoryTrigger>();

        [JsonPropertyName("userOverrides")]
        public UserOverrides UserOverrides { get; set; }
    }
}
