using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class DimensionTypeInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("instances")]
        public int Instances { get; set; }

        [JsonProperty("usesProjectUnits")]
        public bool UsesProjectUnits { get; set; }

        [JsonProperty("bold")]
        public bool Bold { get; set; }

        [JsonProperty("color")]
        public List<int> Color { get; set; }

        [JsonProperty("italic")]
        public bool Italic { get; set; }

        [JsonProperty("leaderType")]
        public string LeaderType { get; set; }

        [JsonProperty("lineWeight")]
        public int LineWeight { get; set; }

        [JsonProperty("textFont")]
        public string TextFont { get; set; }

        [JsonProperty("textSize")]
        public double TextSize { get; set; }

        [JsonProperty("textSizeString")]
        public string TextSizeString { get; set; }

        [JsonProperty("underline")]
        public bool Underline { get; set; }

        [JsonProperty("styleType")]
        public string StyleType { get; set; }

        [JsonConstructor]
        public DimensionTypeInfo()
        {
        }

        public DimensionTypeInfo(DimensionType dt)
        {
            Name = dt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
#if RELEASE2016 || RELEASE2015
// (Konrad) Revit 2016 API doesn't have that info exposed.
            UsesProjectUnits = false;
#else
            UsesProjectUnits = dt.GetUnitsFormatOptions().UseDefault;
#endif
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

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Log.AppendLog(LogMessageType.EXCEPTION, errorContext.Error.Message);
            errorContext.Handled = true;
        }
    }
}
