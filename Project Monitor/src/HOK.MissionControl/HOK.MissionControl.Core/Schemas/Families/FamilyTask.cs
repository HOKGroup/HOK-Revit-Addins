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
        [DataMember(Name = "name")]
        public string name {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("name"); }
        }

        private string _assignedTo;
        [DataMember(Name = "assignedTo")]
        public string assignedTo
        {
            get { return _assignedTo; }
            set { _assignedTo = value; RaisePropertyChanged("assignedTo"); }
        }

        private string _message;
        [DataMember(Name = "message")]
        public string message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged("message"); }
        }

        private string _comments;
        [DataMember(Name = "comments")]
        public string comments
        {
            get { return _comments; }
            set { _comments = value; RaisePropertyChanged("comments"); }
        }

        private string _submittedBy;
        [DataMember(Name = "submittedBy")]
        public string submittedBy
        {
            get { return _submittedBy; }
            set { _submittedBy = value; RaisePropertyChanged("submittedBy"); }
        }

        private string _completedBy;
        [DataMember(Name = "completedBy")]
        public string completedBy
        {
            get { return _completedBy; }
            set { _completedBy = value; RaisePropertyChanged("completedBy"); }
        }

        [DataMember(Name = "submittedOn")]
        public DateTime? submittedOn { get; set; }

        [DataMember(Name = "completedOn")]
        public DateTime? completedOn { get; set; }

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
