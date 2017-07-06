using Autodesk.Revit.DB;
using System;
using System.ComponentModel;

namespace HOK.XYZLocator
{
    public class ProjectLocationProperties:INotifyPropertyChanged
    {
        private string name;
        private Transform transformValue;
        private ProjectPosition position;

        public string Name
        {
            get => name;
            set { name = value; NotifyPropertyChanged("Name"); }
        }
        public Transform TransformValue
        {
            get => transformValue;
            set { transformValue = value; NotifyPropertyChanged("TransformValue"); }
        }
        public ProjectPosition Position
        {
            get => position;
            set { position = value; NotifyPropertyChanged("Position");  }
        }

        public ProjectLocationProperties(ProjectLocation location)
        {
            name = location.Name;
            transformValue = location.GetTransform();
#if RELEASE2018
            position = location.GetProjectPosition(XYZ.Zero);
#else
            position = location.get_ProjectPosition(XYZ.Zero);
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    public enum OriginType
    {
        None = 0,
        InternalOrigin = 1,
        BasePoint = 2,
        SurveyPoint = 3
    }

    public class XYZLocation
    {
        public ProjectLocationProperties Location1 { get; set; } = null;
        public string Description1 { get; set; } = "";
        public OriginType Origin1 { get; set; } = OriginType.None;
        public ProjectLocationProperties Location2 { get; set; } = null;
        public string Description2 { get; set; } = "";
        public OriginType Origin2 { get; set; } = OriginType.None;

        public void GetTransformedValues(XYZ xyz, out XYZ tXYZ1, out XYZ tXYZ2)
        {
            tXYZ1 = XYZ.Zero;
            tXYZ2 = XYZ.Zero;
            try
            {
                switch (Origin1)
                {
                    case OriginType.InternalOrigin:
                        tXYZ1 = xyz;
                        break;
                    case OriginType.BasePoint:
                        break;
                    case OriginType.SurveyPoint:
                        tXYZ1 = Location1.TransformValue.Inverse.OfPoint(xyz);
                        break;
                }

                switch (Origin2)
                {
                    case OriginType.InternalOrigin:
                        tXYZ2 = xyz;
                        break;
                    case OriginType.BasePoint:
                        break;
                    case OriginType.SurveyPoint:
                        tXYZ2 = Location2.TransformValue.Inverse.OfPoint(xyz);
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
    }
}
