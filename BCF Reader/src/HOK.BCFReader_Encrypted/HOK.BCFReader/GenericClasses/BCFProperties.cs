using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace HOK.BCFReader.GenericClasses
{
    public class BCF
    {
        public BCF() { }
        public string IssueNumber { get; set; }
        public BCFMarkUp MarkUp { get; set; }
        public BCFViewPoint ViewPoint { get; set; }
        public Image SnapShot { get; set; }

    }

    public class BCFMarkUp
    {
        public BCFMarkUp() { }
        public List<IfcFile> Header { get; set; }
        public Topic MarkUpTopic { get; set; }
        public List<Comment> Comments { get; set; }
    }

    public class BCFViewPoint
    {
        public BCFViewPoint() { }
        public List<Component> Components { get; set; }
        public OrthogonalCameraProperties OrthogonalCamera { get; set; }
        public PerspectiveCameraProperties PerspectiveCamera { get; set; }
        public List<Line> Lines { get; set; }
        public List<ClippingPlane> ClippingPlanes { get; set; }
    }

    public class IfcFile
    {
        public IfcFile() { }
        public string IfcGuid { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }
    }

    public class Topic
    {
        public Topic() { }
        public string TopicGuid { get; set; }
        public string ReferenceLink { get; set; }
        public string Title { get; set; }
    }

    public class Comment
    {
        public Comment() { }
        public string CommentGuid { get; set; }
        public string VerbalStatus { get; set; }
        public CommentStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string Author { get; set; }
        public string CommentString { get; set; }
        public Topic Topic { get; set; }
        public string Action { get; set; }
    }

    public enum CommentStatus { Error=0, Warning, Info, Unknown }

    public class Component
    {
        public Component() { }
        public string IfcGuid { get; set; }
        public string OriginatingSystem { get; set; }
        public string AuthoringToolId { get; set; }
    }

    public class PerspectiveCameraProperties
    {
        public PerspectiveCameraProperties() { }
        public Point CameraViewPoint { get; set; }
        public Point CameraDirection { get; set; }
        public Point CameraUpVector { get; set; }
        public double FieldOfView { get; set; } //minInclusive=45, maxInclusive=60
    }

    public class OrthogonalCameraProperties
    {
        public OrthogonalCameraProperties() { }
        public Point CameraViewPoint { get; set; }
        public Point CameraDirection { get; set; }
        public Point CameraUpVector { get; set; }
        public double ViewToWorldScale { get; set; } //annotation: view's visible size in meters
    }

    public class Line
    {
        public Line() { }
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
    }

    public class ClippingPlane
    {
        public ClippingPlane() { }
        public Point Location { get; set; }
        public Point Direction { get; set; }
    }

    public class Point
    {
        public Point() { }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
