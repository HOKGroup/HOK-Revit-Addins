using System;
using System.Windows.Data;
using System.Windows.Media;
using HOK.AddInManager.Classes;

namespace HOK.AddInManager.Utils
{
    public class ColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var color = new SolidColorBrush(Colors.Black);
            if (null == value) return color;

            var loadType = (LoadType)value;
            switch (loadType)
            {
                case LoadType.Always:
                    color = new SolidColorBrush(Colors.Red);
                    break;
                case LoadType.ThisSessionOnly:
                    color = new SolidColorBrush(Colors.Blue);
                    break;
                case LoadType.Never:
                    break;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
