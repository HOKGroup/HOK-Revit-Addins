using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.Keynote.ClassModels
{
    public class KeynoteInfo
    {
        public string _id { get; set; }
        public string key { get; set; }
        public string parentKey { get; set; }
        public string description { get; set; }
        public string keynoteSet_id { get; set; }

        public KeynoteInfo()
        {
        }

        public KeynoteInfo(string idVal, string keyVal, string parentKeyVal, string descriptionVal, string keynoteSetId)
        {
            _id = idVal;
            key = keyVal;
            parentKey = parentKeyVal;
            description = descriptionVal;
            keynoteSet_id = keynoteSetId;
        }
    }
}
