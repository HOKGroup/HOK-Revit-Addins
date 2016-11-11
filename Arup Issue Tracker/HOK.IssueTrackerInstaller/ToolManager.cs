using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.IssueTrackerInstaller
{
    public class ToolManager
    {
        private string dataDirectory = "";
        private string addInDirectory = "";
        private string appDataDirectory = "";
        private string rvtDllFile = "";
        private string navDllFile = "";
        private ObservableCollection<string> rvtFiles = new ObservableCollection<string>();
        private ObservableCollection<string> navFiles = new ObservableCollection<string>();
        private ObservableCollection<ToolInfo> tools = new ObservableCollection<ToolInfo>();

        public ObservableCollection<ToolInfo> Tools { get { return tools; } set { tools = value; } }

        public ToolManager()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                dataDirectory = ApplicationDeployment.CurrentDeployment.DataDirectory;
            }
            appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            addInDirectory = appDataDirectory + @"\Autodesk\Revit\Addins\";
            

            rvtDllFile = @"\Arup.IssueTracker.Revit\ARUP.IssueTracker.Revit.dll";
            rvtFiles.Add(@"\ARUP.IssueTracker.Revit.addin");
            rvtFiles.Add(@"\Arup.IssueTracker.Revit\ARUP.IssueTracker.dll");
            rvtFiles.Add(@"\Arup.IssueTracker.Revit\ARUP.IssueTracker.Revit.dll");
            rvtFiles.Add(@"\Arup.IssueTracker.Revit\Arup.RestSharp.dll");
            rvtFiles.Add(@"\Arup.IssueTracker.Revit\Ionic.Zip.dll");

            navDllFile = @"\ARUP.IssueTracker.Navisworks\ARUP.IssueTracker.Navisworks.dll";
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\ARUP.IssueTracker.dll");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\ARUP.IssueTracker.Navisworks.dll");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\Arup.RestSharp.dll");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\en-US\RibbonDefinition.xaml");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\Ionic.Zip.dll");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\Images\ARUP.IssueTracker.Icon16x16.png");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\Images\ARUP.IssueTracker.Icon32x32.png");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\Images\icon.ico");
            navFiles.Add(@"\ARUP.IssueTracker.Navisworks\Images\snapshot.png");

            GetToolInfo();
        }

        public void GetToolInfo()
        {
            try
            {

                //desktop
                ToolInfo rvt2014Info = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker for Revit 2014",
                    ToolTypeEnum = ToolType.REVIT,
                    IsSelected = true,
                    Version = "2014",
                    InstallDirectory = addInDirectory,
                    SourceDirectory = dataDirectory,
                    DllPath = rvtDllFile,
                    Files = rvtFiles
                };
                tools.Add(rvt2014Info);

                ToolInfo rvt2015Info = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker for Revit 2015",
                    ToolTypeEnum = ToolType.REVIT,
                    IsSelected = true,
                    Version = "2015",
                    InstallDirectory = addInDirectory,
                    SourceDirectory = dataDirectory,
                    DllPath = rvtDllFile,
                    Files = rvtFiles
                };
                tools.Add(rvt2015Info);

                ToolInfo rvt2016Info = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker for Revit 2016",
                    ToolTypeEnum = ToolType.REVIT,
                    IsSelected = true,
                    Version = "2016",
                    InstallDirectory = addInDirectory,
                    SourceDirectory = dataDirectory,
                    DllPath = rvtDllFile,
                    Files = rvtFiles
                };
                tools.Add(rvt2016Info);

                ToolInfo nav2014Info = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker for Navisworks 2014",
                    ToolTypeEnum = ToolType.NAVISWORKS,
                    IsSelected = true,
                    Version = "2014",
                    InstallDirectory = appDataDirectory + @"\Autodesk Navisworks Manage 2014\Plugins",
                    SourceDirectory = dataDirectory,
                    DllPath = navDllFile,
                    Files = navFiles
                };
                tools.Add(nav2014Info);

                ToolInfo nav2015Info = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker for Navisworks 2015",
                    ToolTypeEnum = ToolType.NAVISWORKS,
                    IsSelected = true,
                    Version = "2015",
                    InstallDirectory = appDataDirectory + @"\Autodesk Navisworks Manage 2015\Plugins",
                    SourceDirectory = dataDirectory,
                    DllPath = navDllFile,
                    Files = navFiles
                };
                tools.Add(nav2015Info);

                ToolInfo nav2016Info = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker for Navisworks 2016",
                    ToolTypeEnum = ToolType.NAVISWORKS,
                    IsSelected = true,
                    Version = "2016",
                    InstallDirectory = appDataDirectory + @"\Autodesk Navisworks Manage 2016\Plugins",
                    SourceDirectory = dataDirectory,
                    DllPath = navDllFile,
                    Files = navFiles
                };
                tools.Add(nav2016Info); 

                ToolInfo desktopInfo = new ToolInfo()
                {
                    ToolName = "Arup Issue Tracker Desktop",
                    ToolTypeEnum = ToolType.DESKTOP,
                    IsSelected = true,
                    ExePath = "http://assets.hok.com/app/buildingsmart/ArupIssueTracker/Win/ARUP.IssueTracker.Win.application"
                };
                tools.Add(desktopInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get tool's information.\n" + ex.Message, "Get Tool Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public string GetUninstallString(string productDisplayName)
        {
            string uninstallString = "";
            try
            {
                RegistryKey currentUser = Registry.CurrentUser;
                string productsRoot = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
                RegistryKey products = currentUser.OpenSubKey(productsRoot);
                string[] productFolders = products.GetSubKeyNames();

                foreach (string p in productFolders)
                {
                    RegistryKey key = products.OpenSubKey(p);
                    string displayName = (string)key.GetValue("displayName");
                    if (null != displayName && displayName == productDisplayName)
                    {
                        uninstallString = (string)key.GetValue("UninstallString");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return uninstallString;
        }
    }

    public enum ToolType
    {
        NONE, DESKTOP, REVIT, NAVISWORKS 
    }

    public class ToolInfo:INotifyPropertyChanged
    {
        private string toolName = "";
        private ToolType toolTypeEnum = ToolType.NONE;
        private bool isSelected = false;
        private string version = "";
        private string installDirectory = "";
        private string sourceDirectory = "";
        private string dllPath = "";
        //for clickOnce
        private string exePath = "";
        private ObservableCollection<string> files = new ObservableCollection<string>();

        public string ToolName { get { return toolName; } set { toolName = value; NotifyPropertyChanged("ToolName"); } }
        public ToolType ToolTypeEnum { get { return toolTypeEnum; } set { toolTypeEnum = value; NotifyPropertyChanged("ToolTypeEnum"); } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; NotifyPropertyChanged("IsSelected"); } }
        public string Version { get { return version; } set { version = value; NotifyPropertyChanged("Version"); } }
        public string InstallDirectory { get { return installDirectory; } set { installDirectory = value; NotifyPropertyChanged("InstallDirectory"); } }
        public string SourceDirectory { get { return sourceDirectory; } set { sourceDirectory = value; NotifyPropertyChanged("SourceDirectory"); } }
        public string DllPath { get { return dllPath; } set { dllPath = value; NotifyPropertyChanged("DllPath"); } }

        public string ExePath { get { return exePath; } set { exePath = value; NotifyPropertyChanged("ExePath"); } }
        public ObservableCollection<string> Files { get { return files; } set { files = value; NotifyPropertyChanged("Files"); } }

        public ToolInfo()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
