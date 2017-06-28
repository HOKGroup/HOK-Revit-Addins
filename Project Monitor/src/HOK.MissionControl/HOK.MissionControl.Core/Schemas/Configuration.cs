using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Schemas
{
    public class Configuration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string name { get; set; } = "";
        public List<RvtFile> files { get; set; } = new List<RvtFile>();
        public string SheetDatabase { get; set; } = "";
        public List<ProjectUpdater> updaters { get; set; } = new List<ProjectUpdater>();
    }
}
