using System;
using System.Reflection;

namespace HOK.Core.Utilities
{
    /// <summary>
    /// Name of the Addin.
    /// </summary>
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

    /// <summary>
    /// Description used by this addin on Tooltip.
    /// </summary>
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

    /// <summary>
    /// Text displayed on the button. 
    /// </summary>
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

    /// <summary>
    /// Name of the Panel on a Ribbon that this addin is in.
    /// </summary>
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

    /// <summary>
    /// Fully qualified name of the Namespace that Resource folder is in. Usually main class namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NamespaceAttribute : Attribute
    {
        public string Namespace { get; protected set; }

        public NamespaceAttribute(string resourceName, Type resourceType)
        {
            var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
            Namespace = value;
        }
    }

    ///// <summary>
    ///// Name of the *.addin manifest file that this addin goes with.
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Class)]
    //public class AddinNameAttribute : Attribute
    //{
    //    public string AddinName { get; protected set; }

    //    public AddinNameAttribute(string resourceName, Type resourceType)
    //    {
    //        var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
    //        AddinName = value;
    //    }
    //}

    /// <summary>
    /// Name of the Icon image resource. Has to be of Embedded Resource type, and NOT loaded via resex.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ImageAttribute : Attribute
    {
        public string ImageName { get; protected set; }

        public ImageAttribute(string resourceName, Type resourceType)
        {
            var value = ResourceHelper.GetResourceLookup<string>(resourceType, resourceName);
            ImageName = value;
        }
    }

    /// <summary>
    /// Resource lookup that allows to retrieve resource from DLL.
    /// </summary>
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
