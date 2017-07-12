using System.Windows;
using System.Windows.Media;

namespace HOK.Core.WpfUtilities
{
    public static class DataGrid
    {
        public static T FindVisualChild<T>(Visual parent) where T : Visual
        {
            var child = default(T);
            try
            {
                var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (var i = 0; i < numVisuals; i++)
                {
                    var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T ?? FindVisualChild<T>(v);
                    if (child != null)
                    {
                        break;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return child;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            try
            {
                var parent = element;
                while (parent != null)
                {
                    var correctlyTyped = parent as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }
    }
}
