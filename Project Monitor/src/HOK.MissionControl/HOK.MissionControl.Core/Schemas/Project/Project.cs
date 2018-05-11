using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas
{
    /// <summary>
    /// 
    /// </summary>
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("office")]
        public string Office { get; set; } = "";

        [JsonProperty("address")]
        public ProjectAddress Address { get; set; } = new ProjectAddress();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("configurations")]
        public List<string> Configurations { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("triggerRecords")]
        public List<string> TriggerRecords { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("sheets")]
        public List<string> Sheets { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("modelStats")]
        public List<string> ModelStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("linkStats")]
        public List<string> LinkStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("styleStats")]
        public List<string> StyleStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("familyStats")]
        public List<string> FamilyStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("worksetStats")]
        public List<string> WorksetStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("viewStats")]
        public List<string> ViewStats { get; set; } = new List<string>();
    }
}
