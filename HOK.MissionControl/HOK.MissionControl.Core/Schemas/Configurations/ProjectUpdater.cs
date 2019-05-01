using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class wrapper around most of the category based updaters.
    /// </summary>
    public class ProjectUpdater
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("updaterId")]
        public string UpdaterId { get; set; }

        [JsonProperty("updaterName")]
        public string UpdaterName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("addInId")]
        public string AddInId { get; set; }

        [JsonProperty("addInName")]
        public string AddInName { get; set; }

        [JsonProperty("isUpdaterOn")]
        public bool IsUpdaterOn { get; set; }

        [JsonProperty("categoryTriggers")]
        public List<CategoryTrigger> CategoryTriggers { get; set; } = new List<CategoryTrigger>();

        [JsonProperty("userOverrides")]
        public UserOverrides UserOverrides { get; set; }
    }
}
