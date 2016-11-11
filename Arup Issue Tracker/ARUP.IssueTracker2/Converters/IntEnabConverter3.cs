using System;
using System.Windows.Data;
using System.Globalization;

namespace ARUP.IssueTracker.Converters
{

    public class IntEnabConverter3 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value !=-1)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


    }
}
