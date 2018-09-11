using System.Collections.Generic;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Worksets
{
    public class WorksetStats
    {
        [JsonProperty("onOpened")]
        public List<WorksetEvent> OnOpened { get; set; }

        [JsonProperty("onSynched")]
        public List<WorksetEvent> OnSynched { get; set; }

        [JsonProperty("itemCount")]
        public List<WorksetItemData> ItemCount { get; set; }
    }
}
