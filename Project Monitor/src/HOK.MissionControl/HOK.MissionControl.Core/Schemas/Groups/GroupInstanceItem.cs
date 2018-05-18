using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupInstanceItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("ownerViewId")]
        public int OwnerViewId { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonConstructor]
        public GroupInstanceItem()
        {
        }

        public GroupInstanceItem(Element group)
        {
            var doc = group.Document;
            var created = WorksharingUtils.GetWorksharingTooltipInfo(doc, group.Id).Creator.ToLower();
            var levelId = group.LevelId == null ? ElementId.InvalidElementId : group.LevelId;
            var levelName = string.Empty;
            if (levelId != ElementId.InvalidElementId)
                levelName = doc.GetElement(levelId).Name;

            CreatedBy = created;
            Level = levelName;
            OwnerViewId = group.OwnerViewId == null 
                ? -1 
                : group.OwnerViewId.IntegerValue;
        }
    }
}
