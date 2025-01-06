using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas
{
    /// <summary>
    /// 
    /// </summary>
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("office")]
        public string Office { get; set; } = "";

        [JsonPropertyName("address")]
        public ProjectAddress Address { get; set; } = new ProjectAddress();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("configurations")]
        public List<string> Configurations { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("triggerRecords")]
        public List<string> TriggerRecords { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("sheets")]
        public List<string> Sheets { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("modelStats")]
        public List<string> ModelStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("linkStats")]
        public List<string> LinkStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("styleStats")]
        public List<string> StyleStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("familyStats")]
        public List<string> FamilyStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("worksetStats")]
        public List<string> WorksetStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("viewStats")]
        public List<string> ViewStats { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("groupStats")]
        public List<string> GroupStats { get; set; } = new List<string>();
    }
}
