using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        public string Id { get; set; }

        [DataMember]
        public string centralPath { get; set; }

        [DataMember]
        public List<SheetItem> sheets { get; set; } = new List<SheetItem>();

        [DataMember]
        public List<RevisionItem> revisions { get; set; } = new List<RevisionItem>();
    }
}
