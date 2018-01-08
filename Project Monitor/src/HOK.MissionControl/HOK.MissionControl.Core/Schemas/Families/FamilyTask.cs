using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

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
        [JsonProperty("_id")]
        public string Id { get; set; }

        private string _name;
        [DataMember]
        public string name {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("name"); }
        }

        private string _assignedTo;
        [DataMember]
        public string assignedTo
        {
            get { return _assignedTo; }
            set { _assignedTo = value; RaisePropertyChanged("assignedTo"); }
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

        private string _submittedBy;
        [DataMember]
        public string submittedBy
        {
            get { return _submittedBy; }
            set { _submittedBy = value; RaisePropertyChanged("submittedBy"); }
        }

        private string _completedBy;
        [DataMember]
        public string completedBy
        {
            get { return _completedBy; }
            set { _completedBy = value; RaisePropertyChanged("completedBy"); }
        }

        [DataMember]
        public DateTime? submittedOn { get; set; }

        [DataMember]
        public DateTime? completedOn { get; set; }

        /// <summary>
        /// Utility method used to copy all properties from one object to another, and trigger PropertyChange for UI updates.
        /// </summary>
        /// <param name="other">Object to copy properties from.</param>
        public void CopyProperties(FamilyTask other)
        {
            Id = other.Id;
            assignedTo = other.assignedTo;
            submittedOn = other.submittedOn;
            completedOn = other.completedOn;
            submittedBy = other.submittedBy;
            completedBy = other.completedBy;
            name = other.name;
            message = other.message;
            comments = other.message;
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
