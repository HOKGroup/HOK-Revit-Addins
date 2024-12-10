using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Families
{
    /// <summary>
    /// Summarized Family stats for Dashbord generation.
    /// </summary>
    [DataContract]
    public class FamilyData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        [DataMember(Name = "totalFamilies")]
        [JsonPropertyName("totalFamilies")]
        public int TotalFamilies { get; set; }

        [DataMember(Name = "unusedFamilies")]
        [JsonPropertyName("unusedFamilies")]
        public int UnusedFamilies { get; set; }

        [DataMember(Name = "oversizedFamilies")]
        [JsonPropertyName("oversizedFamilies")]
        public int OversizedFamilies { get; set; }

        [DataMember(Name = "inPlaceFamilies")]
        [JsonPropertyName("inPlaceFamilies")]
        public int InPlaceFamilies { get; set; }

        [DataMember(Name = "createdBy")]
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [DataMember(Name = "createdOn")]
        [JsonPropertyName("createdOn")]
        public DateTime? CreatedOn { get; set; }

        [DataMember(Name = "families")]
        [JsonPropertyName("families")]
        public List<FamilyItem> Families { get; set; } = new List<FamilyItem>();
    }
}
