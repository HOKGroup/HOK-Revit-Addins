using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HOK.SmartBCF.Schemas;
using System.Drawing;
using System.Windows;

namespace HOK.SmartBCF.Utils
{
    [ValueConversion(typeof(byte[]), typeof(ImageSource))]
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
            {
                return ImageConverter.Convert((byte[])value);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    public class BitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ImageSource imageSource = null;
            if (null != value)
            {
                Bitmap bitmap = (Bitmap)value;
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(48, 48));
                if (null != bitmapSource)
                {
                    imageSource = bitmapSource as ImageSource;
                }
            }
            return imageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class ImageConverter
    {
        public static ImageSource Convert(byte[] byteArray)
        {
            ImageSource imgSource = null;
            try
            {
                BitmapImage biImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(byteArray);
                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.EndInit();
                imgSource = biImg as ImageSource;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return imgSource;
        }
    }

    [ValueConversion(typeof(ObservableCollection<Component>), typeof(ObservableCollection<Component>))]
    public class ComponentFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
            {
                ObservableCollection<Component> components = (ObservableCollection<Component>)value;
                var revitComponents = from comp in components where !string.IsNullOrEmpty(comp.AuthoringToolId) select comp;
                if (revitComponents.Count() > 0)
                {
                    return new ObservableCollection<Component>(revitComponents.ToList());
                }
                else
                {
                    return new ObservableCollection<Component>();
                }
            }
            else
            {
                return new ObservableCollection<Component>();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
    public class CommentFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                ObservableCollection<Comment> comments = values[0] as ObservableCollection<Comment>;
                ViewPoint selectedViewpoint = values[1] as ViewPoint;
                if (null != comments && null != selectedViewpoint)
                {
                    var selectedComments = from comment in comments where comment.Viewpoint.Guid == selectedViewpoint.Guid select comment;
                    return new ObservableCollection<Comment>(selectedComments.ToList());
                }
                else
                {
                    return new ObservableCollection<Comment>();
                }
            }
            else
            {
                return new ObservableCollection<Comment>();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewPointIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string indexText = "";
            if (values.Length == 2)
            {
                Markup markup = values[0] as Markup;
                ViewPoint selectedViewpoint = values[1] as ViewPoint;
                if (null != markup && null != selectedViewpoint)
                {
                    int index = markup.Viewpoints.IndexOf(selectedViewpoint);
                    indexText = "( " + (index + 1) + " of " + markup.Viewpoints.Count + ")";
                }
            }
            return indexText;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(byte[]), typeof(System.Windows.Media.Brush))]
    public class ColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value)
            {
                byte[] color = (byte[])value;
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(color[0], color[1], color[2]));
            }
            else
            {
                return new SolidColorBrush(Colors.White);
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class StarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ? "*" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(ObservableCollection<RevitExtension>), typeof(ObservableCollection<RevitExtension>))]
    public class ExtensionFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (null != value && null != parameter)
            {
                ObservableCollection<RevitExtension> extensions = (ObservableCollection<RevitExtension>)value;
                var selectedExtensions = from ext in extensions where ext.ParameterName == parameter.ToString() select ext;
                if (selectedExtensions.Count() > 0)
                {
                    ObservableCollection<RevitExtension> extensionCollection = new ObservableCollection<RevitExtension>(selectedExtensions.ToList());
                    extensionCollection.Insert(0, extensions[0]);

                    return new ObservableCollection<RevitExtension>(extensionCollection);
                }
                else
                {
                    return new ObservableCollection<RevitExtension>();
                }
            }
            else
            {
                return new ObservableCollection<RevitExtension>();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum ComponentOption
    {
        OnlyVisible, SelectedElements, None
    }

    public class ComponentOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }

}
