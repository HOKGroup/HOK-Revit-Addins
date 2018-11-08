using Autodesk.Revit.DB;
using System;
using System.ComponentModel;
using System.Drawing;

namespace HOK.SmartBCF.AddIn.Util
{
    public class RevitComponent : Schemas.Component, INotifyPropertyChanged
    {
        private Element rvtElement;
        private ElementId elementId = ElementId.InvalidElementId;
        private CategoryProperties category = new CategoryProperties();
        private Bitmap previewImage;
        private bool isSpatialElement;
        private Transform transformValue = Transform.Identity;
        private bool isLinked;
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

        public RevitComponent(Schemas.Component component, Element element, RevitLinkProperties link)
        {
            try
            {
                Action = component.Action;
                AuthoringToolId = component.AuthoringToolId;
                Color = component.Color;
                ElementName = component.ElementName;
                Guid = component.Guid;
                IfcGuid = component.IfcGuid;
                OriginatingSystem = component.OriginatingSystem;
                Responsibility = component.Responsibility;
                Selected = component.Selected;
                ViewPointGuid = component.ViewPointGuid;
                Visible = component.Visible;

                RvtElement = element;
                ElementId = element.Id;
                IfcProjectGuid = link.IfcProjectGuid;
                TransformValue = link.TransformValue;
                IsLinked = link.IsLinked;
                ElementName = element.Name;
                Category = new CategoryProperties(element.Category);

                var typeId = element.GetTypeId();
                var elementType = link.LinkedDocument.GetElement(typeId) as ElementType;
                if (null != elementType)
                {
                    PreviewImage = elementType.GetPreviewImage(new Size(48, 48));
                }

            }
            catch (Exception)
            {
                // ignored
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
