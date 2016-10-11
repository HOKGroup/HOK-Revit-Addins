using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolibriBatchSetup.Schema
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRoot("batch")]
    public class Batch : INotifyPropertyChanged
    {
        private string nameField = "Simple Batch";
        private string defaultField = "root";
        private Target targetField = new Target();
        private string filePath = "";

        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name { get { return nameField; } set { nameField = value; NotifyPropertyChanged("Name"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("default")]
        public string Default { get { return defaultField; } set { defaultField = value; NotifyPropertyChanged("Default"); } }
        [System.Xml.Serialization.XmlElementAttribute("target")]
        public Target Target { get { return targetField; } set { targetField = value; NotifyPropertyChanged("Target"); } }
        [System.Xml.Serialization.XmlIgnore]
        public string FilePath { get { return filePath; } set { filePath = value; NotifyPropertyChanged("FilePath"); } }

        public Batch()
        {
        }

        public Batch(string fileName)
        {
            filePath = fileName;
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

    [System.SerializableAttribute()]
    public class Target:INotifyPropertyChanged
    {
        private ObservableCollection<GenericElement> elements = new ObservableCollection<GenericElement>();
        private object exitField = new object();
        private string nameField = "root";
        
        [System.Xml.Serialization.XmlElementAttribute("openmodel", typeof(OpenModel))]
        [System.Xml.Serialization.XmlElementAttribute("openruleset", typeof(OpenRuleset))]
        [System.Xml.Serialization.XmlElementAttribute("openclassification", typeof(OpenClassification))]
        [System.Xml.Serialization.XmlElementAttribute("check", typeof(Check))]
        [System.Xml.Serialization.XmlElementAttribute("autocomment", typeof(AutoComment))]
        [System.Xml.Serialization.XmlElementAttribute("writereport", typeof(WriterReport))]
        [System.Xml.Serialization.XmlElementAttribute("createpresentation", typeof(CreatePresentation))]
        [System.Xml.Serialization.XmlElementAttribute("generalreport", typeof(GeneralReport))]
        [System.Xml.Serialization.XmlElementAttribute("bcfreport", typeof(BCFReport))]
        [System.Xml.Serialization.XmlElementAttribute("coordinationreport", typeof(CoordinationReport))]
        [System.Xml.Serialization.XmlElementAttribute("savemodel", typeof(SaveModel))]
        [System.Xml.Serialization.XmlElementAttribute("closemodel", typeof(CloseModel))]
        [System.Xml.Serialization.XmlElementAttribute("updatemodel", typeof(UpdateModel))]
        [System.Xml.Serialization.XmlElementAttribute("updatepresentation", typeof(UpdatePresentation))]
        public ObservableCollection<GenericElement> Elements { get { return elements; } set { elements = value; NotifyPropertyChanged("Elements"); } }

        [System.Xml.Serialization.XmlElementAttribute("exit")]
        public object Exit { get { return exitField; } set { exitField = value; NotifyPropertyChanged("Exit"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("name")]
        public string Name { get { return nameField; } set { nameField = value; NotifyPropertyChanged("Name"); } }
       
        public Target()
        {
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

    public class GenericElement: INotifyPropertyChanged
    {
        //generic element for xml elements
        public GenericElement() { }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public class OpenModel : GenericElement
    {
        private string file = "";
        private string shortName = "";
        private string discipline = "";
        private string fileExtension = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; fileExtension = System.IO.Path.GetExtension(file); NotifyPropertyChanged("File"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("shortname")]
        public string ShortName { get { return shortName; } set { shortName = value; NotifyPropertyChanged("ShortName"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("discipline")]
        public string Discipline { get { return discipline; } set { discipline = value; NotifyPropertyChanged("Discipline"); } }
        [System.Xml.Serialization.XmlIgnore]
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; NotifyPropertyChanged("FileExtension"); } }
       

        public OpenModel() { }

        public OpenModel(string fileName)
        {
            file = fileName;
            fileExtension = System.IO.Path.GetExtension(file);
        }

        public OpenModel(InputModel model)
        {
            file = model.File;
            shortName = model.ShortName;
            discipline = model.Discipline;
            fileExtension = model.FileExtension;
        }
    }

    public class UpdateModel : GenericElement
    {
        private string file = "";
        private string with = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; NotifyPropertyChanged("File"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("with")]
        public string With { get { return with; } set { with = value; NotifyPropertyChanged("With"); } }

        public UpdateModel() { }

        public UpdateModel(string oldPath, string updatePath)
        {
            file = oldPath;
            with = updatePath;
        }

        public UpdateModel(InputModel model)
        {
            file = model.File;
            with = model.With;
        }
    }

    //Custom Class for InputModel to combine properties from OpenModel and UpdateModel
    public class InputModel : GenericElement
    {
        private bool isUpdate = false; //false:OpenModel, true:UpdateModel
        private string file = "";
        private string shortName = "";
        private string discipline = "";
        private string fileExtension = "";
        private string with = "";

        public bool IsUpdate 
        { 
            get { return isUpdate; } 
            set 
            { 
                isUpdate = value;
                if (isUpdate && string.IsNullOrEmpty(with))
                {
                    this.With = file;
                }
                else
                {
                    this.With = "";
                }
                NotifyPropertyChanged("IsUpdate"); 
            } 
        }
        public string File { get { return file; } set { file = value; NotifyPropertyChanged("File");  } }
        public string ShortName { get { return shortName; } set { shortName = value; NotifyPropertyChanged("ShortName"); } }
        public string Discipline { get { return discipline; } set { discipline = value; NotifyPropertyChanged("Discipline"); } }
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; NotifyPropertyChanged("FileExtension"); } }
        public string With { get { return with; } set { with = value; NotifyPropertyChanged("With"); } }

        public InputModel()
        {
        }

        public InputModel(string fileName)
        {
            file = fileName;
        }

        public InputModel(OpenModel model)
        {
            isUpdate = false;
            file = model.File;
            shortName = model.ShortName;
            discipline = model.Discipline;
            fileExtension = model.FileExtension;
        }

        public InputModel(UpdateModel model)
        {
            isUpdate = true;
            file = model.File;
            with = model.With;
        }
    }

    public class OpenRuleset : GenericElement
    {
        private string file = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; NotifyPropertyChanged("File"); } }

        public OpenRuleset() { }

        public OpenRuleset(string fileName)
        {
            file = fileName;
        }
    }

    public class OpenClassification : GenericElement
    {
        private string file = "";
        private string provider = "";
        private string resource = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; NotifyPropertyChanged("File"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("provider")]
        public string Provider { get { return provider; } set { provider = value; NotifyPropertyChanged("Provider"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("resource")]
        public string Resource { get { return resource; } set { resource = value; NotifyPropertyChanged("Resource"); } }

        public OpenClassification() { }

        public OpenClassification(string fileName)
        {
            file = fileName;
        }

        public OpenClassification(string providerName, string resourceName)
        {
            provider = providerName;
            resource = resourceName;
        }
    
    }

    public class Check : GenericElement
    {
        private bool isSpecified = false;

        [System.Xml.Serialization.XmlIgnore]
        public bool IsSpecified { get { return isSpecified; } set { isSpecified = value; NotifyPropertyChanged("IsSpecified"); } }

        public Check() { }
    }

    public class AutoComment : GenericElement
    {
        private bool isSpecified = false;
        private string zoom = "TRUE";
        private string maxSnapshotsInCategory = "3";
        
        [System.Xml.Serialization.XmlIgnore]
        public bool IsSpecified { get { return isSpecified; } set { isSpecified = value; NotifyPropertyChanged("IsSpecified"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("zoom")]
        public string Zoom { get { return zoom; } set { zoom = value; NotifyPropertyChanged("Zoom"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("maxsnapshotsincategory")]
        public string MaxSnapshotsInCategory { get { return maxSnapshotsInCategory; } set { maxSnapshotsInCategory = value; NotifyPropertyChanged("MaxSnapshotsInCategory"); } }

    }

    public class WriterReport : GenericElement
    {
        private string pdffile;
        private string rtffile;
        private bool major;
        //private bool majorSpecified;
        private bool normal;
        //private bool normalSpecified;
        private bool minor;
        //private bool minorSpecified;
        private bool rejected;
        //private bool rejectedSpecified;
        private bool accepted;
        //private bool acceptedSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute("pdffile")]
        public string PdfFile { get { return pdffile; } set { pdffile = value; NotifyPropertyChanged("PdfFile"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("rtffile")]
        public string RtfFile { get { return rtffile; } set { rtffile = value; NotifyPropertyChanged("RtfFile"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("major")]
        public bool Major { get { return major; } set { major = value; NotifyPropertyChanged("Major"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool MajorSpecified { get { return majorSpecified; } set { majorSpecified = value; NotifyPropertyChanged("MajorSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("normal")]
        public bool Normal { get { return normal; } set { normal = value; NotifyPropertyChanged("Normal"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool NormalSpecified { get { return normalSpecified; } set { normalSpecified = value; NotifyPropertyChanged("NormalSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("minor")]
        public bool Minor { get { return minor; } set { minor = value; NotifyPropertyChanged("Minor"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool MinorSpecified { get { return minorSpecified; } set { minorSpecified = value; NotifyPropertyChanged("MinorSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("rejected")]
        public bool Rejected { get { return rejected; } set { rejected = value; NotifyPropertyChanged("Rejected"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool RejectedSpecified { get { return rejectedSpecified; } set { rejectedSpecified = value; NotifyPropertyChanged("RejectedSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("accepted")]
        public bool Accepted { get { return accepted; } set { accepted = value; NotifyPropertyChanged("Accepted"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool AcceptedSpecified { get { return acceptedSpecified; } set { acceptedSpecified = value; NotifyPropertyChanged("AcceptedSpecified"); } }

    }

    public class CreatePresentation : GenericElement
    {
        private bool isSelected = false;

        [System.Xml.Serialization.XmlIgnore]
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public CreatePresentation() { }
    }

    public class UpdatePresentation : GenericElement
    {
        private bool isSelected = false;

        [System.Xml.Serialization.XmlIgnore]
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public UpdatePresentation() { }
    }

    public class GeneralReport : GenericElement
    {
        private string pdffile;
        private string rtffile;
        private bool major;
        //private bool majorSpecified;
        private bool normal;
        //private bool normalSpecified;
        private bool minor;
        //private bool minorSpecified;
        private bool rejected;
        //private bool rejectedSpecified;
        private bool accepted;
        //private bool acceptedSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute("pdffile")]
        public string PdfFile { get { return pdffile; } set { pdffile = value; NotifyPropertyChanged("PdfFile"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("rtffile")]
        public string RtfFile { get { return rtffile; } set { rtffile = value; NotifyPropertyChanged("RtfFile"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("major")]
        public bool Major { get { return major; } set { major = value; NotifyPropertyChanged("Major"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool MajorSpecified { get { return majorSpecified; } set { majorSpecified = value; NotifyPropertyChanged("MajorSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("normal")]
        public bool Normal { get { return normal; } set { normal = value; NotifyPropertyChanged("Normal"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool NormalSpecified { get { return normalSpecified; } set { normalSpecified = value; NotifyPropertyChanged("NormalSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("minor")]
        public bool Minor { get { return minor; } set { minor = value; NotifyPropertyChanged("Minor"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool MinorSpecified { get { return minorSpecified; } set { minorSpecified = value; NotifyPropertyChanged("MinorSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("rejected")]
        public bool Rejected { get { return rejected; } set { rejected = value; NotifyPropertyChanged("Rejected"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool RejectedSpecified { get { return rejectedSpecified; } set { rejectedSpecified = value; NotifyPropertyChanged("RejectedSpecified"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("accepted")]
        public bool Accepted { get { return accepted; } set { accepted = value; NotifyPropertyChanged("Accepted"); } }

        //[System.Xml.Serialization.XmlIgnoreAttribute]
        //public bool AcceptedSpecified { get { return acceptedSpecified; } set { acceptedSpecified = value; NotifyPropertyChanged("AcceptedSpecified"); } }

    }

    public class BCFReport : GenericElement
    {
        private string fileField = "";
        private string versionField = "2";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return fileField; } set { fileField = value; NotifyPropertyChanged("File"); } }

        [System.Xml.Serialization.XmlAttributeAttribute("version", DataType = "integer")]
        public string Version { get { return versionField; } set { versionField = value; NotifyPropertyChanged("Version"); } }

        public BCFReport() { }

        public BCFReport(string file)
        {
            fileField = file;
        }

    }

    public class CoordinationReport : GenericElement
    {
        private string file = "";
        private string templateFile = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; NotifyPropertyChanged("File"); } }
        [System.Xml.Serialization.XmlAttributeAttribute("templatefile")]
        public string TemplateFile { get { return templateFile; } set { templateFile = value; NotifyPropertyChanged("TemplateFile"); } }

    }
    
    public class SaveModel : GenericElement
    {
        private string file = "";
        private string fileExtension = "";

        [System.Xml.Serialization.XmlAttributeAttribute("file")]
        public string File { get { return file; } set { file = value; NotifyPropertyChanged("File"); } }
        [System.Xml.Serialization.XmlIgnore]
        public string FileExtension { get { return fileExtension; } set { fileExtension = value; NotifyPropertyChanged("FileExtension"); } }

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
