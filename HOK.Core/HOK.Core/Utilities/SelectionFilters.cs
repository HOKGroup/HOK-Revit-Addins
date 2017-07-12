using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HOK.Core.Utilities
{
    /// <summary>
    /// Filters selection for Rooms only.
    /// </summary>
    public class RoomElementFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category != null)
            {
                return elem.Category.Name == "Rooms";
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    /// <summary>
    /// Filters selection for Walls only.
    /// </summary>
    public class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (null != elem.Category)
            {
                return elem.Category.Name == "Walls";
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
