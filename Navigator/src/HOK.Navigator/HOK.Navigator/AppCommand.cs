using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Events;
using HOK.Core.Utilities;

namespace HOK.Navigator
{
    public class AppCommand : IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentDirectory = "";
        private string currentAssembly = "";
        private string versionNumber = "";
        public PushButton helpButton;
        private bool onCitrix;

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ApplicationClosing -= EventAppClosing;
            application.ControlledApplication.ApplicationInitialized -= EventAppInitialize;

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            m_app = application;
            tabName = "   HOK   ";
            versionNumber = m_app.ControlledApplication.VersionNumber;
            
            currentAssembly = Assembly.GetAssembly(GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);

            var machineName = Environment.MachineName.ToUpper();
            if (machineName.Contains("SVR") || machineName.Contains("VS"))
            {
                onCitrix = true;
            }

            application.ControlledApplication.ApplicationInitialized += EventAppInitialize;
            application.ApplicationClosing += EventAppClosing;
            return Result.Succeeded;
        }

        private void EventAppInitialize(object sender, ApplicationInitializedEventArgs arg)
        {
            CreatePushButton();
        }

        /// <summary>
        /// Creates push button for Navigator.
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

                var created = m_app.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name == "Help");
                var helpPanel = created ?? m_app.CreateRibbonPanel(tabName, "Help");
                var assembly = Assembly.GetExecutingAssembly();
                var hokImage = LoadBitmapImage(assembly, "hok.png");

                var buttonData = new PushButtonData("HOK Navigator", "HOK" + Environment.NewLine + " Navigator ", currentAssembly, "HOK.Navigator.HelpCommand")
                {
                    AvailabilityClassName = "HOK.Navigator.Availability",
                    LargeImage = hokImage,
                    Image = hokImage
                };
                helpButton = helpPanel.AddItem(buttonData) as PushButton;
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

        private void EventAppClosing(object sender, ApplicationClosingEventArgs arg)
        {
            if (onCitrix)
            {
                InstallerTrigger.Activated = false;
            }
            ReguralUpdate();
        }

        private static void ReguralUpdate()
        {
            try
            {
                if (!InstallerTrigger.Activated) return;

                var activatedInstaller = new Dictionary<string, bool>();
                var installerUrl = new Dictionary<string, string>();
                activatedInstaller = InstallerTrigger.ActivatedInstaller;
                installerUrl = InstallerTrigger.InstallerUrl;

                if (activatedInstaller.Count <= 0) return;

                foreach (var installerName in activatedInstaller.Keys)
                {
                    if (!activatedInstaller[installerName]) continue;
                    if (!installerUrl.ContainsKey(installerName)) continue;

                    var url = installerUrl[installerName];
                    System.Diagnostics.Process.Start(url);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }
    }

    public static class InstallerTrigger
    {
        public static bool Activated { get; set; }
        public static Dictionary<string, string> InstallerUrl { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, bool> ActivatedInstaller { get; set; } = new Dictionary<string, bool>();
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
