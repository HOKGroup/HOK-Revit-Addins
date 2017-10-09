using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HOK.MissionControl.Core.Schemas
{
    public class AddinLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string pluginName { get; set; }
        public string user { get; set; }
        public string revitVersion { get; set; }
        public string office { get; set; }
        public DateTime createdOn { get; set; } = new DateTime();

        public AddinLog()
        {
        }

        // TODO: We should NOT be passing a document into the constructor here.
        // TODO: Apps in a "zero doc" state will fail to initialize.
        public AddinLog(string name, Document doc)
        {
            pluginName = name;
            user = Environment.UserName.ToLower();
            revitVersion = doc.Application.VersionNumber;
        }

        public AddinLog(string name, string version)
        {
            pluginName = name;
            user = Environment.UserName.ToLower();
            revitVersion = version;
            office = GetOffice();
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

    public class AddinData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<AddinLog> usageLogs { get; set; } = new List<AddinLog>();
    }
}
