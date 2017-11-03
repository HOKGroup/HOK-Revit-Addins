using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

//namespace HOK.MissionControl.Core.Schemas.Sheets
//{
//    /// <summary>
//    /// Class used to store sheets in DB.
//    /// </summary>
//    public class SheetItem
//    {
//        [BsonId]
//        [BsonRepresentation(BsonType.ObjectId)]
//        [JsonProperty("_id")]
//        public string Id { get; set; }
//        public string name { get; set; }
//        public string number { get; set; }
//        public string uniqueId { get; set; }
//        public string revisionNumber { get; set; }
//        public bool isSelected { get; set; }
//        public string identifier { get; set; } // centralPath + uniqueId

//        public bool isPlaceholder { get; set; }
//        public bool isDeleted { get; set; }
//        public string assignedTo { get; set; }
//        public string message { get; set; }

//        [JsonConstructor]
//        public SheetItem()
//        {
//        }

//        public SheetItem(ViewSheet sheet, string centralPath)
//        {
//            name = sheet.get_Parameter(BuiltInParameter.SHEET_NAME).AsString();
//            number = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
//            uniqueId = sheet.UniqueId;
//            revisionNumber = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
//            identifier = centralPath.ToLower() + "-" + sheet.UniqueId; //prevents possibility of sheet being copied between models
//            isPlaceholder = sheet.IsPlaceholder;
//        }
//    }
//}
