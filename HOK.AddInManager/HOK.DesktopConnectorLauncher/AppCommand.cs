using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using HOK.Core.Utilities;
using Microsoft.Win32;

namespace HOK.DesktopConnectorLauncher
{
    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentAssembly = "";
        public PushButton helpButton;
        public PushButton desktopConnectorButton;

        public Result OnStartup(UIControlledApplication application)
        {
            m_app = application;
            tabName = "   HOK   ";
            currentAssembly = Assembly.GetAssembly(GetType()).Location;

            application.ControlledApplication.ApplicationInitialized += EventAppInitialize;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized -= EventAppInitialize;

            return Result.Succeeded;
        }

        private void EventAppInitialize(object sender, ApplicationInitializedEventArgs arg)
        {
            // Get registry key
            string keyName = @"HKEY_CURRENT_USER\SOFTWARE\HOK\BIM\";
            string valueName = @"DesktopConnectorPath_Citrix";
            string path = (string)Registry.GetValue(keyName, valueName, null);
            if (path != null && System.IO.File.Exists(path)) {
                CreatePushButton();
            }
            // If exists + is valid path
        }

        /// <summary>
        /// Creates push button for Desktop Connector.
        /// </summary>
        private void CreatePushButton()
        {
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

                var created = m_app.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == " Addins ");
                var addinsPanel = created ?? m_app.CreateRibbonPanel(tabName, " Addins ");
                var assembly = Assembly.GetExecutingAssembly();
                var iconImage = LoadBitmapImage(assembly, "desktopConnector.png");
                var addinManagerButton = addinsPanel.GetItems().FirstOrDefault();

                var buttonData = new PushButtonData("HOK Desktop Connector Launcher", "Launch Autodesk" + Environment.NewLine + "Desktop Connector ", currentAssembly, "HOK.DesktopConnectorLauncher.DesktopConnectorCommand")
                {
                    AvailabilityClassName = "HOK.DesktopConnectorLauncher.Availability",
                    LargeImage = iconImage,
                    Image = iconImage
                };

                if (addinManagerButton != null)
                {
                    SplitButtonData splitButtonData = new SplitButtonData("Addins", "Text");
                    SplitButton splitButton = addinsPanel.AddItem(splitButtonData) as SplitButton;
                    var pbAddinManager = addinManagerButton as PushButton;
                    PushButtonData newAddinManagerButton = new PushButtonData(pbAddinManager.Name, pbAddinManager.ItemText, pbAddinManager.AssemblyName, pbAddinManager.ClassName)
                    {
                        AvailabilityClassName = pbAddinManager.AvailabilityClassName,
                        LargeImage = pbAddinManager.LargeImage,
                        Image = pbAddinManager.Image,
                        ToolTip = pbAddinManager.ToolTip
                        
                    };
                    newAddinManagerButton.SetContextualHelp(pbAddinManager.GetContextualHelp());
                    pbAddinManager.Enabled = false;
                    pbAddinManager.Visible = false;
                    splitButton.AddPushButton(newAddinManagerButton);
                    splitButton.AddPushButton(buttonData);

                } else
                {
                    desktopConnectorButton = addinsPanel.AddItem(buttonData) as PushButton; 
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private static BitmapImage LoadBitmapImage(Assembly assembly, string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return image;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
