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
        [JsonProperty("_id")]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public int sequence { get; set; }

        [DataMember]
        public string number { get; set; }

        [DataMember]
        public string date { get; set; }

        [DataMember]
        public string issuedTo { get; set; }

        [DataMember]
        public string issuedBy { get; set; }

        [DataMember]
        public string uniqueId { get; set; }

        [JsonConstructor]
        public RevisionItem()
        {
        }

        public RevisionItem(Element rev)
        {
            description = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION)?.AsString();
            sequence = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM).AsInteger(); //TODO: Potentially an issue.
            number = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_NUM)?.AsString();
            date = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE)?.AsString();
            issuedTo = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO)?.AsString();
            issuedBy = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY)?.AsString();
            uniqueId = rev.UniqueId;
        }
    }
}
