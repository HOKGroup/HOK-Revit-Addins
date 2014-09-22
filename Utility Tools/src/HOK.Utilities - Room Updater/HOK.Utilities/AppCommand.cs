using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Reflection;
using System.Windows;

namespace HOK.Utilities
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
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            tabName = " HOK Tools ";
            try { application.CreateRibbonTab(tabName); }
            catch { }

            RibbonPanel rp = application.CreateRibbonPanel(tabName, "HOK Utilities");

            PushButton pushButton = rp.AddItem(new PushButtonData("Room Updater", "Room Updater", currentAssembly, "HOK.Utilities.RoomCommand")) as PushButton;
            pushButton.LargeImage = LoadBitmapImage(assembly, "cube.png");
            pushButton.ToolTip = "Assign room data into enclosed elements.";

            return Result.Succeeded;
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
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }
    }
}
