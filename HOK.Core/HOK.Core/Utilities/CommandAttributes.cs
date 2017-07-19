using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace HOK.Core.Utilities
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        public string Name { get; protected set; }

        public NameAttribute(string resourceName, Type resourceType)
        {
            var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
            Name = value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; protected set; }

        public DescriptionAttribute(string resourceName, Type resourceType)
        {
            var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
            Description = value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ButtonTextAttribute : Attribute
    {
        public string ButtonText { get; protected set; }

        public ButtonTextAttribute(string resourceName, Type resourceType)
        {
            var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
            ButtonText = value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PanelNameAttribute : Attribute
    {
        public string PanelName { get; protected set; }

        public PanelNameAttribute(string resourceName, Type resourceType)
        {
            var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
            PanelName = value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImageAttribute : Attribute
    {
        public BitmapSource Image { get; protected set; }
        public string ImageName { get; protected set; }

        public ImageAttribute(string resourceName, Type resourceType)
        {
            //var bitmap = Properties.Resources.ResourceManager.GetObject(resourceName) as Bitmap;
            //if (bitmap != null)
            //{
            //    var bitmapImage = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
            //        IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            //    Image = bitmapImage;
            //}
            ImageName = resourceName;
        }
    }

    public class ResourceHelper
    {
        public static T GetResourceLookup<T>(Type resourceType, string resourceName)
        {
            if (resourceType == null || resourceName == null) return default(T);
            var property = resourceType.GetProperty(resourceName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (property == null)
            {
                return default(T);
            }

            return (T)property.GetValue(null, null);
        }
    }
}
