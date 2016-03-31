using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.ElementFlatter.Class
{
    public class DirectShapeInfo:INotifyPropertyChanged
    {
        private ElementId shapeId = ElementId.InvalidElementId;
        private ElementId originId = ElementId.InvalidElementId;

        public ElementId ShapeId { get { return shapeId; } set { shapeId = value; } }
        public ElementId OriginId { get { return originId; } set { originId = value; } }

        public DirectShapeInfo()
        {
        }

        public DirectShapeInfo(ElementId sId, ElementId oId)
        {
            shapeId = sId;
            originId = oId;
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
