using System;
using System.Windows.Data;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{

    [ValueConversion(typeof(Int16), typeof(String))]
    public class IntEnabConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value != 0)
                return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
