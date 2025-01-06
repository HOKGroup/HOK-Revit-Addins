using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Links
{
    public class LinkDataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("totalImportedDwg")]
        public int TotalImportedDwg { get; set; }

        [JsonPropertyName("importedDwgFiles")]
        public List<DwgFileInfo> ImportedDwgFiles { get; set; }

        [JsonPropertyName("unusedLinkedImages")]
        public int UnusedLinkedImages { get; set; }

        [JsonPropertyName("totalDwgStyles")]
        public int TotalDwgStyles { get; set; }

        [JsonPropertyName("totalImportedStyles")]
        public int TotalImportedStyles { get; set; }

        [JsonPropertyName("totalLinkedModels")]
        public int TotalLinkedModels { get; set; }

        [JsonPropertyName("totalLinkedDwg")]
        public int TotalLinkedDwg { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
