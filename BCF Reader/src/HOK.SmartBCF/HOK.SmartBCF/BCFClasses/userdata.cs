using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class UserData
{
    private List<CommentExtension> commentExtensionsField = new List<CommentExtension>();

    [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [System.Xml.Serialization.XmlArrayItemAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public List<CommentExtension> CommentExtensions
    {
        get
        {
            return this.commentExtensionsField;
        }
        set
        {
            this.commentExtensionsField = value;
        }
    }
}

[System.SerializableAttribute()]
public partial class CommentExtension
{
    private string commentGuidField;
    private string commentActionField;

    public CommentExtension()
    {
    }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string CommentGuid
    {
        get
        {
            return this.commentGuidField;
        }
        set
        {
            this.commentGuidField = value;
        }
    }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string CommentAction
    {
        get
        {
            return this.commentActionField;
        }
        set
        {
            this.commentActionField = value;
        }
    }
}
