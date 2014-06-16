using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using Autodesk.Revit.UI.Events;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using Autodesk.Revit.DB.Events;
using System.Net.NetworkInformation;
using System.Reflection;

namespace HOK.Navigator
{
    public class AppCommand:IExternalApplication
    {
        private UIControlledApplication m_app;
        private string tabName = "";
        private string currentDirectory = "";
        private string currentAssembly = "";
        private string versionNumber = "";
        public PushButton helpButton = null;
        private ToolManager toolManager;
        private bool onCitrix = false;

        public Result OnShutdown(UIControlledApplication application)
        {
            application.ApplicationClosing -= new EventHandler<ApplicationClosingEventArgs>(EventAppClosing);
            application.ControlledApplication.ApplicationInitialized -= new EventHandler<ApplicationInitializedEventArgs>(EventAppInitialize);

            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            m_app = application;
            tabName = "   HOK   ";
            versionNumber = m_app.ControlledApplication.VersionNumber;
            toolManager = new ToolManager(versionNumber);
            
            currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            currentDirectory = Path.GetDirectoryName(currentAssembly);

            string machineName = Environment.MachineName.ToUpper();
            if (machineName.Contains("SVR") || machineName.Contains("VS"))
            {
                onCitrix = true;
            }

            application.ControlledApplication.ApplicationInitialized += new EventHandler<ApplicationInitializedEventArgs>(EventAppInitialize);
            application.ApplicationClosing += new EventHandler<ApplicationClosingEventArgs>(EventAppClosing);
            return Result.Succeeded;
        }

        private void EventAppInitialize(object sender, ApplicationInitializedEventArgs arg)
        {
            CreatePushButton();
            if (CheckNetworkConnection())
            {
                toolManager.FindOutDatedTools();
            }
        }

        private bool CheckNetworkConnection()
        {
            bool result = false;
            try
            {
                NetworkInterface[] netIntrfc = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface networkInterface in netIntrfc)
                {
                    if (networkInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel && networkInterface.Name.Contains("group.hok.com"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to verify the newtwork connection.\n" + ex.Message, "Check Network Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return result;
        }

        private void CreatePushButton()
        {
            try
            {
                //create a new button
                try { m_app.CreateRibbonTab(tabName); }
                catch { }

                foreach (RibbonPanel panel in m_app.GetRibbonPanels(tabName))
                {
                    if (panel.Name == "Help")
                    {
                        foreach (RibbonItem ribbonItem in panel.GetItems())
                        {
                            if (ribbonItem.ItemText.Equals("   Help   ") || ribbonItem.ItemText.Equals("HOK Navigator"))
                            {
                                helpButton = ribbonItem as PushButton;
                                break;
                            }
                        }
                    }
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                BitmapImage hokImage = LoadBitmapImage(assembly, "hok.png");

                if (null != helpButton)
                {
                    //already exsit
                    helpButton.ItemText = "HOK Navigator";
                    helpButton.LargeImage = hokImage;
                    helpButton.Image = hokImage;
                    helpButton.AvailabilityClassName = "HOK.Navigator.Availability";
                    helpButton.AssemblyName = currentAssembly;
                    helpButton.ClassName = "HOK.Navigator.HelpCommand";
                }
                else
                {
                    RibbonPanel helpPanel = m_app.CreateRibbonPanel(tabName, "Help");
                    PushButtonData buttonData = new PushButtonData("HOK Navigator", "HOK Navigator", currentAssembly, "HOK.Navigator.HelpCommand");
                    buttonData.AvailabilityClassName = "HOK.Navigator.Availability";
                    buttonData.LargeImage = hokImage;
                    buttonData.Image = hokImage;
                    helpButton = helpPanel.AddItem(buttonData) as PushButton;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private BitmapImage LoadBitmapImage(Assembly assembly, string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void ReguralUpdate()
        {
            try
            {
                if (toolManager.OutDatedTools.Count > 0)
                {
                    //install tools without the notification message
                    toolManager.InstallTools();
                }

                if (InstallerTrigger.Activated)
                {
                    Dictionary<string, bool> activatedInstaller = new Dictionary<string, bool>();
                    Dictionary<string, string> installerUrl = new Dictionary<string, string>();
                    activatedInstaller = InstallerTrigger.ActivatedInstaller;
                    installerUrl = InstallerTrigger.InstallerUrl;

                    if (activatedInstaller.Count > 0)
                    {
                        foreach (string installerName in activatedInstaller.Keys)
                        {
                            if (activatedInstaller[installerName])
                            {
                                if (installerUrl.ContainsKey(installerName))
                                {
                                    string url = installerUrl[installerName];
                                    System.Diagnostics.Process.Start(url);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update AddIns.\n" + ex.Message, "HOK Navigator: Regular Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    public static class InstallerTrigger
    {
        private static bool activated = false;
        public static bool Activated { get { return activated; } set { activated = value; } }

        private static Dictionary<string, string> installerUrl = new Dictionary<string, string>();
        public static Dictionary<string, string> InstallerUrl { get { return installerUrl; } set { installerUrl = value; } }

        private static Dictionary<string, bool> activatedInstaller = new Dictionary<string, bool>();
        public static Dictionary<string, bool> ActivatedInstaller { get { return activatedInstaller; } set { activatedInstaller = value; } }
    }

    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, Autodesk.Revit.DB.CategorySet selectedCategories)
        {
            return true;
        }
    }
}
