using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using HOK.Core.Utilities;

namespace HOK.BetaToolsManager
{
    public class AddinInstallerModel
    {
        public string VersionNumber { get; set; }
        public string BetaDirectory { get; set; } = @"\\Group\hok\FWR\RESOURCES\Apps\HOK AddIns Installer\Beta Files\";
        public string InstallDirectory { get; set; }
        public string TempInstallDirectory { get; set; }
        public Dictionary<ToolEnum, ToolProperties> ToolInfoDictionary { get; set; } = new Dictionary<ToolEnum, ToolProperties>();

        public AddinInstallerModel(string version)
        {
            VersionNumber = version;
        }

        public ObservableCollection<AddinWrapper> LoadAddins()
        {
            BetaDirectory = BetaDirectory + VersionNumber + @"\HOK-Addin.bundle\Contents_Beta\"; // at HOK Group drive
            InstallDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                               + @"\Autodesk\Revit\Addins\"
                               + VersionNumber
                               + @"\HOK-Addin.bundle\Contents_Beta\"; // user roaming location

            var dic = new Dictionary<string, AddinWrapper>();
            if (Directory.Exists(InstallDirectory))
            {
                foreach (var file in Directory.GetFiles(InstallDirectory, "*.dll"))
                {
                    // (Konrad) Using LoadFrom() instead of LoadFile() because
                    // LoadFile() doesn't load dependent assemblies causing exception later.
                    var assembly = Assembly.LoadFrom(file);
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        types = e.Types;
                    }
                    foreach (var t in types.Where(x => x != null && x.GetInterface("IExternalCommand") != null))
                    {
                        MemberInfo info = t;
                        var nameAttribute = info.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() as NameAttribute;
                        var descAttribute = t.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault() as DescriptionAttribute;
                        var imageAttribute = t.GetCustomAttributes(typeof(ImageAttribute), true).FirstOrDefault() as ImageAttribute;
                        var buttonTextAttribute = t.GetCustomAttributes(typeof(ButtonTextAttribute), true).FirstOrDefault() as ButtonTextAttribute;
                        var panelNameAttribute = t.GetCustomAttributes(typeof(PanelNameAttribute), true).FirstOrDefault() as PanelNameAttribute;
                        if (nameAttribute == null
                            || descAttribute == null
                            || imageAttribute == null
                            || buttonTextAttribute == null
                            || panelNameAttribute == null) continue;

                        var aw = new AddinWrapper
                        {
                            Name = nameAttribute.Name,
                            Description = descAttribute.Description,
                            //Image = imageAttribute.Image,
                            ImageName = imageAttribute.ImageName, // (Konrad) Image name is assigned from Image attribute so order matters here.
                            Panel = panelNameAttribute.PanelName,
                            ButtonText = buttonTextAttribute.ButtonText,
                            FullName = t.FullName
                        };
                        dic.Add(aw.Name, aw);
                    }
                }
            }
            var output = new ObservableCollection<AddinWrapper>(dic.Values.ToList().OrderBy(x => x.Name));
            return output;
        }
    }
}
