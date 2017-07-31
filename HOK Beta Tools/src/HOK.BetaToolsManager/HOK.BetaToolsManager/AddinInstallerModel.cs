using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Newtonsoft.Json;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerModel
    {
        public string VersionNumber { get; set; }
        public string BetaDirectory { get; set; } = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        //public string BetaDirectory { get; set; } = @"C:\Users\konrad.sobon\Desktop\test_beta_location\";
        public string InstallDirectory { get; set; }
        public string TempDirectory { get; set; }
        public string BetaTempDirectory { get; set; }
        public ObservableCollection<AddinWrapper> Addins { get; set; }

        public AddinInstallerModel(string version)
        {
            VersionNumber = version;
            BetaDirectory = BetaDirectory + VersionNumber + @"\"; // at HOK Group drive
            BetaTempDirectory = BetaDirectory + @"Temp\"; // at HOK drive
            InstallDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                               + @"\Autodesk\Revit\Addins\"
                               + VersionNumber + @"\"; // user roaming location
            TempDirectory = InstallDirectory + @"Temp\";
            
            LoadAddinsOnStartup();
        }

        /// <summary>
        /// Removes addins from the ribbon.
        /// </summary>
        /// <param name="addins"></param>
        public void UninstallAddins(ObservableCollection<AddinWrapper> addins)
        {
            foreach (var addin in addins)
            {
                if (!addin.IsSelected) continue;

                // Reset installed version flag to update datagrid control
                addin.InstalledVersion = "Not installed";
                addin.IsInstalled = false;

                if (string.IsNullOrEmpty(addin.Panel))
                {
                    // no UI addin
                    // remove addin file
                    if (File.Exists(InstallDirectory + Path.GetFileName(addin.AddinFilePath)))
                    {
                        try
                        {
                            File.Delete(InstallDirectory + Path.GetFileName(addin.AddinFilePath));
                        }
                        catch
                        {
                            Log.AppendLog(LogMessageType.ERROR, "Could not delete Addin File. Moving on.");
                        }
                    }
                }

                // (Konrad) Button needs to be disabled after DLLs were removed since it doesn't work anyways.
                var app = AppCommand.Instance.m_app;
                var panel = app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);
                var button = panel?.GetItems().FirstOrDefault(x => x.ItemText == addin.ButtonText);
                if (button != null)
                {
                    button.Visible = false;
                    panel.Visible = panel.GetItems().Any(x => x.Visible);
                }
            }
        }

        /// <summary>
        /// Adds addins to the ribbon.
        /// </summary>
        /// <param name="addins"></param>
        public void InstallUpdateAddins(ObservableCollection<AddinWrapper> addins)
        {
            foreach (var addin in addins)
            {
                if (!addin.IsSelected) continue;

                // (Konrad) Bind button to Temp directory dll
                if (string.IsNullOrEmpty(addin.Panel))
                {
                    // no UI addin
                    // move the addin file
                    if (File.Exists(InstallDirectory + Path.GetFileName(addin.AddinFilePath)))
                    {
                        try
                        {
                            File.Delete(InstallDirectory + Path.GetFileName(addin.AddinFilePath));
                        }
                        catch
                        {
                            Log.AppendLog(LogMessageType.ERROR, "Could not delete Addin File. Moving on.");
                        }
                    }
                    File.Copy(TempDirectory + Path.GetFileName(addin.AddinFilePath), InstallDirectory + Path.GetFileName(addin.AddinFilePath));

                    if (!Directory.Exists(
                        InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                    {
                        Directory.CreateDirectory(
                            InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                        CopyIfNewer(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name,
                            InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                    }
                    else
                    {
                        CopyIfNewer(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name,
                            InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                    }
                }
                else
                {
                    var app = AppCommand.Instance.m_app;
                    var panel = app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);
                    var button = panel?.GetItems().FirstOrDefault(x => x.ItemText == addin.ButtonText);
                    if (button != null)
                    {
                        button.Visible = true;
                        panel.Visible = panel.GetItems().Any(x => x.Visible);
                        ((PushButton)button).AssemblyName = InstallDirectory + addin.DllRelativePath;
                    }
                }

                // Reset installed version flag to update datagrid control
                addin.InstalledVersion = addin.Version;
                addin.IsInstalled = true;
            }
        }

        /// <summary>
        /// Main method for loading and parsing of the beta/installed folders.
        /// </summary>
        private void LoadAddinsOnStartup()
        {
            var addins = File.Exists(InstallDirectory + "BetaSettings.json")
                ? DeserializeSetting(InstallDirectory + "BetaSettings.json")
                : null;

            var dic = new Dictionary<string, AddinWrapper>();

            // (Konrad) It's possible for user to be offline, and have no access to Beta HOK Drive.
            // In that case we still want to create addins, but instead use the Temp location on local drive.
            var betaTemp2 = Directory.Exists(BetaDirectory)
                ? BetaTempDirectory
                : Directory.Exists(TempDirectory)
                    ? TempDirectory
                    : string.Empty;

            if (betaTemp2 != string.Empty)
            {
                if (!Directory.Exists(TempDirectory))
                    Directory.CreateDirectory(TempDirectory);

                // (Konrad) Create a copy of all installed plugins by copying the temp dir from beta
                // We only do this if Beta is accessible otherwise we use local temp
                if (Directory.Exists(BetaDirectory))
                    CopyIfNewer(BetaTempDirectory, TempDirectory);

                // (Konrad) Get all addins from beta directory, check their versions agains installed
                foreach (var file in Directory.GetFiles(betaTemp2, "*.addin"))
                {
                    var dllRelativePath = ParseXml(file); // relative to temp
                    var dllPath = betaTemp2 + dllRelativePath;

                    // (Konrad) Using LoadFrom() instead of LoadFile() because
                    // LoadFile() doesn't load dependent assemblies causing exception later.
                    var assembly = Assembly.LoadFrom(dllPath);
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types = e.Types;
                    }
                    foreach (var t in types.Where(x => x != null &&
                                                       (x.GetInterface("IExternalCommand") != null ||
                                                        x.GetInterface("IExternalApplication") != null)))
                    {
                        MemberInfo info = t;
                        var nameAttr = (NameAttribute) info.GetCustomAttributes(typeof(NameAttribute), true)
                            .FirstOrDefault();
                        var descAttr = (DescriptionAttribute) t.GetCustomAttributes(typeof(DescriptionAttribute), true)
                            .FirstOrDefault();
                        var imageAttr = (ImageAttribute) t.GetCustomAttributes(typeof(ImageAttribute), true)
                            .FirstOrDefault();
                        var namespaceAttr = (NamespaceAttribute) t.GetCustomAttributes(typeof(NamespaceAttribute), true)
                            .FirstOrDefault();

                        if (nameAttr == null || descAttr == null || imageAttr == null ||
                            namespaceAttr == null) continue;

                        var bitmap =
                            (BitmapSource) ButtonUtil.LoadBitmapImage(assembly, namespaceAttr.Namespace,
                                imageAttr.ImageName);
                        var version = assembly.GetName().Version.ToString();

                        var installedVersion = "Not installed";
                        var installed = false;
                        var autoUpdate = false;

                        var addin = addins?.FirstOrDefault(x => x.DllRelativePath == dllRelativePath);
                        if (addin != null)
                        {
                            installed = addin.IsInstalled;
                            autoUpdate = addin.AutoUpdate;

                            if (installed)
                            {
                                installedVersion = addin.AutoUpdate ? version : addin.InstalledVersion;

                                // if directory doesn't exist in "installed" it means that it was not yet installed before.
                                if (!Directory.Exists(
                                    InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                                {
                                    Directory.CreateDirectory(
                                        InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                                    CopyIfNewer(betaTemp2 + new DirectoryInfo(addin.BetaResourcesPath).Name,
                                        InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                                }
                                else
                                {
                                    // directory exists, which means it was installed before, let's check if its on autoupdate
                                    if (addin.AutoUpdate)
                                    {
                                        // let's automatically copy the latest version in
                                        // we can use temp directory here since it was already either updated with latest
                                        // or is the only source of files (no network drive)
                                        CopyIfNewer(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name,
                                            InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                                    }
                                }
                            }
                        }

                        var aw = new AddinWrapper
                        {
                            Name = nameAttr.Name,
                            Description = descAttr.Description,
                            Image = bitmap,
                            ImageName = imageAttr.ImageName,
                            CommandNamespace = t.FullName,
                            Version = version,
                            IsInstalled = installed,
                            InstalledVersion = installedVersion,
                            BetaResourcesPath = Path.GetDirectoryName(dllPath),
                            AddinFilePath = file,
                            DllRelativePath = dllRelativePath,
                            AutoUpdate = autoUpdate
                        };

                        if (t.GetInterface("IExternalCommand") != null)
                        {
                            var buttonTextAttr =
                                (ButtonTextAttribute) t.GetCustomAttributes(typeof(ButtonTextAttribute), true)
                                    .FirstOrDefault();
                            var panelNameAttr =
                                (PanelNameAttribute) t.GetCustomAttributes(typeof(PanelNameAttribute), true)
                                    .FirstOrDefault();

                            aw.Panel = panelNameAttr?.PanelName;
                            aw.ButtonText = buttonTextAttr?.ButtonText;
                        }

                        dic.Add(aw.Name, aw);
                    }
                }
            }
            var output = new ObservableCollection<AddinWrapper>(dic.Values.ToList().OrderBy(x => x.Name));
            Addins = output;
        }

        /// <summary>
        /// Read Addin Manifest file to extract DLL path.
        /// </summary>
        /// <param name="file">File path.</param>
        /// <returns></returns>
        private static string ParseXml(string file)
        {
            var value = string.Empty;
            var response = File.ReadAllText(file);
            var doc = XDocument.Parse(response);

            foreach (var element in doc.Descendants("Assembly"))
            {
                value = (string)element;
                break;
            }
            return value;
        }

        /// <summary>
        /// Copies contents of one directory into another if/when source is newer version of DLL, or file changed.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        private static void CopyIfNewer(string sourceDir, string targetDir)
        {
            // (Konrad) Precreate all Directories - limited overhead since existing are automatically skipped
            foreach (var dirPath in Directory.GetDirectories(sourceDir, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));

            // (Konrad) Copy all files only if newer versions exist.
            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file) == ".dll")
                {
                    var sourceVersion = FileVersionInfo.GetVersionInfo(file).FileVersion;
                    if (File.Exists(file.Replace(sourceDir, targetDir)))
                    {
                        var targetVersion = FileVersionInfo.GetVersionInfo(file.Replace(sourceDir, targetDir))
                            .FileVersion;
                        if (sourceVersion != targetVersion)
                        {
                            File.Copy(file, file.Replace(sourceDir, targetDir), true);
                        }
                    }
                    else
                    {
                        File.Copy(file, file.Replace(sourceDir, targetDir), true);
                    }
                }
                else
                {
                    var sourceSize = new FileInfo(file).Length;
                    if (File.Exists(file.Replace(sourceDir, targetDir)))
                    {
                        var targetSize = new FileInfo(file.Replace(sourceDir, targetDir)).Length;
                        if (sourceSize != targetSize)
                        {
                            File.Copy(file, file.Replace(sourceDir, targetDir), true);
                        }
                    }
                    else
                    {
                        File.Copy(file, file.Replace(sourceDir, targetDir), true);
                    }
                }
            }
        }

        /// <summary>
        /// Deserializes Settings file for installed addins.
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <returns>Addin Wrapper objects.</returns>
        public ObservableCollection<AddinWrapper> DeserializeSetting(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var settings = JsonConvert.DeserializeObject<ObservableCollection<AddinWrapper>>(json);
            return settings ?? new ObservableCollection<AddinWrapper>();
        }
    }
}
