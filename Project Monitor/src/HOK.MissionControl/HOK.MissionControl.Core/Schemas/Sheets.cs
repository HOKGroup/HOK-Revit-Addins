using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas
{
    public class RevisionItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "description")]
        public string description { get; set; }

        [DataMember(Name = "sequence")]
        public int sequence { get; set; }

        [DataMember(Name = "number")]
        public string number { get; set; }

        [DataMember(Name = "date")]
        public string date { get; set; }

        [DataMember(Name = "issuedTo")]
        public string issuedTo { get; set; }

        [DataMember(Name = "issuedBy")]
        public string issuedBy { get; set; }

        [DataMember(Name = "uniqueId")]
        public string uniqueId { get; set; }

        public RevisionItem()
        {
        }

        public RevisionItem(Element rev)
        {
            description = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION).AsString();
            sequence = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM).AsInteger();
            number = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_NUM).AsString();
            date = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE).AsString();
            issuedTo = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_TO).AsString();
            issuedBy = rev.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED_BY).AsString();
            uniqueId = rev.UniqueId;
        }
    }

    public class SheetTask: INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        //private string _name;
        //public string name {
        //    get { return _name; }
        //    set { _name = value; RaisePropertyChanged("name"); }
        //}

        public string name { get; set; }
        public string number { get; set; }
        public string revisionNumber { get; set; }
        public string uniqueId { get; set; }
        public bool isSelected { get; set; }
        public string identifier { get; set; }

        public string assignedTo { get; set; }
        public string message { get; set; }
        public string comments { get; set; }
        public string submittedBy { get; set; }
        public string completedBy { get; set; }
        public DateTime? submittedOn { get; set; }
        public DateTime? completedOn { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as SheetTask;
            return item != null && identifier.Equals(item.identifier);
        }

        public override int GetHashCode()
        {
            return identifier.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }

    public class SheetItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "number")]
        public string number { get; set; }

        [DataMember(Name = "revisionNumber")]
        public string revisionNumber { get; set; }

        [DataMember(Name = "uniqueId")]
        public string uniqueId { get; set; }

        [DataMember(Name = "isSelected")]
        public bool isSelected { get; set; }

        [DataMember(Name = "identifier")]
        public string identifier { get; set; }

        [JsonConstructor]
        public SheetItem()
        {
        }

        public SheetItem(Element sheet, string centralPath)
        {
            name = sheet.get_Parameter(BuiltInParameter.SHEET_NAME).AsString();
            number = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
            revisionNumber = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            uniqueId = sheet.UniqueId;
            identifier = centralPath.ToLower() + "-" + sheet.UniqueId;
        }
    }

    public class SheetsData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "centralPath")]
        public string centralPath { get; set; }

        [DataMember(Name = "sheets")]
        public List<SheetItem> sheets { get; set; } = new List<SheetItem>();

        [DataMember(Name = "sheetsChanges")]
        public List<SheetTask> sheetsChanges { get; set; } = new List<SheetTask>();

        [DataMember(Name = "revisions")]
        public List<RevisionItem> revisions { get; set; } = new List<RevisionItem>();
    }
}
