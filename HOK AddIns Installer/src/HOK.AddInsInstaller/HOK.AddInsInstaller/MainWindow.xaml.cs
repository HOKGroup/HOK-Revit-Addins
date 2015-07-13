using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Outlook = Microsoft.Office.Interop.Outlook;

namespace HOK.AddInsInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private Dictionary<string/*toolVersion*/, ToolPackageInfo> toolDictionary = new Dictionary<string, ToolPackageInfo>();

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "HOK AddIns Installer v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            toolDictionary = ToolManager.GetToolPackageInfo();

            if (toolDictionary.Count > 0)
            {
                List<ToolPackageInfo> toolList = toolDictionary.Values.ToList();
                toolList = toolList.OrderBy(t => t.TargetSoftware).ToList();
                comboBoxTarget.ItemsSource = toolList;
                comboBoxTarget.DisplayMemberPath = "TargetSoftware";

                comboBoxTarget.SelectedIndex = 0;
            }
        }

        private void comboBoxTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBoxTarget.SelectedItem != null)
                {
                    ToolPackageInfo selectedPackage = (ToolPackageInfo)comboBoxTarget.SelectedItem;
                    List<ToolInfo> toolInfoList = selectedPackage.ToolInfoDictionary.Values.ToList();
                    toolInfoList = toolInfoList.OrderBy(t => t.ToolName).ToList();

                    dataGridTool.ItemsSource = null;
                    dataGridTool.ItemsSource = toolInfoList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot display the list of tools.\n"+ex.Message, "Target Software Changed", MessageBoxButton.OK, MessageBoxImage.Warning);
                   
            }
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedIndex = comboBoxTarget.SelectedIndex;
                ToolPackageInfo toolPackage = (ToolPackageInfo)comboBoxTarget.SelectedItem;
                List<ToolInfo> toolInfoList = (List<ToolInfo>)dataGridTool.ItemsSource;
                bool selected = false;
                if (toolDictionary.ContainsKey(toolPackage.VersionNumber))
                {
                    foreach (ToolInfo info in toolInfoList)
                    {
                        if (info.IsSelected) { selected = true; }
                        if (toolDictionary[toolPackage.VersionNumber].ToolInfoDictionary.ContainsKey(info.ToolName))
                        {
                            toolDictionary[toolPackage.VersionNumber].ToolInfoDictionary.Remove(info.ToolName);
                            toolDictionary[toolPackage.VersionNumber].ToolInfoDictionary.Add(info.ToolName, info);
                        }
                    }
                }
                

                if (selected)
                {
                    if (CheckPrerequisite())
                    {
                        toolPackage = toolDictionary[toolPackage.VersionNumber];
                        int installedTools = 0;
                        toolPackage = ToolManager.InstallTool(toolPackage, out installedTools);

                        if (installedTools > 0)
                        {
                            MessageBoxResult msgResult = MessageBox.Show(installedTools.ToString() + " tools were successfully installed.\nThe new Addins will be available when Revit is restarted.\nWould you like to exit the installer?",
                                "Installation Complete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (msgResult == MessageBoxResult.Yes)
                            {
                                this.Close();
                            }
                            else
                            {
                                if (toolDictionary.ContainsKey(toolPackage.VersionNumber))
                                {
                                    toolDictionary.Remove(toolPackage.VersionNumber);
                                    toolDictionary.Add(toolPackage.VersionNumber, toolPackage);
                                }

                                //bind to combo box
                                if (toolDictionary.Count > 0)
                                {
                                    comboBoxTarget.ItemsSource = null;
                                    dataGridTool.ItemsSource = null;

                                    List<ToolPackageInfo> toolList = toolDictionary.Values.ToList();
                                    toolList = toolList.OrderBy(t => t.TargetSoftware).ToList();
                                    comboBoxTarget.ItemsSource = toolList;
                                    comboBoxTarget.DisplayMemberPath = "TargetSoftware";

                                    comboBoxTarget.SelectedIndex = selectedIndex;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install selected tools.\n"+ex.Message, "Install Addins", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUninstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedIndex = comboBoxTarget.SelectedIndex;
                ToolPackageInfo toolPackage = (ToolPackageInfo)comboBoxTarget.SelectedItem;
                List<ToolInfo> toolInfoList = (List<ToolInfo>)dataGridTool.ItemsSource;
                bool selected = false;
                if (toolDictionary.ContainsKey(toolPackage.VersionNumber))
                {
                    foreach (ToolInfo info in toolInfoList)
                    {
                        if (info.IsSelected) { selected = true; }
                        if (toolDictionary[toolPackage.VersionNumber].ToolInfoDictionary.ContainsKey(info.ToolName))
                        {
                            toolDictionary[toolPackage.VersionNumber].ToolInfoDictionary.Remove(info.ToolName);
                            toolDictionary[toolPackage.VersionNumber].ToolInfoDictionary.Add(info.ToolName, info);
                        }
                    }
                }

                if (selected)
                {
                    if (CheckPrerequisite())
                    {
                        toolPackage = toolDictionary[toolPackage.VersionNumber];
                        int removedTools = 0;
                        toolPackage = ToolManager.UninstallTool(toolPackage, out removedTools);

                        if (removedTools > 0)
                        {
                            MessageBoxResult msgResult = MessageBox.Show(removedTools.ToString() + " tools were successfully uninstalled.\nWould you like to exit the installer?",
                                "Uninstallation Complete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (msgResult == MessageBoxResult.Yes)
                            {
                                this.Close();
                            }
                            else
                            {
                                if (toolDictionary.ContainsKey(toolPackage.VersionNumber))
                                {
                                    toolDictionary.Remove(toolPackage.VersionNumber);
                                    toolDictionary.Add(toolPackage.VersionNumber, toolPackage);
                                }

                                //bind to combo box
                                if (toolDictionary.Count > 0)
                                {
                                    comboBoxTarget.ItemsSource = null;
                                    dataGridTool.ItemsSource = null;

                                    List<ToolPackageInfo> toolList = toolDictionary.Values.ToList();
                                    toolList = toolList.OrderBy(t => t.TargetSoftware).ToList();
                                    comboBoxTarget.ItemsSource = toolList;
                                    comboBoxTarget.DisplayMemberPath = "TargetSoftware";

                                    comboBoxTarget.SelectedIndex = selectedIndex;
                                }
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uninstall selected tools.\n"+ex.Message, "Uninstall Addins", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool CheckPrerequisite()
        {
            bool pass = true;
            try
            {
                Process[] prs = Process.GetProcesses();

                for (int i = 0; i < prs.Length; i++)
                {
                    if (prs[i].ProcessName == "Revit")
                    {
                        MessageBoxResult msgResult = MessageBox.Show("The installer has detected that Revit is currently running.\n"
                        + "You can install/uninstall the addins, but an application restart is required before the new tools are available.\n"
                        + "Would you like to continue?"
                          , "Application in Use", MessageBoxButton.OKCancel, MessageBoxImage.Information);

                        if (msgResult == MessageBoxResult.OK)
                        {
                            pass = true;
                        }
                        else
                        {
                            pass = false;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check prerequisite conditions.\n" + ex.Message, "Check Prerequisite", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return pass;
        }

        private void buttonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<ToolInfo> toolInfoList = (List<ToolInfo>)dataGridTool.ItemsSource;
                List<ToolInfo> updatedList = new List<ToolInfo>();
                foreach (ToolInfo toolInfo in toolInfoList)
                {
                    toolInfo.IsSelected = true;
                    updatedList.Add(toolInfo);
                }

                dataGridTool.ItemsSource = null;
                dataGridTool.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all items.\n"+ex.Message, "Check All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCheckNone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<ToolInfo> toolInfoList = (List<ToolInfo>)dataGridTool.ItemsSource;
                List<ToolInfo> updatedList = new List<ToolInfo>();
                foreach (ToolInfo toolInfo in toolInfoList)
                {
                    toolInfo.IsSelected = false;
                    updatedList.Add(toolInfo);
                }

                dataGridTool.ItemsSource = null;
                dataGridTool.ItemsSource = updatedList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to uncheck all items.\n"+ex.Message, "Uncheck All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void hyperlinkEmail_Click(object sender, RoutedEventArgs e)
        {
            Outlook.Application outlookApplication = new Outlook.Application();
            Outlook.NameSpace nameSpace = outlookApplication.GetNamespace("MAPI");
            Outlook.Folder folderInbox = (Outlook.Folder)nameSpace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.MailItem mailItem = (Outlook.MailItem)outlookApplication.CreateItem(Outlook.OlItemType.olMailItem);

            mailItem.Subject = "HOK Addins Installer Problem Report";
            mailItem.Body = "**** This email will go to the developer of the installer. ****\n";

            mailItem.Recipients.Add("jinsol.kim@hok.com");
            mailItem.Display(false);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

       
    }
}
