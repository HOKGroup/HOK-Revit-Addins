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
    public enum InstallVersion
    {
        None,
        ModelManager2014,
        ModelManager2015,
        ModelManagerBoth
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string addInPath2014 = "";
        private string addInPath2015 = "";
        private string dataDirectory = "";
        private string keyAddress2014 = "Software\\Autodesk\\Revit\\Autodesk Revit 2014\\ModelManager";
        private string keyAddress2015 = "Software\\Autodesk\\Revit\\Autodesk Revit 2015\\ModelManager";
        private InstallVersion version = InstallVersion.None;
        private Dictionary<string/*fileName*/, string/*path*/> filesDictionary = new Dictionary<string, string>();

        public MainWindow()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            addInPath2014 = appData + @"\Autodesk\Revit\Addins\2014";
            addInPath2015 = appData + @"\Autodesk\Revit\Addins\2015";

            InitializeComponent();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                dataDirectory = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            this.Title = "HOK Tools Installer - Model Manager v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            filesDictionary = new Dictionary<string, string>();
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
        }
       

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCompany.Text))
            {
                MessageBox.Show("Please enter a valid company name in the text box.", "Company Name Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (radioButton2014.IsChecked == true) { version = InstallVersion.ModelManager2014; }
                else if (radioButton2015.IsChecked == true) { version = InstallVersion.ModelManager2015; }
                else if (radioButtonBoth.IsChecked == true) { version = InstallVersion.ModelManagerBoth; }


                switch (version)
                {
                    case InstallVersion.ModelManager2014:
                        if (InstallModelManager(version))
                        {
                            SetRegistryKey(keyAddress2014, "CompanyName", textBoxCompany.Text);
                            SetRegistryKey(keyAddress2014, "ModelBuilderActivated", false);
                        }
                        break;
                    case InstallVersion.ModelManager2015:
                        if (InstallModelManager(version))
                        {
                            SetRegistryKey(keyAddress2015, "CompanyName", textBoxCompany.Text);
                            SetRegistryKey(keyAddress2015, "ModelBuilderActivated", false);
                        }
                        break;
                    case InstallVersion.ModelManagerBoth:
                        if (InstallModelManager(InstallVersion.ModelManager2014))
                        {
                            SetRegistryKey(keyAddress2014, "CompanyName", textBoxCompany.Text);
                            SetRegistryKey(keyAddress2014, "ModelBuilderActivated", false);
                        }
                        if (InstallModelManager(InstallVersion.ModelManager2015))
                        {
                            SetRegistryKey(keyAddress2015, "CompanyName", textBoxCompany.Text);
                            SetRegistryKey(keyAddress2015, "ModelBuilderActivated", false);
                        }
                        break;
                }

                MessageBoxResult msResult = MessageBox.Show("The HOK Model Manager is successfully installed!!\nWould you like to finish the installer?", "Completed Installation", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (msResult == MessageBoxResult.OK)
                {
                    this.Close();
                }
            }
        }

        private bool InstallModelManager(InstallVersion installVersion)
        {
            bool result = false;
            try
            {
                string addInPath="";
                string versionNumber = "";
                if (installVersion == InstallVersion.ModelManager2014) { addInPath = addInPath2014; versionNumber = "2014";  }
                else if (installVersion == InstallVersion.ModelManager2015) { addInPath = addInPath2015; versionNumber="2015"; }

                if (CreateDirectories(addInPath))
                {
                    string sourceDirectory=dataDirectory+"\\"+versionNumber;
                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        foreach (string fileName in filesDictionary.Keys)
                        {
                            string filePath = filesDictionary[fileName];

                            File.Copy(sourceDirectory + fileName, addInPath + filePath, true);
                        }
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

        private bool CreateDirectories(string addInPath)
        {
            bool result = false;
            try
            {
                if (Directory.Exists(addInPath))
                {
                    if (!Directory.Exists(addInPath + "\\HOK-Addin.bundle"))
                    {
                        Directory.CreateDirectory(addInPath + "\\HOK-Addin.bundle");
                    }
                    if (!Directory.Exists(addInPath + "\\HOK-Addin.bundle\\Contents_External"))
                    {
                        Directory.CreateDirectory(addInPath + "\\HOK-Addin.bundle\\Contents_External");
                    }
                    if (!Directory.Exists(addInPath + "\\HOK-Addin.bundle\\Contents_External\\Resources"))
                    {
                        Directory.CreateDirectory(addInPath + "\\HOK-Addin.bundle\\Contents_External\\Resources");
                    }

                    if (Directory.Exists(addInPath + "\\HOK-Addin.bundle\\Contents_External\\Resources"))
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

        private void SetRegistryKey(string keyAddress, string keyName, object value)
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
            try
            {
                if (radioButton2014.IsChecked == true) { version = InstallVersion.ModelManager2014; }
                else if (radioButton2015.IsChecked == true) { version = InstallVersion.ModelManager2015; }
                else if (radioButtonBoth.IsChecked == true) { version = InstallVersion.ModelManagerBoth; }

                switch (version)
                {
                    case InstallVersion.ModelManager2014:
                        if (UninstallModelManager(addInPath2014))
                        {
                            try { Registry.CurrentUser.DeleteSubKey(keyAddress2014, false); }
                            catch { }
                        }
                        break;
                    case InstallVersion.ModelManager2015:
                        if (UninstallModelManager(addInPath2015))
                        {
                            try { Registry.CurrentUser.DeleteSubKey(keyAddress2015, false); }
                            catch { }
                        }
                        break;
                    case InstallVersion.ModelManagerBoth:
                        if (UninstallModelManager(addInPath2014))
                        {
                            try { Registry.CurrentUser.DeleteSubKey(keyAddress2014, false); }
                            catch { }
                        }
                        if (UninstallModelManager(addInPath2015))
                        {
                            try { Registry.CurrentUser.DeleteSubKey(keyAddress2015, false); }
                            catch { }
                        }
                        break;
                }
            }
            catch { }

            MessageBoxResult msResult = MessageBox.Show("The HOK Model Manager is successfully removed!!\nWould you like to finish the installer?", "Completed Uninstallation", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (msResult == MessageBoxResult.OK)
            {
                this.Close();
            }
        }

        private bool UninstallModelManager(string addInPath)
        {
            bool result = false;
            try
            {
                foreach (string fileName in filesDictionary.Keys)
                {
                    string filePath = filesDictionary[fileName];
                    if (File.Exists(addInPath + filePath))
                    {
                        File.Delete(addInPath + filePath);
                    }
                }
                string dllPath = addInPath + "\\HOK-Addin.bundle\\Contents_External\\HOK.ModelManager.dll";
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
