using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Links
{
    public class LinkDataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("totalImportedDwg")]
        public int TotalImportedDwg { get; set; }

        [JsonProperty("importedDwgFiles")]
        public List<DwgFileInfo> ImportedDwgFiles { get; set; }

        [JsonProperty("unusedLinkedImages")]
        public int UnusedLinkedImages { get; set; }

        [JsonProperty("totalDwgStyles")]
        public int TotalDwgStyles { get; set; }

        [JsonProperty("totalImportedStyles")]
        public int TotalImportedStyles { get; set; }

        [JsonProperty("totalLinkedModels")]
        public int TotalLinkedModels { get; set; }

        [JsonProperty("totalLinkedDwg")]
        public int TotalLinkedDwg { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
