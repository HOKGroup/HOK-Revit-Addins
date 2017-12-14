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
    /// Class used when user submits proposed changes to Sheets.
    /// </summary>
    [DataContract]
    public class SheetItem : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember]
        public string centralPath { get; set; }

        [DataMember]
        public string collectionId { get; set; }

        [DataMember]
        public string fileName { get; set; }

        private string _name;
        [DataMember]
        public string name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("name"); }
        }

        private string _number;
        [DataMember]
        public string number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged("number"); }
        }

        private string _uniqueId;
        [DataMember]
        public string uniqueId
        {
            get { return _uniqueId; }
            set { _uniqueId = value; RaisePropertyChanged("uniqueId"); }
        }

        private string _revisionNumber;
        [DataMember]
        public string revisionNumber
        {
            get { return _revisionNumber; }
            set { _revisionNumber = value; RaisePropertyChanged("revisionNumber"); }
        }

        private bool _isSelected;
        [DataMember]
        public bool isSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("isSelected"); }
        }

        private string _identifier;
        [DataMember]
        public string identifier
        {
            get { return _identifier; }
            set { _identifier = value; RaisePropertyChanged("identifier"); }
        }

        private bool _isPlaceholder;
        [DataMember]
        public bool isPlaceholder
        {
            get { return _isPlaceholder; }
            set { _isPlaceholder = value; RaisePropertyChanged("isPlaceholder"); }
        }

        private bool _isDeleted;
        [DataMember]
        public bool isDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; RaisePropertyChanged("isDeleted"); }
        }

        [DataMember]
        public List<SheetTask> tasks { get; set; } = new List<SheetTask>();

        [JsonConstructor]
        public SheetItem()
        {
        }

        public SheetItem(ViewSheet sheet, string path)
        {
            name = sheet.get_Parameter(BuiltInParameter.SHEET_NAME).AsString();
            number = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
            uniqueId = sheet.UniqueId;
            revisionNumber = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            identifier = path.ToLower() + "-" + sheet.UniqueId; //prevents possibility of sheet being copied between models
            isPlaceholder = sheet.IsPlaceholder;
            centralPath = path;
            fileName = Path.GetFileNameWithoutExtension(path);
        }

        // (Konrad) Comparison methods used when updating UI. IndexOf() uses it.
        public override bool Equals(object obj)
        {
            var item = obj as SheetItem;
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
}
