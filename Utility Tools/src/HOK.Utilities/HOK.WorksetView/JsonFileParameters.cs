using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.WorksetView
{
    public class JsonFileParameters
    {
        public string Application { get; set; }
        public string FileType { get; set; }
        public string Date { get; set; }
        public string SchemaVersion { get; set; }
        public string FileVersion { get; set; }
        public string UniuqeID { get; set; }
        public string Author { get; set; }
        public string ViewPrefix { get; set; }
        public string ViewSuffix { get; set; }
        public data data { get; set; }

    }
    public class data
    {
        public List<views> views { get; set; }
    }
    public class views
    {
        public string viewname { get; set; }
        public List<string> visible3Dcateogries { get; set; }
        public List<string> visible2Dcateogries { get; set; }
    }
}
