using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using HOK.ModelManager.RegistryManager;

namespace HOK.ModelManager
{

    public class AppCommand:IExternalApplication
    {
        private string tabName = "";

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
            string currentDirectory = Path.GetDirectoryName(currentAssembly);

            tabName = " HOK Tools ";
            try { application.CreateRibbonTab(tabName); }
            catch { }

            RibbonPanel rp = application.CreateRibbonPanel(tabName, "Model Manager");
            SplitButtonData splitButtonData = new SplitButtonData("ModelManager", "Model Manager");
            SplitButton splitButton = rp.AddItem(splitButtonData) as SplitButton;

            PushButton projectButton = splitButton.AddPushButton(new PushButtonData("ProjectReplication", "Project Replication", currentAssembly, "HOK.ModelManager.ProjectCommand"));
            Uri uriImage = new Uri(currentDirectory + "/Resources/project.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            projectButton.LargeImage = largeImage;
            projectButton.ToolTip = "Transfer data from opened models to the active Revit model.";

            if (ModelBuilderActivated())
            {
                PushButton modelButton = splitButton.AddPushButton(new PushButtonData("ModelBuilder", "Model Builder", currentAssembly, "HOK.ModelManager.ModelCommand"));
                uriImage = new Uri(currentDirectory + "/Resources/model.png");
                largeImage = new BitmapImage(uriImage);
                modelButton.LargeImage = largeImage;
                modelButton.ToolTip = "Specify resource locations and transfer the data into the active Revit model.";
            }

            return Result.Succeeded;
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
