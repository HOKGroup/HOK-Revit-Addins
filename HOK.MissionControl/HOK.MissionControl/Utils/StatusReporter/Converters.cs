using System;
using System.Windows.Data;
using System.Windows.Media;
using HOK.MissionControl.Tools.Communicator;

namespace HOK.MissionControl.Utils.StatusReporter
{
    /// <summary>
    /// Converts Status values to Colors used for the background.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return new SolidColorBrush(Colors.DarkGray);

            var status = (Status) value;
            switch (status)
            {
                case Status.Error:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(217, 83, 79));
                case Status.Info:
                    return new SolidColorBrush(Colors.DarkGray);
                case Status.Success:
                    return new SolidColorBrush(System.Windows.Media.Color.FromRgb(103, 184, 87));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts status values to image used in the window.
    /// </summary>
    public class StatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            if (value == null) return new Uri("pack://application:,,,/HOK.MissionControl;component/Resources/statusInfo_48x48.png");

            var status = (Status)value;
            switch (status)
            {
                case Status.Error:
                    return new Uri("pack://application:,,,/HOK.MissionControl;component/Resources/statusError_48x48.png");
                case Status.Info:
                    return new Uri("pack://application:,,,/HOK.MissionControl;component/Resources/statusInfo_48x48.png");
                case Status.Success:
                    return new Uri("pack://application:,,,/HOK.MissionControl;component/Resources/statusSuccess_48x48.png");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts status values to text message displayed in a header.
    /// </summary>
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "Info";

            var status = (Status)value;
            switch (status)
            {
                case Status.Error:
                    return "Error!";
                case Status.Info:
                    return "Info";
                case Status.Success:
                    return "Success!";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
