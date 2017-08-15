using HOK.MissionControl.Core.Schemas;
using System.Collections.Generic;

namespace HOK.MissionControl.Core.Utils
{
    public static class MissionControlSetup
    {
        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, Configuration> Configurations { get; set; } = new Dictionary<string, Configuration>();

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, Project> Projects { get; set; } = new Dictionary<string, Project>();

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> HealthRecordIds { get; set; } = new Dictionary<string, string>();
    }
}
