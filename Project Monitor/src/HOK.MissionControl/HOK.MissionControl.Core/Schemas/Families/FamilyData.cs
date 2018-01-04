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
    public class FamilyData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        public string centralPath { get; set; }

        [DataMember(Name = "totalFamilies")]
        public int totalFamilies { get; set; }

        [DataMember(Name = "unusedFamilies")]
        public int unusedFamilies { get; set; }

        [DataMember(Name = "oversizedFamilies")]
        public int oversizedFamilies { get; set; }

        [DataMember(Name = "inPlaceFamilies")]
        public int inPlaceFamilies { get; set; }

        [DataMember(Name = "createdBy")]
        public string createdBy { get; set; }

        [DataMember(Name = "createdOn")]
        public DateTime? createdOn { get; set; }

        [DataMember(Name = "families")]
        public List<FamilyItem> families { get; set; } = new List<FamilyItem>();
    }
}
