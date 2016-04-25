using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.Keynote.ClassModels
{
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
