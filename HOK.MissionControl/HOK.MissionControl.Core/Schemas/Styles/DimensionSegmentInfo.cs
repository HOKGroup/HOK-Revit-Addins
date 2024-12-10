using System;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class DimensionSegmentInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("isOverriden")]
        public bool IsOverriden { get; set; }

        [JsonPropertyName("value")]
        public double? Value { get; set; }

        [JsonPropertyName("valueString")]
        public string ValueString { get; set; } //not set in constructor

        [JsonPropertyName("valueOverride")]
        public string ValueOverride { get; set; }

        [JsonPropertyName("ownerViewId")]
        public long OwnerViewId { get; set; } //not set in constructor

        [JsonPropertyName("ownerViewType")]
        public string OwnerViewType { get; set; } //not set in constructor

        [JsonConstructor]
        public DimensionSegmentInfo()
        {
        }

        public DimensionSegmentInfo(DimensionSegment dim)
        {
            IsOverriden = !string.IsNullOrEmpty(dim.ValueOverride);
            Value = dim.Value;
            ValueOverride = dim.ValueOverride;
        }

        public DimensionSegmentInfo(Dimension dim)
        {
            IsOverriden = !string.IsNullOrEmpty(dim.ValueOverride);
            Value = dim.Value;
            ValueOverride = dim.ValueOverride;
        }

        //[OnError]
        //internal void OnError(StreamingContext context, ErrorContext errorContext)
        //{
        //    Log.AppendLog(LogMessageType.EXCEPTION, errorContext.Error.Message);
        //    errorContext.Handled = true;
        //}
    }
}
