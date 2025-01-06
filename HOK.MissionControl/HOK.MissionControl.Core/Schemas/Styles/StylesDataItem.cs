using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class StylesDataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("textStats")]
        public List<TextNoteTypeInfo> TextStats { get; set; } = new List<TextNoteTypeInfo>();

        [JsonPropertyName("dimStats")]
        public List<DimensionTypeInfo> DimStats { get; set; } = new List<DimensionTypeInfo>();

        [JsonPropertyName("dimSegmentStats")]
        public List<DimensionSegmentInfo> DimSegmentStats { get; set; } = new List<DimensionSegmentInfo>();
    }
}
