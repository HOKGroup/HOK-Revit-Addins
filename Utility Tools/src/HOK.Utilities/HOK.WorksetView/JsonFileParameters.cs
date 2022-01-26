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
        [JsonProperty("viewname")]
        public string ViewName { get; set; }
        [JsonProperty("visible3Dcategories")]
        public List<string> Visible3DCategories { get; set; }
        [JsonProperty("visible2Dcategories")]
        public List<string> Visible2DCategories { get; set; }
    }
}
