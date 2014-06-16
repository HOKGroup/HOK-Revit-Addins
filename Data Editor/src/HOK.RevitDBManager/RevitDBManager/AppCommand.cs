using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;

namespace RevitDBManager
{
    class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("HOK Revit Data");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            PushButton pb1 = rp.AddItem(new PushButtonData("Data Sync", "Data Sync", currentAssembly, "RevitDBManager.Command")) as PushButton;
            Uri uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/sync.ico");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pb1.LargeImage = largeImage;

            //previously Editor, changed to Setup
            PushButton pb2 = rp.AddItem(new PushButtonData("Setup", "  Setup  ", currentAssembly, "RevitDBManager.EditorCommand")) as PushButton;
            Uri uriImage2 = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/editor.ico");
            BitmapImage largeImage2 = new BitmapImage(uriImage2);
            pb2.LargeImage = largeImage2;

            //previously Viewer, changed to Editor
            PushButton pb3 = rp.AddItem(new PushButtonData("Data Editor", "Data Editor", currentAssembly, "RevitDBManager.ViewerCommand")) as PushButton;
            Uri uriImage3 = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/view.ico");
            BitmapImage largeImage3 = new BitmapImage(uriImage3);
            pb3.LargeImage = largeImage3;

            return Result.Succeeded;
        }
    }
}
