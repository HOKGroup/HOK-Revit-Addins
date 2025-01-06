using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Configurations
{
    /// <summary>
    /// Class holding all different possible user overrides for health checks.
    /// </summary>
    public class UserOverrides
    {
        [JsonPropertyName("dimensionValueCheck")]
        public DimensionValueCheck DimensionValueCheck { get; set; }

        [JsonPropertyName("familyNameCheck")]
        public FamilyNameCheck FamilyNameCheck { get; set; }
    }

    /// <summary>
    /// Override for Dimension Values.
    /// </summary>
    public class DimensionValueCheck
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("values")]
        public List<string> Values { get; set; } = new List<string>();
    }

    /// <summary>
    /// Override for Family Name.
    /// </summary>
    public class FamilyNameCheck
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("values")]
        public List<string> Values { get; set; } = new List<string>();
    }
}
