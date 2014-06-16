using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using System.IO;
using System.Windows.Media.Imaging;

namespace HOK.LPDCalculator
{
    public class AppCommand:IExternalApplication
    {
        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            RibbonPanel rp = application.CreateRibbonPanel("HOK Sustainability");
            string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;

            PushButton pb1 = rp.AddItem(new PushButtonData("LPD Analysis", "LPD Analysis", currentAssembly, "HOK.LPDCalculator.Command")) as PushButton;
            Uri uriImage = new Uri(Path.GetDirectoryName(currentAssembly) + "/Resources/bulb.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pb1.LargeImage = largeImage;
            return Result.Succeeded;
        }
    }
}
