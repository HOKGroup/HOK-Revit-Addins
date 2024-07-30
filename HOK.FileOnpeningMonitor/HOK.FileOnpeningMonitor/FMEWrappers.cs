using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.FileOnpeningMonitor
{
    public class Transformation
    {
        public List<PublishedParameter> publishedParameters { get; set; }
        public TmDirectives tmDirectives { get; set; }
        public List<NmDirective> nmDirectives { get; set; }
    } 

    public class PublishedParameter
    {
        public string name;
        public string value;
    }

    public class TmDirectives
    {
        public bool rtc { get; set; }
        public int ttc { get; set; }
        public int ttl { get; set; }
        public string tag { get; set; }
    }

    public class NmDirective
    {
        public List<object> directives { get; set; }
        public List<string> successTopics { get; set; }
        public List<string> failureTopics { get; set; }
    }
}
