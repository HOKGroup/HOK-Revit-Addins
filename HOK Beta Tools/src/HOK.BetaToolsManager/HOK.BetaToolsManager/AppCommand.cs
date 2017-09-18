using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Newtonsoft.Json;
using Autodesk.Revit.UI.Events;

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
        private static Queue<Action<UIApplication>> Tasks;

        public Result OnStartup(UIControlledApplication application)
        {
            Instance = this;
            m_app = application;
            var versionNumber = m_app.ControlledApplication.VersionNumber;
            Tasks = new Queue<Action<UIApplication>>();

            application.Idling += OnIdling;

            try
            {
                m_app.CreateRibbonTab(tabName);
            }
            catch
            {
                Log.AppendLog(LogMessageType.WARNING, "Ribbon tab was not created. It might already exist.");
            }
            
            var panelsVisibility = new Dictionary<string, bool>();
            ViewModel = new AddinInstallerModel(versionNumber);
            foreach (var addin in ViewModel.Addins)
            {
                // (Konrad) Currently the only way to distinguish between ExternalCommands and ExternalApplications
                // is via "ButtonText" attribute. It should be empty for ExternalApplications. 
                if (string.IsNullOrEmpty(addin.ButtonText))
                {
                    if (addin.IsInstalled)
                    {
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
                        File.Copy(ViewModel.TempDirectory + Path.GetFileName(addin.AddinFilePath), ViewModel.InstallDirectory + Path.GetFileName(addin.AddinFilePath));

                        if (addin.AdditionalButtonNames != null)
                        {
                            panelsVisibility.Add(addin.Panel, true);
                        }
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

                var button = (PushButton)panel.AddItem(new PushButtonData(addin.Name + "_Command", addin.ButtonText, dllPath, addin.CommandNamespace));
                button.LargeImage = addin.Image;
                button.ToolTip = addin.Description;

                button.Visible = addin.IsInstalled;

                if (panelsVisibility.ContainsKey(addin.Panel))
                {
                    if (panelsVisibility[addin.Panel]) panel.Visible = true;
                }
                else
                {
                    panel.Visible = panel.GetItems().Any(x => x.Visible);
                }
            }

            currentAssembly = Assembly.GetAssembly(GetType()).Location;
            CreateInstallerPanel();

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            SerializeSetting(ViewModel.InstallDirectory + "BetaSettings.json");

            return Result.Succeeded;
        }

        /// <summary>
        /// Handled Idling events. Currently Communicator uses it to interact with Revit.
        /// It checks a queue for any outstanding tasks and executes them.
        /// </summary>
        private static void OnIdling(object sender, IdlingEventArgs e)
        {
            var app = (UIApplication)sender;
            lock (Tasks)
            {
                if (Tasks.Count <= 0) return;

                var task = Tasks.Dequeue();
                task(app);
            }
        }

        /// <summary>
        /// Adds action to task list.
        /// </summary>
        /// <param name="task">Task to be executed.</param>
        public static void EnqueueTask(Action<UIApplication> task)
        {
            lock (Tasks)
            {
                Tasks.Enqueue(task);
            }
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
                installerButton.AvailabilityClassName = "HOK.BetaToolsManager.Availability";
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

    /// <summary>
    /// Determines if Button is enabled in ZeroDocument state.
    /// </summary>
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
