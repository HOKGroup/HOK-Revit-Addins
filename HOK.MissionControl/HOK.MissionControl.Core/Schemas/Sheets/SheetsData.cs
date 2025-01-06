using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Sheets collection schema.
    /// </summary>
    [DataContract]
    public class SheetData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        [DataMember(Name = "sheets")]
        [JsonPropertyName("sheets")]
        public List<SheetItem> Sheets { get; set; } = new List<SheetItem>();

        [DataMember(Name = "revisions")]
        [JsonPropertyName("revisions")]
        public List<RevisionItem> Revisions { get; set; } = new List<RevisionItem>();
    }
}
