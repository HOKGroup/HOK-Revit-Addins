using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HOK.Navigator
{
    public enum ToolEnum
    {
        ElementTools, 
        ParameterTools,
        SheetManager, 
        BCFReader,
        MassTool, 
        RevitData,
        Analysis,
        Utility,
#if RELEASE2014 || RELEASE2015
        ModelManager,
        ColorEditor,
#endif
        RibbonTab,
    }

    public class ToolManager
    {
        private string versionNumber = "";
        private string masterDirectory = "";
        private string betaDirectory ="";
        private string installDirectory = "";
        private string currentAssembly = "";
        private string currentDirectory = "";
        private Dictionary<ToolEnum, ToolProperties> outDatedTools = new Dictionary<ToolEnum, ToolProperties>();

        public Dictionary<ToolEnum, ToolProperties> OutDatedTools { get { return outDatedTools; } set { outDatedTools = value; } }

        public ToolManager(string version)
        {
            versionNumber = version;
            masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\"+versionNumber;
            betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\"+versionNumber;
            installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\"+versionNumber;

            currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);
        }

        public void FindOutDatedTools()
        {
            try
            {
                Array arrayTools = Enum.GetValues(typeof(ToolEnum));

                foreach (ToolEnum tool in arrayTools)
                {
                    ToolProperties tp = new ToolProperties();
                    tp.ToolName = tool;
                    switch (tool)
                    {
                        case ToolEnum.RibbonTab:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll";
                            tp.DllName = "HOK.RibbonTab.dll";
                            break;
                        case ToolEnum.ElementTools:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll";
                            tp.DllName = "HOK.ElementTools.dll";
                            break;
                        case ToolEnum.ParameterTools:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll";
                            tp.DllName = "HOK.ParameterTools.dll";
                            break;
                        case ToolEnum.SheetManager:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll";
                            tp.DllName = "HOK.SheetManager.dll";
                            break;
                        case ToolEnum.BCFReader:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll";
                            tp.DllName = "HOK.BCFReader.dll";
                            break;
                        case ToolEnum.MassTool:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll";
                            tp.DllName = "HOK.RoomsToMass.dll";
                            break;
                        case ToolEnum.RevitData:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll";
                            tp.DllName = "HOK.RevitDBManager.dll";
                            break;
                        case ToolEnum.Analysis:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll";
                            tp.DllName = "HOK.AVFManager.dll";
                            break;
                        case ToolEnum.Utility:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll";
                            tp.DllName = "HOK.Utilities.dll";
                            break;
#if RELEASE2014 || RELEASE2015
                        case ToolEnum.ModelManager:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ModelManager.dll";
                            tp.DllName = "HOK.ModelManager.dll";
                            break;
                        case ToolEnum.ColorEditor:
                            tp.DllPath = "\\HOK-Addin.bundle\\Contents\\HOK.ColorSchemeEditor.dll";
                            tp.DllName = "HOK.ColorSchemeEditor.dll";
                            break;
#endif
                    }

                    string installedPath = installDirectory + tp.DllPath;
                    string standardPath = masterDirectory + tp.DllPath;
                    string betaPath = betaDirectory + tp.DllPath;

                    if (File.Exists(installedPath))
                    {
                        tp.InstalledVersion = FileVersionInfo.GetVersionInfo(installedPath);
                        Version instVersion = new Version(tp.InstalledVersion.FileVersion);
                        if (File.Exists(standardPath))
                        {
                            tp.StandardVersion = FileVersionInfo.GetVersionInfo(standardPath);
                            Version stdVersion = new Version(tp.StandardVersion.FileVersion);

                            if (instVersion.CompareTo(stdVersion) < 0)
                            {
                                //installed version is eariler than standard version
                                tp.IsBeta = false;
                                outDatedTools.Add(tp.ToolName, tp);
                            }
                            else if (instVersion.CompareTo(stdVersion) > 0)
                            {
                                if (File.Exists(betaPath))
                                {
                                    tp.BetaVersion = FileVersionInfo.GetVersionInfo(betaPath);
                                    Version btVersion = new Version(tp.BetaVersion.FileVersion);
                                    if (instVersion.CompareTo(btVersion) < 0)
                                    {
                                        tp.IsBeta = true;
                                        outDatedTools.Add(tp.ToolName, tp);
                                    }
                                }
                            }
                        }
                        else if (File.Exists(betaPath))
                        {
                            tp.BetaVersion = FileVersionInfo.GetVersionInfo(betaPath);
                            Version btVersion = new Version(tp.BetaVersion.FileVersion);
                            if (instVersion.CompareTo(btVersion) < 0)
                            {
                                tp.IsBeta = true;
                                outDatedTools.Add(tp.ToolName, tp);
                            }
                        }
                    }
                    else if(File.Exists(standardPath))
                    {
                        tp.IsBeta = false;
                        outDatedTools.Add(tp.ToolName, tp);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void InstallTools()
        {
            try
            {
                List<string> toolLists = new List<string>();
                foreach (ToolEnum toolName in outDatedTools.Keys)
                {
                    ToolProperties tp = outDatedTools[toolName];
                    string toolInfo = versionNumber + "_" + toolName.ToString();
                    if (tp.IsBeta) { toolInfo = toolInfo + "_beta"; }
                    if (!toolLists.Contains(toolInfo)) { toolLists.Add(toolInfo); }
                }

                if (toolLists.Count > 0)
                {
                    string navigatorUtil = currentDirectory + "\\HOK.NavigatorUtil.exe";
                    string[] strArray = toolLists.ToArray();
                    string args = string.Join(" ", strArray);
                    if (File.Exists(navigatorUtil))
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(navigatorUtil, args);
                        Process.Start(startInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

    }

    public class ToolProperties
    {
        public ToolEnum ToolName { get; set; }
        public string DllName { get; set; }
        public string DllPath { get; set; }
        public FileVersionInfo StandardVersion { get; set; }
        public FileVersionInfo BetaVersion { get; set; }
        public FileVersionInfo InstalledVersion { get; set; }
        public bool IsBeta { get; set; }
    }

}
