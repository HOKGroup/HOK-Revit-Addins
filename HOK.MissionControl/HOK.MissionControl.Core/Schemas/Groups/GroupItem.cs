using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Groups
{
    public class GroupItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("memberCount")]
        public int MemberCount { get; set; } = 0;

        [JsonPropertyName("instances")]
        public List<GroupInstanceItem> Instances { get; set; } = new List<GroupInstanceItem>();
    }
}
