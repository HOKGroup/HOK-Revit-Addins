using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Autodesk.Revit.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Sheet schema.
    /// </summary>
    [DataContract]
    public class SheetItem : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "collectionId")]
        [JsonProperty("collectionId")]
        public string CollectionId { get; set; }

        [DataMember(Name = "fileName")]
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [DataMember(Name = "isNewSheet")]
        [JsonProperty("isNewSheet")]
        public bool IsNewSheet { get; set; }

        private string _name;
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("Name"); }
        }

        private string _number;
        [DataMember(Name = "number")]
        [JsonProperty("number")]
        public string Number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged("Number"); }
        }

        private string _uniqueId;
        [DataMember(Name = "uniqueId")]
        [JsonProperty("uniqueId")]
        public string UniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; RaisePropertyChanged("UniqueId"); }
        }

        private string _revisionNumber;
        [DataMember(Name = "revisionNumber")]
        [JsonProperty("revisionNumber")]
        public string RevisionNumber
        {
            get { return _revisionNumber; }
            set { _revisionNumber = value; RaisePropertyChanged("RevisionNumber"); }
        }

        private bool _isSelected;
        [DataMember(Name = "isSelected")]
        [JsonProperty("isSelected")]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        private bool _isPlaceholder;
        [DataMember(Name = "isPlaceholder")]
        [JsonProperty("isPlaceholder")]
        public bool IsPlaceholder
        {
            get { return _isPlaceholder; }
            set { _isPlaceholder = value; RaisePropertyChanged("IsPlaceholder"); }
        }

        private bool _isDeleted;
        [DataMember(Name = "isDeleted")]
        [JsonProperty("isDeleted")]
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; RaisePropertyChanged("IsDeleted"); }
        }

        [DataMember(Name = "tasks")]
        [JsonProperty("tasks")]
        public List<SheetTask> Tasks { get; set; } = new List<SheetTask>();

        [JsonConstructor]
        public SheetItem()
        {
        }

        public SheetItem(ViewSheet sheet, string path)
        {
            Name = sheet.get_Parameter(BuiltInParameter.SHEET_NAME).AsString();
            Number = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
            UniqueId = sheet.UniqueId;
            RevisionNumber = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            IsPlaceholder = sheet.IsPlaceholder;
            FileName = !string.IsNullOrEmpty(path) ? Path.GetFileNameWithoutExtension(path) : string.Empty;
            IsNewSheet = false;
            IsDeleted = false;
            IsSelected = false;
        }

        // (Konrad) Comparison methods used when updating UI. IndexOf() uses it.
        public override bool Equals(object obj)
        {
            var item = obj as SheetItem;
            return item != null && Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
