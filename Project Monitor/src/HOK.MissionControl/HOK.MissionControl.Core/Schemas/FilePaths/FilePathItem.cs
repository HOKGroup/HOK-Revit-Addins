using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.FilePaths
{
    public class FilePathItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonConstructor]
        public FilePathItem()
        {
        }

        public FilePathItem(string centralPath)
        {
            CentralPath = centralPath.ToLower();
        }
    }
}
