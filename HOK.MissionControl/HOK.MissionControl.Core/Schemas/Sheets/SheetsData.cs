using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

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
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [DataMember(Name = "sheets")]
        [JsonProperty("sheets")]
        public List<SheetItem> Sheets { get; set; } = new List<SheetItem>();

        [DataMember(Name = "revisions")]
        [JsonProperty("revisions")]
        public List<RevisionItem> Revisions { get; set; } = new List<RevisionItem>();
    }
}
