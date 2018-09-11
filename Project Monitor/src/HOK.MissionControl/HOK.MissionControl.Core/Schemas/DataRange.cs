using System;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas
{
    public class DataRangeRequest
    {
        [JsonProperty("from")]
        public DateTime? From { get; set; }

        [JsonProperty("to")]
        public DateTime? To { get; set; }

        [JsonProperty("centralPath")]
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

        public DataRangeRequest(string centralPath)
        {
            CentralPath = centralPath;
            From = null;
            To = null;
        }
    }
}
