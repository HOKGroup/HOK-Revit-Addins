using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Class used to store Revisions in DB.
    /// </summary>
    public class RevisionItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public string description { get; set; }
        public int sequence { get; set; }
        public string number { get; set; }
        public string date { get; set; }
        public string issuedTo { get; set; }
        public string issuedBy { get; set; }
        public string uniqueId { get; set; }

        [JsonConstructor]
        public RevisionItem()
        {
        }

        public RevisionItem(Element rev)
        {
            description = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION).AsString();
            sequence = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM).AsInteger();
            number = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_NUM).AsString();
            date = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE).AsString();
            issuedTo = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO).AsString();
            issuedBy = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY).AsString();
            uniqueId = rev.UniqueId;
        }
    }
}
