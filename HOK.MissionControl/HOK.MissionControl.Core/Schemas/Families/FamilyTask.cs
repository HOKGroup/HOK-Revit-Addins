using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HOK.MissionControl.Core.Schemas.Families
{
    /// <summary>
    /// Task object added to Family.
    /// </summary>
    [DataContract]
    public class FamilyTask : INotifyPropertyChanged
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(Name = "_id")]
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        private string _name;
        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        public string Name {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("Name"); }
        }

        private string _assignedTo;
        [DataMember(Name = "assignedTo")]
        [JsonPropertyName("assignedTo")]
        public string AssignedTo
        {
            get { return _assignedTo; }
            set { _assignedTo = value; RaisePropertyChanged("AssignedTo"); }
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

        private string _submittedBy;
        [DataMember(Name = "submittedBy")]
        [JsonPropertyName("submittedBy")]
        public string SubmittedBy
        {
            get { return _submittedBy; }
            set { _submittedBy = value; RaisePropertyChanged("SubmittedBy"); }
        }

        private string _completedBy;
        [DataMember(Name = "completedBy")]
        [JsonPropertyName("completedBy")]
        public string CompletedBy
        {
            get { return _completedBy; }
            set { _completedBy = value; RaisePropertyChanged("CompletedBy"); }
        }

        [DataMember(Name = "submittedOn")]
        [JsonPropertyName("submittedOn")]
        public DateTime? SubmittedOn { get; set; }

        [DataMember(Name = "completedOn")]
        [JsonPropertyName("completedOn")]
        public DateTime? CompletedOn { get; set; }

        /// <summary>
        /// Utility method used to copy all properties from one object to another, and trigger PropertyChange for UI updates.
        /// </summary>
        /// <param name="other">Object to copy properties from.</param>
        public void CopyProperties(FamilyTask other)
        {
            Id = other.Id;
            AssignedTo = other.AssignedTo;
            SubmittedOn = other.SubmittedOn;
            CompletedOn = other.CompletedOn;
            SubmittedBy = other.SubmittedBy;
            CompletedBy = other.CompletedBy;
            Name = other.Name;
            Message = other.Message;
            Comments = other.Comments;
        }

        // (Konrad) Comparison methods used when updating UI. IndexOf() uses it.
        // (Konrad) We can compare Tasks by their MongoDB id
        public override bool Equals(object obj)
        {
            var item = obj as FamilyTask;
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
