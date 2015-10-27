using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolibriBatchSetup.Schema
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRoot("batch")]
    public class Batch
    {
        private string nameField = "Simple Batch";
        private string defaultField = "root";
        private Target targetField = new Target();
        private string filePath = "";

        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name { get { return nameField; } set { nameField = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute("default")]
        public string Default { get { return defaultField; } set { defaultField = value; } }
        [System.Xml.Serialization.XmlElementAttribute("target")]
        public Target Target { get { return targetField; } set { targetField = value; } }
        [System.Xml.Serialization.XmlIgnore]
        public string FilePath { get { return filePath; } set { filePath = value; } }

        public Batch()
        {
        }

        public Batch(string fileName)
        {
            filePath = fileName;
        }
    }

    [System.SerializableAttribute()]
    public class Target
    {
        private List<GenericElement> elements = new List<GenericElement>();
        private object exitField = new object();
        private string nameField = "root";
        
        [System.Xml.Serialization.XmlElementAttribute("openmodel", typeof(OpenModel))]
        [System.Xml.Serialization.XmlElementAttribute("openruleset", typeof(OpenRuleset))]
        [System.Xml.Serialization.XmlElementAttribute("check", typeof(Check))]
        [System.Xml.Serialization.XmlElementAttribute("autocomment", typeof(AutoComment))]
        [System.Xml.Serialization.XmlElementAttribute("createpresentation", typeof(CreatePresentation))]
        [System.Xml.Serialization.XmlElementAttribute("coordinationreport", typeof(CoordinationReport))]
        [System.Xml.Serialization.XmlElementAttribute("generalreport", typeof(GeneralReport))]
        [System.Xml.Serialization.XmlElementAttribute("writereport", typeof(WriterReport))]
        [System.Xml.Serialization.XmlElementAttribute("bcfreport", typeof(BCFReport))]
        [System.Xml.Serialization.XmlElementAttribute("savemodel", typeof(SaveModel))]
        [System.Xml.Serialization.XmlElementAttribute("closemodel", typeof(CloseModel))]
        public List<GenericElement> Elements { get { return elements; } set { elements = value; } }

        [System.Xml.Serialization.XmlElementAttribute("exit")]
        public object Exit { get { return exitField; } set { exitField = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name { get { return nameField; } set { nameField = value; } }
       
        public Target()
        {
        }
    }

    public class GenericElement
    {
        //generic element for xml elements
        public GenericElement() { }
    }

 

    public class OpenModel : GenericElement 
    {
        private string file = "";
        private string fileExtension = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; fileExtension = System.IO.Path.GetExtension(file); } }
        [System.Xml.Serialization.XmlIgnore]
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; } }

        public OpenModel() { }

        public OpenModel(string fileName)
        {
            file = fileName;
            fileExtension = System.IO.Path.GetExtension(file); 
        }
    }

    public class OpenRuleset : GenericElement
    {
        private string file = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; } }

        public OpenRuleset() { }

        public OpenRuleset(string fileName)
        {
            file = fileName;
        }
    }

    public class Check : GenericElement
    {
        public Check() { }
    }

    public class AutoComment : GenericElement
    {
        private string zoom = "TRUE";
        private string maxSnapshotsInCategory = "3";

        [System.Xml.Serialization.XmlAttributeAttribute("zoom")]
        public string Zoom { get { return zoom; } set { zoom = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute("maxsnapshotsincategory")]
        public string MaxSnapshotsInCategory { get { return maxSnapshotsInCategory; } set { maxSnapshotsInCategory = value; } }
    }

    public class CreatePresentation : GenericElement
    {
        public CreatePresentation() { }
    }

    public class CoordinationReport : GenericElement
    {
        private string file = "";
        private string templateFile = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute("templatefile")]
        public string TemplateFile { get { return templateFile; } set { templateFile = value; } }
    }

    public class GeneralReport : GenericElement
    {
        private string pdffileField;
        private string rtffileField;
        private bool majorField;
        private bool majorFieldSpecified;
        private bool normalField;
        private bool normalFieldSpecified;
        private bool minorField;
        private bool minorFieldSpecified;
        private bool rejectedField;
        private bool rejectedFieldSpecified;
        private bool acceptedField;
        private bool acceptedFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string pdffile {get{return pdffileField;}set{pdffileField = value;}}
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rtffile { get { return rtffileField; } set { rtffileField = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool major { get { return major; } set { major = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool majorSpecified { get { return majorSpecified; } set { majorSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool normal { get { return normal; } set { normal = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool normalSpecified { get { return normalSpecified; } set { normalSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool minor { get { return minor; } set { minor = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool minorSpecified { get { return minorSpecified; } set { minorSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool rejected { get { return rejectedField; } set { rejectedField = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rejectedSpecified { get { return rejectedFieldSpecified; } set { rejectedFieldSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool accepted { get { return acceptedField; } set { acceptedField = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool acceptedSpecified { get { return acceptedFieldSpecified; } set { acceptedFieldSpecified = value; } }
        
    }

    public class WriterReport : GenericElement
    {
        private string pdffileField;
        private string rtffileField;
        private bool majorField;
        private bool majorFieldSpecified;
        private bool normalField;
        private bool normalFieldSpecified;
        private bool minorField;
        private bool minorFieldSpecified;
        private bool rejectedField;
        private bool rejectedFieldSpecified;
        private bool acceptedField;
        private bool acceptedFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string pdffile { get { return pdffileField; } set { pdffileField = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string rtffile { get { return rtffileField; } set { rtffileField = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool major { get { return major; } set { major = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool majorSpecified { get { return majorSpecified; } set { majorSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool normal { get { return normal; } set { normal = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool normalSpecified { get { return normalSpecified; } set { normalSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool minor { get { return minor; } set { minor = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool minorSpecified { get { return minorSpecified; } set { minorSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool rejected { get { return rejectedField; } set { rejectedField = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool rejectedSpecified { get { return rejectedFieldSpecified; } set { rejectedFieldSpecified = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool accepted { get { return acceptedField; } set { acceptedField = value; } }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool acceptedSpecified { get { return acceptedFieldSpecified; } set { acceptedFieldSpecified = value; } }
    }

    public class BCFReport : GenericElement
    {
        private string fileField = "";
        private string versionField = "2";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return fileField; } set { fileField = value; } }
        [System.Xml.Serialization.XmlAttributeAttribute("version", DataType = "integer")]
        public string Version { get { return versionField; } set { versionField = value; } }

        public BCFReport() { }

        public BCFReport(string file)
        {
            fileField = file;
        }
        
    }

    public class SaveModel : GenericElement
    {
        private string file = "";
        private string fileExtension = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; } }
        [System.Xml.Serialization.XmlIgnore]
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; } }

        public SaveModel() { }

        public SaveModel(string fileName)
        {
            file = fileName;
            fileExtension = System.IO.Path.GetExtension(file); 
        }
    }

    public class CloseModel : GenericElement
    {
        public CloseModel() { }
    }

   

}
