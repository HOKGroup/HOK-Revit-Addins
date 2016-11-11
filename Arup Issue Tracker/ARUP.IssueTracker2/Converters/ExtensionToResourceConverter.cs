using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using ARUP.IssueTracker.UserControls;
using System.Windows.Media;

namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(String), typeof(Canvas))]
    public class ExtensionToResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string extension = (string)value;
            string resourceKey = "other";
            switch (extension)
            {
                case ".doc":
                    resourceKey = "doc";
                    break;
                case ".docx":
                    resourceKey = "doc";
                    break;
                case ".pdf":
                    resourceKey = "pdf";
                    break;
                case ".jpg":
                    resourceKey = "jpg";
                    break;
                case ".jpeg":
                    resourceKey = "jpg";
                    break;
                case ".png":
                    resourceKey = "jpg";
                    break;
                case ".bmp":
                    resourceKey = "jpg";
                    break;
                case ".gif":
                    resourceKey = "jpg";
                    break;
                case ".ppt":
                    resourceKey = "ppt";
                    break;
                case ".pptx":
                    resourceKey = "ppt";
                    break;
                case ".html":
                    resourceKey = "html";
                    break;
                case ".xls":
                    resourceKey = "xls";
                    break;
                case ".xlsx":
                    resourceKey = "xls";
                    break;
                case ".xlsm":
                    resourceKey = "xls";
                    break;
            }

            UserControl jiraPan = parameter as UserControl;
            return jiraPan.FindResource(resourceKey) as Canvas;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        public static DependencyObject FindChild(DependencyObject parent, string name)
        {
            // confirm parent and name are valid.
            if (parent == null || string.IsNullOrEmpty(name)) return null;

            if (parent is FrameworkElement && (parent as FrameworkElement).Name == name) return parent;

            DependencyObject result = null;

            if (parent is FrameworkElement) (parent as FrameworkElement).ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                result = FindChild(child, name);
                if (result != null) break;
            }

            return result;
        }


    }
}
