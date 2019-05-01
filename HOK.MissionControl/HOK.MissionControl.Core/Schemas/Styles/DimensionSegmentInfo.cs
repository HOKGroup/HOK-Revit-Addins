using System;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class DimensionSegmentInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty("isOverriden")]
        public bool IsOverriden { get; set; }

        [JsonProperty("value")]
        public double? Value { get; set; }

        [JsonProperty("valueString")]
        public string ValueString { get; set; } //not set in constructor

        [JsonProperty("valueOverride")]
        public string ValueOverride { get; set; }

        [JsonProperty("ownerViewId")]
        public int OwnerViewId { get; set; } //not set in constructor

        [JsonProperty("ownerViewType")]
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

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Log.AppendLog(LogMessageType.EXCEPTION, errorContext.Error.Message);
            errorContext.Handled = true;
        }
    }
}
