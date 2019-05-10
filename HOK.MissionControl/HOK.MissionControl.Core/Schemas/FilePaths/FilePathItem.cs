#region References

using System;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Utils;

#endregion

namespace HOK.MissionControl.Core.Schemas.FilePaths
{
    public class FilePathItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("centralPath")]
        public string CentralPath { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("isDisabled")]
        public bool IsDisabled { get; set; }

        [JsonProperty("revitVersion")]
        public string RevitVersion { get; set; } = string.Empty;

        [JsonProperty("projectNumber")]
        public string ProjectNumber { get; set; } = "00.00000.00";

        [JsonProperty("projectName")]
        public string ProjectName { get; set; } = string.Empty;

        [JsonProperty("fileLocation")]
        public string FileLocation { get; set; } = string.Empty;

        [JsonConstructor]
        public FilePathItem()
        {
        }

        public FilePathItem(string centralPath, string version)
        {
            CentralPath = centralPath.ToLower();
            RevitVersion = version;

            GetProjectNumberName(centralPath);
            GetProjectLocation(centralPath);
        }

        #region Utilities

        /// <summary>
        /// Retrieves project location from the file path. 
        /// </summary>
        /// <param name="centralPath">Central Path to parse.</param>
        private void GetProjectLocation(string centralPath)
        {
            if (string.IsNullOrEmpty(centralPath)) return;

            //TODO: Replace all of the below Regex patters with patterns retrieved from settings.
            try
            {
                if (centralPath.StartsWith("rsn://", StringComparison.OrdinalIgnoreCase))
                {
                    const string regexPattern = @"(?<=\/\/)\w{2,3}";
                    var regServer = new Regex(regexPattern, RegexOptions.IgnoreCase);
                    var regMatch = regServer.Match(centralPath);
                    if (regMatch.Success)
                    {
                        FileLocation = regMatch.Value.Trim();
                    }
                }
                else if (AppSettings.Instance.LocalPathRgx.Any(x => Regex.IsMatch(centralPath, x, RegexOptions.IgnoreCase)))
                {
                    const string regexPattern = @"^\\\\group\\hok\\(.+?(?=\\))|^\\\\(.{2,3})-\d{2}svr(\.group\.hok\.com)?\\";
                    var regServer = new Regex(regexPattern, RegexOptions.IgnoreCase);
                    var regMatch = regServer.Match(centralPath);
                    if (regMatch.Success)
                    {
                        FileLocation = string.IsNullOrEmpty(regMatch.Groups[1].Value)
                            ? regMatch.Groups[2].Value.Trim()
                            : regMatch.Groups[1].Value.Trim();
                    }
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Retrieves Project Number and Project Name from the file path.
        /// </summary>
        /// <param name="centralPath">Central Path to parse.</param>
        private void GetProjectNumberName(string centralPath)
        {
            if (string.IsNullOrEmpty(centralPath)) return;

            //TODO: Replace all of the below Regex patters with patterns retrieved from settings.
            var regexPattern = string.Empty;
            try
            {
                if (centralPath.StartsWith("rsn://", StringComparison.OrdinalIgnoreCase))
                {
                    regexPattern = @"\/([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\/";
                }
                else if (AppSettings.Instance.LocalPathRgx.Any(x => Regex.IsMatch(centralPath, x, RegexOptions.IgnoreCase)))
                {
                    regexPattern = @"\\([0-9]{2}[\.|\-][0-9]{4,5}[\.|\-][0-9]{2})(.*?)\\";
                }

                if (string.IsNullOrEmpty(regexPattern)) return;

                var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                var match = regex.Match(centralPath);
                if (!match.Success) return;

                ProjectNumber = match.Groups[1].Value.Trim();
                ProjectName = match.Groups[2].Value.Trim();
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Validates a file path, making sure that its either a revit server file, bim 360 or hok network location.
        /// </summary>
        /// <param name="centralPath">Central Path to validate.</param>
        /// <returns>True if file path is valid, otherwise false.</returns>
        public static bool IsValidFilePath(string centralPath)
        {
            var result = false;
            if (centralPath.StartsWith("rsn://", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
            }
            else if (AppSettings.Instance.LocalPathRgx.Any(x => Regex.IsMatch(centralPath, x, RegexOptions.IgnoreCase)))
            {
                result = true;
            }
            else if (centralPath.StartsWith("bim 360://", StringComparison.OrdinalIgnoreCase))
            {
                result = true;
            }

            return result;
        }

        #endregion
    }
}
