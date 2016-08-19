using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.Schemas
{
    public enum TopicType
    {
        Error = 0, Info = 1, Unknown = 2, Warning = 3, Issue = 4, Fault = 5, Clash = 6, Request = 7, Inquiry = 8, Remark = 9, Undefined =10
    }
    public enum TopicStatus
    {
        Active = 0, Assigned = 0, Closed = 1, Open = 2, Resolved = 3
    }

    public enum TopicLable
    {
        Architecture = 0, Structure = 1, Mechanical = 2, Electrical = 3, Specifications = 4, Technology = 5
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class RevitExtensionInfo :INotifyPropertyChanged
    {
        private ObservableCollection<RevitExtension> extensions = new ObservableCollection<RevitExtension>();
       
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public ObservableCollection<RevitExtension> Extensions { get { return extensions; } set { extensions = value; NotifyPropertyChanged("Actions"); } }

        public RevitExtensionInfo() { }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public RevitExtensionInfo Clone()
        {
            return ((RevitExtensionInfo)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class RevitExtension : INotifyPropertyChanged
    {
        private string guid = System.Guid.Empty.ToString();
        private string parameterName = "";
        private string parameterValue = "";
        private byte[] color;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Guid { get { return guid; } set { guid = value; NotifyPropertyChanged("Guid"); } }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ParameterName { get { return parameterName; } set { parameterName = value; NotifyPropertyChanged("ParameterName"); } }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ParameterValue { get { return parameterValue; } set { parameterValue = value; NotifyPropertyChanged("ParameterValue"); } }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "hexBinary")]
        public byte[] Color { get { return color; } set { color = value; NotifyPropertyChanged("Color"); } }

        public RevitExtension() { }

        public RevitExtension(string id, string name, string value, byte[] colorArray)
        {
            guid = id;
            parameterName = name;
            ParameterValue = value;
            color = colorArray;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class ComponentExtensionInfo : INotifyPropertyChanged
    {
        private string viewpointGuid = "";
        private ObservableCollection<ComponentExtension> extensions = new ObservableCollection<ComponentExtension>();

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewpointGuid { get { return viewpointGuid; } set { viewpointGuid = value; NotifyPropertyChanged("ViewpointGuid"); } }

        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public ObservableCollection<ComponentExtension> Extensions { get { return extensions; } set { extensions = value; NotifyPropertyChanged("Extensions"); } }

        public ComponentExtensionInfo()
        {
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class ComponentExtension : INotifyPropertyChanged
    {
        private string ifcGuid = "";
        private string elementName = "";
        private string actionGuid = "";
        private string responsibilityGuid = "";

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "normalizedString")]
        public string IfcGuid { get { return ifcGuid; } set { ifcGuid = value; NotifyPropertyChanged("IfcGuid"); } }

        [System.Xml.Serialization.XmlElementAttribute()]
        public string ElementName { get { return elementName; } set { elementName = value; NotifyPropertyChanged("ElementName"); } }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ActionGuid { get { return actionGuid; } set { actionGuid = value; NotifyPropertyChanged("Action"); } }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ResponsibilityGuid { get { return responsibilityGuid; } set { responsibilityGuid = value; NotifyPropertyChanged("Responsibility"); } }

        public ComponentExtension()
        {
        }

        public ComponentExtension(Component comp)
        {
            ifcGuid = comp.IfcGuid;
            elementName = comp.ElementName;
            actionGuid = comp.Action.Guid;
            responsibilityGuid = comp.Responsibility.Guid;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
