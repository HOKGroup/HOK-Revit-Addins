using System.Collections.Generic;
using Autodesk.Revit.DB;
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

        [JsonConstructor]
        public GroupItem()
        {
        }

        public GroupItem(ElementType gt)
        {
            Name = gt.Name;
            
            // (Konrad) If there is a Detail Group attached to Model Group
            // it will have the same name as Model Group but different Category.
            Type = gt.Category.Name == "Attached Detail Groups" 
                ? "Attached Detail Group" 
                : gt.FamilyName;
        }
    }
}
