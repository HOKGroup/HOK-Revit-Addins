using HOK.AddInManager.Classes;
using HOK.AddInManager.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace HOK.AddInManager.Utils
{
    public class ColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush color = new SolidColorBrush(Colors.Black);
            if (null != value)
            {
                LoadType loadType = (LoadType)value;
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
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
