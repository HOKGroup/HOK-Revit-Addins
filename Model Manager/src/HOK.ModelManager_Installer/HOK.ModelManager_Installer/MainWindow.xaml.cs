using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Security.Permissions;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum, All = "HKEY_CURRENT_USER")]

namespace HOK.ModelManager_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string addInPath2014 = "";
        private string dataDirectory = "";
        private string keyAddress = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\ModelManager";

        public MainWindow()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            addInPath2014 = appData + @"\Autodesk\Revit\Addins\2014";
           
            InitializeComponent();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                dataDirectory = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            this.Title = "HOK Tools Installer - Model Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            labelReleaseNum.Content = GetReleasedFileVersion();
            labelInstalledNum.Content = GetInstalledFileVersion();
 
        }

        private string GetReleasedFileVersion()
        {
            string versionNumber = "";
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    System.Diagnostics.FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(dataDirectory + "\\2014\\HOK.ModelManager.dll");
                    if (null != versionInfo)
                    {
                        versionNumber = "v." + versionInfo.FileVersion;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the released version number.\n"+ex.Message, "Released Version Number", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return versionNumber;
        }

        private string GetInstalledFileVersion()
        {
            string versionNumber = "";
            try
            {
                string dllPath = addInPath2014 + "\\HOK-Addin.bundle\\Contents_External\\HOK.ModelManager.dll";
                if (File.Exists(dllPath))
                {
                    System.Diagnostics.FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(dllPath);
                    if (null != versionInfo)
                    {
                        versionNumber = "v." + versionInfo.FileVersion;
                        buttonUninstall.IsEnabled = true;
                    }
                }
                else
                {
                    versionNumber = "Not Installed";
                    buttonUninstall.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the installed version number.\n" + ex.Message, "Installed Version Number", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return versionNumber;
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCompany.Text))
            {
                MessageBox.Show("Please enter a valid company name in the text box.", "Company Name Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (InstallModelManager())
                {
                    SetRegistryKey("CompanyName", textBoxCompany.Text);
                    SetRegistryKey("ModelBuilderActivated", false);

                    MessageBoxResult msResult = MessageBox.Show("The HOK Model Manager is successfully installed!!\nWould you like to finish the installer?", "Completed Installation", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (msResult == MessageBoxResult.OK)
                    {
                        this.Close();
                    }
                }
            }
        }

        private bool InstallModelManager()
        {
            bool result = false;
            try
            {
                if (CreateDirectories())
                {
                    Dictionary<string/*fileName*/, string/*path*/> filesDictionary = new Dictionary<string, string>();
                    filesDictionary.Add("\\HOK.ModelManager.addin", "\\HOK.ModelManager.addin");
                    filesDictionary.Add("\\HOK.ModelManager.dll", "\\HOK-Addin.bundle\\Contents_External\\HOK.ModelManager.dll");
                    filesDictionary.Add("\\Google.GData.AccessControl.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.AccessControl.dll");
                    filesDictionary.Add("\\Google.GData.Client.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Client.dll");
                    filesDictionary.Add("\\Google.GData.Documents.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Documents.dll");
                    filesDictionary.Add("\\Google.GData.Extensions.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Extensions.dll");
                    filesDictionary.Add("\\Google.GData.Spreadsheets.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Spreadsheets.dll");
                    filesDictionary.Add("\\Newtonsoft.Json.dll", "\\HOK-Addin.bundle\\Contents_External\\Newtonsoft.Json.dll");
                    filesDictionary.Add("\\System.Runtime.dll", "\\HOK-Addin.bundle\\Contents_External\\System.Runtime.dll");
                    filesDictionary.Add("\\model.png", "\\HOK-Addin.bundle\\Contents_External\\Resources\\model.png");
                    filesDictionary.Add("\\project.png", "\\HOK-Addin.bundle\\Contents_External\\Resources\\project.png");

                    string sourceDirectory=dataDirectory+"\\2014";
                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        foreach (string fileName in filesDictionary.Keys)
                        {
                            string filePath = filesDictionary[fileName];

                            File.Copy(sourceDirectory + fileName, addInPath2014 + filePath, true);
                        }
                    }

                    if (GetReleasedFileVersion() == GetInstalledFileVersion())
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install the Model Manager.\n"+ex.Message, "Install Model Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        private bool CreateDirectories()
        {
            bool result = false;
            try
            {
                if (Directory.Exists(addInPath2014))
                {
                    if (!Directory.Exists(addInPath2014 + "\\HOK-Addin.bundle"))
                    {
                        Directory.CreateDirectory(addInPath2014 + "\\HOK-Addin.bundle");
                    }
                    if (!Directory.Exists(addInPath2014 + "\\HOK-Addin.bundle\\Contents_External"))
                    {
                        Directory.CreateDirectory(addInPath2014 + "\\HOK-Addin.bundle\\Contents_External");
                    }
                    if (!Directory.Exists(addInPath2014 + "\\HOK-Addin.bundle\\Contents_External\\Resources"))
                    {
                        Directory.CreateDirectory(addInPath2014 + "\\HOK-Addin.bundle\\Contents_External\\Resources");
                    }

                    if (Directory.Exists(addInPath2014 + "\\HOK-Addin.bundle\\Contents_External\\Resources"))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create Addins directories.\n"+ex.Message, "Create Directories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private void SetRegistryKey(string keyName, object value)
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
                MessageBox.Show(keyName+": Failed to set registry key.\n"+ex.Message, "Set Registry Key", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUninstall_Click(object sender, RoutedEventArgs e)
        {
            if (UninstallModelManager())
            {
                Registry.CurrentUser.DeleteSubKey(keyAddress, false);
                MessageBoxResult msResult = MessageBox.Show("The HOK Model Manager is successfully removed!!\nWould you like to finish the installer?", "Completed Uninstallation", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (msResult == MessageBoxResult.OK)
                {
                    this.Close();
                }
            }
        }

        private bool UninstallModelManager()
        {
            bool result = false;
            try
            {
                Dictionary<string/*fileName*/, string/*path*/> filesDictionary = new Dictionary<string, string>();
                filesDictionary.Add("\\HOK.ModelManager.addin", "\\HOK.ModelManager.addin");
                filesDictionary.Add("\\HOK.ModelManager.dll", "\\HOK-Addin.bundle\\Contents_External\\HOK.ModelManager.dll");
                filesDictionary.Add("\\Google.GData.AccessControl.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.AccessControl.dll");
                filesDictionary.Add("\\Google.GData.Client.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Client.dll");
                filesDictionary.Add("\\Google.GData.Documents.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Documents.dll");
                filesDictionary.Add("\\Google.GData.Extensions.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Extensions.dll");
                filesDictionary.Add("\\Google.GData.Spreadsheets.dll", "\\HOK-Addin.bundle\\Contents_External\\Google.GData.Spreadsheets.dll");
                filesDictionary.Add("\\Newtonsoft.Json.dll", "\\HOK-Addin.bundle\\Contents_External\\Newtonsoft.Json.dll");
                filesDictionary.Add("\\System.Runtime.dll", "\\HOK-Addin.bundle\\Contents_External\\System.Runtime.dll");
                filesDictionary.Add("\\model.png", "\\HOK-Addin.bundle\\Contents_External\\Resources\\model.png");
                filesDictionary.Add("\\project.png", "\\HOK-Addin.bundle\\Contents_External\\Resources\\project.png");

                foreach (string fileName in filesDictionary.Keys)
                {
                    string filePath = filesDictionary[fileName];
                    if (File.Exists(addInPath2014 + filePath))
                    {
                        File.Delete(addInPath2014 + filePath);
                    }
                }
                string dllPath = addInPath2014 + "\\HOK-Addin.bundle\\Contents_External\\HOK.ModelManager.dll";
                if (!File.Exists(dllPath))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uninstall the HOK Model Manager.\n"+ex.Message, "Uninstall Model Manager", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
