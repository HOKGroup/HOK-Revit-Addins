using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class VisualizationInfo : INotifyPropertyChanged
    {
        private ObservableCollection<Component> componentsField;
        private OrthogonalCamera orthogonalCameraField;
        private PerspectiveCamera perspectiveCameraField;
        private List<Line> linesField;
        private List<ClippingPlane> clippingPlanesField;
        private List<VisualizationInfoBitmaps> bitmapsField;
        private string viewPointGuidField = "";
        private bool isPerspectiveField;

        public VisualizationInfo()
        {
            this.bitmapsField = new List<VisualizationInfoBitmaps>();
            this.clippingPlanesField = new List<ClippingPlane>();
            this.linesField = new List<Line>();
            this.perspectiveCameraField = new PerspectiveCamera();
            this.orthogonalCameraField = new OrthogonalCamera();
            this.componentsField = new ObservableCollection<Component>();
            this.isPerspectiveField = false;
        }

        public VisualizationInfo(VisualizationInfo visInfo)
        {
            this.Components = visInfo.Components;
            this.OrthogonalCamera = visInfo.OrthogonalCamera;
            this.PerspectiveCamera = visInfo.PerspectiveCamera;
            this.Lines = visInfo.Lines;
            this.ClippingPlanes = visInfo.ClippingPlanes;
            this.Bitmaps = visInfo.Bitmaps;
            this.isPerspectiveField = visInfo.isPerspectiveField;
        }

        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public ObservableCollection<Component> Components
        {
            get { return this.componentsField; }
            set { this.componentsField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public OrthogonalCamera OrthogonalCamera
        {
            get { return this.orthogonalCameraField; }
            set { this.orthogonalCameraField = value; isPerspectiveField = (orthogonalCameraField.ViewToWorldScale != 0) ? false : true; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PerspectiveCamera PerspectiveCamera
        {
            get { return this.perspectiveCameraField; }
            set { this.perspectiveCameraField = value; isPerspectiveField = (perspectiveCameraField.FieldOfView != 0) ? true : false; }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public List<Line> Lines
        {
            get { return this.linesField; }
            set { this.linesField = value; }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public List<ClippingPlane> ClippingPlanes
        {
            get { return this.clippingPlanesField; }
            set { this.clippingPlanesField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute("Bitmaps")]
        public List<VisualizationInfoBitmaps> Bitmaps
        {
            get { return this.bitmapsField; }
            set { this.bitmapsField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid
        {
            get { return this.viewPointGuidField; }
            set { this.viewPointGuidField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IsPersepective
        {
            get { return this.isPerspectiveField; }
            set { this.isPerspectiveField = value; }
        }

        public virtual VisualizationInfo Clone()
        {
            return ((VisualizationInfo)(this.MemberwiseClone()));
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
    public class Component : INotifyPropertyChanged
    {
        private string guidField = "";
        private string originatingSystemField = "";
        private string authoringToolIdField = "";
        private string ifcGuidField = "";
        private bool selectedField;
        private bool selectedFieldSpecified;
        private bool visibleField;
        private byte[] colorField;
        private string viewPointGuidField = "";

        //extension
        private string elementName = "";
        private RevitExtension action = new RevitExtension();
        private RevitExtension responsibility = new RevitExtension();
     
        public Component()
        {
            this.guidField = System.Guid.NewGuid().ToString();
            this.visibleField = true;
        }

        public Component(Component component)
        {
            this.Guid = component.Guid;
            this.OriginatingSystem = component.OriginatingSystem;
            this.AuthoringToolId = component.AuthoringToolId;
            this.IfcGuid = component.IfcGuid;
            this.Selected = component.Selected;
            this.SelectedSpecified = component.SelectedSpecified;
            this.Visible = component.Visible;
            this.Color = component.Color;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; NotifyPropertyChanged("Guid"); }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string OriginatingSystem
        {
            get { return this.originatingSystemField; }
            set { this.originatingSystemField = value; NotifyPropertyChanged("OriginatingSystem"); }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AuthoringToolId
        {
            get { return this.authoringToolIdField; }
            set { this.authoringToolIdField = value; NotifyPropertyChanged("AuthoringToolId"); }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "normalizedString")]
        public string IfcGuid
        {
            get { return this.ifcGuidField; }
            set { this.ifcGuidField = value; NotifyPropertyChanged("IfcGuid"); }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Selected
        {
            get { return this.selectedField; }
            set { this.selectedField = value; NotifyPropertyChanged("Selected"); }
                
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SelectedSpecified
        {
            get { return this.selectedFieldSpecified; }
            set { this.selectedFieldSpecified = value; NotifyPropertyChanged("SelectedSpecified"); }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Visible
        {
            get { return this.visibleField; }
            set { this.visibleField = value; NotifyPropertyChanged("Visible"); }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "hexBinary")]
        public byte[] Color
        {
            get { return this.colorField; }
            set { this.colorField = value; NotifyPropertyChanged("Color"); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid
        {
            get { return this.viewPointGuidField; }
            set { this.viewPointGuidField = value; NotifyPropertyChanged("ViewPointGuid"); }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ElementName { get { return elementName; } set { elementName = value; NotifyPropertyChanged("ElementName"); } }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public RevitExtension Action
        {
            get { return action; }
            set
            {
                if (null != value)
                {
                    action = value;
                    if (null != action.Color)
                    {
                        this.Color = action.Color;
                    }
                }
                
                NotifyPropertyChanged("Action");
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public RevitExtension Responsibility
        {
            get { return responsibility; }
            set 
            {
                if (null != value)
                {
                    responsibility = value;
                    NotifyPropertyChanged("Responsibility"); 
                }
            }
        }

        public virtual Component Clone()
        {
            return ((Component)(this.MemberwiseClone()));
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
    public class ClippingPlane
    {
        private string guidField = "";
        private Point locationField;
        private Direction directionField;
        private string viewPointGuidField = "";

        public ClippingPlane()
        {
            this.guidField = System.Guid.NewGuid().ToString();
            this.directionField = new Direction();
            this.locationField = new Point();
        }

        public ClippingPlane(ClippingPlane clippingPlane)
        {
            this.Guid = clippingPlane.Guid;
            this.Location = clippingPlane.Location;
            this.Direction = clippingPlane.Direction;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Point Location
        {
            get { return this.locationField; }
            set { this.locationField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction Direction
        {
            get { return this.directionField; }
            set { this.directionField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid
        {
            get { return this.viewPointGuidField; }
            set { this.viewPointGuidField = value; }
        }

        public virtual ClippingPlane Clone()
        {
            return ((ClippingPlane)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class Point
    {
        private string guidField = "";
        private double xField;
        private double yField;
        private double zField;

        public Point()
        {
            this.guidField = System.Guid.NewGuid().ToString();
        }

        public Point(Point point)
        {
            this.Guid = point.Guid;
            this.X = point.X;
            this.Y = point.Y;
            this.Z = point.Z;

        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double X
        {
            get { return this.xField; }
            set { this.xField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double Y
        {
            get { return this.yField; }
            set { this.yField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double Z
        {
            get { return this.zField; }
            set { this.zField = value; }
        }

        public virtual Point Clone()
        {
            return ((Point)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class Direction
    {
        private string guidField = "";
        private double xField;
        private double yField;
        private double zField;

        public Direction()
        {
            this.guidField = System.Guid.NewGuid().ToString();
        }

        public Direction(Direction direction)
        {
            this.Guid = direction.Guid;
            this.X = direction.X;
            this.Y = direction.Y;
            this.Z = direction.Z;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double X
        {
            get { return this.xField; }
            set { this.xField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double Y
        {
            get { return this.yField; }
            set { this.yField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double Z
        {
            get { return this.zField; }
            set { this.zField = value; }
        }

        public virtual Direction Clone()
        {
            return ((Direction)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class Line
    {
        private string guidField = "";
        private Point startPointField;
        private Point endPointField;

        private string viewPointGuidField = "";

        public Line()
        {
            this.guidField = System.Guid.NewGuid().ToString();
            this.endPointField = new Point();
            this.startPointField = new Point();
        }

        public Line(Line line)
        {
            this.Guid = line.Guid;
            this.EndPoint = line.EndPoint;
            this.StartPoint = line.StartPoint;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Point StartPoint
        {
            get { return this.startPointField; }
            set { this.startPointField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Point EndPoint
        {
            get { return this.endPointField; }
            set { this.endPointField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid 
        { 
            get { return viewPointGuidField; } 
            set { viewPointGuidField = value; } 
        }

        public virtual Line Clone()
        {
            return ((Line)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class PerspectiveCamera
    {
        private string guidField = "";
        private Point cameraViewPointField;
        private Direction cameraDirectionField;
        private Direction cameraUpVectorField;
        private double fieldOfViewField;
        private string viewPointGuidField = "";

        public PerspectiveCamera()
        {
            this.guidField = System.Guid.NewGuid().ToString();
            this.cameraUpVectorField = new Direction();
            this.cameraDirectionField = new Direction();
            this.cameraViewPointField = new Point();
        }

        public PerspectiveCamera(PerspectiveCamera persCamera)
        {
            this.Guid = persCamera.Guid;
            this.CameraViewPoint = persCamera.CameraViewPoint;
            this.CameraDirection = persCamera.CameraDirection;
            this.CameraUpVector = persCamera.CameraUpVector;
            this.FieldOfView = persCamera.FieldOfView;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Point CameraViewPoint
        {
            get { return this.cameraViewPointField; }
            set { this.cameraViewPointField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction CameraDirection
        {
            get { return this.cameraDirectionField; }
            set { this.cameraDirectionField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction CameraUpVector
        {
            get { return this.cameraUpVectorField; }
            set { this.cameraUpVectorField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double FieldOfView
        {
            get { return this.fieldOfViewField; }
            set { this.fieldOfViewField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid 
        { 
            get { return viewPointGuidField; } 
            set { viewPointGuidField = value; } 
        }

        public virtual PerspectiveCamera Clone()
        {
            return ((PerspectiveCamera)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public class OrthogonalCamera
    {
        private string guidField = "";
        private Point cameraViewPointField;
        private Direction cameraDirectionField;
        private Direction cameraUpVectorField;
        private double viewToWorldScaleField;
        private string viewPointGuidField = "";

        public OrthogonalCamera()
        {
            this.guidField = System.Guid.NewGuid().ToString();
            this.cameraUpVectorField = new Direction();
            this.cameraDirectionField = new Direction();
            this.cameraViewPointField = new Point();
        }

        public OrthogonalCamera(OrthogonalCamera orthoCamera)
        {
            this.Guid = orthoCamera.Guid;
            this.CameraUpVector = orthoCamera.CameraUpVector;
            this.CameraDirection = orthoCamera.CameraDirection;
            this.CameraViewPoint = orthoCamera.CameraViewPoint;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Point CameraViewPoint
        {
            get { return this.cameraViewPointField; }
            set { this.cameraViewPointField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction CameraDirection
        {
            get { return this.cameraDirectionField; }
            set { this.cameraDirectionField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction CameraUpVector
        {
            get { return this.cameraUpVectorField; }
            set { this.cameraUpVectorField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ViewToWorldScale
        {
            get { return this.viewToWorldScaleField; }
            set { this.viewToWorldScaleField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid
        {
            get { return this.viewPointGuidField; }
            set { this.viewPointGuidField = value; }
        }

        public virtual OrthogonalCamera Clone()
        {
            return ((OrthogonalCamera)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class VisualizationInfoBitmaps
    {
        private string guidField = "";
        private BitmapFormat bitmapField;
        private string referenceField = "";
        private Point locationField;
        private Direction normalField;
        private Direction upField;
        private double heightField;
        private byte[] bitmapImageField;
        private string viewPointGuidField = "";

        public VisualizationInfoBitmaps()
        {
            this.bitmapField = BitmapFormat.PNG;
            this.guidField = System.Guid.NewGuid().ToString();
            this.upField = new Direction();
            this.normalField = new Direction();
            this.locationField = new Point();
        }

        public VisualizationInfoBitmaps(VisualizationInfoBitmaps visInfoBitmaps)
        {
            this.Guid = visInfoBitmaps.Guid;
            this.Bitmap = visInfoBitmaps.Bitmap;
            this.Reference = visInfoBitmaps.Reference;
            this.Location = visInfoBitmaps.Location;
            this.Normal = visInfoBitmaps.Normal;
            this.Up = visInfoBitmaps.Up;
            this.Height = visInfoBitmaps.Height;
            this.BitmapImage = visInfoBitmaps.BitmapImage;
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string Guid
        {
            get { return this.guidField; }
            set { this.guidField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public BitmapFormat Bitmap
        {
            get { return this.bitmapField; }
            set { this.bitmapField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Reference
        {
            get { return this.referenceField; }
            set { this.referenceField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Point Location
        {
            get { return this.locationField; }
            set { this.locationField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction Normal
        {
            get { return this.normalField; }
            set { this.normalField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Direction Up
        {
            get { return this.upField; }
            set { this.upField = value; }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double Height
        {
            get { return this.heightField; }
            set { this.heightField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public byte[] BitmapImage
        {
            get { return this.bitmapImageField; }
            set { this.bitmapImageField = value; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ViewPointGuid
        {
            get { return this.viewPointGuidField; }
            set { this.viewPointGuidField = value; }
        }

        public virtual VisualizationInfoBitmaps Clone()
        {
            return ((VisualizationInfoBitmaps)(this.MemberwiseClone()));
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum BitmapFormat
    {
        PNG,
        JPG
    }
}
