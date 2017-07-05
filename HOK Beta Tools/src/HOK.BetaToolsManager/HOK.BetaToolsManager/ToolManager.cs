using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HOK.BetaToolsManager
{
    public class ToolManager
    {
        public string VersionNumber { get; set; }
        public string BetaDirectory { get; set; } = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        public string InstallDirectory { get; set; }
        public string TempInstallDirectory { get; set; }
        public Dictionary<ToolEnum, ToolProperties> ToolInfoDictionary { get; set; } = new Dictionary<ToolEnum, ToolProperties>();

        public ToolManager(string version)
        {
            VersionNumber = version;
            BetaDirectory = BetaDirectory + VersionNumber + @"\HOK-Addin.bundle\Contents_Beta\";
            InstallDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\" + VersionNumber + @"\HOK-Addin.bundle\Contents_Beta\";
            TempInstallDirectory = Path.Combine(InstallDirectory, "Temp");

            if (ExistBetaContentFolder())
            {
                ToolInfoDictionary = GetToolInfo();
            }
        }

        private bool ExistBetaContentFolder()
        {
            var exist = false;
            try
            {
                if (!Directory.Exists(InstallDirectory))
                {
                    Directory.CreateDirectory(InstallDirectory);
                }
                if (Directory.Exists(TempInstallDirectory))
                {
                    Directory.Delete(TempInstallDirectory, true);
                }

                var resourceFolder = Path.Combine(InstallDirectory, "Resources");
                if (Directory.Exists(InstallDirectory) && !Directory.Exists(resourceFolder))
                {
                    Directory.CreateDirectory(resourceFolder);
                }
                if (Directory.Exists(InstallDirectory) && !Directory.Exists(TempInstallDirectory))
                {
                    //copy files from install directory to temp directory.
                    var installedFiles = Directory.GetFiles(InstallDirectory);

                    Directory.CreateDirectory(TempInstallDirectory);
                    if (Directory.Exists(TempInstallDirectory))
                    {
                        foreach (var filePath in installedFiles)
                        {
                            var fileName = Path.GetFileName(filePath);
                            var tempFile = Path.Combine(TempInstallDirectory, fileName);
                            try
                            {
                                File.Copy(filePath, tempFile, true);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    var tempResourceFolder = Path.Combine(TempInstallDirectory, "Resources");
                    if (!Directory.Exists(tempResourceFolder))
                    {
                        Directory.CreateDirectory(tempResourceFolder);
                    }
                    else
                    {
                        var installedResources = Directory.GetFiles(resourceFolder);
                        foreach (var filePath in installedResources)
                        {
                            var fileName = Path.GetFileName(filePath);
                            var tempFile = Path.Combine(tempResourceFolder, fileName);
                            try { File.Copy(filePath, tempFile, true); }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }
                
                if (Directory.Exists(resourceFolder))
                {
                    exist = true;  
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show("Beta directories cannot be created.\n" + ex.Message,"Beta Folder Exist", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            return exist;
        }

        private Dictionary<ToolEnum, ToolProperties> GetToolInfo()
        {
            var dictionary = new Dictionary<ToolEnum, ToolProperties>();
            try
            {
                var toolArray = Enum.GetValues(typeof(ToolEnum));
                
                foreach (ToolEnum tool in toolArray)
                {
                    var tp = new ToolProperties
                    {
                        ToolEnumVal = tool,
                        InstallingFiles = GetFiles(tool)
                    };

                    switch (tool)
                    {
                        case ToolEnum.ElementTools:
                            tp.ToolName = "Element Tools";
                            tp.DllName = "HOK.ElementTools.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("element.ico");
                            
                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.ParameterTools:
                            tp.ToolName = "Parameter Tools";
                            tp.DllName = "HOK.ParameterTools.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("parameter.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.SheetManager:
                            tp.ToolName = "Sheet Manager";
                            tp.DllName = "HOK.SheetManager.dll";
                            tp.BetaPath =  BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("sheet.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.BCFReader:
                            tp.ToolName = "BCF Reader";
                            tp.DllName = "HOK.BCFReader.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("comment.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.MassTool:
                            tp.ToolName = "Mass Tools";
                            tp.DllName = "HOK.RoomsToMass.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("cube.png");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.RevitData:
                            tp.ToolName = "Data Manager";
                            tp.DllName = "HOK.RevitDBManager.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("editor.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.AVF:
                            tp.ToolName = "Analysis Tools";
                            tp.DllName = "HOK.AVFManager.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("chart.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.LPDAnalysis:
                            tp.ToolName = "LPD Analysis";
                            tp.DllName = "HOK.LPDCalculator.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("bulb.png");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.LEEDView:
                            tp.ToolName = "LEED View Analysis";
                            tp.DllName = "HOK.ViewAnalysis.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("eq.ico");

                            dictionary.Add(tool, tp);
                            break;

                        case ToolEnum.ElementFlatter:
                            tp.ToolName = "Element Flatter";
                            tp.DllName = "HOK.ElementFlatter.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("create.ico");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.Utility:
                            tp.ToolName = "Utility Tools";
                            tp.DllName = "HOK.Utilities.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("toolbox.png");

                            dictionary.Add(tool, tp);
                            break;
                        case ToolEnum.ColorEditor:
                            tp.ToolName = "Color Editor";
                            tp.DllName = "HOK.ColorSchemeEditor.dll";
                            tp.BetaPath = BetaDirectory + tp.DllName;
                            tp.BetaExist = File.Exists(tp.BetaPath);
                            tp.InstallPath = InstallDirectory + tp.DllName;
                            tp.InstallExist = File.Exists(tp.InstallPath);
                            if (tp.InstallExist) { tp.TempAssemblyPath = GetTempInstallPath(tp.InstallPath); }
                            tp.ToolIcon = ImageUtil.LoadBitmapImage("color32.png");
                            dictionary.Add(tool, tp);
                            break;
                       
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get information about beta tools.\n" + ex.Message, "Get Tool Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return dictionary;
        }

        private string GetTempInstallPath(string installPath)
        {
            var tempPath="";
            try
            {
                var fileName = Path.GetFileName(installPath);
                tempPath = Path.Combine(TempInstallDirectory, fileName);
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

        private static List<string> GetFiles(ToolEnum toolName)
        {
            var fileNames = new List<string>();
            try
            {
                switch (toolName)
                {
                    case ToolEnum.ElementTools:
                        fileNames.Add("HOK.ElementTools.dll");
                        break;

                    case ToolEnum.ParameterTools:
                        fileNames.Add("HOK.ParameterTools.dll");
                        break;

                    case ToolEnum.SheetManager:
                        fileNames.Add("HOK.SheetManager.dll");
                        break;

                    case ToolEnum.BCFReader:
                        fileNames.Add("HOK.BCFReader.dll");
                        fileNames.Add("ICSharpCode.SharpZipLib.dll");
                        fileNames.Add("Resources\\markup.xsd");
                        fileNames.Add("Resources\\visinfo.xsd");
                        break;

                    case ToolEnum.MassTool:
                        fileNames.Add("HOK.RoomsToMass.dll");
                        break;

                    case ToolEnum.RevitData:
                        fileNames.Add("HOK.RevitDBManager.dll");
                        fileNames.Add("Resources\\default.bmp");
                        fileNames.Add("Resources\\eye.ico");
                        break;

                    case ToolEnum.AVF:
                        fileNames.Add("HOK.AVFManager.dll");
                        fileNames.Add("Resources\\DefaultSettings.xml");
                        fileNames.Add("Resources\\PointOfView.rfa");
                        break;

                    case ToolEnum.LPDAnalysis:
                        fileNames.Add("HOK.LPDCalculator.dll");
                        break;

                    case ToolEnum.LEEDView:
                        fileNames.Add("HOK.ViewAnalysis.dll");
                        fileNames.Add("Resources\\Addins Shared Parameters.txt");
                        break;

                    case ToolEnum.ElementFlatter:
                        fileNames.Add("HOK.ElementFlatter.dll");
                        break;

                    case ToolEnum.Utility:
                        fileNames.Add("HOK.Utilities.dll");
                        fileNames.Add("HOK.Arrowhead.dll");
                        fileNames.Add("HOK.CeilingHeight.dll");
                        fileNames.Add("HOK.DoorRoom.dll");
                        fileNames.Add("HOK.FinishCreator.dll");
                        fileNames.Add("HOK.LevelManager.dll");
                        fileNames.Add("HOK.RoomElevation.dll");
                        fileNames.Add("HOK.RoomUpdater.dll");
                        fileNames.Add("HOK.ViewDepth.dll");
                        fileNames.Add("HOK.WorksetView.dll");
                        fileNames.Add("HOK.CameraDuplicator.dll");
                        fileNames.Add("HOK.RenameFamily.dll");
                        fileNames.Add("HOK.XYZLocator.dll");
                        fileNames.Add("HOK.RoomMeasure.dll");
                        break;

                    case ToolEnum.ColorEditor:
                        fileNames.Add("HOK.ColorSchemeEditor.dll");
                        fileNames.Add("ICSharpCode.SharpZipLib.dll");
                        fileNames.Add("Xceed.Wpf.Toolkit.dll");
                        break;
                    case ToolEnum.SmartBCF:
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
            return fileNames;
        }

    }

    public class ToolProperties
    {
        public ToolEnum ToolEnumVal { get; set; } = ToolEnum.None;
        public string ToolName { get; set; } = "";
        public string DllName { get; set; } = "";
        public string InstallPath { get; set; } = "";
        public string BetaPath { get; set; } = "";
        public string TempAssemblyPath { get; set; } = "";
        public FileVersionInfo BetaVersionInfo { get; set; }
        public FileVersionInfo InstalledVersionInfo { get; set; }
        public string BetaVersionNumber { get; set; } = "Not Exist";
        public string BetaReleasedDate { get; set; } = "";
        public string InstallVersionNumber { get; set; } = "Not Installed";
        public bool BetaExist { get; set; }
        public bool InstallExist { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSelected { get; set; }
        public BitmapImage ToolIcon { get; set; }
        public List<string> InstallingFiles { get; set; } = new List<string>();

        public ToolProperties()
        {
        }

        public ToolProperties(ToolProperties tp)
        {
            ToolEnumVal = tp.ToolEnumVal;
            ToolName = tp.ToolName;
            DllName = tp.DllName;
            InstallPath = tp.InstallPath;
            BetaPath = tp.BetaPath;
            BetaVersionInfo = tp.BetaVersionInfo;
            InstalledVersionInfo = tp.InstalledVersionInfo;
            BetaVersionNumber = tp.BetaVersionNumber;
            BetaReleasedDate = tp.BetaReleasedDate;
            InstallVersionNumber = tp.InstallVersionNumber;
            BetaExist = tp.BetaExist;
            InstallExist = tp.InstallExist;
            IsEnabled = tp.IsEnabled;
            IsSelected = tp.IsSelected;
            ToolIcon = tp.ToolIcon;
            InstallingFiles = tp.InstallingFiles;
        }
    }

    public static class ImageUtil
    {
        public static BitmapImage LoadBitmapImage(string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(prefix + imageName);

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
