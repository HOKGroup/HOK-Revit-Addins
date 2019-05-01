using System;
using System.Windows.Data;
using System.Windows.Media;

namespace HOK.MissionControl.Tools.Communicator.Tasks.SheetTaskAssistant
{
    /// <summary>
    /// Compares two strings and if they are different returns Red. If they match returns Gray.
    /// </summary>
    public class StringToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // (Konrad) Create Sheet task will not have Element set. One of the values is null;
            var first = values[0] as string;
            var second = values[1] as string;

            if (first == null || second == null) return new SolidColorBrush(Color.FromRgb(217, 83, 79));

            return first == second ? new SolidColorBrush(Colors.DarkGray) : new SolidColorBrush(Color.FromRgb(217, 83, 79));
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Checks if identifier is null or empty string and returns boolean. Used by UI to disable OpenSheet button.
    /// </summary>
    public class IdentifierToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
