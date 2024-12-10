using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        [DataMember(Name = "_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [DataMember(Name = "isNewSheet")]
        [JsonPropertyName("isNewSheet")]
        public bool IsNewSheet { get; set; }

        [DataMember(Name = "sheetId")]
        [JsonPropertyName("sheetId")]
        public string SheetId { get; set; }

        [DataMember(Name = "uniqueId")]
        [JsonPropertyName("uniqueId")]
        public string UniqueId { get; set; }

        [DataMember(Name = "revisionNumber")]
        [JsonPropertyName("revisionNumber")]
        public string RevisionNumber { get; set; }

        [DataMember(Name = "assignedTo")]
        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        [DataMember(Name = "submittedOn")]
        [JsonPropertyName("submittedOn")]
        public DateTime? SubmittedOn { get; set; }

        [DataMember(Name = "completedOn")]
        [JsonPropertyName("completedOn")]
        public DateTime? CompletedOn { get; set; }

        [DataMember(Name = "submittedBy")]
        [JsonPropertyName("submittedBy")]
        public string SubmittedBy { get; set; }

        [DataMember(Name = "completedBy")]
        [JsonPropertyName("completedBy")]
        public string CompletedBy { get; set; }

        [DataMember(Name = "collectionId")]
        [JsonPropertyName("collectionId")]
        public string CollectionId { get; set; }

        [DataMember(Name = "centralPath")]
        [JsonPropertyName("centralPath")]
        public string CentralPath { get; set; }

        private string _name;
        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("Name"); }
        }

        private string _number;
        [DataMember(Name = "number")]
        [JsonPropertyName("number")]
        public string Number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged("Number"); }
        }

        private bool _isSelected;
        [DataMember(Name = "isSelected")]
        [JsonPropertyName("isSelected")]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("IsSelected"); }
        }

        private bool _isPlaceholder;
        [DataMember(Name = "isPlaceholder")]
        [JsonPropertyName("isPlaceholder")]
        public bool IsPlaceholder
        {
            get { return _isPlaceholder; }
            set { _isPlaceholder = value; RaisePropertyChanged("IsPlaceholder"); }
        }

        private bool _isDeleted;
        [DataMember(Name = "isDeleted")]
        [JsonPropertyName("isDeleted")]
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; RaisePropertyChanged("IsDeleted"); }
        }

        private string _message;
        [DataMember(Name = "message")]
        [JsonPropertyName("message")]
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged("Message"); }
        }

        private string _comments;
        [DataMember(Name = "comments")]
        [JsonPropertyName("comments")]
        public string Comments
        {
            get { return _comments; }
            set { _comments = value; RaisePropertyChanged("Comments"); }
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
            UniqueId = other.UniqueId;
            RevisionNumber = other.RevisionNumber;
            AssignedTo = other.AssignedTo;
            SubmittedOn = other.SubmittedOn;
            CompletedOn = other.CompletedOn;
            SubmittedBy = other.SubmittedBy;
            CompletedBy = other.CompletedBy;
            Name = other.Name;
            Number = other.Number;
            IsSelected = other.IsSelected;
            IsPlaceholder = other.IsSelected;
            IsDeleted = other.IsDeleted;
            Message = other.Message;
            Comments = other.Message;
            CentralPath = other.CentralPath;
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
