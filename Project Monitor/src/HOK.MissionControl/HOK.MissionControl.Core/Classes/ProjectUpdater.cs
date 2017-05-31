using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Classes
{
    public class ProjectUpdater
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string updaterId { get; set; } = "";
        public string updaterName { get; set; } = "";
        public string description { get; set; } = "";
        public string addInId { get; set; } = "";
        public string addInName { get; set; } = "";
        public bool isUpdaterOn { get; set; } = false;
        public List<CategoryTrigger> CategoryTriggers { get; set; } = new List<CategoryTrigger>();
    }
}
