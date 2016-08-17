using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.XYZLocator
{
    public class ProjectLocationProperties:INotifyPropertyChanged
    {
        private string name = "";
        private Transform transformValue = null;
        private ProjectPosition position = null;

        public string Name { get { return name; } set { name = value; NotifyPropertyChanged("Name"); } }
        public Transform TransformValue { get { return transformValue; } set { transformValue = value; NotifyPropertyChanged("TransformValue"); } }
        public ProjectPosition Position { get { return position; } set { position = value; NotifyPropertyChanged("Position");  } }

        public ProjectLocationProperties(ProjectLocation location)
        {
            name = location.Name;
            transformValue = location.GetTransform();
            position = location.get_ProjectPosition(XYZ.Zero);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    public enum OriginType
    {
        None =0, InternalOrigin=1, BasePoint=2, SurveyPoint=3
    }

    public class XYZLocation
    {
        private ProjectLocationProperties location1 = null;
        private OriginType origin1 = OriginType.None;
        private string description1 = "";
        private ProjectLocationProperties location2 = null;
        private OriginType origin2 = OriginType.None;
        private string description2 = "";

        public ProjectLocationProperties Location1 { get { return location1; } set { location1 = value; } }
        public string Description1 { get { return description1; } set { description1 = value; } }
        public OriginType Origin1 { get { return origin1; } set { origin1 = value; } }
        public ProjectLocationProperties Location2 { get { return location2; } set { location2 = value; } }
        public string Description2 { get { return description2; } set { description2 = value; } }
        public OriginType Origin2 { get { return origin2; } set { origin2 = value; } }

        public XYZLocation()
        {
        }

        public void GetTransformedValues(XYZ xyz, out XYZ tXYZ1, out XYZ tXYZ2)
        {
            tXYZ1 = XYZ.Zero;
            tXYZ2 = XYZ.Zero;
            try
            {
                switch (origin1)
                {
                    case OriginType.InternalOrigin:
                        tXYZ1 = xyz;
                        break;
                    case OriginType.BasePoint:
                        break;
                    case OriginType.SurveyPoint:
                        tXYZ1 = location1.TransformValue.Inverse.OfPoint(xyz);
                        break;
                }

                switch (origin2)
                {
                    case OriginType.InternalOrigin:
                        tXYZ2 = xyz;
                        break;
                    case OriginType.BasePoint:
                        break;
                    case OriginType.SurveyPoint:
                        tXYZ2 = location2.TransformValue.Inverse.OfPoint(xyz);
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }
}
