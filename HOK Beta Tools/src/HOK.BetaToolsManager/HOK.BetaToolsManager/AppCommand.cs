using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Newtonsoft.Json;

namespace HOK.BetaToolsManager
{
    public class AppCommand : IExternalApplication
    {
        public UIControlledApplication m_app;
        public static AppCommand Instance;
        private const string tabName = "  HOK - Beta";
        private string currentAssembly = "";
        public AddinInstallerModel ViewModel { get; set; }
        public static List<AddinWrapper> AddinsToSetPathsFor { get; } = new List<AddinWrapper>();

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            Instance = this;
            m_app = application;
            var versionNumber = m_app.ControlledApplication.VersionNumber;

            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch
            {
                Log.AppendLog(LogMessageType.WARNING, "Ribbon tab was not created. It might already exist.");
            }
            
            ViewModel = new AddinInstallerModel(versionNumber);
            var addins = ViewModel.Addins;

            foreach (var addin in addins)
            {
                if (string.IsNullOrEmpty(addin.Panel))
                {
                    if (addin.IsInstalled)
                    {
                        // there should be a manifest file existing in Installed directory
                        // that will make Revit latch onto the proper dll by default
                        // we only need to repath it to installed folder for installed plugins
                        var response = File.ReadAllText(addin.AddinFilePath);
                        var doc = XDocument.Parse(response);

                        foreach (var element in doc.Descendants("Assembly"))
                        {
                            var currentPath = (string)element;
                            var newPath = currentPath.Replace("Temp\\", "");
                            element.SetValue(newPath);
                            break;
                        }
                        if (File.Exists(ViewModel.InstallDirectory + Path.GetFileName(addin.AddinFilePath)))
                        {
                            try
                            {
                                File.Delete(ViewModel.InstallDirectory + Path.GetFileName(addin.AddinFilePath));
                            }
                            catch
                            {
                                Log.AppendLog(LogMessageType.ERROR, "Could not delete existing Addin Manifest.");
                            }
                        }
                        doc.Save(ViewModel.InstallDirectory + Path.GetFileName(addin.AddinFilePath));
                    }
                    continue;
                }

                // (Konrad) Temp path dll, to file moved from install location
                // Keeps install location free from being locked by Revit.
                // If addin hasn't been installed yet, we create a button for it,
                // but assign it a Temp.dll reference so that we can re-assign it later.
                var relativePath = addin.DllRelativePath.Replace("Temp\\", "");
                var dllPath = addin.IsInstalled ? ViewModel.InstallDirectory + relativePath : ViewModel.TempDirectory + "Temp.dll";

                var panel = m_app.GetRibbonPanels("  HOK - Beta").FirstOrDefault(x => x.Name == addin.Panel) 
                    ?? m_app.CreateRibbonPanel(tabName, addin.Panel);

                var button = (PushButton)panel.AddItem(new PushButtonData(addin.Panel, addin.ButtonText, dllPath, addin.CommandNamespace));
                button.LargeImage = addin.Image;
                button.ToolTip = addin.Description;

                button.Visible = addin.IsInstalled;
                panel.Visible = panel.GetItems().Any(x => x.Visible);
            }

            currentAssembly = Assembly.GetAssembly(GetType()).Location;
            CreateInstallerPanel();

            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            SerializeSetting(ViewModel.InstallDirectory + "BetaSettings.json");

            return Result.Succeeded;
        }

        /// <summary>
        /// Creates a beta installed button.
        /// </summary>
        private void CreateInstallerPanel()
        {
            try
            {
                var installerPanel = m_app.CreateRibbonPanel(tabName, "Installer");

                var installerButton = (PushButton)installerPanel.AddItem(new PushButtonData("AddinInstallerCommand", 
                    "Beta Addin" + Environment.NewLine + "Installer", 
                    currentAssembly, 
                    "HOK.BetaToolsManager.AddinInstallerCommand"));

                var assembly = Assembly.GetExecutingAssembly();
                installerButton.LargeImage = ButtonUtil.LoadBitmapImage(assembly, typeof(AppCommand).Namespace, "betaPluginManager_32x32.png");
                installerButton.Image = installerButton.LargeImage;
                installerButton.ToolTip = "HOK Beta Tools Installer";
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// This serializes settings file with installed/beta addins.
        /// </summary>
        /// <param name="filePath">Path to settings file.</param>
        public void SerializeSetting(string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(ViewModel.Addins, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
