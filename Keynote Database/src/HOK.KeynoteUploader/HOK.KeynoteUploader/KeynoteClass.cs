using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteUploader
{
    public class KeynoteInfo : INotifyPropertyChanged
    {
        private string idValue = "";
        private string keyValue = "";
        private string parentKeyValue = "";
        private string descriptionValue = "";
        private string keynoteSetIdValue = "";

        public string _id { get { return idValue; } set { idValue = value; NotifyPropertyChanged("_id"); } }
        public string key { get { return keyValue; } set { keyValue = value; NotifyPropertyChanged("key"); } }
        public string parentKey { get { return parentKeyValue; } set { parentKeyValue = value; NotifyPropertyChanged("parentKey"); } }
        public string description { get { return descriptionValue; } set { descriptionValue = value; NotifyPropertyChanged("description"); } }
        public string keynoteSet_id { get { return keynoteSetIdValue; } set { keynoteSetIdValue = value; NotifyPropertyChanged("keynoteSet_id"); } }

        public KeynoteInfo()
        {
        }

        public KeynoteInfo(string idStr, string keyStr, string parentKeyStr, string descriptionStr, string keynoteSetIdStr)
        {
            _id = idStr;
            key = keyStr;
            parentKey = parentKeyStr;
            description = descriptionStr;
            keynoteSet_id = keynoteSetIdStr;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class KeynoteProjectInfo
    {
        public string _id { get; set; }
        public string projectNumber { get; set; }
        public string projectName { get; set; }
        public string office { get; set; }
        public string keynoteSet_id { get; set; }

        public KeynoteProjectInfo()
        {

        }

        public KeynoteProjectInfo(string idVal, string pNumber, string pName, string officeName, string keynoteSetId)
        {
            _id = idVal;
            projectNumber = pNumber;
            projectName = pName;
            office = officeName;
            keynoteSet_id = keynoteSetId;
        }
    }

    public class KeynoteSetInfo
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string createdBy { get; set; }
        public DateTime dateModified { get; set; }
        public string modifiedBy { get; set; }

        public KeynoteSetInfo()
        {
        }

        public KeynoteSetInfo(string idVal, string setName, string createdByStr, DateTime modifiedDate, string modifiedByStr)
        {
            _id = idVal;
            name = setName;
            createdBy = createdByStr;
            dateModified = modifiedDate;
            modifiedBy = modifiedByStr;

        }
    }
}
