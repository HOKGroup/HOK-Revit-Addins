using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.IO;
using System.Diagnostics;

namespace HOK.NavigatorUtil
{
    public enum ToolEnum
    {
        RibbonTab = 0,
        ElementTools,
        ParameterTools,
        SheetManager,
        BCFReader,
        MassTool,
        RevitData,
        Analysis,
        Utility,
        ModelManager,
        ColorEditor
    }

    public class Program
    {
        private static string[] arguments;
        private static string versionNumber = "";
        private static string masterDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Addin Files\";
        private static string betaDirectory = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        private static string installDirectory = "";
        private static int checkingTime = 0;
        private static List<string> failedTools = new List<string>();

        public static void Main(string[] args)
        {
            try
            {
                arguments = args;
                installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\";
                //Console.WriteLine("Initializing HOK Navigator Util... ");
                //Console.WriteLine("Please wait for the updating process.");

                if (args.Length > 0)
                {
                    versionNumber = FindVersion(args[0]);
                    if (VerifyFolderStructure(versionNumber))
                    {
                        foreach (string arg in arguments)
                        {
                            FindToolInfo(arg);
                        }
                    }

                    if (failedTools.Count > 0)
                    {
                        StringBuilder strbuilder = new StringBuilder();
                        strbuilder.AppendLine("There was a problem with installing the updates to the HOK Revit Addins.\nPlease Contract your buildingSMART Manager.");
                        strbuilder.AppendLine("");
                        foreach (string tool in failedTools)
                        {
                            strbuilder.AppendLine(tool);
                        }
                        //MessageBox.Show(strbuilder.ToString(), "HOK Navigator Util", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                //Console.WriteLine(ex.Message);
            }
        }

        private static string FindVersion(string arg)
        {
            string version = "";
            try
            {
                string[] toolInfo = arg.Split('_');
                if (toolInfo.Length > 1)
                {
                    version = toolInfo[0];
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return version;
        }

        private static bool VerifyFolderStructure(string version)
        {
            bool exist = false;
            try
            {
                string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + version;
                if (Directory.Exists(appDataDirectory))
                {
                    if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle"))
                    {
                        Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle");
                    }
                    if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents"))
                    {
                        Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle\\Contents");
                    }
                    if (!Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents\\Resources"))
                    {
                        Directory.CreateDirectory(appDataDirectory + "\\HOK-Addin.bundle\\Contents\\Resources");
                    }
                }

                if (Directory.Exists(appDataDirectory + "\\HOK-Addin.bundle\\Contents\\Resources"))
                {
                    exist = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                //MessageBox.Show("HOK Navigator cannot find the HOK-Addin.bundle in the addin folder.\nPlease contact a local buildingSMART manager.\n"+ex.Message, "HOK Addin Folder Structure Missing "+versionNumber, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return exist;
        }

        private static void FindToolInfo(string arg)
        {
            try
            {
                string[] toolInfo=arg.Split('_');
                string version = "";
                string toolName = "";
                bool isBeta = false;
                if (toolInfo.Length > 1)
                {
                    if (toolInfo.Contains("beta"))
                    {
                        isBeta = true;
                    }
                    version = toolInfo[0];
                    toolName = toolInfo[1];

                    InstallTool(version, toolName, isBeta);
                } 
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private static void InstallTool(string version, string toolName, bool isBeta)
        {
            try
            {
                if (CheckProcess()==false)
                {
                    string sourceDirectory = masterDirectory + version;
                    if (isBeta) { sourceDirectory = betaDirectory + version; }
                    string destDirectory = installDirectory + version;
                    ToolEnum toolEnum = (ToolEnum)Enum.Parse(typeof(ToolEnum), toolName);

                    List<string> fileNames = GetFiles(toolEnum);
                    foreach (string fileName in fileNames)
                    {
                        if (File.Exists(sourceDirectory + fileName))
                        {
                            try { File.Copy(sourceDirectory + fileName, destDirectory + fileName, true); }
                            catch
                            {
                                string toolInfo = isBeta ? version + "_" + toolName + "_beta" : version + "_" + toolName;
                                if (!failedTools.Contains(toolInfo))
                                {
                                    failedTools.Add(toolInfo);
                                }
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                //Console.WriteLine(message);
            }
        }

        private static bool CheckProcess()
        {
            bool running = false;
            try
            {
                int versionInt = 0;
                int.TryParse(versionNumber, out versionInt);

                System.Threading.Thread.Sleep(5000);
                Process[] processes = Process.GetProcessesByName("Revit");
                if (processes.Length > 0)
                {
                    foreach (Process process in processes)
                    {
                        FileVersionInfo versionInfo = process.MainModule.FileVersionInfo;
                        if (null != versionInfo)
                        {
                            if (versionInfo.FileMajorPart == versionInt)
                            {
                                checkingTime++;
                                if (checkingTime < 12)
                                {
                                    CheckProcess();
                                }
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return running;
        }

        private static List<string> GetFiles(ToolEnum toolName)
        {
            List<string> fileNames = new List<string>();
            try
            {
                switch (toolName)
                {
                    case ToolEnum.RibbonTab:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\AdWindows.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\dao360.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\DBManagedServices.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\ManagedMC3.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.Dao.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Access.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Office.Interop.Excel.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Microsoft.Vbe.Interop.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\office.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFramework.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\UIFrameworkServices.dll");

                        fileNames.Add("\\HOK.RibbonTab.addin");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RibbonTab.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Tooltip.txt");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Help.txt");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\HOK.Installer.txt");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\hok.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\info.ico");
                        break;

                    case ToolEnum.ElementTools:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ElementTools.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\element.ico");
                        break;

                    case ToolEnum.ParameterTools:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ParameterTools.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\parameter.ico");
                        break;

                    case ToolEnum.SheetManager:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.SheetManager.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sheet.ico");
                        break;

                    case ToolEnum.BCFReader:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.BCFReader.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\comment.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\markup.xsd");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\visinfo.xsd");
                        break;

                    case ToolEnum.MassTool:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RoomsToMass.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\cube.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\refresh.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\tooltip.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\shape.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass Shared Parameters.txt");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\Mass.rfa");
                        break;

                    case ToolEnum.RevitData:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.RevitDBManager.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\default.bmp");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\editor.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\eye.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\sync.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\view.ico");
                        break;

                    case ToolEnum.Analysis:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.AVFManager.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\chart.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\DefaultSettings.xml");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\PointOfView.rfa");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.LPDCalculator.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\bulb.png");
                        break;

                    case ToolEnum.Utility:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.Utilities.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\height.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\level.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\finish.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\camera.ico");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\viewTooltip.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\arrowhead.png");
                        break;
                    case ToolEnum.ModelManager:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ModelManager.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.AccessControl.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Client.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Documents.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Extensions.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Google.GData.Spreadsheets.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Newtonsoft.Json.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\System.Runtime.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\model.png");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Resources\\project.png");
                        break;
                    case ToolEnum.ColorEditor:
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\HOK.ColorSchemeEditor.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\ICSharpCode.SharpZipLib.dll");
                        fileNames.Add("\\HOK-Addin.bundle\\Contents\\Xceed.Wpf.Toolkit.dll");
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return fileNames;
        }

    }
}
