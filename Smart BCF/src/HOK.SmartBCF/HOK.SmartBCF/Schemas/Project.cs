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
    public class ProjectExtension
    {
        private string guidField = "";
        private Project projectField;
        private string extensionSchemaField = "";
        private string fileId = "";

        public ProjectExtension()
        {
            this.guidField = System.Guid.NewGuid().ToString();
            this.projectField = new Project();
        }

        public ProjectExtension(ProjectExtension projectExt)
        {
            this.Guid = projectExt.Guid;
            this.Project = projectExt.Project;
            this.ExtensionSchema = projectExt.ExtensionSchema;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Project Project
        {
            get { return this.projectField; }
            set { this.projectField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string ExtensionSchema
        {
            get { return this.extensionSchemaField; }
            set { this.extensionSchemaField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string FileId
        {
            get { return fileId; }
            set { fileId = value; }
        }

        public virtual ProjectExtension Clone()
        {
            return ((ProjectExtension)(this.MemberwiseClone()));
        }

    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class Project
    {
        private string guidField = "";
        private string nameField = "";
        private string projectIdField = "";
        private string extensionGuidField = "";

        public Project()
        {
            this.guidField = System.Guid.NewGuid().ToString();
        }

        public Project(Project project)
        {
            this.Guid = project.Guid;
            this.Name = project.Name;
            this.ProjectId = project.ProjectId;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProjectId
        {
            get { return this.projectIdField; }
            set { this.projectIdField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ExtensionGuid
        {
            get { return this.extensionGuidField; }
            set { this.extensionGuidField = value; }
        }

        public virtual Project Clone()
        {
            return ((Project)(this.MemberwiseClone()));
        }
    }
}
