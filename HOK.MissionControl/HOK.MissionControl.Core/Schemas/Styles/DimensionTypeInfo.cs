using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class DimensionTypeInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("instances")]
        public int Instances { get; set; }

        [JsonPropertyName("usesProjectUnits")]
        public bool UsesProjectUnits { get; set; }

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        [JsonPropertyName("color")]
        public List<int> Color { get; set; }

        [JsonPropertyName("italic")]
        public bool Italic { get; set; }

        [JsonPropertyName("leaderType")]
        public string LeaderType { get; set; }

        [JsonPropertyName("lineWeight")]
        public int LineWeight { get; set; }

        [JsonPropertyName("textFont")]
        public string TextFont { get; set; }

        [JsonPropertyName("textSize")]
        public double TextSize { get; set; }

        [JsonPropertyName("textSizeString")]
        public string TextSizeString { get; set; }

        [JsonPropertyName("underline")]
        public bool Underline { get; set; }

        [JsonPropertyName("styleType")]
        public string StyleType { get; set; }

        [JsonConstructor]
        public DimensionTypeInfo()
        {
        }

        public DimensionTypeInfo(DimensionType dt)
        {
            Name = dt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
            UsesProjectUnits = dt.GetUnitsFormatOptions().UseDefault;
            Bold = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD)));
            Color = ElementUtilities.RevitColorIntegerToRGBA(dt.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger());
            Italic = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC)));
            LeaderType = dt.get_Parameter(BuiltInParameter.DIM_LEADER_TYPE).AsValueString();
            LineWeight = dt.get_Parameter(BuiltInParameter.LINE_PEN).AsInteger();
            TextFont = dt.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            TextSize = dt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            TextSizeString = dt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
            Underline = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE)));
            StyleType = dt.StyleType.ToString();
        }
    }
}
