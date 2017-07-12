using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using HOK.AddInManager.Classes;
using HOK.AddInManager.Utils;
using HOK.Core.Utilities;

namespace HOK.AddInManager
{
    public class AppCommand : IExternalApplication
    {
        internal static AppCommand thisApp;
        private UIControlledApplication m_app;
        private const string tabName = "   HOK   ";
        private string versionNumber = "";
        private string sourceDirectory = "";
        private string installDirectory = "";
        private string addinResources = "";
        public ToolTipProperties addinManagerToolTip = new ToolTipProperties();

        public Addins addins = new Addins();
        private string csvFileName = "";
        private const string settingFile = "HOKAddinSettings.xml";
        public string settingPath = "";

        public Result OnStartup(UIControlledApplication application)
        {
            thisApp = this;
            m_app = application;
            try
            {
                try
                {
                    m_app.CreateRibbonTab(tabName);
                }
                catch (Exception ex)
                {
                    Log.AppendLog(ex.Message);
                }

                versionNumber = m_app.ControlledApplication.VersionNumber;
                sourceDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber + "\\_HOK";
                installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber;
                addinResources = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber + "\\HOK-Addin.bundle\\Contents\\Resources";

                application.ControlledApplication.ApplicationInitialized += ReadAddinSettingsOnInitialized;
                application.ApplicationClosing += ApplicationOnClosing;

                //ribbon panel
                 var toolTipText = Path.Combine(addinResources, "HOK.Tooltip.txt");
                 addinManagerToolTip = ToolTipReader.ReadToolTip(toolTipText, "Addin Manager");
                    
                var panel = m_app.CreateRibbonPanel(tabName, " Addins ");
                var currentAssembly = Assembly.GetExecutingAssembly();

                var installerButton = (PushButton)panel.AddItem(new PushButtonData("Addin Manager", "Addin Manager", currentAssembly.Location, "HOK.AddInManager.Command"));
                var pluginImage = ButtonUtil.LoadBitmapImage(currentAssembly, typeof(AppCommand).Namespace, "pluginManager_32.png");
                installerButton.LargeImage = pluginImage;
                installerButton.ToolTip = addinManagerToolTip.Description;
                installerButton.AvailabilityClassName = "HOK.AddInManager.Availability";
                var contextualHelp = new ContextualHelp(ContextualHelpType.Url, addinManagerToolTip.HelpUrl);
                installerButton.SetContextualHelp(contextualHelp);
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized -= ReadAddinSettingsOnInitialized;
            application.ApplicationClosing -= ApplicationOnClosing;
            return Result.Succeeded;
        }

        /// <summary>
        /// Reads in the XML Settings file.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void ReadAddinSettingsOnInitialized(object source, ApplicationInitializedEventArgs args)
        {
            try
            {
                //read list of tools
                csvFileName = "HOK" + versionNumber + "Addins.csv";
                csvFileName = Path.Combine(addinResources, csvFileName);
                if (File.Exists(csvFileName))
                {
                    addins.AddinCollection = CsvUtil.ReadAddInList(csvFileName, sourceDirectory, installDirectory);
                }

                //override settings
                settingPath = Path.Combine(installDirectory, settingFile);
                SettingUtil.ReadConfig(settingPath, ref addins);

                //add addins
                AddToolsBySettings();
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        public void RemoveToolsBySettings()
        {
            try
            {
                var toolsFound = addins.AddinCollection.Where(x => x.ToolLoadType == LoadType.Never).ToList();
                if (!toolsFound.Any()) return;

                foreach (var addin in toolsFound)
                {
                    foreach (var installPath in addin.InstallPaths)
                    {
                        if (File.Exists(installPath))
                        {
                            File.Delete(installPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        public void AddToolsBySettings()
        {
            try
            {
                var toolsFound = addins.AddinCollection.Where(x => x.ToolLoadType == LoadType.Always).ToList();
                if (!toolsFound.Any()) return;

                foreach (var addin in toolsFound)
                {
                    for (var i = 0; i < addin.AddInPaths.Length; i++)
                    {
                        if (File.Exists(addin.AddInPaths[i]) && !File.Exists(addin.InstallPaths[i]))
                        {
                            File.Copy(addin.AddInPaths[i], addin.InstallPaths[i], true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        public void LoadToolsBySettings()
        {
            try
            {
                var toolsFound = addins.AddinCollection.Where(x => x.ToolLoadType == LoadType.ThisSessionOnly).ToList();
                if (!toolsFound.Any()) return;

                foreach (var addin in toolsFound)
                {
                    foreach (var addinPath in addin.AddInPaths)
                    {
                        try
                        {
                            m_app.LoadAddIn(addinPath);
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
            }
        }

        private void ApplicationOnClosing(object source, ApplicationClosingEventArgs args)
        {
            try
            {
                //change InSessionOnly to Never
                for (var i = 0; i < addins.AddinCollection.Count; i++)
                {
                    var addin = addins.AddinCollection[i];
                    if (addin.ToolLoadType == LoadType.ThisSessionOnly)
                    {
                        addins.AddinCollection[i].ToolLoadType = LoadType.Never;
                    }
                }
               
                //write configuration
                RemoveToolsBySettings();
                SettingUtil.WriteConfig(settingPath, addins);
            }
            catch (Exception ex)
            {
                Log.AppendLog(ex.Message);
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
