using System.Collections.Generic;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Models
{
    public class ModelStats
    {
        [JsonProperty("opentimes")]
        public List<ModelEventData> OpenTimes { get; set; }

        [JsonProperty("synchtimes")]
        public List<ModelEventData> SynchTimes { get; set; }

        [JsonProperty("modelsizes")]
        public List<ModelEventData> ModelSizes { get; set; }
    }
}
