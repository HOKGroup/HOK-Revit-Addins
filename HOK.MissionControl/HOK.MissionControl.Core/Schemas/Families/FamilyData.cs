using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

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
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [DataMember(Name = "totalFamilies")]
        [JsonProperty("totalFamilies")]
        public int TotalFamilies { get; set; }

        [DataMember(Name = "unusedFamilies")]
        [JsonProperty("unusedFamilies")]
        public int UnusedFamilies { get; set; }

        [DataMember(Name = "oversizedFamilies")]
        [JsonProperty("oversizedFamilies")]
        public int OversizedFamilies { get; set; }

        [DataMember(Name = "inPlaceFamilies")]
        [JsonProperty("inPlaceFamilies")]
        public int InPlaceFamilies { get; set; }

        [DataMember(Name = "createdBy")]
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [DataMember(Name = "createdOn")]
        [JsonProperty("createdOn")]
        public DateTime? CreatedOn { get; set; }

        [DataMember(Name = "families")]
        [JsonProperty("families")]
        public List<FamilyItem> Families { get; set; } = new List<FamilyItem>();
    }
}
