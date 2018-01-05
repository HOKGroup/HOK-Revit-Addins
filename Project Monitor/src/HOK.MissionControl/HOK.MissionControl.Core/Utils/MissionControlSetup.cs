using HOK.MissionControl.Core.Schemas;
using System.Collections.Generic;

namespace HOK.MissionControl.Core.Utils
{
    public static class MissionControlSetup
    {
        /// <summary>
        /// Clears out all values stored in the dictionaries.
        /// </summary>
        public static void ClearAll()
        {
            Configurations.Clear();
            Projects.Clear();
            HealthRecordIds.Clear();
            FamiliesIds.Clear();
            SheetsIds.Clear();
        }

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
        
        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> FamiliesIds { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> SheetsIds { get; set; } = new Dictionary<string, string>();
    }
}
