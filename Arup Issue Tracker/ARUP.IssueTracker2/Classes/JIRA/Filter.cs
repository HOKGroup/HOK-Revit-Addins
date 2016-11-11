using System.Collections.Generic;

namespace ARUP.IssueTracker.Classes
{
    public class Filter
    {
        public string self { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public Owner owner { get; set; }
        public string jql { get; set; }
        public string viewUrl { get; set; }
        public string searchUrl { get; set; }
        public bool favourite { get; set; }
        public List<object> sharePermissions { get; set; }
    }
}
