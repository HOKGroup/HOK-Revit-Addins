using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Families
{
    /// <summary>
    /// Individual Family object data.
    /// </summary>
    [DataContract]
    public class FamilyItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [DataMember(Name = "size")]
        [JsonPropertyName("size")]
        public string Size { get; set; }

        [DataMember(Name = "sizeValue")]
        [JsonPropertyName("sizeValue")]
        public long SizeValue { get; set; }

        [DataMember(Name = "instances")]
        [JsonPropertyName("instances")]
        public int Instances { get; set; }

        [DataMember(Name = "elementId")]
        [JsonPropertyName("elementId")]
        public long ElementId { get; set; }

        [DataMember(Name = "refPlaneCount")]
        [JsonPropertyName("refPlaneCount")]
        public int RefPlaneCount { get; set; }

        [DataMember(Name = "arrayCount")]
        [JsonPropertyName("arrayCount")]
        public int ArrayCount { get; set; }

        [DataMember(Name = "imageCount")]
        [JsonPropertyName("imageCount")]
        public int ImageCount { get; set; }

        [DataMember(Name = "voidCount")]
        [JsonPropertyName("voidCount")]
        public int VoidCount { get; set; }

        [DataMember(Name = "nestedFamilyCount")]
        [JsonPropertyName("nestedFamilyCount")]
        public int NestedFamilyCount { get; set; }

        [DataMember(Name = "parametersCount")]
        [JsonPropertyName("parametersCount")]
        public int ParametersCount { get; set; }

        [DataMember(Name = "isFailingChecks")]
        [JsonPropertyName("isFailingChecks")]
        public bool IsFailingChecks { get; set; }

        [DataMember(Name = "isDeleted")]
        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "tasks")]
        [JsonPropertyName("tasks")]
        public List<FamilyTask> Tasks { get; set; } = new List<FamilyTask>();
    }
}
