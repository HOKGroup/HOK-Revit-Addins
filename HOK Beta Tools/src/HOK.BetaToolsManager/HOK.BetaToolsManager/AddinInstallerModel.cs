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
                if (!addin.Install) continue;

                // Reset installed version flag to update datagrid control
                addin.InstalledVersion = "Not installed";

                // (Konrad) Button needs to be disabled after DLLs were removed since it doesn't work anyways.
                var app = AppCommand.Instance.m_app;
                var panel = app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel);
                var button = panel?.GetItems().FirstOrDefault(x => x.ItemText == addin.ButtonText);
                if (button != null)
                {
                    button.Visible = false;
                    panel.Visible = panel.GetItems().Any(x => x.Visible);
                    ((PushButton)button).AssemblyName = InstallDirectory + "Temp.dll";
                }

                // remove dependancies directory
                if (Directory.Exists(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                {
                    try
                    {
                        Directory.Delete(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name, true);
                    }
                    catch
                    {
                        Log.AppendLog(LogMessageType.ERROR, "Could not delete addin directory. Moving on.");
                    }

                }
            }
        }

        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        public void InstallUpdateAddins(ObservableCollection<AddinWrapper> addins)
        {
            foreach (var addin in addins)
            {
                if (!addin.Install) continue;

                //// move the directory with dependancies to install location
                //if (!Directory.Exists(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                //{
                //    try
                //    {
                //        Directory.CreateDirectory(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                //    }
                //    catch
                //    {
                //        Log.AppendLog(LogMessageType.ERROR, "Could not delete addin directory. Moving on.");
                //    }
                //}
                //// either fill out the empty directory with content
                //// or if it existed just override it with new one
                //CopyDirectory(addin.BetaResourcesPath, InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);


                //// if temp version of the currently installed addin doesnt exist yet, let's add it in
                //// and make a copy of all contents so that we can bind the button to temp location
                //if (!Directory.Exists(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                //{
                //    try
                //    {
                //        Directory.CreateDirectory(TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                //    }
                //    catch
                //    {
                //        Log.AppendLog(LogMessageType.ERROR, "Could not delete addin directory. Moving on.");
                //    }
                //    // don't touch temp if it existed hence this is just an update
                //    CopyDirectory(addin.BetaResourcesPath, TempDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);
                //}
                


                // (Konrad) Bind button to Temp directory dll
                if (string.IsNullOrEmpty(addin.Panel))
                {
                    // no UI addin
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
                        ((PushButton)button).AssemblyName = TempDirectory + addin.DllRelativePath;
                    }
                }

                // Reset installed version flag to update datagrid control
                addin.InstalledVersion = addin.Version;
                addin.IsInstalled = true;

                //// move the addin file
                //if (File.Exists(InstallDirectory + Path.GetFileName(addin.AddinFilePath)))
                //{
                //    try
                //    {
                //        File.Delete(InstallDirectory + Path.GetFileName(addin.AddinFilePath));
                //    }
                //    catch
                //    {
                //        Log.AppendLog(LogMessageType.ERROR, "Could not delete Addin File. Moving on.");
                //    }
                //}
                //File.Copy(addin.AddinFilePath, InstallDirectory + Path.GetFileName(addin.AddinFilePath));

                //// move the directory with dependancies
                //if (Directory.Exists(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name))
                //{
                //    try
                //    {
                //        Directory.Delete(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name, true);
                //    }
                //    catch (Exception e)
                //    {
                //        Log.AppendLog(LogMessageType.ERROR, "Could not delete addin directory. Moving on.");
                //    }
                //}
                //Directory.CreateDirectory(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);

                ////CopyFilesRecursively(new DirectoryInfo(addin.BetaResourcesPath), new DirectoryInfo(InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name));
                //CopyDirectory(addin.BetaResourcesPath, InstallDirectory + new DirectoryInfo(addin.BetaResourcesPath).Name);


            }
        }

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

        private void LoadAddinsOnStartup()
        {
            var dic = new Dictionary<string, AddinWrapper>();
            if (Directory.Exists(BetaDirectory))
            {
                // create Temp folder if it doesn't exists.
                if (!Directory.Exists(TempDirectory))
                {
                    Directory.CreateDirectory(TempDirectory);
                }

                // (Konrad) Create a copy of all installed plugins in temp location
                // we will bind all buttons to temp Dlls so that we can keep install folder untouched
                // exclude Temp folder 
                foreach (var dir in Directory.GetDirectories(BetaDirectory))
                {
                    if (!Directory.Exists(InstallDirectory + new DirectoryInfo(dir).Name))
                        Directory.CreateDirectory(InstallDirectory + new DirectoryInfo(dir).Name);
                    CopyDirectory(dir, InstallDirectory + new DirectoryInfo(dir).Name);
                }

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
                        if (File.Exists(TempDirectory + dllRelativePath))
                        {
                            var a = Assembly.LoadFile(TempDirectory + dllRelativePath);
                            installedVersion = a.GetName().Version.ToString();
                            installed = true;
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

        public List<string> GetNamesOfAssembliesReferencedBy(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(assemblyName => assemblyName.Name + ".dll").ToList();
        }
    }

    //public static class Serialization
    //{
    //    /// <summary>
    //    /// Deserializes JSON string into Collection of AddinWrappers.
    //    /// </summary>
    //    /// <param name="filePath">Path to file.</param>
    //    /// <returns></returns>
    //    public static ObservableCollection<AddinWrapper> Deserialize(string filePath)
    //    {
    //        var jsonString = File.ReadAllText(filePath);
    //        var settings = new JsonSerializerSettings
    //        {
    //            TypeNameHandling = TypeNameHandling.Auto
    //        };
    //        var deserialized = JsonConvert.DeserializeObject<ObservableCollection<AddinWrapper>>(jsonString, settings);

    //        return deserialized;
    //    }

    //    /// <summary>
    //    /// Serializes Addins selections for future use.
    //    /// </summary>
    //    /// <param name="filePath">Path to file.</param>
    //    /// <param name="addins">Collection of AddinWrappers to be serialized.</param>
    //    /// <returns></returns>
    //    public static string Serialize(string filePath, ObservableCollection<AddinWrapper> addins)
    //    {
    //        var settings = new JsonSerializerSettings
    //        {
    //            TypeNameHandling = TypeNameHandling.Auto
    //        };
    //        var jsonString = JsonConvert.SerializeObject(addins, Formatting.Indented, settings);
    //        File.WriteAllText(filePath, jsonString);

    //        return new FileInfo(filePath).Length > 0 ? filePath : string.Empty;
    //    }
    //}
}
