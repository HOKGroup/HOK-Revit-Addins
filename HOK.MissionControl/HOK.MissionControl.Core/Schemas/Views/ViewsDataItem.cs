using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Views
{
    /// <summary>
    /// View Stats Data Collection
    /// </summary>
    public class ViewsDataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("totalViews")]
        public int TotalViews { get; set; }

        [JsonProperty("totalSheets")]
        public int TotalSheets { get; set; }

        [JsonProperty("totalSchedules")]
        public int TotalSchedules { get; set; }

        [JsonProperty("viewsOnSheet")]
        public int ViewsOnSheet { get; set; }

        [JsonProperty("viewsOnSheetWithTemplate")]
        public int ViewsOnSheetWithTemplate { get; set; }

        [JsonProperty("schedulesOnSheet")]
        public int SchedulesOnSheet { get; set; }

        [JsonProperty("unclippedViews")]
        public int UnclippedViews { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
