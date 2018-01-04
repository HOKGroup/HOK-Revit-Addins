using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Families
{
    /// <summary>
    /// Individual Family object data.
    /// </summary>
    public class FamilyItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "size")]
        public string size { get; set; }

        [DataMember(Name = "sizeValue")]
        public long sizeValue { get; set; }

        [DataMember(Name = "instances")]
        public int instances { get; set; }

        [DataMember(Name = "elementId")]
        public int elementId { get; set; }

        [DataMember(Name = "refPlaneCount")]
        public int refPlaneCount { get; set; }

        [DataMember(Name = "arrayCount")]
        public int arrayCount { get; set; }

        [DataMember(Name = "voidCount")]
        public int voidCount { get; set; }

        [DataMember(Name = "nestedFamilyCount")]
        public int nestedFamilyCount { get; set; }

        [DataMember(Name = "parametersCount")]
        public int parametersCount { get; set; }

        [DataMember(Name = "isFailingChecks")]
        public bool isFailingChecks { get; set; }

        [DataMember(Name = "isDeleted")]
        public bool isDeleted { get; set; }

        [DataMember(Name = "tasks")]
        public List<FamilyTask> tasks { get; set; } = new List<FamilyTask>();
    }
}
