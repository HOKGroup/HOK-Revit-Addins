using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
#if Release18 || Release17 || Release16 || Release15
        public string BetaDirectory { get; set; } = @"\\group\sysvol\group.hok.com\HOK\Tools\Revit\";
#else
        public string BetaDirectory { get; set; } = @"C:\Users\" + Environment.UserName + @"\Desktop\BetaFiles Testing\";
#endif
        public string VersionNumber { get; set; }
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

                var app1 = AppCommand.Instance.m_app;
                var panel = app1.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);

                if (string.IsNullOrEmpty(addin.ButtonText))
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

                    // (Konrad) We are using Idling event here for the reason that depending on order
                    // at which these buttons are installed or added to the ribbon, at the moment that
                    // we are adding this particular tool, it could not be ready on the ribbon yet, 
                    // button variable will be null and we won't be able to disable it.
                    AppCommand.EnqueueTask(app =>
                    {
                        if (panel != null && addin.AdditionalButtonNames != null)
                        {
                            var splits = addin.AdditionalButtonNames.Split(';');
                            foreach (var name in splits)
                            {
                                var button1 = panel.GetItems().FirstOrDefault(x => x.Name == name);
                                if (button1 != null)
                                {
                                    button1.Visible = false;
                                    panel.Visible = panel.GetItems().Any(x => x.Visible);
                                }
                            }
                        }
                    });
                }

                // (Konrad) Button needs to be disabled after DLLs were removed since it doesn't work anyways.
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

                // (Konrad) Currently the only way to distinguish between ExternalCommands and ExternalApplications
                // is via "ButtonText" attribute. It should be empty for ExternalApplications. 
                if (string.IsNullOrEmpty(addin.ButtonText))
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
                        CopyAll(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name,
                            InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                    }
                    else
                    {
                        CopyAll(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name,
                            InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                    }

                    // (Konrad) We are using Idling event here for the reason that depending on order
                    // at which these buttons are installed or added to the ribbon, at the moment that
                    // we are adding this particular tool, it could not be ready on the ribbon yet, 
                    // button variable will be null and we won't be able to disable it.
                    AppCommand.EnqueueTask(app =>
                    {
                        var panel = app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);
                        if (panel != null && addin.AdditionalButtonNames != null)
                        {
                            var splits = addin.AdditionalButtonNames.Split(';');
                            foreach (var name in splits)
                            {
                                var button = panel.GetItems().FirstOrDefault(x => x.Name == name);
                                if (button != null)
                                {
                                    button.Enabled = false;
                                    panel.Visible = panel.GetItems().Any(x => x.Visible);
                                }
                            }
                        }
                    });
                }
                else
                {
                    var app = AppCommand.Instance.m_app;
                    var panel = app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);
                    var button = panel?.GetItems().FirstOrDefault(x => x.ItemText == addin.ButtonText);
                    if (button != null)
                    {
                        button.Visible = true;
                        button.Enabled = false;
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
                    CopyAll(BetaTempDirectory, TempDirectory);

                // (Konrad) We use the direcotry on the local drive that was already copied over to scan for addins.
                var availableAddins = Directory.GetFiles(betaTemp2, "*.addin");

                // Cleans any holdovers from old beta addins
                RemoveLegacyPlugins(availableAddins);

                // (Konrad) Get all addins from beta directory
                foreach (var file in availableAddins)
                {
                    var dllRelativePath = ParseXml(file); // relative to temp
                    var dllPath = TempDirectory + dllRelativePath;
                    if(!File.Exists(dllPath)) continue;

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
                        var nameAttr = (NameAttribute) info.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault();
                        var descAttr = (DescriptionAttribute) t.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                        var imageAttr = (ImageAttribute) t.GetCustomAttributes(typeof(ImageAttribute), true).FirstOrDefault();
                        var namespaceAttr = (NamespaceAttribute) t.GetCustomAttributes(typeof(NamespaceAttribute), true).FirstOrDefault();
                        var panelNameAttr = (PanelNameAttribute) t.GetCustomAttributes(typeof(PanelNameAttribute), true).FirstOrDefault();

                        if (nameAttr == null || descAttr == null || imageAttr == null ||
                            namespaceAttr == null || panelNameAttr == null) continue;

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
                                    CopyAll(betaTemp2 + new DirectoryInfo(addin.BetaResourcesPath).Name,
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
                                        CopyAll(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name,
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
                            AutoUpdate = autoUpdate,
                            Panel = panelNameAttr.PanelName
                        };

                        if (t.GetInterface("IExternalCommand") != null)
                        {
                            var buttonTextAttr =
                                (ButtonTextAttribute) t.GetCustomAttributes(typeof(ButtonTextAttribute), true)
                                    .FirstOrDefault();
                            
                            aw.ButtonText = buttonTextAttr?.ButtonText;
                        }
                        else
                        {
                            var additionalButtonNamesAttr = (AdditionalButtonNamesAttribute) t
                                .GetCustomAttributes(typeof(AdditionalButtonNamesAttribute), true)
                                .FirstOrDefault();

                            aw.AdditionalButtonNames = additionalButtonNamesAttr?.AdditionalNames;
                        }

                        dic.Add(aw.Name, aw);
                    }
                }
            }
            var output = new ObservableCollection<AddinWrapper>(dic.Values.ToList().OrderBy(x => x.Name));
            Addins = output;
        }

        /// <summary>
        /// In case that we discontinued some Beta Plugin, or moved it to ProgramData, it's possible for some *.addin
        /// files to remain behind on user machines. It's best to remove them, to prevent any potential load errors.
        /// </summary>
        /// <param name="availableAddins">An array of *.addin files that are available to be installed in Temp folder.</param>
        private void RemoveLegacyPlugins(string[] availableAddins)
        {
            var availableFileNames = availableAddins.Select(Path.GetFileName).ToList();
            var installedAddins = Directory.GetFiles(InstallDirectory, "*.addin");
            foreach (var installedAddin in installedAddins)
            {
                var fileName = Path.GetFileName(installedAddin);
                if (!string.IsNullOrEmpty(fileName) && fileName.StartsWith("HOK.") && !availableFileNames.Contains(fileName))
                {
                    var dllPath = ParseXml(installedAddin);
                    var folder = Path.GetDirectoryName(dllPath);
                    try
                    {
                        if (Directory.Exists(folder)) Directory.Delete(Path.Combine(InstallDirectory, folder), true);
                        File.Delete(installedAddin);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }
            }
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
        /// Copies all contents of one directory to another.
        /// </summary>
        /// <param name="sourceDir">Source directory path.</param>
        /// <param name="targetDir">Target directory path.</param>
        private static void CopyAll(string sourceDir, string targetDir)
        {
            try
            {
                foreach (var dirPath in Directory.GetDirectories(sourceDir, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourceDir, targetDir));

                foreach (var newPath in Directory.GetFiles(sourceDir, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourceDir, targetDir), true);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
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
