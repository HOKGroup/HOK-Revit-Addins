using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Diagnostics;
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

namespace HOK.IssueTrackerInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ToolManager toolManager;
        private ObservableCollection<ToolInfo> tools = new ObservableCollection<ToolInfo>();

        public MainWindow()
        {
            InitializeComponent();
            toolManager = new ToolManager();
            tools = toolManager.Tools;
            listBoxTools.ItemsSource = tools;
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ToolInfo info in tools)
                {
                    if (info.IsSelected)
                    {
                        bool installed = InstallTool(info);
                        if (installed)
                        {
                            statusLable.Text = "Installed.";
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool InstallTool(ToolInfo info)
        {
            bool installed = false;
            try
            {
                if (info.ToolTypeEnum == ToolType.REVIT)
                {
                    if (!Directory.Exists(info.InstallDirectory + info.Version)) { return false; }
                    string addInDirectory = info.InstallDirectory + info.Version + @"\Arup.IssueTracker.Revit";
                    if (!Directory.Exists(addInDirectory))
                    {
                        Directory.CreateDirectory(addInDirectory);
                    }

                    if (ApplicationDeployment.IsNetworkDeployed)
                    {

                        foreach (string file in info.Files)
                        {
                            string sourceFile = info.SourceDirectory + @"\Revit\" + info.Version + file;
                            string installPath = info.InstallDirectory + info.Version + file;
                            File.Copy(sourceFile, installPath, true);
                        }
                    }

                    string dllPath = info.InstallDirectory + info.Version + info.DllPath;
                    if (File.Exists(dllPath))
                    {
                        installed = true;
                    }
                }
                else if (info.ToolTypeEnum == ToolType.NAVISWORKS)
                {
                    if (!Directory.Exists(info.InstallDirectory)) { return false; }
                    string pluginDirectory = info.InstallDirectory + @"\ARUP.IssueTracker.Navisworks";
                    if (!Directory.Exists(pluginDirectory))
                    {
                        Directory.CreateDirectory(pluginDirectory);

                        string langDirectory = System.IO.Path.Combine(pluginDirectory, "en-US");
                        Directory.CreateDirectory(langDirectory);
                        string imageDirectory = System.IO.Path.Combine(pluginDirectory, "Images");
                        Directory.CreateDirectory(imageDirectory);
                    }

                    if (ApplicationDeployment.IsNetworkDeployed)
                    {
                        foreach (string file in info.Files)
                        {
                            string sourceFile = info.SourceDirectory + @"\Navisworks\" + info.Version + file;
                            string installPath = info.InstallDirectory + file;
                            File.Copy(sourceFile, installPath, true);
                        }
                    }

                    string dllPath = info.InstallDirectory + info.DllPath;
                    if (File.Exists(dllPath))
                    {
                        installed = true;
                    }
                }
                else if (info.ToolTypeEnum == ToolType.DESKTOP)
                {
                    MessageBoxResult result = MessageBox.Show("The desktop version of " + info.ToolName + " will be installed.\nPlease follow the on-screen instructions to complete the process.", "ClickOnce Installeation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.OK)
                    {
                        Process.Start(info.ExePath);
                        installed = true;
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(info.ToolName + "\nFailed to install tool.\n" + ex.Message, "Install Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return installed;
        }

        private void buttonUninstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ToolInfo info in tools)
                {
                    if (info.IsSelected)
                    {
                        bool removed = UninstallTool(info);
                        if (removed)
                        {
                            statusLable.Text = "Removed.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool UninstallTool(ToolInfo info)
        {
            bool removed = false;
            try
            {
                if (info.ToolTypeEnum == ToolType.REVIT)
                {
                    foreach (string file in info.Files)
                    {
                        string installPath = info.InstallDirectory + info.Version + file;
                        File.Delete(installPath);
                    }

                    string addInDirectory = info.InstallDirectory + info.Version + @"\Arup.IssueTracker.Revit";
                    if (Directory.Exists(addInDirectory))
                    {
                        Directory.Delete(addInDirectory, true);
                    }

                    string dllPath = info.InstallDirectory + info.Version + info.DllPath;
                    if (!File.Exists(dllPath))
                    {
                        removed = true;
                    }
                }
                else if (info.ToolTypeEnum == ToolType.NAVISWORKS)
                {
                    foreach (string file in info.Files)
                    {
                        string installPath = info.InstallDirectory + file;
                        File.Delete(installPath);
                    }

                    string pluginDirectory = info.InstallDirectory + @"\ARUP.IssueTracker.Navisworks";
                    if (Directory.Exists(pluginDirectory))
                    {
                        Directory.Delete(pluginDirectory, true);
                    }

                    string dllPath = info.InstallDirectory + info.DllPath;
                    if (!File.Exists(dllPath))
                    {
                        removed = true;
                    }
                }
                else if (info.ToolTypeEnum == ToolType.DESKTOP)
                {
                    string uninstallString = toolManager.GetUninstallString(info.ToolName);

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.WorkingDirectory = @"C:\Windows\System32";
                    psi.FileName = @"C:\Windows\System32\cmd.exe";
                    psi.Arguments = "/c " + uninstallString;
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(psi);
                    removed = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(info.ToolName + "\nFailed to remove the tool.\n" + ex.Message, "Uninstall Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return removed;
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < tools.Count; i++)
                {
                    tools[i].IsSelected = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void buttonUncheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < tools.Count; i++)
                {
                    tools[i].IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

}
