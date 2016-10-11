using SolibriBatchSetup.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SolibriBatchSetup.Utils
{
    [ValueConversion(typeof(int), typeof(bool))]
    public class ExpanderConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isExpanded = false;
            if (null != value)
            {
                ProcessUnit unit = value as ProcessUnit;
                if (null != unit)
                {
                    isExpanded = true;
                }
            }
            return isExpanded;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class LabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = Visibility.Visible;
            if (null != value)
            {
                ProcessUnit unit = value as ProcessUnit;
                if (null != unit)
                {
                    visibility = Visibility.Hidden;
                }
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
