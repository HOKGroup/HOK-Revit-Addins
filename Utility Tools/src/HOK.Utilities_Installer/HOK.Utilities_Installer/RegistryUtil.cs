using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace HOK.Utilities_Installer
{
    public enum RevitVersion
    {
        None,
        Revit2013,
        Revit2014,
        Revit2015
    }

    [assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, All = "HKEY_CURRENT_USER")]

    class RegistryUtil
    {
        //KeyNames: InstalledOn

        private static string keyAddress2013 = "Software\\Autodesk\\Revit\\Autodesk Revit 2013\\UtilityTools";
        private static string keyAddress2014 = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\UtilityTools";
        private static string keyAddress2015 = "Software\\Autodesk\\Revit\\Autodesk Revit 2015\\UtilityTools";

        public static void SetRegistryKey(RevitVersion revitVersion, string keyName, object value)
        {
            try
            {
                string keyAddress = "";
                switch (revitVersion)
                {
                    case RevitVersion.Revit2013:
                        keyAddress = keyAddress2013;
                        break;
                    case RevitVersion.Revit2014:
                        keyAddress = keyAddress2014;
                        break;
                    case RevitVersion.Revit2015:
                        keyAddress = keyAddress2015;
                        break;
                }
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

        public static string GetRegistryKey(RevitVersion revitVersion, string keyName)
        {
            string value = "";
            string keyAddress = "";
            switch (revitVersion)
            {
                case RevitVersion.Revit2013:
                    keyAddress = keyAddress2013;
                    break;
                case RevitVersion.Revit2014:
                    keyAddress = keyAddress2014;
                    break;
                case RevitVersion.Revit2015:
                    keyAddress = keyAddress2015;
                    break;
            }
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
