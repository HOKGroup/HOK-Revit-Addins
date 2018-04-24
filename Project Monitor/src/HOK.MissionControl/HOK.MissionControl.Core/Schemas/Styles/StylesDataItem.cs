using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    public class StylesDataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("textStats")]
        public List<TextNoteTypeInfo> TextStats { get; set; } = new List<TextNoteTypeInfo>();

        [JsonProperty("dimStats")]
        public List<DimensionTypeInfo> DimStats { get; set; } = new List<DimensionTypeInfo>();

        [JsonProperty("dimSegmentStats")]
        public List<DimensionSegmentInfo> DimSegmentStats { get; set; } = new List<DimensionSegmentInfo>();
    }
}
