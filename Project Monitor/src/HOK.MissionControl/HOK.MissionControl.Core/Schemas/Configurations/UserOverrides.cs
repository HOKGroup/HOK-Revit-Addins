using System.Collections.Generic;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class holding all different possible user overrides for health checks.
    /// </summary>
    public class UserOverrides
    {
        [JsonProperty("dimensionValueCheck")]
        public DimensionValueCheck DimensionValueCheck { get; set; }

        [JsonProperty("familyNameCheck")]
        public FamilyNameCheck FamilyNameCheck { get; set; }
    }

    /// <summary>
    /// Override for Dimension Values.
    /// </summary>
    public class DimensionValueCheck
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; } = new List<string>();
    }

    /// <summary>
    /// Override for Family Name.
    /// </summary>
    public class FamilyNameCheck
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("values")]
        public List<string> Values { get; set; } = new List<string>();
    }
}
