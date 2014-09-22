using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace HOK.Utilities
{
    class RegistryUtil
    {
        //KeyNames: InstalledOn
#if RELEASE2013
        private static string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2013\\UtilityTools";
#elif RELEASE2014
        private static string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\UtilityTools";
#elif RELEASE2015
        private static string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2015\\UtilityTools";
#endif

        public static void SetRegistryKey(string keyName, object value)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null == registryKey)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(keyAddress);
                }
                registryKey.SetValue(keyName, value);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static string GetRegistryKey(string keyName)
        {
            string value = "";
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
            if (null != registryKey)
            {
                string[] valueNames = registryKey.GetValueNames();
                if (valueNames.Contains(keyName))
                {
                    value = registryKey.GetValue(keyName).ToString();
                }
            }

            return value;
        }
    }
}
