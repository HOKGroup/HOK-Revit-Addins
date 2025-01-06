using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas
{
    public class DataRangeRequest
    {
        [JsonPropertyName("from")]
        public DateTime? From { get; set; }

        [JsonPropertyName("to")]
        public DateTime? To { get; set; }

        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        [JsonConstructor]
        public DataRangeRequest()
        {
        }

        public DataRangeRequest(string centralPath, DateTime? from = null, DateTime? to = null)
        {
            CentralPath = centralPath;
            From = from ?? DateTime.UtcNow.AddMonths(-1);
            To = to ?? DateTime.UtcNow;
        }
    }
}
