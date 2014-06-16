using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;

namespace HOK.BCFReader
{
    public class AppCommand:IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("BCF");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            PushButton pushBttn = rp.AddItem(new PushButtonData("BCF Reader", "BCF Reader", currentAssembly, "HOK.BCFReader.Command")) as PushButton;
            Uri uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/comment.ico");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushBttn.LargeImage = largeImage;
            return Result.Succeeded;
        }
    }
}
