using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(String), typeof(Visibility))]
    public class StringVisinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string c = null;
            if (value != null)
                c = value.ToString();

            return (string.IsNullOrWhiteSpace(c)) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
