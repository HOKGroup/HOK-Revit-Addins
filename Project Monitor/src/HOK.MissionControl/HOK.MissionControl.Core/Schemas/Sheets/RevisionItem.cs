using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Class used to store Revisions in DB.
    /// </summary>
    [DataContract]
    public class RevisionItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        [DataMember(Name = "sequence")]
        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        [DataMember(Name = "number")]
        [JsonProperty("number")]
        public string Number { get; set; }

        [DataMember(Name = "date")]
        [JsonProperty("date")]
        public string Date { get; set; }

        [DataMember(Name = "issuedTo")]
        [JsonProperty("issuedTo")]
        public string IssuedTo { get; set; }

        [DataMember(Name = "issuedBy")]
        [JsonProperty("issuedBy")]
        public string IssuedBy { get; set; }

        [DataMember(Name = "uniqueId")]
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

        [JsonConstructor]
        public RevisionItem()
        {
        }

        public RevisionItem(Element rev)
        {
            Description = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION)?.AsString();
            Sequence = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM).AsInteger(); //TODO: Potentially an issue.
            Number = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_NUM)?.AsString();
            Date = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE)?.AsString();
            IssuedTo = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO)?.AsString();
            IssuedBy = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY)?.AsString();
            UniqueId = rev.UniqueId;
        }
    }
}
