using System;
using System.Windows.Data;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(Int16), typeof(String))]
    public class SendIssueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string c;
            if ((int)value == 1)
                c = "Send " + value.ToString() + " Issue »";
            else
                c = "Send " + value.ToString() + " Issues »";
            return c;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
