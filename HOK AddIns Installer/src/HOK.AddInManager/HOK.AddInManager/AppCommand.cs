using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using HOK.AddInManager.Classes;
using HOK.AddInManager.Properties;
using HOK.AddInManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HOK.AddInManager
{
    public class AppCommand:IExternalApplication
    {
        internal static AppCommand thisApp = null;
        private UIControlledApplication m_app;
        private string tabName = "   HOK   ";
        private string versionNumber = "";
        private string sourceDirectory = "";
        private string installDirectory = "";
        private string addinResources = "";
        public ToolTipProperties addinManagerToolTip = new ToolTipProperties();


        public Addins addins = new Addins();
        private string csvFileName = "";
        private string settingFile = "HOKAddinSettings.xml";
        public string settingPath = "";

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized -= ReadAddinSettingsOnInitialized;
            application.ApplicationClosing -= ApplicationOnClosing;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            thisApp = this;
            m_app = application;
            try
            {
                try { m_app.CreateRibbonTab(tabName); }
                catch { }

                versionNumber = m_app.ControlledApplication.VersionNumber;
                sourceDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber + "\\_HOK";
                installDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber;
                addinResources = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\REVIT\\Addins\\" + versionNumber +
                           "\\HOK-Addin.bundle\\Contents\\Resources";

                application.ControlledApplication.ApplicationInitialized += ReadAddinSettingsOnInitialized;
                application.ApplicationClosing += ApplicationOnClosing;

                //ribbon panel
                 string toolTipText = System.IO.Path.Combine(addinResources, "HOK.Tooltip.txt");
                 addinManagerToolTip = ToolTipReader.ReadToolTip(toolTipText, "Addin Manager");
                    
                RibbonPanel panel = m_app.CreateRibbonPanel(tabName, "Add-Ins");
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

                PushButton installerButton = panel.AddItem(new PushButtonData("Addin Manager", "Addin Manager", currentAssembly, "HOK.AddInManager.Command")) as PushButton;
                installerButton.AvailabilityClassName = "HOK.AddInManager.Availability";
                BitmapSource pluginImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Resources.plugin.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                installerButton.LargeImage = pluginImage;
                installerButton.ToolTip = addinManagerToolTip.Description;
                ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, addinManagerToolTip.HelpUrl);
                installerButton.SetContextualHelp(contextualHelp);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return Result.Succeeded;
        }

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
                string message = ex.Message;
            }
        }

        public void RemoveToolsBySettings()
        {
            try
            {
                var toolsFound = from addin in addins.AddinCollection where addin.ToolLoadType == LoadType.Never select addin;
                if (toolsFound.Count() > 0)
                {
                    foreach (AddinInfo addin in toolsFound)
                    {
                        foreach (string installPath in addin.InstallPaths)
                        {
                            if (File.Exists(installPath))
                            {
                                File.Delete(installPath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void AddToolsBySettings()
        {
            try
            {
                var toolsFound = from addin in addins.AddinCollection where addin.ToolLoadType == LoadType.Always select addin;
                if (toolsFound.Count() > 0)
                {
                    foreach (AddinInfo addin in toolsFound)
                    {
                        for (int i = 0; i < addin.AddInPaths.Length; i++)
                        {
                            if (File.Exists(addin.AddInPaths[i]) && !File.Exists(addin.InstallPaths[i]))
                            {
                                File.Copy(addin.AddInPaths[i], addin.InstallPaths[i], true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void LoadToolsBySettings()
        {
            try
            {
                var toolsFound = from addin in addins.AddinCollection where addin.ToolLoadType == LoadType.ThisSessionOnly select addin;
                if (toolsFound.Count() > 0)
                {
                    foreach (AddinInfo addin in toolsFound)
                    {
                        foreach (string addinPath in addin.AddInPaths)
                        {
                            try
                            {
                                m_app.LoadAddIn(addinPath);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private void ApplicationOnClosing(object source, ApplicationClosingEventArgs args)
        {
            try
            {
                //change InSessionOnly to Never
                for (int i = 0; i < addins.AddinCollection.Count; i++)
                {
                    AddinInfo addin = addins.AddinCollection[i];
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
                string message = ex.Message;
            }
        }

    }

    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
