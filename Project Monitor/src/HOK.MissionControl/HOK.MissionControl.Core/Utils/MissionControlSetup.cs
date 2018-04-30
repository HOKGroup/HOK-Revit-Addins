using System;
using HOK.MissionControl.Core.Schemas;
using System.Collections.Generic;
using HOK.MissionControl.Core.Schemas.Configurations;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Links;
using HOK.MissionControl.Core.Schemas.Models;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Schemas.Styles;
using HOK.MissionControl.Core.Schemas.Views;
using HOK.MissionControl.Core.Schemas.Worksets;

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
            FamilyData.Clear();
            SheetsData.Clear();
            WorksetsData.Clear();
            ModelsData.Clear();
            TriggerRecords.Clear();
            ViewsData.Clear();
            StylesData.Clear();
            LinksData.Clear();
        }

        /// <summary>
        /// (Konrad) Key is Central Path of Document. Since that was stored with lower case in DB we can either track it everywhere and make sure that
        /// we are using toLower() religiously or we can make these Dictionaries ignore case. I chose the latter. 
        /// </summary>
        public static Dictionary<string, Configuration> Configurations { get; set; } 
            = new Dictionary<string, Configuration>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, Project> Projects { get; set; } 
            = new Dictionary<string, Project>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, FamilyData> FamilyData { get; set; } 
            = new Dictionary<string, FamilyData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, SheetData> SheetsData { get; set; } 
            = new Dictionary<string, SheetData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, WorksetData> WorksetsData { get; set; } 
            = new Dictionary<string, WorksetData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, ModelData> ModelsData { get; set; } 
            = new Dictionary<string, ModelData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, string> TriggerRecords { get; set; } 
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, ViewsData> ViewsData { get; set; } 
            = new Dictionary<string, ViewsData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, StylesData> StylesData { get; set; } 
            = new Dictionary<string, StylesData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Key is Central Path of Document.
        /// </summary>
        public static Dictionary<string, LinkData> LinksData { get; set; }
            = new Dictionary<string, LinkData>(StringComparer.OrdinalIgnoreCase);
    }
}
