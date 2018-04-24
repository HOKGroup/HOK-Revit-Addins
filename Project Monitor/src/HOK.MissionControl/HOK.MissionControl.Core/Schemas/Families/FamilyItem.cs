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
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember(Name = "size")]
        [JsonProperty("size")]
        public string Size { get; set; }

        [DataMember(Name = "sizeValue")]
        [JsonProperty("sizeValue")]
        public long SizeValue { get; set; }

        [DataMember(Name = "instances")]
        [JsonProperty("instances")]
        public int Instances { get; set; }

        [DataMember(Name = "elementId")]
        [JsonProperty("elementId")]
        public int ElementId { get; set; }

        [DataMember(Name = "refPlaneCount")]
        [JsonProperty("refPlaneCount")]
        public int RefPlaneCount { get; set; }

        [DataMember(Name = "arrayCount")]
        [JsonProperty("arrayCount")]
        public int ArrayCount { get; set; }

        [DataMember(Name = "voidCount")]
        [JsonProperty("voidCount")]
        public int VoidCount { get; set; }

        [DataMember(Name = "nestedFamilyCount")]
        [JsonProperty("nestedFamilyCount")]
        public int NestedFamilyCount { get; set; }

        [DataMember(Name = "parametersCount")]
        [JsonProperty("parametersCount")]
        public int ParametersCount { get; set; }

        [DataMember(Name = "isFailingChecks")]
        [JsonProperty("isFailingChecks")]
        public bool IsFailingChecks { get; set; }

        [DataMember(Name = "isDeleted")]
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "tasks")]
        [JsonProperty("tasks")]
        public List<FamilyTask> Tasks { get; set; } = new List<FamilyTask>();
    }
}
