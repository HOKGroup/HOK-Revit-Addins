using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Sheets collection schema.
    /// </summary>
    public class SheetData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public string centralPath { get; set; }
        public List<SheetItem> sheets { get; set; } = new List<SheetItem>();
        public List<SheetItem> sheetsChanges { get; set; } = new List<SheetItem>();
        public List<RevisionItem> revisions { get; set; } = new List<RevisionItem>();
    }
}
