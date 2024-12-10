using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Models
{
    public class ModelStats
    {
        [JsonPropertyName("opentimes")]
        public List<ModelEventData> OpenTimes { get; set; }

        [JsonPropertyName("synchtimes")]
        public List<ModelEventData> SynchTimes { get; set; }

        [JsonPropertyName("modelsizes")]
        public List<ModelEventData> ModelSizes { get; set; }
    }
}
