using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Worksets
{
    public class WorksetStats
    {
        [JsonPropertyName("onOpened")]
        public List<WorksetEvent> OnOpened { get; set; }

        [JsonPropertyName("onSynched")]
        public List<WorksetEvent> OnSynched { get; set; }

        [JsonPropertyName("itemCount")]
        public List<WorksetItemData> ItemCount { get; set; }
    }
}
