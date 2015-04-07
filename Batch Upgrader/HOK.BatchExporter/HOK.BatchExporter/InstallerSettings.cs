using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using System.Security.Permissions;
using System.Reflection;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, All = "HKEY_CURRENT_USER")]

namespace HOK.BatchExporter
{
    public class InstallerSettings
    {
        private string revitVersion = "";
        private string keyAddress = "";
        private string masterDirectory = "";
        private string addinFolder ="";
        private string assemblyVersion = "";

        private string[] filesToCopy = new string[]
        {
            @"\HOK.BatchUpgraderAddIn.addin",
            @"\HOK-BatchProcessor.bundle\Contents\DBManagedServices.dll",
            @"\HOK-BatchProcessor.bundle\Contents\HOK.BatchUpgraderAddIn.dll",
            @"\HOK-BatchProcessor.bundle\Contents\HOK.BatchUpgradTrigger.exe",
            @"\HOK-BatchProcessor.bundle\Contents\Microsoft.Win32.TaskScheduler.dll",
            @"\HOK-BatchProcessor.bundle\Contents\Resources\BatchUpgradeConfiguration.xml"
        };


        public InstallerSettings(string versionNumber)
        {
            revitVersion = versionNumber;
            keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit " + revitVersion + "\\BatchUpgrader";
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK Batch Upgrader\Addins\" + revitVersion;
            addinFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + revitVersion;
            assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public bool CheckInstallation()
        {
            bool result = false;
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                if (null == registryKey)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(keyAddress);
                }

                if (null == registryKey.GetValue("BatchUpgraderInstalled"))
                {
                    registryKey.SetValue("BatchUpgraderInstalled", false);
                }
                if (null == registryKey.GetValue("BatchUpgraderVersion"))
                {
                    registryKey.SetValue("BatchUpgraderVersion", "");
                }

                bool installed = Convert.ToBoolean(registryKey.GetValue("BatchUpgraderInstalled").ToString());
                string version = registryKey.GetValue("BatchUpgraderVersion").ToString();

                if (installed == true && version == assemblyVersion)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caanot check the status of the installation.\n"+ex.Message, "CheckInstallation", MessageBoxButton.OK);
                return false;
            }
            return result;
        }

        public void InstallFiles()
        {
            if (CopyFilesFromMaster())
            {
                SetInstalled();
            }
        }

        private void SetInstalled()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyAddress, true);
                registryKey.SetValue("BatchUpgraderInstalled", true);
                registryKey.SetValue("BatchUpgraderVersion", assemblyVersion);
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool CopyFilesFromMaster()
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(addinFolder)) { Directory.CreateDirectory(addinFolder); }

                string bundleFolder = addinFolder + @"\HOK-BatchProcessor.bundle";
                if (!Directory.Exists(bundleFolder)) { Directory.CreateDirectory(bundleFolder); }

                string contentFolder = bundleFolder + @"\Contents";
                if (!Directory.Exists(contentFolder)) { Directory.CreateDirectory(contentFolder); }

                string resourceFolder = contentFolder + @"\Resources";
                if (!Directory.Exists(resourceFolder)) { Directory.CreateDirectory(resourceFolder); }

                
                foreach (string fileName in filesToCopy)
                {
                    if (File.Exists(masterDirectory + fileName))
                    {
                        try { File.Copy(masterDirectory + fileName, addinFolder + fileName, true); }
                        catch { continue; }
                    }
                }
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caanot copy files from master folder.\n" + ex.Message, "CopyFilesFromMaster", MessageBoxButton.OK);
                return false;
            }
            return result;
        }
    }
}
