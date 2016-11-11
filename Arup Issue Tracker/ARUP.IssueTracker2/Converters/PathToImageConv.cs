using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Configuration;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(String), typeof(BitmapImage))]
    public class PathToImageConv : IValueConverter
    {
        Configuration config;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!string.IsNullOrEmpty(value.ToString()))
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(value.ToString());
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                return bi;
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }



    }
}
