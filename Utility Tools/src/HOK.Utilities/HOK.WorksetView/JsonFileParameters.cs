using Newtonsoft.Json;
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
        public string UniqueID { get; set; }
        public string Author { get; set; }
        public string ViewPrefix { get; set; }
        public string ViewSuffix { get; set; }
        public Data Data { get; set; }
        
    }
    public class Data
    {
        public List<Views> Views { get; set; }
    }
    public class Views
    {
        public string Viewname { get; set; }
        public List<string> Visible3Dcateogries { get; set; }
        public List<string> Visible2Dcateogries { get; set; }
    }
}
