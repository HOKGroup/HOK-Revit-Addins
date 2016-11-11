using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(Boolean), typeof(Visibility))]
    public class ExpandedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool c = System.Convert.ToBoolean(value);
            return (c) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }
}
