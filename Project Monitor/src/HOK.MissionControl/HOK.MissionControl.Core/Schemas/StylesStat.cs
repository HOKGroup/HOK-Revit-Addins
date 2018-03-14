using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using HOK.MissionControl.Utils;

namespace HOK.MissionControl.Core.Schemas
{
    public class StylesStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
        public string user { get; set; }
        public List<TextNoteTypeInfo> textStats { get; set; } = new List<TextNoteTypeInfo>();
        public List<DimensionTypeInfo> dimStats { get; set; } = new List<DimensionTypeInfo>();
        public List<DimensionSegmentInfo> dimSegmentStats { get; set; } = new List<DimensionSegmentInfo>();
    }

    public class DimensionSegmentInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
        public bool isOverriden { get; set; }
        public string value { get; set; }
        public string valueOverride { get; set; }
        public bool isLocked { get; set; }

        [JsonConstructor]
        public DimensionSegmentInfo()
        {
        }

        public DimensionSegmentInfo(DimensionSegment dim)
        {
            isOverriden = !string.IsNullOrEmpty(dim.ValueOverride);
            value = dim.ValueString;
            valueOverride = dim.ValueOverride;
            isLocked = dim.IsLocked;
        }
    }

    public class DimensionTypeInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
        public string name { get; set; }
        public int instances { get; set; }
        public bool usesProjectUnits { get; set; }
        public bool bold { get; set; }
        public List<int> color { get; set; }
        public bool italic { get; set; }
        public string leaderType { get; set; }
        public int lineWeight { get; set; }
        public string textFont { get; set; }
        public string textSize { get; set; }
        public bool underline { get; set; }

        [JsonConstructor]
        public DimensionTypeInfo()
        {
        }

        public DimensionTypeInfo(DimensionType dt)
        {
            name = dt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
            usesProjectUnits = dt.GetUnitsFormatOptions().UseDefault;
            bold = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD)));
            color = ElementUtilities.RevitColorIntegerToRGBA(dt.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger());
            italic = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC)));
            leaderType = dt.get_Parameter(BuiltInParameter.DIM_LEADER_TYPE).AsValueString();
            lineWeight = dt.get_Parameter(BuiltInParameter.LINE_PEN).AsInteger();
            textFont = dt.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            textSize = dt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsString();
            underline = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE)));
        }
    }

    public class TextNoteTypeInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.Now;
        public string name { get; set; }
        public int instances { get; set; }
        public bool bold { get; set; }
        public List<int> color { get; set; }
        public bool italic { get; set; }
        public string leaderArrowhead { get; set; }
        public int lineWeight { get; set; }
        public string textFont { get; set; }
        public string textSize { get; set; }
        public bool underline { get; set; }

        [JsonConstructor]
        public TextNoteTypeInfo()
        {
        }

        public TextNoteTypeInfo(Element tnt)
        {
            name = tnt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
            bold = ElementUtilities.RevitBoolToBool(
                (int) ElementUtilities.GetParameterValue(tnt.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD)));
            color = ElementUtilities.RevitColorIntegerToRGBA(tnt.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger());
            italic = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(tnt.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC)));
            leaderArrowhead = tnt.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD).AsValueString();
            lineWeight = tnt.get_Parameter(BuiltInParameter.LINE_PEN).AsInteger();
            textFont = tnt.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            textSize = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsString();
            underline = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(tnt.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE)));
        }
    }
}
