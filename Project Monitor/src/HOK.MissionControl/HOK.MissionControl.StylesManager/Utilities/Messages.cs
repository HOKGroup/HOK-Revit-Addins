using System.Collections.Generic;

namespace HOK.MissionControl.StylesManager.Utilities
{
    /// <summary>
    /// Message sent when Dimension Overrides are cleared.
    /// </summary>
    public class OverridesCleared
    {
        public List<DimensionWrapper> Dimensions { get; set; }
    }

    /// <summary>
    /// Message sent when Dimension Types are deleted.
    /// </summary>
    public class DimensionsDeleted
    {
        public List<DimensionTypeWrapper> Dimensions { get; set; }
    }

    /// <summary>
    /// Message sent when Text Styles are deleted.
    /// </summary>
    public class TextStylesDeleted
    {
        public List<TextStyleWrapper> TextStyles { get; set; }
    }
}
