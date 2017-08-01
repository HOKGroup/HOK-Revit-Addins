using System;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HOK.Core.WpfUtilities
{
    [ValueConversion(typeof(Bitmap), typeof(BitmapSource))]
    public class BitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null == value) return null;

            var bitmap = (Bitmap)value;
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(), 
                IntPtr.Zero, 
                System.Windows.Int32Rect.Empty, 
                BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
