using System;
using System.Collections.Generic;
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

namespace HOK.BetaToolsManager
{
    /// <summary>
    /// Interaction logic for BetaInstallerWindow.xaml
    /// </summary>
    public partial class BetaInstallerWindow : Window
    {
        private string versionNumber = "";
        private string betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        private string installDirectory = "";
        
        private Dictionary<ToolEnum, ToolProperties> toolInfoDictionary = new Dictionary<ToolEnum, ToolProperties>();
        private List<ToolProperties> betaToolList = new List<ToolProperties>();

        public Dictionary<ToolEnum, ToolProperties> ToolInfoDictionary { get { return toolInfoDictionary; } set { toolInfoDictionary = value; } }

        public BetaInstallerWindow(string version, Dictionary<ToolEnum, ToolProperties> dictionary)
        {
            versionNumber = version;
            betaDirectory = betaDirectory + versionNumber + @"\HOK-Addin.bundle\Contents\";
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + versionNumber + @"\HOK-Addin.bundle\Contents_Beta\";

            toolInfoDictionary = dictionary;
            InitializeComponent();

            foreach (ToolProperties tp in toolInfoDictionary.Values)
            {
                if (tp.BetaExist ||tp.InstallExist)
                {
                    ToolProperties tool = GetToolVersion(tp);
                    betaToolList.Add(tool);
                }
            }

            dataGridTools.ItemsSource = null;
            dataGridTools.ItemsSource = betaToolList;
        }

        private ToolProperties GetToolVersion(ToolProperties tool)
        {
            ToolProperties tp = new ToolProperties(tool);
            try
            {
                if (tp.BetaExist)
                {
                    tp.BetaVersionInfo = FileVersionInfo.GetVersionInfo(tp.BetaPath);
                    tp.BetaVersionNumber = "v." + tp.BetaVersionInfo.FileVersion;
                    FileInfo fileInfo = new FileInfo(tp.BetaPath);
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
            this.Close();
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int numOfTools = 0;
                if (null != dataGridTools.ItemsSource)
                {
                    betaToolList = (List<ToolProperties>)dataGridTools.ItemsSource;
                    foreach (ToolProperties tool in betaToolList)
                    {
                        if (tool.IsSelected)
                        {
                            List<string> fileNames = tool.InstallingFiles;
                           
                            foreach (string fileName in fileNames)
                            {
                                try
                                {
                                    File.Copy(betaDirectory + fileName, installDirectory + fileName, true);
                                }
                                catch (Exception ex)
                                {
                                    string message = ex.Message;
                                }
                            }

                            //update tool info dictionary
                            if (File.Exists(tool.InstallPath))
                            {
                                tool.InstallExist = true;
                                tool.InstalledVersionInfo = FileVersionInfo.GetVersionInfo(tool.InstallPath);
                                tool.InstallVersionNumber = "v." + tool.InstalledVersionInfo.FileVersion;
                                tool.IsSelected = false;
                            }
                            
                            if (!string.IsNullOrEmpty(tool.InstallPath1))
                            {
                                tool.InstallExist1 = File.Exists(tool.InstallPath1);
                            }
                            toolInfoDictionary.Remove(tool.ToolEnumVal);
                            toolInfoDictionary.Add(tool.ToolEnumVal, tool);
                            numOfTools++;
                        }
                    }

                    if (numOfTools > 0)
                    {
                        MessageBox.Show(numOfTools.ToString()+" tools are successfully installed in the beta directory.", "Beta Installation", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install beta tools.\n"+ex.Message, "Install Beta Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (null != dataGridTools.ItemsSource)
                {
                    int numOfTool = 0;

                    betaToolList = (List<ToolProperties>)dataGridTools.ItemsSource;
                    foreach (ToolProperties tool in betaToolList)
                    {
                        if (tool.IsSelected)
                        {
                            List<string> fileNames = tool.InstallingFiles;
                            foreach (string fileName in fileNames)
                            {
                                try
                                {
                                    File.Delete(installDirectory + fileName);
                                }
                                catch (Exception ex)
                                {
                                    string message = ex.Message;
                                }
                            }
                            //update tool info dictionary
                            if (!File.Exists(tool.InstallPath))
                            {
                                tool.InstallExist = false;
                                tool.InstalledVersionInfo = null;
                                tool.InstallVersionNumber = "Not Installed";
                                tool.IsSelected = false;

                                if (!string.IsNullOrEmpty(tool.InstallPath1))
                                {
                                    tool.InstallExist1 = File.Exists(tool.InstallPath1);
                                }
                            }

                            toolInfoDictionary.Remove(tool.ToolEnumVal);
                            toolInfoDictionary.Add(tool.ToolEnumVal, tool);
                            numOfTool++;
                        }
                    }

                    if (numOfTool > 0)
                    {
                        MessageBox.Show(numOfTool.ToString()+" tools are removed successfully.", "Beta Uninstallation", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
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
