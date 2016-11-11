using System;
using System.Globalization;
using System.Windows.Data;

namespace ARUP.IssueTracker.Converters
{

    [ValueConversion(typeof(Boolean), typeof(string))]
public class BoolStarConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "" : "*";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
    
}
