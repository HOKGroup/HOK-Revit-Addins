using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using static HOK.Core.Utilities.ElementIdExtension;

namespace HOK.MissionControl.Utils
{
    public static class ElementUtilities
    {
        public static Document Doc { get; set; }

        /// <summary>
        /// Converts Revit Color parameter stored as integer to RGB array.
        /// </summary>
        /// <param name="value">Integer value of the parameter.</param>
        /// <returns>An array of RGB values.</returns>
        public static List<int> RevitColorIntegerToRGBA(int value)
        {
            var red = value % 256;

            value = value / 256;
            var green = value % 256;

            value = value / 256;
            var blue = value % 256;

            return new List<int> { red, green, blue };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool RevitBoolToBool(int value)
        {
            return value != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dim"></param>
        /// <returns></returns>
        public static bool DimensionHasOverride(Dimension dim)
        {
            return dim.Segments.Cast<DimensionSegment>().Any(d => !string.IsNullOrEmpty(d.ValueOverride));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static object GetParameterValue(Parameter p)
        {
            switch (p.StorageType)
            {
                case StorageType.None:
                    throw new System.ArgumentOutOfRangeException();
                case StorageType.Integer:
                    // (Konrad) It appears that by default yes/no parameters
                    // have a greyed out box checked to 1 even though stored
                    // integer value would return 0
                    return p.HasValue ? p.AsInteger() : 1;
                case StorageType.Double:
                    return p.AsDouble();
                case StorageType.String:
                    return p.AsString();
                case StorageType.ElementId:
                    return GetElementIdValue(p.AsElementId());
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
