using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using HOK.MissionControl.Utils;
using Newtonsoft.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas
{
    public class StylesStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.UtcNow;
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
        public DateTime createdOn { get; set; } = DateTime.UtcNow;
        public bool isOverriden { get; set; }
        public double? value { get; set; }
        public string valueString { get; set; } //not set in constructor
        public string valueOverride { get; set; }
        public bool isLocked { get; set; }
        public int ownerViewId { get; set; } //not set in constructor
        public string ownerViewType { get; set; } //not set in constructor

        [JsonConstructor]
        public DimensionSegmentInfo()
        {
        }

        public DimensionSegmentInfo(DimensionSegment dim)
        {
            isOverriden = !string.IsNullOrEmpty(dim.ValueOverride);
            value = dim.Value;
            valueOverride = dim.ValueOverride;
            isLocked = dim.IsLocked;
        }

        public DimensionSegmentInfo(Dimension dim)
        {
            isOverriden = !string.IsNullOrEmpty(dim.ValueOverride);
            value = dim.Value;
            valueOverride = dim.ValueOverride;
            isLocked = dim.IsLocked;
        }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Log.AppendLog(LogMessageType.EXCEPTION, errorContext.Error.Message);
            errorContext.Handled = true;
        }
    }

    public class DimensionTypeInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.UtcNow;
        public string name { get; set; }
        public int instances { get; set; }
        public bool usesProjectUnits { get; set; }
        public bool bold { get; set; }
        public List<int> color { get; set; }
        public bool italic { get; set; }
        public string leaderType { get; set; }
        public int lineWeight { get; set; }
        public string textFont { get; set; }
        public double textSize { get; set; }
        public string textSizeString { get; set; }
        public bool underline { get; set; }
        public string styleType { get; set; }

        [JsonConstructor]
        public DimensionTypeInfo()
        {
        }

        public DimensionTypeInfo(DimensionType dt)
        {
            name = dt.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString();
#if RELEASE2016 || RELEASE2015
            // (Konrad) Revit 2016 API doesn't have that info exposed.
            usesProjectUnits = false;
#else
            usesProjectUnits = dt.GetUnitsFormatOptions().UseDefault;
#endif
            bold = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_BOLD)));
            color = ElementUtilities.RevitColorIntegerToRGBA(dt.get_Parameter(BuiltInParameter.LINE_COLOR).AsInteger());
            italic = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC)));
            leaderType = dt.get_Parameter(BuiltInParameter.DIM_LEADER_TYPE).AsValueString();
            lineWeight = dt.get_Parameter(BuiltInParameter.LINE_PEN).AsInteger();
            textFont = dt.get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            textSize = dt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            textSizeString = dt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
            underline = ElementUtilities.RevitBoolToBool(
                (int)ElementUtilities.GetParameterValue(dt.get_Parameter(BuiltInParameter.TEXT_STYLE_UNDERLINE)));
            styleType = dt.StyleType.ToString();
        }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            Log.AppendLog(LogMessageType.EXCEPTION, errorContext.Error.Message);
            errorContext.Handled = true;
        }
    }

    public class TextNoteTypeInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        public string Id { get; set; }
        public DateTime createdOn { get; set; } = DateTime.UtcNow;
        public string name { get; set; }
        public int instances { get; set; }
        public bool bold { get; set; }
        public List<int> color { get; set; }
        public bool italic { get; set; }
        public string leaderArrowhead { get; set; }
        public int lineWeight { get; set; }
        public string textFont { get; set; }
        public double textSize { get; set; }
        public string textSizeString { get; set; }
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
            textSize = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            textSizeString = tnt.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
            underline = ElementUtilities.RevitBoolToBool(
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
