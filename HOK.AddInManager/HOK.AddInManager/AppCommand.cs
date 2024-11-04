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
using Nice3point.Revit.Toolkit.External;

namespace HOK.AddInManager
{
    public class AppCommand : ExternalApplication
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

        public override void OnStartup()
        {
            thisApp = this;
            m_app = Application;
            try
            {
                try
                {
                    m_app.CreateRibbonTab(tabName);
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }

                versionNumber = m_app.ControlledApplication.VersionNumber;
                sourceDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber + "\\_HOK";
                installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber;
                addinResources = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber + "\\HOK-Addin.bundle\\Contents\\Resources";

                Application.ControlledApplication.ApplicationInitialized += ReadAddinSettingsOnInitialized;
                Application.ApplicationClosing += ApplicationOnClosing;

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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public override void OnShutdown()
        {
            Application.ControlledApplication.ApplicationInitialized -= ReadAddinSettingsOnInitialized;
            Application.ApplicationClosing -= ApplicationOnClosing;
        }

        /// <summary>
        /// Reads in the XML Settings file.
        /// </summary>
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
                addins.AddinCollection.Where(x => x.ToolLoadType == LoadType.Always && x.DropdownOptionsFlag == 1).ToList().ForEach(AddPlugin);
                var addinsToLoad = addins.AddinCollection.Where(x => x.ToolLoadType == LoadType.Always && x.DropdownOptionsFlag == 2).ToList();
                foreach (var addin in addinsToLoad)
                {
                    foreach (var addinPath in addin.AddInPaths)
                    {
                        try
                        {
                            m_app.LoadAddIn(addinPath);
                        }
                        catch (Exception e)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        
        /// <summary>
        /// Goes through all plugins and applies appropriate action.
        /// </summary>
        public void ProcessPlugins()
        {
            if (!addins.AddinCollection.Any()) return;

            foreach (var addin in addins.AddinCollection)
            {
                switch (addin.ToolLoadType)
                {
                    case LoadType.Never:
                        RemovePlugin(addin);
                        break;
                    case LoadType.ThisSessionOnly:
                        foreach (var addinPath in addin.AddInPaths)
                        {
                            try
                            {
                                m_app.LoadAddIn(addinPath);
                            }
                            catch (Exception e)
                            {
                                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                            }
                        }
                        break;
                    case LoadType.Always:
                        if (addin.DropdownOptionsFlag == 2) {
                            foreach (var addinPath in addin.AddInPaths)
                            {
                                try
                                {
                                    m_app.LoadAddIn(addinPath);
                                }
                                catch (Exception e)
                                {
                                    Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                                }
                            }
                        } else {
                            AddPlugin(addin); // Copy to user profile method
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Adds plugin.
        /// </summary>
        /// <param name="addin">Plugin information.</param>
        private static void AddPlugin(AddinInfo addin)
        {
            for (var i = 0; i < addin.AddInPaths.Length; i++)
            {
                if (File.Exists(addin.AddInPaths[i]) && !File.Exists(addin.InstallPaths[i]))
                {
                    try
                    {
                        File.Copy(addin.AddInPaths[i], addin.InstallPaths[i], true);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Removes plugin.
        /// </summary>
        /// <param name="addin">Plugin information.</param>
        private static void RemovePlugin(AddinInfo addin)
        {
            foreach (var installPath in addin.InstallPaths)
            {
                if (File.Exists(installPath))
                {
                    try
                    {
                        File.Delete(installPath);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Handler for ApplicationClosing event.
        /// </summary>
        private void ApplicationOnClosing(object source, ApplicationClosingEventArgs args)
        {
            try
            {
                //change InSessionOnly to Never
                foreach (var addin in addins.AddinCollection)
                {
                    if (addin.ToolLoadType != LoadType.ThisSessionOnly) continue;

                    // (Konrad) Override plug-in settings, remove it from current session and serialize
                    addin.ToolLoadType = LoadType.Never;
                    RemovePlugin(addin);
                }
                SettingUtil.WriteConfig(settingPath, addins);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
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
