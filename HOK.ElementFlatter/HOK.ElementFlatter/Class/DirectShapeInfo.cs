using Autodesk.Revit.DB;

namespace HOK.ElementFlatter.Class
{
    public class DirectShapeInfo
    {
        public ElementId ShapeId { get; set; } = ElementId.InvalidElementId;
        public ElementId OriginId { get; set; } = ElementId.InvalidElementId;

        public DirectShapeInfo()
        {
        }

        public DirectShapeInfo(ElementId sId, ElementId oId)
        {
            ShapeId = sId;
            OriginId = oId;
        }
    }
}
