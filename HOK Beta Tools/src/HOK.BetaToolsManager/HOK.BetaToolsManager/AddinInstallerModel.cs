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
        public string VersionNumber { get; set; }
        //public string BetaDirectory { get; set; } = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        public string BetaDirectory { get; set; } = @"C:\Users\konrad.sobon\Desktop\test_beta_location\";
        public string InstallDirectory { get; set; }
        public string TempDirectory { get; set; }
        public ObservableCollection<AddinWrapper> Addins { get; set; }

        public AddinInstallerModel(string version)
        {
            VersionNumber = version;
            BetaDirectory = BetaDirectory + VersionNumber + @"\"; // at HOK Group drive
            InstallDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                               + @"\Autodesk\Revit\Addins\"
                               + VersionNumber + @"\"; // user roaming location
            TempDirectory = InstallDirectory + @"Temp\";

            LoadAddinsOnStartup();
        }

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
                    File.Copy(addin.AddinFilePath, InstallDirectory + Path.GetFileName(addin.AddinFilePath));
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

        private void LoadAddinsOnStartup()
        {
            var addins = File.Exists(InstallDirectory + "BetaSettings.json")
                ? DeserializeSetting(InstallDirectory + "BetaSettings.json")
                : null;

            var dic = new Dictionary<string, AddinWrapper>();
            if (Directory.Exists(BetaDirectory))
            {
                // create Temp folder if it doesn't exists and temp.dll to bind to
                if (!Directory.Exists(TempDirectory))
                    Directory.CreateDirectory(TempDirectory);

                // (Konrad) Create a copy of all installed plugins by copying the temp dir from beta
                CopyDirectory(BetaDirectory + @"Temp\", TempDirectory);

                // (Konrad) Get all addins from beta directory, check their versions agains installed
                foreach (var file in Directory.GetFiles(BetaDirectory, "*.addin"))
                {
                    var dllRelativePath = ParseXml(file);
                    var dllPath = BetaDirectory + dllRelativePath;

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
                    foreach (var t in types.Where(x => x != null && (x.GetInterface("IExternalCommand") != null || x.GetInterface("IExternalApplication") != null)))
                    {
                        MemberInfo info = t;
                        var nameAttr = (NameAttribute)info.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault();
                        var descAttr = (DescriptionAttribute)t.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                        var imageAttr = (ImageAttribute)t.GetCustomAttributes(typeof(ImageAttribute), true).FirstOrDefault();
                        var namespaceAttr = (NamespaceAttribute)t.GetCustomAttributes(typeof(NamespaceAttribute), true).FirstOrDefault();

                        if (nameAttr == null || descAttr == null || imageAttr == null || namespaceAttr == null) continue;

                        var bitmap = (BitmapSource)ButtonUtil.LoadBitmapImage(assembly, namespaceAttr.Namespace, imageAttr.ImageName);
                        var version = assembly.GetName().Version.ToString();

                        var installedVersion = "Not installed";
                        var installed = false;
                        var addin = addins?.FirstOrDefault(x => x.Name == nameAttr.Name);
                        if (addin != null)
                        {
                            installedVersion = addin.InstalledVersion;
                            installed = addin.IsInstalled;

                            if (installed)
                            {
                                // get the latest version of it from beta
                                if (!Directory.Exists(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                                    Directory.CreateDirectory(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                                CopyDirectory(addin.BetaResourcesPath, InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
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
                            DllRelativePath = dllRelativePath
                        };

                        if (t.GetInterface("IExternalCommand") != null)
                        {
                            var buttonTextAttr = (ButtonTextAttribute)t.GetCustomAttributes(typeof(ButtonTextAttribute), true).FirstOrDefault();
                            var panelNameAttr = (PanelNameAttribute)t.GetCustomAttributes(typeof(PanelNameAttribute), true).FirstOrDefault();

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
        /// Copy Directory and its contents to another directory.
        /// </summary>
        /// <param name="sourcePath">Source Path</param>
        /// <param name="destinationPath">Destination Path</param>
        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                foreach (var dirPath in Directory.GetDirectories(sourcePath, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

                foreach (var newPath in Directory.GetFiles(sourcePath, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
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
            if (settings == null) return new ObservableCollection<AddinWrapper>();

            return settings;
        }
    }
}
