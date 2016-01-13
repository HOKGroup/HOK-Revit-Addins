using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.Schemas
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Version
    {
        private string guidField = "";
        private string detailedVersionField = "";
        private string versionIdField = "";
        private string fileId = "";

        public Version()
        {
            this.guidField = System.Guid.NewGuid().ToString();
        }

        public Version(Version version)
        {
            this.Guid = version.Guid;
            this.DetailedVersion = version.DetailedVersion;
            this.VersionId = version.VersionId;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DetailedVersion
        {
            get { return this.detailedVersionField; }
            set { this.detailedVersionField = value; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string VersionId
        {
            get { return this.versionIdField; }
            set { this.versionIdField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FileId
        {
            get { return fileId; }
            set { fileId = value; }
        }

        public virtual Version Clone()
        {
            return ((Version)(this.MemberwiseClone()));
        }
    }
}
