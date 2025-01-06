using System.Runtime.Serialization;
using HOK.Core.Utilities;
using HOK.MissionControl.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Styles
{
    /// <summary>
    /// 
    /// </summary>
    public class TextNoteTypeInfo
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

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        [JsonPropertyName("color")]
        public List<int> Color { get; set; }

        [JsonPropertyName("italic")]
        public bool Italic { get; set; }

        [JsonPropertyName("leaderArrowhead")]
        public string LeaderArrowhead { get; set; }

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
    }
}
