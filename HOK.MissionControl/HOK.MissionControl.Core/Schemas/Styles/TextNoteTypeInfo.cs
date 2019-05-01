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
    public class TextNoteTypeInfo
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

        [JsonProperty("bold")]
        public bool Bold { get; set; }

        [JsonProperty("color")]
        public List<int> Color { get; set; }

        [JsonProperty("italic")]
        public bool Italic { get; set; }

        [JsonProperty("leaderArrowhead")]
        public string LeaderArrowhead { get; set; }

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

        [JsonConstructor]
        public TextNoteTypeInfo()
        {
        }

        public TextNoteTypeInfo(Element tnt)
        {
            Name = tnt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
            Bold = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(tnt.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD)));
            Color = ElementUtilities.RevitColorIntegerToRGBA(tnt.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger());
            Italic = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(tnt.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC)));
            LeaderArrowhead = tnt.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD).AsValueString();
            LineWeight = tnt.get_Parameter(BuiltInParameter.LINE_PEN).AsInteger();
            TextFont = tnt.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            TextSize = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            TextSizeString = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
            Underline = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(tnt.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE)));
        }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Log.AppendLog(LogMessageType.EXCEPTION, errorContext.Error.Message);
            errorContext.Handled = true;
        }
    }
}
