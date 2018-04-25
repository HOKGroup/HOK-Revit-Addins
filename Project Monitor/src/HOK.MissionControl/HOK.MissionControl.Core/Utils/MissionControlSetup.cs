using System;
using HOK.MissionControl.Core.Schemas;
using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas.Configurations;

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
            FamiliesIds.Clear();
            SheetsIds.Clear();
            WorksetsIds.Clear();
            ModelsIds.Clear();
            TriggerRecords.Clear();
        }

        /// <summary>
        /// (Konrad) Key is Central Path of Document. Since that was stored with lower case in DB we can either track it everywhere and make sure that
        /// we are using toLower() religiously or we can make these Dictionaries ignore case. I chose the latter. 
        /// </summary>
        public static Dictionary<string, Configuration> Configurations { get; set; } = new Dictionary<string, Configuration>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, Project> Projects { get; set; } = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> HealthRecordIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> FamiliesIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> SheetsIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> WorksetsIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> ModelsIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> TriggerRecords { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
