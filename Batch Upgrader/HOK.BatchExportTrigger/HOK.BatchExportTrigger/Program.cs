using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Security.Permissions;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, All = "HKEY_CURRENT_USER")]

namespace HOK.BatchExportTrigger
{
    public class Program
    {
        static void Main(string[] args)
        {
            RegistryKey registryKey=null;
            try
            {
                string programFile = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
#if RELEASE2014
                 string revitExe = programFile + "\\Autodesk\\Revit 2014\\Revit.exe";
#elif RELEASE2015
                string revitExe = programFile + "\\Autodesk\\Revit 2015\\Revit.exe";
#endif

                if (File.Exists(revitExe))
                {
#if RELEASE2014
                    string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\BatchUpgrader";
#elif RELEASE2015
                    string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2015\\BatchUpgrader";
#endif
                    
                    registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                    if (null == registryKey)
                    {
                        registryKey = Registry.CurrentUser.CreateSubKey(keyAddress);
                    }

                    string taskName =args[0];
                    registryKey.SetValue("CurrentTaskName", taskName);
                    registryKey.SetValue("ActivateAddIn", true);
                    bool runBatch = Convert.ToBoolean(registryKey.GetValue("ActivateAddIn").ToString());
                    if (runBatch)
                    {
                        System.Diagnostics.Process.Start(revitExe);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (null != registryKey)
                {
                    registryKey.SetValue("ActivateAddIn", false);
                }
            }
        }
    }
}
