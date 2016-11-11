using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{


    [ValueConversion(typeof(String), typeof(Visibility))]
    public class IntVisibConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int c = (int)value;
            return (null == c || c == 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
