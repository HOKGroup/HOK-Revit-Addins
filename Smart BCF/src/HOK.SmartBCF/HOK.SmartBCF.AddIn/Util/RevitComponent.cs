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
        private bool elementExist = false;
        private Element rvtElement = null;
        private ElementId elementId = ElementId.InvalidElementId;
        private CategoryProperties category = new CategoryProperties();
        private Bitmap previewImage = null;
        private bool isSpatialElement = false;
        private RoomProperties roomData = null;
        private bool elementMatched = false;
        private Transform transformValue = Transform.Identity;
        private bool isLinked = false;
        private string ifcProjectGuid = "";

        public bool ElementExist { get { return elementExist; } set { elementExist = value; NotifyPropertyChanged("ElementExist"); } }
        public Element RvtElement { get { return rvtElement; } set { rvtElement = value; NotifyPropertyChanged("RvtElement"); } }
        public ElementId ElementId { get { return elementId; } set { elementId = value; NotifyPropertyChanged("ElementId"); } }
        public CategoryProperties Category { get { return category; } set { category = value; NotifyPropertyChanged("Category"); } }
        public Bitmap PreviewImage { get { return previewImage; } set { previewImage = value; NotifyPropertyChanged("PreviewImage"); } }
        public bool IsSpatialElement { get { return isSpatialElement; } set { isSpatialElement = value; NotifyPropertyChanged("IsSpatialElement"); } }
        public RoomProperties RoomData { get { return roomData; } set { roomData = value; NotifyPropertyChanged("RoomData"); } }
        public bool ElementMatched { get { return elementMatched; } set { elementMatched = value; NotifyPropertyChanged("ElementMatched"); } }
        public Transform TransformValue { get { return transformValue; } set { transformValue = value; NotifyPropertyChanged("TransformValue"); } }
        public bool IsLinked { get { return isLinked; } set { isLinked = value; NotifyPropertyChanged("IsLinked"); } }
        public string IfcProjectGuid { get { return ifcProjectGuid; } set { ifcProjectGuid = value; NotifyPropertyChanged("IfcProjectGuid"); } }

        public RevitComponent()
        {
        }

        

        public RevitComponent(HOK.SmartBCF.Schemas.Component component, RoomProperties rp)
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

            elementExist = true;
            elementId = rp.RoomId;
            rvtElement = rp.RoomElement;
            this.ElementName = rp.RoomName;
            category = new CategoryProperties(rp.RoomElement.Category);
            isSpatialElement = true;
            roomData = rp;
            elementMatched = true;
            ifcProjectGuid = rp.IfcProjectGuid;
            transformValue = rp.TransformValue;
            isLinked = rp.IsLinked;
        }

        public RevitComponent(HOK.SmartBCF.Schemas.Component component, RevitLinkProperties link)
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

            if (!string.IsNullOrEmpty(component.AuthoringToolId))
            {
                int toolId = -1;
                if (int.TryParse(component.AuthoringToolId, out toolId))
                {
                    elementId = new ElementId(toolId);
                    Document doc = link.LinkedDocument;
                    Element element = doc.GetElement(elementId);
                    if (null != element)
                    {
                        rvtElement = element;
                        elementExist = true;
                        ifcProjectGuid = link.IfcProjectGuid;
                        transformValue = link.TransformValue;
                        isLinked = link.IsLinked;

                        this.ElementName = element.Name;
                        if (null != element.Category)
                        {
                            category = new CategoryProperties(element.Category);
                        }

                        ElementId typeId = element.GetTypeId();
                        ElementType elementType = doc.GetElement(typeId) as ElementType;
                        if (null != elementType)
                        {
                            previewImage = elementType.GetPreviewImage(new Size(48, 48));
                        }

                        string elementIfcGuid = element.IfcGUID();
                        if (this.IfcGuid == elementIfcGuid)
                        {
                            elementMatched = true;
                        }
                    }
                }
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
