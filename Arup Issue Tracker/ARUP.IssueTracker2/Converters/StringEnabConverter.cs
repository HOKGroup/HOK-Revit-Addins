using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(String), typeof(bool))]
    public class StringEnabConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string c = (string)value;
            return (string.IsNullOrWhiteSpace(c)) ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
