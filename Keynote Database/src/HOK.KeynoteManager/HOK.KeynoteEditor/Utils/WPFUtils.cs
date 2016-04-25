using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HOK.KeynoteEditor.Utils
{
    public static class WPFUtils
    {
        public static T FindVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            try
            {
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                    {
                        child = FindVisualChild<T>(v);
                    }
                    if (child != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return child;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            try
            {
                UIElement parent = element;
                while (parent != null)
                {
                    T correctlyTyped = parent as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return null;
        }
    }
}
