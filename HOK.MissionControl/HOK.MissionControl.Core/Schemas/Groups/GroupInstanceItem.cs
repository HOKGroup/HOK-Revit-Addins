﻿using System;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.MissionControl.Core.Schemas.Groups
{
    /// <summary>
    /// Wrapper class for individual Group objects.
    /// </summary>
    public class GroupInstanceItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("ownerViewId")]
        public long OwnerViewId { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonConstructor]
        public GroupInstanceItem()
        {
        }

        public GroupInstanceItem(Element group)
        {
            var doc = group.Document;
            var created = VerifyUsername(WorksharingUtils.GetWorksharingTooltipInfo(doc, group.Id).LastChangedBy);
            var levelId = group.LevelId == null 
                ? ElementId.InvalidElementId 
                : group.LevelId;
            var levelName = string.Empty;
            if (levelId != ElementId.InvalidElementId) levelName = doc.GetElement(levelId).Name;
            CreatedBy = created;
            Level = levelName;
            OwnerViewId = group.OwnerViewId == null 
                ? -1 
                : GetElementIdValue(group.OwnerViewId);
        }

        /// <summary>
        /// Removes email address from username.
        /// </summary>
        /// <param name="name">Username in current Revit session.</param>
        /// <returns></returns>
        private static string VerifyUsername(string name)
        {
            var isEmail = name.IndexOf("@", StringComparison.Ordinal) != -1;
            return isEmail 
                ? name.Substring(0, name.IndexOf("@", StringComparison.Ordinal)).ToLower() 
                : name.ToLower();
        }
    }
}
