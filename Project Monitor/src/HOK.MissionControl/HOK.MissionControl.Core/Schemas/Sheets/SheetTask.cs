using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace HOK.MissionControl.Core.Schemas.Sheets
{
    /// <summary>
    /// Sheets Tasks schema
    /// </summary>
    public class SheetTask : INotifyPropertyChanged
    {
        public string _id { get; set; }
        public string identifier { get; set; }
        public string uniqueId { get; set; }
        public string revisionNumber { get; set; }
        public string assignedTo { get; set; }
        public DateTime? submittedOn { get; set; }
        public DateTime? completedOn { get; set; }
        public string submittedBy { get; set; }
        public string completedBy { get; set; }

        public string centralPath { get; set; }
        public string collectionId { get; set; }
        public string fileName { get; set; }

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged("name"); }
        }

        private string _number;
        public string number
        {
            get { return _number; }
            set { _number = value; RaisePropertyChanged("number"); }
        }

        private bool _isSelected;
        public bool isSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; RaisePropertyChanged("isSelected"); }
        }

        private bool _isPlaceholder;
        public bool isPlaceholder
        {
            get { return _isPlaceholder; }
            set { _isPlaceholder = value; RaisePropertyChanged("isPlaceholder"); }
        }

        private bool _isDeleted;
        public bool isDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; RaisePropertyChanged("isDeleted"); }
        }

        private string _message;
        public string message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged("message"); }
        }

        private string _comments;
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
            _id = other._id;
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
            return item != null && _id.Equals(item._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
