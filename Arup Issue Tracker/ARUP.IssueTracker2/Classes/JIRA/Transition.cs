using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARUP.IssueTracker.Classes
{
    public class Transition
    {
        public string id { get; set; }
        public string name { get; set; }
        public To to { get; set; }
    }

    public class To
    {
        public string self { get; set; }
        public string description { get; set; }
        public string iconUrl { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }
}
