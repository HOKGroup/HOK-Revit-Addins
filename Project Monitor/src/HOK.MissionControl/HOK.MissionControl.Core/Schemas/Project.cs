using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Schemas
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string number { get; set; } = "";
        public string name { get; set; } = "";
        public string office { get; set; } = "";
        public ProjectAddress address { get; set; } = new ProjectAddress();
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> configurations { get; set; } = new List<string>();
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> worksets { get; set; } = new List<string>();
    }
}
