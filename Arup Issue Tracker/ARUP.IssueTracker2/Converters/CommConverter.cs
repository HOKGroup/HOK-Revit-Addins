using System;
using System.Windows.Data;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(Int16), typeof(String))]
    public class CommConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string c;
            if ((int)value == 1)
                c = "Comment";
            else
                c = "Comments";
            return value.ToString() + " " + c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
