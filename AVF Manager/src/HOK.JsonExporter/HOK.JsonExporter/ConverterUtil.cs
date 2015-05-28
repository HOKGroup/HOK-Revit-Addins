using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace HOK.JsonExporter
{
    public static class ConverterUtil
    {
        const double _eps = 1.0e-9;

        const double _mm_per_inch = 25.4;
        const double _inch_per_foot = 12;
        const double _foot_to_mm = _inch_per_foot * _mm_per_inch;

        public static int FootToMm(double a)
        {
            double one_half = a > 0 ? 0.5 : -0.5;
            return (int)(a * _foot_to_mm + one_half);
        }

        
        public static int ColorToInt(Color color)
        {
            return ((int)color.Red) << 16
            | ((int)color.Green) << 8
            | (int)color.Blue;
        }
    }
}
