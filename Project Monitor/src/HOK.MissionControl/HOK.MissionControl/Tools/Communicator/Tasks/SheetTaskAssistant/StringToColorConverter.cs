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
            return (string)values[0] == (string)values[1] ? new SolidColorBrush(Colors.DarkGray) : new SolidColorBrush(Color.FromRgb(217, 83, 79));
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
