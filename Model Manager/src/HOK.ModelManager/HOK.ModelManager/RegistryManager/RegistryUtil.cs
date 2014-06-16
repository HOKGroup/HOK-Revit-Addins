using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace HOK.ModelManager.RegistryManager
{
    public static class RegistryUtil
    {
        //KeyNames: CompanyName, ModelBuilderActivated

        private static string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\ModelManager";
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
