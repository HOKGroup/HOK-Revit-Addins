using System;
using System.Windows.Data;

namespace HOK.MissionControl.StylesManager
{
    public class InstanceCountToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "Normal";

            switch ((int)value)
            {
                case 0:
                case 1:
                    return "Red";
                case 2:
                case 3:
                case 4:
                case 5:
                    return "Orange";
                default:
                    return "Normal";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return null;

            var specific = (bool)value;
            return specific ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
