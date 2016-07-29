using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.RhinoImporter.RhinoUtils
{
    public class RhinoGeometryContainer : INotifyPropertyChanged
    {
        private string filePath = "";
        private ObservableCollection<RhinoObjectInfo> points = new ObservableCollection<RhinoObjectInfo>();
        private ObservableCollection<RhinoObjectInfo> curves = new ObservableCollection<RhinoObjectInfo>();
        private ObservableCollection<RhinoObjectInfo> surfaces = new ObservableCollection<RhinoObjectInfo>();
        private ObservableCollection<RhinoObjectInfo> breps = new ObservableCollection<RhinoObjectInfo>();
        private ObservableCollection<RhinoObjectInfo> meshes = new ObservableCollection<RhinoObjectInfo>();
        private ObservableCollection<RhinoObjectInfo> extrusions = new ObservableCollection<RhinoObjectInfo>();

        public string FilePath { get { return filePath; } set { filePath = value; } }
        public ObservableCollection<RhinoObjectInfo> Points { get { return points; } set { points = value; NotifyPropertyChanged("Points"); } }
        public ObservableCollection<RhinoObjectInfo> Curves { get { return curves; } set { curves = value; NotifyPropertyChanged("Curves"); } }
        public ObservableCollection<RhinoObjectInfo> Surfaces { get { return surfaces; } set { surfaces = value; NotifyPropertyChanged("Surfaces"); } }
        public ObservableCollection<RhinoObjectInfo> Breps { get { return breps; } set { breps = value; NotifyPropertyChanged("Breps"); } }
        public ObservableCollection<RhinoObjectInfo> Meshes { get { return meshes; } set { meshes = value; NotifyPropertyChanged("Meshes"); } }
        public ObservableCollection<RhinoObjectInfo> Extrusions { get { return extrusions; } set { extrusions = value; NotifyPropertyChanged("Extrusions"); } }

        public RhinoGeometryContainer()
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

    public class RhinoObjectInfo : INotifyPropertyChanged
    {
        private Guid objectId = Guid.Empty;
        private string objectName = "";
        private GeometryBase geometry = null;

        public Guid ObjectId { get { return objectId;} set { objectId = value;  NotifyPropertyChanged("ObjectId"); } }
        public string ObjectName { get { return objectName; } set { objectName = value; NotifyPropertyChanged("ObjectName"); } }
        public GeometryBase Geometry
        {
            get { return geometry; }
            set { geometry = value; NotifyPropertyChanged("Geometry"); }
        }

        public RhinoObjectInfo(Guid guid, string name, GeometryBase geo)
        {
            objectId = guid;
            objectName = name;
            geometry = geo;
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
