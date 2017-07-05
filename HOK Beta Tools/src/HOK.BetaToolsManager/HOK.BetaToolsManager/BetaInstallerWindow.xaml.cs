using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace HOK.BetaToolsManager
{
    /// <summary>
    /// Interaction logic for BetaInstallerWindow.xaml
    /// </summary>
    public partial class BetaInstallerWindow
    {
        private readonly string versionNumber;
        private readonly string betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        private readonly string installDirectory;
        private readonly string tempInstallDirectory;
        private List<ToolProperties> betaToolList = new List<ToolProperties>();
        public Dictionary<ToolEnum, ToolProperties> ToolInfoDictionary { get; set; }

        public BetaInstallerWindow(string version, Dictionary<ToolEnum, ToolProperties> dictionary)
        {
            versionNumber = version;
            betaDirectory = betaDirectory + versionNumber + @"\HOK-Addin.bundle\Contents_Beta\";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + versionNumber + @"\HOK-Addin.bundle\Contents_Beta\";
            tempInstallDirectory = Path.Combine(installDirectory, "Temp")+"\\";

            ToolInfoDictionary = dictionary;
            InitializeComponent();
            Title = "HOK Beta Tools Installer v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            foreach (var tp in ToolInfoDictionary.Values)
            {
                if (!tp.BetaExist && !tp.InstallExist) continue;

                var tool = GetToolVersion(tp);
                betaToolList.Add(tool);
            }

            dataGridTools.ItemsSource = null;
            dataGridTools.ItemsSource = betaToolList;
        }

        private static ToolProperties GetToolVersion(ToolProperties tool)
        {
            var tp = new ToolProperties(tool);
            try
            {
                if (tp.BetaExist)
                {
                    tp.BetaVersionInfo = FileVersionInfo.GetVersionInfo(tp.BetaPath);
                    tp.BetaVersionNumber = "v." + tp.BetaVersionInfo.FileVersion;
                    var fileInfo = new FileInfo(tp.BetaPath);
                    tp.BetaReleasedDate = fileInfo.LastWriteTime.Date.ToString("d");
                }
                if (tp.InstallExist)
                {
                    tp.InstalledVersionInfo = FileVersionInfo.GetVersionInfo(tp.InstallPath);
                    tp.InstallVersionNumber = "v." + tp.InstalledVersionInfo.FileVersion;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get version of the tool.\n"+ex.Message, "Get Tool Version", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return tp;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var numOfTools = 0;
                if (dataGridTools.ItemsSource == null) return;

                betaToolList = (List<ToolProperties>)dataGridTools.ItemsSource;
                foreach (var tool in betaToolList)
                {
                    if (!tool.IsSelected) continue;
   
                    foreach (var fileName in tool.InstallingFiles)
                    {
                        try
                        {
                            var installedFiles = Directory.GetFiles(installDirectory, fileName + "*");
                            if (installedFiles.Any())
                            {
                                foreach (var installedFile in installedFiles)
                                {
                                    try
                                    {
                                        File.Delete(installedFile);
                                    }
                                    catch (Exception ex)
                                    {
                                        var message = ex.Message;
                                    }
                                }
                            }
                            File.Copy(betaDirectory + fileName, installDirectory + fileName, true);
                            //make copies of the files for the push button data
                            File.Copy(betaDirectory + fileName, tempInstallDirectory + fileName, true);
                        }
                        catch (Exception ex)
                        {
                            var message = ex.Message;
                        }
                    }
                    var installPath = Path.Combine(installDirectory, Path.GetFileName(tool.BetaPath));
                    //update tool info dictionary
                    if (File.Exists(installPath))
                    {
                        tool.InstallPath = installPath;
                        tool.TempAssemblyPath = GetTempInstallPath(tool.InstallPath);
                        tool.InstallExist = true;
                        tool.InstalledVersionInfo = FileVersionInfo.GetVersionInfo(tool.InstallPath);
                        tool.InstallVersionNumber = "v." + tool.InstalledVersionInfo.FileVersion;
                        tool.IsSelected = false;
                    }
                          
                    ToolInfoDictionary.Remove(tool.ToolEnumVal);
                    ToolInfoDictionary.Add(tool.ToolEnumVal, tool);
                    numOfTools++;
                }

                if (numOfTools == 0) return;

                MessageBox.Show(numOfTools + " tools are successfully installed in the beta directory.", "Beta Installation", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install beta tools.\n"+ex.Message, "Install Beta Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetTempInstallPath(string installPath)
        {
            var tempPath = "";
            try
            {
                var fileName = Path.GetFileName(installPath);
                tempPath = tempInstallDirectory + fileName;
                if (!File.Exists(tempPath))
                {
                    tempPath = installPath;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return tempPath;
        }



        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridTools.ItemsSource)
                {
                    var numOfTool = 0;

                    betaToolList = (List<ToolProperties>)dataGridTools.ItemsSource;
                    foreach (var tool in betaToolList)
                    {
                        if (tool.IsSelected)
                        {
                            var fileNames = tool.InstallingFiles;
                            foreach (var fileName in fileNames)
                            {
                                try
                                {
                                    File.Delete(installDirectory + fileName);
                                }
                                catch (Exception ex)
                                {
                                    var message = ex.Message;
                                }
                            }
                            //update tool info dictionary
                            if (!File.Exists(tool.InstallPath))
                            {
                                tool.InstallExist = false;
                                tool.InstalledVersionInfo = null;
                                tool.InstallVersionNumber = "Not Installed";
                                tool.IsSelected = false;
                            }

                            ToolInfoDictionary.Remove(tool.ToolEnumVal);
                            ToolInfoDictionary.Add(tool.ToolEnumVal, tool);
                            numOfTool++;
                        }
                    }

                    if (numOfTool > 0)
                    {
                        MessageBox.Show(numOfTool + " tools are removed successfully.", "Beta Uninstallation", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uninstall beta tools.\n" + ex.Message, "Uninstall Beta Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
