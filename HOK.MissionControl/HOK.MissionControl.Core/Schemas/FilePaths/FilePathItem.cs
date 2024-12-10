#region References

using System;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Settings;
using HOK.MissionControl.Core.Utils;
using System.Text.Json.Nodes;

#endregion

namespace HOK.MissionControl.Core.Schemas.FilePaths
{
    public class FilePathItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; }

        [JsonPropertyName("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonPropertyName("revitVersion")]
        public string RevitVersion { get; set; } = string.Empty;

        [JsonPropertyName("projectNumber")]
        public string ProjectNumber { get; set; } = "00.00000.00";

        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonPropertyName("fileLocation")]
        public string FileLocation { get; set; } = string.Empty;

        [JsonConstructor]
        public FilePathItem()
        {
        }

        public FilePathItem(string centralPath, string version)
        {
            CentralPath = centralPath.ToLower();
            RevitVersion = version;

            GetProjectInfo(AppSettings.Instance.ProjectInfo.Source);
        }

        #region Utilities

        /// <summary>
        /// Retrieves Project Info using method specified in Settings.
        /// </summary>
        /// <param name="source">Source of the info ex. FilePath.</param>
        private void GetProjectInfo(ProjectInfoSources source)
        {
            switch (source)
            {
                case ProjectInfoSources.FilePath:
                    if (string.IsNullOrEmpty(CentralPath)) break;

                    var settingsName = AppSettings.Instance.ProjectInfo.ProjectName;
                    var settingsNumber = AppSettings.Instance.ProjectInfo.ProjectNumber;
                    var settingsLocation = AppSettings.Instance.ProjectInfo.ProjectLocation;
                    try
                    {
                        string key;
                        if (CentralPath.StartsWith("rsn://", StringComparison.OrdinalIgnoreCase))
                            key = "revitServer";
                        else if (AppSettings.Instance.LocalPathRgx.Any(x =>
                            Regex.IsMatch(CentralPath, x, RegexOptions.IgnoreCase)))
                            key = "local";
                        else if (CentralPath.Contains("://"))
                            key = "bimThreeSixty";
                        else return;

                        var nameObject = settingsName.ContainsKey(key) 
                            ? (JsonObject)JsonSerializer.SerializeToNode(settingsName[key]) 
                            : null;
                        var numberObject = settingsNumber.ContainsKey(key) 
                            ? (JsonObject)JsonSerializer.SerializeToNode(settingsNumber[key]) 
                            : null;
                        var locationObject = settingsLocation.ContainsKey(key) 
                            ? (JsonObject)JsonSerializer.SerializeToNode(settingsLocation[key]) 
                            : null;

                        ProjectName = GetValueFromObject(nameObject);
                        ProjectNumber = GetValueFromObject(numberObject);
                        FileLocation = GetValueFromObject(locationObject);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        /// <summary>
        /// Utility method for parsing FilePath based ProjectInfo regex.
        /// </summary>
        /// <param name="obj">JSON object that contains regex pattern, match, group info.</param>
        /// <returns>Value extracted using Regex or empty string.</returns>
        private string GetValueFromObject(JsonObject obj)
        {
            var result = string.Empty;
            var pattern = obj?["pattern"] != null
                ? obj["pattern"].Deserialize<string>()
                : string.Empty;
            var match = obj?["match"] != null
                ? obj["match"].Deserialize<int>()
                : -1;
            var group = obj?["group"] != null
                ? obj["group"].Deserialize<int>()
                : -1;
            if (string.IsNullOrWhiteSpace(pattern) || group == -1) return result;

            var matches = Regex.Matches(CentralPath, pattern, RegexOptions.IgnoreCase);
            if (matches.Count <= match) return result;

            var m = matches[match];
            result = m.Groups.Count > group ? m.Groups[group].Value : string.Empty;

            return result;
        }

        /// <summary>
        /// Validates a file path, making sure that its either a revit server file, cloud model or hok network location.
        /// </summary>
        /// <param name="centralPath">Central Path to validate.</param>
        /// <returns>True if file path is valid, otherwise false.</returns>
        public static bool IsValidFilePath(string centralPath)
        {
            var result = false;
            bool isRevitServer = centralPath.StartsWith("rsn://", StringComparison.OrdinalIgnoreCase);
            if (isRevitServer)
            {
                result = true;
            }
            else if (AppSettings.Instance.LocalPathRgx.Any(x => Regex.IsMatch(centralPath, x, RegexOptions.IgnoreCase)))
            {
                result = true;
            }
            else if (!isRevitServer && centralPath.Contains("://"))
            {
                result = true;
            }

            return result;
        }

        #endregion
    }
}
