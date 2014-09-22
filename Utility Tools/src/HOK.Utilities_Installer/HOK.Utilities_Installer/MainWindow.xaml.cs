using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.Utilities_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string addInPath2013 = "";
        private string addInPath2014 = "";
        private string addInPath2015 = "";
        private string dataDirectory = "";

        private Dictionary<string/*fileName*/, string/*path*/> filesDictionary = new Dictionary<string, string>();
        public MainWindow()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            addInPath2013 = appData + @"\Autodesk\Revit\Addins\2013";
            addInPath2014 = appData + @"\Autodesk\Revit\Addins\2014";
            addInPath2015 = appData + @"\Autodesk\Revit\Addins\2015";
            InitializeComponent();

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                dataDirectory = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            this.Title = "HOK Tools Installer - Room Updater v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            filesDictionary = new Dictionary<string, string>();
            filesDictionary.Add("\\HOK.Utilities.addin", "\\HOK.Utilities.addin");
            filesDictionary.Add("\\HOK.Utilities.dll", "\\HOK-Addin.bundle\\Contents_External\\HOK.Utilities.dll");
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool installed = false;
                string keyName = "InstalledOn";
                if ((bool)checkBox2013.IsChecked)
                {
                    string installedDate = RegistryUtil.GetRegistryKey(RevitVersion.Revit2013, keyName);
                    if (string.IsNullOrEmpty(installedDate))
                    {
                        installedDate = DateTime.Now.ToString();
                        RegistryUtil.SetRegistryKey(RevitVersion.Revit2013, keyName, installedDate);
                    }
                    if (CreateDirectories(addInPath2013))
                    {
                        installed = CopyAddInFiles(RevitVersion.Revit2013);
                    }
                }
                if ((bool)checkBox2014.IsChecked)
                {
                    string installedDate = RegistryUtil.GetRegistryKey(RevitVersion.Revit2014, keyName);
                    if (string.IsNullOrEmpty(installedDate))
                    {
                        installedDate = DateTime.Now.ToString();
                        RegistryUtil.SetRegistryKey(RevitVersion.Revit2014, keyName, installedDate);
                    }
                    if (CreateDirectories(addInPath2014))
                    {
                        installed = CopyAddInFiles(RevitVersion.Revit2014);
                    }
                }
                if ((bool)checkBox2015.IsChecked)
                {
                    string installedDate = RegistryUtil.GetRegistryKey(RevitVersion.Revit2015, keyName);
                    if (string.IsNullOrEmpty(installedDate))
                    {
                        installedDate = DateTime.Now.ToString();
                        RegistryUtil.SetRegistryKey(RevitVersion.Revit2015, keyName, installedDate);
                    }
                    if (CreateDirectories(addInPath2015))
                    {
                        installed = CopyAddInFiles(RevitVersion.Revit2015);
                    }
                }

                if (installed)
                {
                    MessageBoxResult result = MessageBox.Show("Utility Tools have been successfully installed.", "Installation Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (MessageBoxResult.OK == result)
                    {
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install Utility Tools.\n"+ex.Message, "Install Utility Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                MessageBox.Show("Failed to create Addins directories.\n" + ex.Message, "Create Directories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private bool CopyAddInFiles(RevitVersion version)
        {
            bool copied = false;
            try
            {
                string versionNumber = "";
                string addInPath = "";
                switch (version)
                {
                    case RevitVersion.Revit2013:
                        versionNumber = "2013";
                        addInPath = addInPath2013;
                        break;
                    case RevitVersion.Revit2014:
                        versionNumber = "2014";
                        addInPath = addInPath2014;
                        break;
                    case RevitVersion.Revit2015:
                        versionNumber = "2015";
                        addInPath = addInPath2015;
                        break;
                }

                string sourceDirectory = dataDirectory + "\\" + versionNumber;
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    foreach (string fileName in filesDictionary.Keys)
                    {
                        string filePath = filesDictionary[fileName];

                        File.Copy(sourceDirectory + fileName, addInPath + filePath, true);
                    }
                }
                copied = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy addIn files.\n"+ex.Message, "Copy AddIn Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return copied;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonUninstall_Click(object sender, RoutedEventArgs e)
        {
            bool uninstalled = false;
            if ((bool)checkBox2013.IsChecked)
            {
                uninstalled = UninstallTools(addInPath2013);
            }
            if ((bool)checkBox2014.IsChecked)
            {
                uninstalled = UninstallTools(addInPath2014);
            }
            if ((bool)checkBox2015.IsChecked)
            {
                uninstalled = UninstallTools(addInPath2015);
            }
            if (uninstalled)
            {
                MessageBoxResult result = MessageBox.Show("Utility Tools have been successfully uninstalled.", "Uninstallation Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                if (result == MessageBoxResult.OK)
                {
                    this.Close();
                }
            }
        }

        private bool UninstallTools(string addInPath)
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
                string dllPath = addInPath + "\\HOK-Addin.bundle\\Contents_External\\HOK.Utilities.dll";
                if (!File.Exists(dllPath))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uninstall.\n" + ex.Message, "Uninstall Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

    }
}
