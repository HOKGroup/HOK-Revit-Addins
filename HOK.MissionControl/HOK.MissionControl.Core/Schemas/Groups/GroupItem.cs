using System.Collections.Generic;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Groups
{
    public class GroupItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("memberCount")]
        public int MemberCount { get; set; } = 0;

        [JsonProperty("instances")]
        public List<GroupInstanceItem> Instances { get; set; } = new List<GroupInstanceItem>();
    }
}
