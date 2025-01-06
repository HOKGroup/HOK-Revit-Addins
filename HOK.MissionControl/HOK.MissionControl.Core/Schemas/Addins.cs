#region References

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Settings;
using HOK.MissionControl.Core.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

#endregion

namespace HOK.MissionControl.Core.Schemas
{
    public class InfoItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class AddinLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("pluginName")]
        public string PluginName { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("revitVersion")]
        public string RevitVersion { get; set; }

        [JsonPropertyName("office")]
        public string Office { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("detailInfo")]
        public List<InfoItem> DetailInfo { get; set; } = new List<InfoItem>();

        public AddinLog()
        {
        }

        public AddinLog(string name, string version)
        {
            PluginName = name;
            User = Environment.UserName.ToLower();
            RevitVersion = version;
            Office = GetOffice();
        }

        /// <summary>
        /// Retrieves office name from machine name ex. NY
        /// </summary>
        /// <returns>Office name.</returns>
        private static string GetOffice()
        {
            try
            {
                var settings = AppSettings.Instance.UserLocation;
                string sourceString;
                switch (settings.Source)
                {
                    case UserLocationSources.MachineName:
                        sourceString = Environment.MachineName;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (string.IsNullOrWhiteSpace(sourceString)) return string.Empty;

                // (Konrad) If we got a Source string ie. Machine Name we can proceed
                // to extract the user location/office from it using regex pattern.
                var matches = Regex.Matches(sourceString, settings.Pattern, RegexOptions.IgnoreCase);
                if (matches.Count <= settings.Match) return string.Empty;

                var m = matches[settings.Match];
                return m.Groups.Count > settings.Group 
                    ? m.Groups[settings.Group].Value 
                    : string.Empty;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return string.Empty;
            }
        }
    }
}
