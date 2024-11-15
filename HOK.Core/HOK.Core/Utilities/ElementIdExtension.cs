using Autodesk.Revit.DB;

namespace HOK.Core.Utilities
{
    public static class ElementIdExtension
    {

#if REVIT2024_OR_GREATER
        public static long GetElementIdValue(ElementId id)
        {
            return id.Value;
        }
    public static ElementId NewElementId(long l) {
            return new ElementId(l);
        }
#else
        public static long GetElementIdValue(ElementId id)
        {
            return id.IntegerValue;
        }
        public static ElementId NewElementId(long l) {
            if (l > int.MaxValue || l < int.MinValue)
            {
                throw new OverflowException("Value for ElementId out of range.");
            }
            return new ElementId((int)l);
        }
#endif

    }
}
