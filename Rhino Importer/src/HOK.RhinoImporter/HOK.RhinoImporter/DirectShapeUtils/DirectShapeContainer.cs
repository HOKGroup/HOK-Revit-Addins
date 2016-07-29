using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.RhinoImporter.DirectShapeUtils
{
    public class DirectShapeContainer : INotifyPropertyChanged
    {
        private string rhinoFile = "";
        private ObservableCollection<DirectShapeInfo> directShapes = new ObservableCollection<DirectShapeInfo>();

        public string RhinoFile { get { return rhinoFile; }set { rhinoFile = value;  NotifyPropertyChanged("RhinoFile"); } }
        public ObservableCollection<DirectShapeInfo> DirectShapes { get { return directShapes; } set { directShapes = value; NotifyPropertyChanged("DirectShapes"); } }

        public DirectShapeContainer()
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

    public class DirectShapeInfo : INotifyPropertyChanged
    {
        private ElementId directShapeId = ElementId.InvalidElementId;
        private Guid rhinoObjectId = Guid.Empty;

        public ElementId DirectShapeId { get { return directShapeId; }set { directShapeId = value; NotifyPropertyChanged("DirectShapeId"); } }
        public Guid RhinoObjectId { get { return rhinoObjectId; }set { rhinoObjectId = value; NotifyPropertyChanged("RhinoObjectId"); } }

        public DirectShapeInfo()
        {

        }

        public DirectShapeInfo(ElementId elementId, Guid objectId)
        {
            directShapeId = elementId;
            RhinoObjectId = objectId;
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
