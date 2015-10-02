using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using HOK.ModelManager.RegistryManager;
using System.Reflection;

namespace HOK.ModelManager
{

    public class AppCommand:IExternalApplication
    {
        private string tabName = "  HOK - Beta";

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            string currentDirectory = Path.GetDirectoryName(currentAssembly);

            try { application.CreateRibbonTab(tabName); }
            catch { }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            RibbonPanel rp = application.CreateRibbonPanel(tabName, "Model Manager");
            SplitButtonData splitButtonData = new SplitButtonData("ModelManager", "Model Manager");
            SplitButton splitButton = rp.AddItem(splitButtonData) as SplitButton;

            PushButton projectButton = splitButton.AddPushButton(new PushButtonData("ProjectReplication", "Project Replication", currentAssembly, "HOK.ModelManager.ProjectCommand"));
            projectButton.LargeImage = LoadBitmapImage("project.png"); ;
            projectButton.ToolTip = "Transfer data from opened models to the active Revit model.";

            if (ModelBuilderActivated())
            {
                PushButton modelButton = splitButton.AddPushButton(new PushButtonData("ModelBuilder", "Model Builder", currentAssembly, "HOK.ModelManager.ModelCommand"));
                modelButton.LargeImage = LoadBitmapImage("model.png"); ;
                modelButton.ToolTip = "Specify resource locations and transfer the data into the active Revit model.";
            }

            return Result.Succeeded;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = new AssemblyName(args.Name);
                string[] assemblyNames = new string[] { "System.Net.Http.Primitives", "Newtonsoft.Json" };
                if (assemblyNames.Contains(name.Name))
                {
                    string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                    string assemblyToLoad = Path.Combine(Path.GetDirectoryName(currentAssembly), name.Name + ".dll");
                    Assembly assemblyLoaded = Assembly.LoadFrom(assemblyToLoad);
                    if (null != assemblyLoaded) { return assemblyLoaded; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load assembly.\n" + ex.Message);
            }
            return null;
        }

        private BitmapImage LoadBitmapImage(string imageName)
        {
            BitmapImage image = new BitmapImage();
            try
            {
                string prefix = typeof(AppCommand).Namespace + ".Resources.";
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }

        private bool ModelBuilderActivated()
        {
            bool activated = false;
            try
            {
                string value = RegistryUtil.GetRegistryKey("ModelBuilderActivated");
                if (!string.IsNullOrEmpty(value))
                {
                    activated = Convert.ToBoolean(value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find an activation key for the Model Builder.\n"+ex.Message, "Model Builder Activation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return activated;
        }
    }
}
