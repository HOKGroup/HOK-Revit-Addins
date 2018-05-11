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
        [DataMember(Name = "_id")]
        [JsonProperty("_id")]
        public string Id { get; set; }

        [DataMember(Name = "isNewSheet")]
        [JsonProperty("isNewSheet")]
        public bool IsNewSheet { get; set; }

        [DataMember(Name = "sheetId")]
        [JsonProperty("sheetId")]
        public string SheetId { get; set; }

        [DataMember(Name = "uniqueId")]
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

        [DataMember(Name = "revisionNumber")]
        [JsonProperty("revisionNumber")]
        public string RevisionNumber { get; set; }

        [DataMember(Name = "assignedTo")]
        [JsonProperty("assignedTo")]
        public string AssignedTo { get; set; }

        [DataMember(Name = "submittedOn")]
        [JsonProperty("submittedOn")]
        public DateTime? SubmittedOn { get; set; }

        [DataMember(Name = "completedOn")]
        [JsonProperty("completedOn")]
        public DateTime? CompletedOn { get; set; }

        [DataMember(Name = "submittedBy")]
        [JsonProperty("submittedBy")]
        public string SubmittedBy { get; set; }

        [DataMember(Name = "completedBy")]
        [JsonProperty("completedBy")]
        public string CompletedBy { get; set; }

        [DataMember(Name = "collectionId")]
        [JsonProperty("collectionId")]
        public string CollectionId { get; set; }

        [DataMember(Name = "fileName")]
        [JsonProperty("fileName")]
        public string FileName { get; set; }

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

        private string _message;
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged("Message"); }
        }

        private string _comments;
        [DataMember(Name = "comments")]
        [JsonProperty("comments")]
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
