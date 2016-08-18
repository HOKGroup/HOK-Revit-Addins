using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HOK.AddInsInstaller
{
    public static class ToolManager
    {
        public static ToolNames[] tools2013 = new ToolNames[] { ToolNames.SmartBCF, ToolNames.FileMonitor, ToolNames.ProjectMonitor};
        public static ToolNames[] tools2014 = new ToolNames[] { ToolNames.SmartBCF, ToolNames.FileMonitor, ToolNames.ProjectMonitor, ToolNames.ElementMover, ToolNames.ProjectReplicator, ToolNames.MissionControl, ToolNames.CaseIssueTracker, ToolNames.ArupIssueTracker };
        public static ToolNames[] tools2015 = new ToolNames[] { ToolNames.SmartBCF, ToolNames.FileMonitor, ToolNames.ProjectMonitor, ToolNames.ElementMover, ToolNames.ProjectReplicator, ToolNames.SheetManager, ToolNames.MissionControl, ToolNames.CaseIssueTracker, ToolNames.ArupIssueTracker };
        public static ToolNames[] tools2016 = new ToolNames[] { ToolNames.SmartBCF, ToolNames.FileMonitor, ToolNames.ProjectMonitor, ToolNames.ElementMover, ToolNames.ProjectReplicator, ToolNames.SheetManager, ToolNames.MissionControl, ToolNames.CaseIssueTracker, ToolNames.ArupIssueTracker };
        public static ToolNames[] tools2017 = new ToolNames[] { ToolNames.SmartBCF};

        public static Dictionary<string/*versionNumber*/, ToolPackageInfo> GetToolPackageInfo()
        {
            Dictionary<string, ToolPackageInfo> toolPackageDictionary = new Dictionary<string, ToolPackageInfo>();
            try
            {
                ProgressWindow pWindow = new ProgressWindow();
                pWindow.Show();

                pWindow.SetStatusLabel("Gathering information about installed components in 2013. . .");
                ToolPackageInfo toolPackage2013 = new ToolPackageInfo("2013");
                toolPackage2013.SetToolInfo(tools2013, pWindow);
                toolPackageDictionary.Add("2013", toolPackage2013);

                pWindow.SetStatusLabel("Gathering information about installed components in 2014. . .");
                ToolPackageInfo toolPackage2014 = new ToolPackageInfo("2014");
                toolPackage2014.SetToolInfo(tools2014, pWindow);
                toolPackageDictionary.Add("2014", toolPackage2014);

                pWindow.SetStatusLabel("Gathering information about installed components in 2015. . .");
                ToolPackageInfo toolPackage2015 = new ToolPackageInfo("2015");
                toolPackage2015.SetToolInfo(tools2015, pWindow);
                toolPackageDictionary.Add("2015", toolPackage2015);

                pWindow.SetStatusLabel("Gathering information about installed components in 2016. . .");
                ToolPackageInfo toolPackage2016 = new ToolPackageInfo("2016");
                toolPackage2016.SetToolInfo(tools2016, pWindow);
                toolPackageDictionary.Add("2016", toolPackage2016);

                pWindow.SetStatusLabel("Gathering information about installed components in 2017. . .");
                ToolPackageInfo toolPackage2017 = new ToolPackageInfo("2017");
                toolPackage2017.SetToolInfo(tools2017, pWindow);
                toolPackageDictionary.Add("2017", toolPackage2017);

                if (null!=pWindow) { pWindow.Close(); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information of tool package.\n" + ex.Message, "Get Tool Package Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return toolPackageDictionary;
        }

        private static bool CreateDefaultDirectories(ToolPackageInfo packageInfo)
        {
            bool created = false;
            try
            {
                string installDirectory = packageInfo.InstallDirectory;
                string addinDirectory = Path.Combine(installDirectory, "HOK-Addin.bundle");
                string contentDirectory = Path.Combine(addinDirectory, "Contents_Beta");
                string resourceDirectory = Path.Combine(contentDirectory, "Resources");
                string x64Directory = Path.Combine(contentDirectory, "x64");
                string x86Directory = Path.Combine(contentDirectory, "x86");

                if (!Directory.Exists(addinDirectory))
                {
                    Directory.CreateDirectory(addinDirectory);
                }

                if (!Directory.Exists(contentDirectory))
                {
                    Directory.CreateDirectory(contentDirectory);
                }

                if (!Directory.Exists(resourceDirectory))
                {
                    Directory.CreateDirectory(resourceDirectory);
                }

                if (!Directory.Exists(x64Directory))
                {
                    Directory.CreateDirectory(x64Directory);
                }
                if (!Directory.Exists(x86Directory))
                {
                    Directory.CreateDirectory(x86Directory);
                }

                created = Directory.Exists(resourceDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create default directories.\n"+ex.Message, "Create Directories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return created;
        }

        public static ToolPackageInfo InstallTool(ToolPackageInfo packageInfo, out int installedTools)
        {
            ToolPackageInfo installedPackage = packageInfo;
            installedTools = 0;
            try
            {
                if (CreateDefaultDirectories(installedPackage))
                {
                    
                    var selectedItems = from tool in installedPackage.ToolInfoDictionary.Values where tool.IsSelected select tool;
                    if (selectedItems.Count() > 0)
                    {
                        List<ToolInfo> selectedTools = selectedItems.ToList();
                        var selectedFiles = from file in selectedItems select file.FilePaths;
                        int numFiles = selectedFiles.Count();

                        ProgressWindow pWindow = new ProgressWindow();
                        pWindow.RefreshProgressBar(numFiles);
                        pWindow.SetStatusLabel("Installing selected tools in " + installedPackage.TargetSoftware + " . . .");
                        pWindow.Show();

                        double progressValue = 0;
                        foreach (ToolInfo tInfo in selectedTools)
                        {
                            bool copied = true;
                            
                            foreach (string path in tInfo.FilePaths)
                            {
                                string betaPath = installedPackage.BetaDirectory + path;
                                string installPath = installedPackage.InstallDirectory + path;
                                if (File.Exists(betaPath))
                                {
                                    try
                                    {
                                        File.Copy(betaPath, installPath, true);
                                    }
                                    catch { copied = false; }
                                }
                                progressValue++;
                                pWindow.SetProgressBar(progressValue);
                            }

                            if (!string.IsNullOrEmpty(tInfo.ExePath))
                            {
                                MessageBoxResult result = MessageBox.Show("The desktop version of "+tInfo.ToolName+" will be installed.\nPlease follow the on-screen instructions to complete the process.", "ClickOnce Installeation", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                                if (result == MessageBoxResult.OK)
                                {
                                    Process.Start(tInfo.ExePath);
                                }
                            }

                            if (copied)
                            {
                                tInfo.InstallVersionInfo = tInfo.BetaVersionInfo;
                                tInfo.InstallVersionNumber = tInfo.BetaVersionNumber;
                                installedPackage.ToolInfoDictionary.Remove(tInfo.ToolName);
                                installedPackage.ToolInfoDictionary.Add(tInfo.ToolName, tInfo);
                                installedTools++;
                            }
                        }

                        if (null != pWindow) { pWindow.Close(); }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(packageInfo.TargetSoftware + " cannot be installed.\n" + ex.Message, "Install Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return installedPackage;
        }

        public static ToolPackageInfo UninstallTool(ToolPackageInfo packageInfo, out int removedTools)
        {
            ToolPackageInfo installedPackage = packageInfo;
            removedTools = 0;
            try
            {
                List<string> toolNames = installedPackage.ToolInfoDictionary.Keys.ToList();
                foreach (string toolName in toolNames)
                {
                    ToolInfo tInfo = installedPackage.ToolInfoDictionary[toolName];
                    if (tInfo.IsSelected)
                    {
                        bool removed = true;
                        foreach (string path in tInfo.FilePaths)
                        {
                            string installPath = installedPackage.InstallDirectory + path;
                            if (File.Exists(installPath))
                            {
                                try
                                {
                                    File.Delete(installPath);
                                }
                                catch { removed = false; }
                            }
                        }
                        if (removed)
                        {
                            tInfo.InstallVersionInfo = null;
                            tInfo.InstallVersionNumber = "Not Installed";
                            installedPackage.ToolInfoDictionary.Remove(toolName);
                            installedPackage.ToolInfoDictionary.Add(toolName, tInfo);
                            removedTools++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(packageInfo.TargetSoftware + " cannot be installed.\n" + ex.Message, "Install Tool", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return installedPackage;
        }

    }

    public class ToolPackageInfo
    {
        private string targetSoftware = "";
        private string versionNumber = "";
        private string betaDirectory = "";
        private string installDirectory = "";
        private Dictionary<string, ToolInfo> toolInfoDictionary = new Dictionary<string, ToolInfo>();

        public string TargetSoftware { get { return targetSoftware; } set { targetSoftware = value; } }
        public string VersionNumber { get { return versionNumber; } set { versionNumber = value; } }
        public string BetaDirectory { get { return betaDirectory; } set { betaDirectory = value; } }
        public string InstallDirectory { get { return installDirectory; } set { installDirectory = value; } }
        public Dictionary<string, ToolInfo> ToolInfoDictionary { get { return toolInfoDictionary; } set { toolInfoDictionary = value; } }

        public ToolPackageInfo(string version)
        {
            versionNumber = version;
            targetSoftware = "Revit " + versionNumber;
            SetDirectory();
        }

        private void SetDirectory()
        {
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\" + versionNumber;
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + versionNumber;
        }

        public void SetToolInfo(ToolNames[] toolNameArray, ProgressWindow pWindow)
        {
            try
            {
                pWindow.RefreshProgressBar(toolNameArray.Length);
                double progressValue = 0;
                foreach (ToolNames toolEnum in toolNameArray)
                {
                    progressValue++;
                    pWindow.SetProgressBar(progressValue);
                    switch (toolEnum)
                    {
                        case ToolNames.SmartBCF:
                            ToolInfo bcfInfo = new ToolInfo(toolEnum);
                            bcfInfo.ToolName = "Smart BCF";
                            bcfInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.SmartBCF.AddIn.dll";
                            if (!File.Exists(betaDirectory + bcfInfo.DllPath)) { break; }
                            bcfInfo.ToolIcon = ImageUtil.LoadBitmapImage("walker.png");

                            bcfInfo.SetBetaVersion(betaDirectory + bcfInfo.DllPath);
                            bcfInfo.SetBetaDate(betaDirectory + bcfInfo.DllPath);
                            bcfInfo.SetInstallVersion(installDirectory + bcfInfo.DllPath);

                            bcfInfo.FilePaths.Add("\\HOK.SmartBCF.addin");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.SmartBCF.AddIn.dll");

                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\EntityFramework.dll");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\EntityFramework.SqlServer.dll");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.SmartBCF.dll");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Data.SQLite.dll");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Data.SQLite.EF6.dll");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Data.SQLite.Linq.dll");
                            bcfInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Windows.Interactivity.dll");

                            bcfInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\x64\SQLite.Interop.dll");
                            bcfInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\x86\SQLite.Interop.dll");
                            bcfInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\Resources\Addins Shared Parameters.txt");

                            bcfInfo.ExePath = @"\\Group\hok\FWR\RESOURCES\Apps\HOK SmartBCF\HOK SmartBCF Manager.application";

                            toolInfoDictionary.Add(bcfInfo.ToolName, bcfInfo);
                            break;
                        case ToolNames.FileMonitor:
                            ToolInfo centralInfo = new ToolInfo(toolEnum);
                            centralInfo.ToolName = "Central File Monitor";
                            centralInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.FileOnpeningMonitor.dll";
                            if (!File.Exists(betaDirectory + centralInfo.DllPath)) { break; }
                            centralInfo.ToolIcon = ImageUtil.LoadBitmapImage("stop.png");

                            centralInfo.SetBetaVersion(betaDirectory + centralInfo.DllPath);
                            centralInfo.SetBetaDate(betaDirectory + centralInfo.DllPath);
                            centralInfo.SetInstallVersion(installDirectory + centralInfo.DllPath);

                            centralInfo.FilePaths.Add("\\HOK.FileOpeningMonitor.addin");
                            centralInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.FileOnpeningMonitor.dll");
                            centralInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\fmeserverapidotnet.dll");

                            toolInfoDictionary.Add(centralInfo.ToolName, centralInfo);
                            break;
                        case ToolNames.ProjectMonitor:
                            ToolInfo projectInfo = new ToolInfo(toolEnum);
                            projectInfo.ToolName = "Project Monitor";
                            projectInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.ProjectMonitor.dll";
                            if (!File.Exists(betaDirectory + projectInfo.DllPath)) { break; }
                            projectInfo.ToolIcon = ImageUtil.LoadBitmapImage("monitor.png");

                            projectInfo.SetBetaVersion(betaDirectory + projectInfo.DllPath);
                            projectInfo.SetBetaDate(betaDirectory + projectInfo.DllPath);
                            projectInfo.SetInstallVersion(installDirectory + projectInfo.DllPath);

                            projectInfo.FilePaths.Add("\\HOK.ProjectMonitor.addin");
                            projectInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.ProjectMonitor.dll");

                            toolInfoDictionary.Add(projectInfo.ToolName, projectInfo);
                            break;
                        case ToolNames.ElementMover:
                            ToolInfo moverInfo = new ToolInfo(toolEnum);
                            moverInfo.ToolName = "Element Mover";
                            moverInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.ElementMover.dll";
                            if (!File.Exists(betaDirectory + moverInfo.DllPath)) { break; }
                            moverInfo.ToolIcon = ImageUtil.LoadBitmapImage("mover.png");

                            moverInfo.SetBetaVersion(betaDirectory + moverInfo.DllPath);
                            moverInfo.SetBetaDate(betaDirectory + moverInfo.DllPath);
                            moverInfo.SetInstallVersion(installDirectory + moverInfo.DllPath);

                            moverInfo.FilePaths.Add("\\HOK.ElementMover.addin");
                            moverInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.ElementMover.dll");

                            toolInfoDictionary.Add(moverInfo.ToolName, moverInfo);
                            break;
                        case ToolNames.ProjectReplicator:
                            ToolInfo replicatorInfo = new ToolInfo(toolEnum);
                            replicatorInfo.ToolName = "Project Replicator";
                            replicatorInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.ModelManager.dll";
                            if (!File.Exists(betaDirectory + replicatorInfo.DllPath)) { break; }
                            replicatorInfo.ToolIcon = ImageUtil.LoadBitmapImage("replicator.png");

                            replicatorInfo.SetBetaVersion(betaDirectory + replicatorInfo.DllPath);
                            replicatorInfo.SetBetaDate(betaDirectory + replicatorInfo.DllPath);
                            replicatorInfo.SetInstallVersion(installDirectory + replicatorInfo.DllPath);

                            replicatorInfo.FilePaths.Add("\\HOK.ModelManager.addin");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.ModelManager.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Auth.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Auth.PlatformServices.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Core.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.Drive.v2.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.Apis.PlatformServices.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.GData.Client.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.GData.Extensions.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Google.GData.Spreadsheets.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\log4net.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Microsoft.Threading.Tasks.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Microsoft.Threading.Tasks.Extensions.Desktop.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Microsoft.Threading.Tasks.Extensions.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Newtonsoft.Json.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Net.Http.Extensions.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\System.Net.Http.Primitives.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Zlib.Portable.dll");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Resources\\client_secrets_ProjectReplicator.json");
                            replicatorInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Resources\\HOK Project Replicator.p12");

                            toolInfoDictionary.Add(replicatorInfo.ToolName, replicatorInfo);
                            break;
                        case ToolNames.SheetManager:
                            ToolInfo sheetInfo = new ToolInfo(toolEnum);
                            sheetInfo.ToolName = "Sheet Manager";
                            sheetInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.SheetManager.AddIn.dll";
                            if (!File.Exists(betaDirectory + sheetInfo.DllPath)) { break; }
                            sheetInfo.ToolIcon = ImageUtil.LoadBitmapImage("sync.png");

                            sheetInfo.SetBetaVersion(betaDirectory + sheetInfo.DllPath);
                            sheetInfo.SetBetaDate(betaDirectory + sheetInfo.DllPath);
                            sheetInfo.SetInstallVersion(installDirectory + sheetInfo.DllPath);

                            sheetInfo.FilePaths.Add(@"\HOK.SheetDataManager.addin");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\HOK.SheetManager.Core.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\HOK.SheetManager.AddIn.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\HOK.MissionControl.Core.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\EntityFramework.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\EntityFramework.SqlServer.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\System.Data.SQLite.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\System.Data.SQLite.EF6.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\System.Data.SQLite.Linq.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\x64\SQLite.Interop.dll");
                            sheetInfo.FilePaths.Add(@"\HOK-Addin.bundle\Contents_Beta\x86\SQLite.Interop.dll");

                            sheetInfo.ExePath = @"\\Group\hok\FWR\RESOURCES\Apps\HOK Sheet Data Editor\HOK SheetManager Editor.application";
                            
                            toolInfoDictionary.Add(sheetInfo.ToolName, sheetInfo);
                            break;
                        case ToolNames.MissionControl:
                            ToolInfo dtmInfo = new ToolInfo(toolEnum);
                            dtmInfo.ToolName = "Mission Control";
                            dtmInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\HOK.MissionControl.dll";
                            if (!File.Exists(betaDirectory + dtmInfo.DllPath)) { break; }
                            dtmInfo.ToolIcon = ImageUtil.LoadBitmapImage("dtm.png");

                            dtmInfo.SetBetaVersion(betaDirectory + dtmInfo.DllPath);
                            dtmInfo.SetBetaDate(betaDirectory + dtmInfo.DllPath);
                            dtmInfo.SetInstallVersion(installDirectory + dtmInfo.DllPath);

                            dtmInfo.FilePaths.Add("\\HOK.MissionControl.addin");
                            dtmInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\HOK.MissionControl.dll");

                            toolInfoDictionary.Add(dtmInfo.ToolName, dtmInfo);
                            break;
                        case ToolNames.CaseIssueTracker:
                            ToolInfo citInfo = new ToolInfo(toolEnum);
                            citInfo.ToolName = "Case Issue Tracker";
                            citInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\Case.IssueTracker.Revit.dll";
                            if (!File.Exists(betaDirectory + citInfo.DllPath)) { break; }
                            citInfo.ToolIcon = ImageUtil.LoadBitmapImage("case_issue.png");

                            citInfo.SetBetaVersion(betaDirectory + citInfo.DllPath);
                            citInfo.SetBetaDate(betaDirectory + citInfo.DllPath);
                            citInfo.SetInstallVersion(installDirectory + citInfo.DllPath);

                            citInfo.FilePaths.Add("\\Case.IssueTracker.Revit.addin");
                            citInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Case.IssueTracker.dll");
                            citInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Case.IssueTracker.Revit.dll");
                            citInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Ionic.Zip.dll");

                            citInfo.ExePath = @"\\Group\hok\FWR\RESOURCES\Apps\Case Issue Tracker\Case.IssueTracker.Win.application";

                            toolInfoDictionary.Add(citInfo.ToolName, citInfo);
                            break;
                        case ToolNames.ArupIssueTracker:
                            ToolInfo aitInfo = new ToolInfo(toolEnum);
                            aitInfo.ToolName = "Arup Issue Tracker";
                            aitInfo.DllPath = @"\HOK-Addin.bundle\Contents_Beta\ARUP.IssueTracker.Revit.dll";
                            if (!File.Exists(betaDirectory + aitInfo.DllPath)) { break; }
                            aitInfo.ToolIcon = ImageUtil.LoadBitmapImage("arup_issue.png");

                            aitInfo.SetBetaVersion(betaDirectory + aitInfo.DllPath);
                            aitInfo.SetBetaDate(betaDirectory + aitInfo.DllPath);
                            aitInfo.SetInstallVersion(installDirectory + aitInfo.DllPath);

                            aitInfo.FilePaths.Add("\\ARUP.IssueTracker.Revit.addin");
                            aitInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\ARUP.IssueTracker.dll");
                            aitInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\ARUP.IssueTracker.Revit.dll");
                            aitInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Arup.RestSharp.dll");
                            aitInfo.FilePaths.Add("\\HOK-Addin.bundle\\Contents_Beta\\Ionic.Zip.dll");

                            aitInfo.ExePath = @"\\Group\hok\FWR\RESOURCES\Apps\Arup Issue Tracker\ARUP.IssueTracker.Win.application";

                            toolInfoDictionary.Add(aitInfo.ToolName, aitInfo);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get tool's information.\n" + ex.Message, "Get Tools Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class ToolInfo
    {
        private string toolName = "";
        private ToolNames toolNameEnum = ToolNames.None;
        private List<string> filePaths = new List<string>();
        private string dllPath = "";
        private string exePath = "";
        private FileVersionInfo betaVersionInfo = null;
        private FileVersionInfo installVersionInfo = null;
        private string betaversionNumber = "Not Exist";
        private string betaReleaseDate = "";
        private string installVersionNumber = "Not Installed";
        private BitmapImage toolIcon = null;
        private bool isSelected = false;

        public string ToolName { get { return toolName; } set { toolName = value; } }
        public ToolNames ToolNameEnum { get { return toolNameEnum; } set { toolNameEnum = value; } }
        public List<string> FilePaths { get { return filePaths; } set { filePaths = value; } }
        public string DllPath { get { return dllPath; } set { dllPath = value; } }
        public string ExePath { get { return exePath; } set { exePath = value; } }
        public FileVersionInfo BetaVersionInfo { get { return betaVersionInfo; } set { betaVersionInfo = value; } }
        public FileVersionInfo InstallVersionInfo { get { return installVersionInfo; } set { installVersionInfo = value; } }
        public string BetaVersionNumber { get { return betaversionNumber; } set { betaversionNumber = value; } }
        public string BetaReleaseDate { get { return betaReleaseDate; } set { betaReleaseDate = value; } }
        public string InstallVersionNumber { get { return installVersionNumber; } set { installVersionNumber = value; } }
        public BitmapImage ToolIcon { get { return toolIcon; } set { toolIcon = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public ToolInfo(ToolNames toolEnum)
        {
            toolNameEnum = toolEnum;
        }

        public void SetBetaVersion(string betaPath)
        {
            if (File.Exists(betaPath))
            {
                betaVersionInfo = FileVersionInfo.GetVersionInfo(betaPath);
                if (null != betaVersionInfo)
                {
                    betaversionNumber = "v." + betaVersionInfo.FileVersion;
                }
            }
        }

        public void SetBetaDate(string betaPath)
        {
            if (File.Exists(betaPath))
            {
                FileInfo betaInfo = new FileInfo(betaPath);
                betaReleaseDate = betaInfo.LastWriteTime.Date.ToString("d");
            }
        }

        public void SetInstallVersion(string installPath)
        {
            if (File.Exists(installPath))
            {
                installVersionInfo = FileVersionInfo.GetVersionInfo(installPath);
                if (null != installVersionInfo)
                {
                    installVersionNumber = "v." + installVersionInfo.FileVersion;
                }
            }
        }
    }


    public enum ToolNames
    {
        None,
        SmartBCF,
        FileMonitor,
        ProjectMonitor,
        ElementMover, 
        ProjectReplicator, 
        SheetManager,
        ElementFlatter,
        MissionControl,
        CaseIssueTracker,
        ArupIssueTracker
    }

    public static class ImageUtil
    {
        public static BitmapImage LoadBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                string prefix = typeof(ImageUtil).Namespace + ".Resources.";
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }
    }
}
