using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Views
{
    /// <summary>
    /// View Stats Data Collection
    /// </summary>
    public class ViewsDataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("totalViews")]
        public int TotalViews { get; set; }

        [JsonPropertyName("totalSheets")]
        public int TotalSheets { get; set; }

        [JsonPropertyName("totalSchedules")]
        public int TotalSchedules { get; set; }

        [JsonPropertyName("viewsOnSheet")]
        public int ViewsOnSheet { get; set; }

        [JsonPropertyName("viewsOnSheetWithTemplate")]
        public int ViewsOnSheetWithTemplate { get; set; }

        [JsonPropertyName("schedulesOnSheet")]
        public int SchedulesOnSheet { get; set; }

        [JsonPropertyName("unclippedViews")]
        public int UnclippedViews { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
