using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using HOK.SmartBCF.Schemas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SmartBCF.AddIn.Util
{
    public class RevitComponent : HOK.SmartBCF.Schemas.Component, INotifyPropertyChanged
    {
        private Element rvtElement = null;
        private ElementId elementId = ElementId.InvalidElementId;
        private CategoryProperties category = new CategoryProperties();
        private Bitmap previewImage = null;
        private bool isSpatialElement = false;
        private Transform transformValue = Transform.Identity;
        private bool isLinked = false;
        private string ifcProjectGuid = "";

        public Element RvtElement { get { return rvtElement; } set { rvtElement = value; NotifyPropertyChanged("RvtElement"); } }
        public ElementId ElementId { get { return elementId; } set { elementId = value; NotifyPropertyChanged("ElementId"); } }
        public CategoryProperties Category { get { return category; } set { category = value; NotifyPropertyChanged("Category"); } }
        public Bitmap PreviewImage { get { return previewImage; } set { previewImage = value; NotifyPropertyChanged("PreviewImage"); } }
        public bool IsSpatialElement { get { return isSpatialElement; } set { isSpatialElement = value; NotifyPropertyChanged("IsSpatialElement"); } }
        public Transform TransformValue { get { return transformValue; } set { transformValue = value; NotifyPropertyChanged("TransformValue"); } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; NotifyPropertyChanged("IsLinked"); } }
        public string IfcProjectGuid { get { return ifcProjectGuid; } set { ifcProjectGuid = value; NotifyPropertyChanged("IfcProjectGuid"); } }

        public RevitComponent()
        {
        }

        public RevitComponent(HOK.SmartBCF.Schemas.Component component, Element element, RevitLinkProperties link)
        {
            try
            {
                this.Action = component.Action;
                this.AuthoringToolId = component.AuthoringToolId;
                this.Color = component.Color;
                this.ElementName = component.ElementName;
                this.Guid = component.Guid;
                this.IfcGuid = component.IfcGuid;
                this.OriginatingSystem = component.OriginatingSystem;
                this.Responsibility = component.Responsibility;
                this.Selected = component.Selected;
                this.ViewPointGuid = component.ViewPointGuid;
                this.Visible = component.Visible;

                this.RvtElement = element;
                this.ElementId = element.Id;
                this.IfcProjectGuid = link.IfcProjectGuid;
                this.TransformValue = link.TransformValue;
                this.IsLinked = link.IsLinked;
                this.ElementName = element.Name;
                this.Category = new CategoryProperties(element.Category);

                ElementId typeId = element.GetTypeId();
                ElementType elementType = link.LinkedDocument.GetElement(typeId) as ElementType;
                if (null != elementType)
                {
                    this.PreviewImage = elementType.GetPreviewImage(new Size(48, 48));
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
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

   
}
