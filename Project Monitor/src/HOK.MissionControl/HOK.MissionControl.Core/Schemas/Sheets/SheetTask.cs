using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Sheet Task schema.
    /// </summary>
    [DataContract]
    public class SheetTask : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonProperty("_id")]
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember]
        public bool isNewSheet { get; set; }

        [DataMember]
        public string sheetId { get; set; }

        [DataMember]
        public string identifier { get; set; }

        [DataMember]
        public string uniqueId { get; set; }

        [DataMember]
        public string revisionNumber { get; set; }

        [DataMember]
        public string assignedTo { get; set; }

        [DataMember]
        public DateTime? submittedOn { get; set; }

        [DataMember]
        public DateTime? completedOn { get; set; }

        [DataMember]
        public string submittedBy { get; set; }

        [DataMember]
        public string completedBy { get; set; }

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

        private bool _isSelected;
        [DataMember]
        public bool isSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("isSelected"); }
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

        private string _message;
        [DataMember]
        public string message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged("message"); }
        }

        private string _comments;
        [DataMember]
        public string comments
        {
            get { return _comments; }
            set { _comments = value; RaisePropertyChanged("comments"); }
        }

        [JsonConstructor]
        public SheetTask()
        {
        }

        /// <summary>
        /// Utility method used to copy all properties from one object to another, and trigger PropertyChange for UI updates.
        /// </summary>
        /// <param name="other">Object to copy properties from.</param>
        public void CopyProperties(SheetTask other)
        {
            Id = other.Id;
            identifier = other.identifier;
            uniqueId = other.uniqueId;
            revisionNumber = other.revisionNumber;
            assignedTo = other.assignedTo;
            submittedOn = other.submittedOn;
            completedOn = other.completedOn;
            submittedBy = other.submittedBy;
            completedBy = other.completedBy;
            name = other.name;
            number = other.number;
            isSelected = other.isSelected;
            isPlaceholder = other.isSelected;
            isDeleted = other.isDeleted;
            message = other.message;
            comments = other.message;
        }

        public override bool Equals(object obj)
        {
            var item = obj as SheetTask;
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
