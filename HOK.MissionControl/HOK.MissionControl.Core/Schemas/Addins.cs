using System;
using System.Collections.Generic;
using System.Linq;
using HOK.Core.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas
{
    public class InfoItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class AddinLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("pluginName")]
        public string PluginName { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("revitVersion")]
        public string RevitVersion { get; set; }

        [JsonProperty("office")]
        public string Office { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty("detailInfo")]
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
                var machineName = Environment.MachineName;
                var splits = machineName.Split('-');
                if (!splits.Any()) return string.Empty;

                var s = splits.FirstOrDefault();
                if (s != null)
                {
                    var office = s.ToUpper();
                    return office;
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                return string.Empty;
            }
        }
    }
}
