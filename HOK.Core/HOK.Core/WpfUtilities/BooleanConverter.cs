using System;
using System.Windows.Data;

namespace HOK.Core.WpfUtilities
{
    /// <summary>
    /// Converts Bool to Yes or No
    ///  - Used by HOK.MissionControl.LinksManager
    /// </summary>
    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return null;

            var specific = (bool) value;
            return specific ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts Bool to Linked or Imported
    ///  - Used by HOK.MissionControl.LinksManager
    /// </summary>
    public class Boolean2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return null;

            var specific = (bool)value;
            return specific ? "Linked" : "Imported";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
